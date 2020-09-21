using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    const float LifeTime = 15.0f;
    

    [SerializeField]
    [SyncVar]
    Vector3 MoveDirection = Vector3.zero;

    [SerializeField]
    [SyncVar]
    float Speed = 0.0f;

    [SyncVar]
    bool NeedMove = false;
    [SyncVar]
    bool Hited = false;
    [SyncVar]
    float FiredTime = 0.0f;

    [SyncVar]
    [SerializeField]
    int Damage = 1;

    [SyncVar]
    [SerializeField]
    int OwnerInstanceID;

    [SyncVar]
    [SerializeField]
    string filePath;
    public string FilePath { get { return filePath; } set { filePath = value; } }

    // Start is called before the first frame update
    void Start()
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
        {
            //클라이언트는 에너미를 호스트로부터 받아서 사용하기 때문에 따로 EnemyCacheSystem 등록해야함

            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            transform.SetParent(inGameSceneMain.BulletManager.transform);
            inGameSceneMain.BulletCacheSystem.Add(FilePath, gameObject);
            gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(ProcessDisappearCondition())
        {
            return;
        }

        if(NeedMove)
        {
            UpdateMove();
        }
        
    }

    void UpdateMove()
    {
        //MoveDirection으로 입력받은 벡터를 방향벡터로 만들고 Speed 및 Time.deltaTime 곱함
        Vector3 moveVector = MoveDirection.normalized * Speed * Time.deltaTime;
        //현재 총알 객체 위치 변경
        moveVector = AdjustMove(moveVector);
        transform.position += moveVector;
    }

    //총알발사 주인 종류, 총알 발사 위치(점), 총알 발사 방향, 스피드, 데미지
    //다른 외부 스크립트(Player, Enemy의 Fire())에 의해서 호출되어서 시작됨
    //즉, 각 액터들이 Fire()메소드를 통해 총알을 생성하고 이 메소드를 호출함
    public void Fire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        OwnerInstanceID = ownerInstanceID;
        SetPosition(firePosition);

        transform.position = firePosition; //시작 위치 설정
        MoveDirection = direction; //방향 설정
        Speed = speed;
        Damage = damage;

        NeedMove = true;
        FiredTime = Time.time;

        UpdateNetworkBullet();
    }

    Vector3 AdjustMove(Vector3 moveVector)
    {
        //AdjustMove - 만약의 경우를 보완하는 메소드

        //Linecast는는 시작과 끝점이 존재하는 광선
        //Raycast는 시작점만 있고 끝점이 없는 무한한 광선

        RaycastHit hitInfo;

        // 만약 시작점(현재 총알 위치)에서 끝점(총알의 도착지점) 중에서 뭔가 부딪친다면?
        // 도작점(moveVector)에 대한 수정이 필요하다.
        if (Physics.Linecast(transform.position, transform.position + moveVector, out hitInfo))
        {
            // 부딪친 객체의 위치 (hitInfo.point)에서 현재 위치(transform.position)를 빼서 수정된 벡터 생성
            moveVector = hitInfo.point - transform.position;
            OnBulletCollision(hitInfo.collider);
        }

        //도착지점 문제 없으므로 그대로 리턴
        return moveVector;
    }

    // 총알이 어딘가에 부딪친것을 처리하는 메소드
    void OnBulletCollision(Collider collider)
    {
        //총알이 충돌이후에 사라져야함
        // 이펙트 효과 추가이후에 destroy 예정

        //두번의 충돌 방지
        if (Hited) return;

        /*
         총알 끼리의 충돌을 신경 쓰지 않기 때문에
         현재 총알 객체가 1.누가 발사한 총알인지, 2.부딪친 충돌체가 누구인지만 체크
        */
        //ActorManager에서 이전에 설정된(Fire에서) OwnerInstanceID를 통해서 GetActor 
        Actor owner = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.GetActor(OwnerInstanceID);
        Actor actor = collider.GetComponentInParent<Actor>();
        if (!actor) return;
        if ( actor.IsDead  || owner.gameObject.layer == actor.gameObject.layer) return;

        //총알의 꼭지점 부분 == 충돌지점 == transform.position == true
        actor.OnBulletHited(Damage, transform.position);

        Collider myCollider = GetComponentInChildren<Collider>();
        myCollider.enabled = false;
        NeedMove = false;
        Hited = true;

        

        

        //총알이 어딘가에 부딪친다면(무조건 비행기) 진행방향의 꼭지점에 effect 0을 생성한다
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.EffectIndexName.hit, transform.position);
        if(!go)
        {
            Debug.LogError("OnBulletCollision -> EffectManager.GenerateEffect 실행 실패 index : 0");
            return;
        }

        go.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Disappear(); //그런뒤에 총알 자체는 사라진다.

    }


    private void OnTriggerEnter(Collider other)
    {
        OnBulletCollision(other);
    }

    
    //총알이 사라질지 상황을 판단하는 메소드
    bool ProcessDisappearCondition()
    {
        //너무 멀어지면 true -> 총알이 사라짐
        //이 부분은 투명벽으로 처리해서 판단하는것도 괜찮을듯
        if(transform.position.x > 15.0f || transform.position.x < -15.0f
            || transform.position.y > 15.0f || transform.position.y < -15.0f)
        {
            Disappear();
            return true;
        }
        //발사된 시간이 LifeTime 보다 큰지 확인
        else if (Time.time - FiredTime > LifeTime)
        {
            Disappear();
            return true;
        }


        return false;
    }

    //총알을 실제로 사라지게하는 메소드
    void Disappear()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.RemoveBullet(this);
        //Destroy(gameObject);
    }

    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        this.gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    public void SetPosition(Vector3 position)
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdSetPosition(position);

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때의 꼼수
        if (isServer)
        {
            // Host 플레이어인경우 RPC로 보내고
            RpcSetPosition(position);
        }
        else
        {
            // Client 플레이어인경우 Cmd로 호스트로 보낸후 자신을 Self 동작
            CmdSetPosition(position);
            if (isLocalPlayer)
                transform.position = position;
        }

    }

    [Command]
    public void CmdSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    public void UpdateNetworkBullet()
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdUpdateNetworkBullet();

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때의 꼼수
        if (isServer)
        {
            // Host 플레이어인경우 RPC로 보내고
            RpcUpdateNetworkBullet();
        }
        else
        {
            // Client 플레이어인경우 Cmd로 호스트로 보낸후 자신을 Self 동작
            CmdUpdateNetworkBullet();

        }

    }

    [Command]
    public void CmdUpdateNetworkBullet()
    {
        //캐시 내용에 변경이 있었음을 기록한 플래그 비트
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcUpdateNetworkBullet()
    {
        base.SetDirtyBit(1);
    }


}
