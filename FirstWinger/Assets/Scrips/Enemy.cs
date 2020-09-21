using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : Actor
{
    public enum State : int
    {
        None = -1, //사용전
        Ready, // 준비 완료
        Appear, // 등장
        Battle, // 전투중
        Dead, // 사망
        Disappear, // 퇴장
    }

    [SerializeField]
    [SyncVar]
    State CurrentState = State.None;

    const float MaxSpeed = 10.0f;
    const float MaxSpeedTime = 0.5f; // 맥스 스피드까지의 도달시간

    [SerializeField]
    [SyncVar]
    Vector3 TargetPosition;

    [SerializeField]
    [SyncVar]
    float CurrentSpeed;

    [SyncVar]
    Vector3 CurrentVelocity;

    [SyncVar]
    float MoveStartTime = 0.0f;
    
    [SerializeField]
    Transform FireTransform;

    [SerializeField]
    [SyncVar]
    float BulletSpeed = 1.0f;

    [SyncVar]
    float LastActionUpdateTime = 0.0f;
    [SerializeField]
    [SyncVar]
    int FireRemainCount = 2;

    [SerializeField]
    [SyncVar]
    int GamePoint = 10;

    [SyncVar]
    [SerializeField]
    string filePath;

    public string FilePath { get { return filePath; } set { filePath = value; } }

    [SyncVar]
    public Vector3 AppearPoint;
    [SyncVar]
    public Vector3 DisappearPoint;

    protected override void Intialize()
    {
        base.Intialize();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
        {
            //클라이언트는 에너미를 호스트로부터 받아서 사용하기 때문에 따로 EnemyCacheSystem 등록해야함
            transform.SetParent(inGameSceneMain.EnemyManager.transform);
            inGameSceneMain.EnemyCacheSystem.Add(FilePath, gameObject);
            gameObject.SetActive(false);
        }

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);
    }


    protected override void UpdateActor()
    {
        switch (CurrentState)
        {
            case State.None:
                break;
            case State.Ready:
                UpdateReady();
                break;
            case State.Dead:

                break;

            //등장하거나 퇴장할때 스피드와 포지션을 조정한다
            //스피드는 Lerp, 포지션은 SmoothDamp
            case State.Appear:
            case State.Disappear:
                UpdateSpeed();
                UpdateMove();
                break;
            case State.Battle:
                UpdateBattle();
                break;
        }

    }

    void UpdateSpeed()
    {
        // Mathf.Lerp 선형 보간(두 숫자 사이의 -> 여기서는 두 속도)
        // a(시작), b(끝) 사이의 어느 지점 c(0~1 사이 0%~100% 지점)을 계산함

        // 현재 MoveStartTime은 Appear에서 설정한 현재 적 객체의 등장 시간
        // 현재 타임에서 위의 MoveStartTime을 빼면 등장 이후에 얼마나 시간이 흘렀는지를 알 수 있다.
        // 즉, 등장 이후에 시간이 얼마나 흘렀는지의 비율에 따라 [ (Time.time - MoveStartTime) / MaxSpeedTime ]
        // CurrentSpeed와 MaxSpeed 사이의 어떤 값(비율)에 찍힐지 정해진다.
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, (Time.time - MoveStartTime) / MaxSpeedTime);
        //CurrentSpeed = MaxSpeed;

    }

    void UpdateMove()
    {
        float dist = Vector3.Distance(TargetPosition, transform.position);
        if(dist == 0)
        {
            Arrive();
            return;
        }

        //타겟 포지션 - 현재 포지션으로 해당 위치에서 타겟으로 향하는 방향벡터를 구하고 speed를 곱한다
        // 속도 벡터
        CurrentVelocity = (TargetPosition - transform.position).normalized * CurrentSpeed;

        //속도 = 거리 / 시간이므로 시간 = 거리 / 속도
        //현재 위치에서 타겟 위치까지 특정 속도로 시작하여 일정 smoothtime동안에 이동함(감속 또는 가속) (최대 MaxSpeed로)
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref CurrentVelocity, dist / CurrentSpeed, MaxSpeed);
    }

    void Arrive() // 도착
    {
        CurrentSpeed = 0.0f;

        if(CurrentState == State.Appear) // 현재 상태가 등장인데 Arrive? -> Battle
        {
            CurrentState = State.Battle;
            LastActionUpdateTime = Time.time; // 배틀 시작 시간 설정
        }
        else if(CurrentState == State.Disappear) // 현재 상태가 퇴장인데 Arrive? -> None
        {
            CurrentState = State.None;

            //Destroy(gameObject);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveEnemy(this);
        }
    }
    public void Reset(SquadronMemberStruct data)
    {
        if(isServer)
        {
            RpcReset(data);
        }
        else
        {
            CmdReset(data);
            if (isLocalPlayer)
                ResetData(data);
        }
    }

    void ResetData(SquadronMemberStruct data)
    {
        EnemyStruct enemyStruct = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID);

        CurrentHP = MaxHP = enemyStruct.MaxHP;
        Damage = enemyStruct.Damage;
        crashDamage = enemyStruct.CrashDamage;
        BulletSpeed = enemyStruct.BulletSpeed;
        FireRemainCount = enemyStruct.FireRemainCount;
        GamePoint = enemyStruct.GamePoint;


        AppearPoint = new Vector3(data.AppearPointX, data.AppearPointY, 0);
        DisappearPoint = new Vector3(data.DisappearPointX, data.DisappearPointY, 0);

        CurrentState = State.Ready;
        LastActionUpdateTime = Time.time;

        isDead = false; //죽은 녀석이 다시 캐시로 들어가는 경우가 있기 때문에 최초 reset마다 초기화
    }

    [Command]
    public void CmdReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    public void Appear(Vector3 targetPos) // targetPos위치로 등장
    {
        CurrentSpeed = MaxSpeed;
        TargetPosition = targetPos;

        CurrentState = State.Appear;

        //등장하는 시간
        MoveStartTime = Time.time;
    }

    void Disappear(Vector3 targetPos) // targetPos위치로 퇴장
    {
        TargetPosition = targetPos;
        CurrentSpeed = 0;

        CurrentState = State.Disappear;

        //퇴장하는 시간
        MoveStartTime = Time.time;
    }

    void UpdateReady()
    {
        if (Time.time - LastActionUpdateTime > 1.0f)
        {
            Appear(AppearPoint);
        }
    }

    void UpdateBattle()
    {
        //배틀 시작시간 이후 1초 지남
        if(Time.time - LastActionUpdateTime > 1.0f)
        {
            // 발사 횟수가 남았다면 0이 될때까지 발사 (1초마다 발사)
            if(FireRemainCount > 0)
            {
                Fire();
                FireRemainCount--;
            }
            else
            {

                Disappear(DisappearPoint);
            }

            LastActionUpdateTime = Time.time;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enemy-OnTriggerEnter: " + other.name);

        Player player = other.GetComponentInParent<Player>();
        if (player && !player.IsDead) // 플레이어에게 부딪쳤다를 통보
        {
            BoxCollider boxCollider = other as BoxCollider;
            Vector3 crashPos = player.transform.position + boxCollider.center;
            crashPos.x += boxCollider.size.x * 0.5f;

            player.OnCrash(crashDamage, crashPos);
        }
            
    }

    public void Fire()
    {
        /*
        //Bullet 자체가 GameObject 이므로 형변환 불필요
        GameObject go = Instantiate(Bullet);
        //위에서 프래팹을 통해 생성한 go 오브젝트에서 Bullet스크립트를 가져온다.
        Bullet bullet = go.GetComponent<Bullet>();
        */

        // -> 위 코드가 아래로 한줄로 대체됨
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.GenerateBullet((int)BulletManager.BulletIndex.Enemy);


        bullet.Fire(actorInstanceID, FireTransform.position, -FireTransform.right, BulletSpeed, Damage);
    }

    protected override void DecreaseHP(int damage, Vector3 damagePos)
    {
        base.DecreaseHP(damage, damagePos);

        //damagePoint: 데이미지 발생한 지점 (충돌이 발생한 지점)을 중심으로 적당히 연산
        //Random.insideUnitSphere -> 3차원 공간에서 반지름이 1인 구체안의 임의의 값을 리턴(2차원이라면 insideUnitCircle)
        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.GenerateDamage((int)DamageManager.DamageIndex.Enemy, damagePoint, damage, Color.magenta);
    }


    protected override void OnDead()
    {
        base.OnDead();

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().GamePointAccumulator.Accumulate(GamePoint);
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveEnemy(this);
        CurrentState = State.Dead;
    }
}
