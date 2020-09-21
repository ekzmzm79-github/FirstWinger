using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : Actor
{
    const string PlayerHUDPath = "Prefabs/PlayerHUD";

    [SerializeField] [SyncVar]
    Vector3 MoveVector = Vector3.zero;

    [SerializeField]
    NetworkIdentity networkIdentity = null;

    [SerializeField]
    float MoveSpeed;

    [SerializeField]
    BoxCollider boxCollider;

    [SerializeField]
    Transform FireTransform;

    [SerializeField]
    float BulletSpeed = 1.0f;

    InputController inputController = new InputController();

    [SerializeField]
    [SyncVar]
    bool Host = false;

    [SerializeField]
    Material ClientPlayerMaterial;

    protected override void Intialize()
    {
        base.Intialize();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();

        if (isLocalPlayer)
            inGameSceneMain.Hero = this;

        if (isServer && isLocalPlayer)
        {
            Host = true;
            RpcSetHost();
        }

        Transform startTransform;
        if (Host) //호스트라면 1번 위치
            startTransform = inGameSceneMain.PlayerStartTransform1;
        else // 아니면 2번위치
        {
            startTransform = inGameSceneMain.PlayerStartTransform2;
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = ClientPlayerMaterial;
        }
            

        SetPosition(startTransform.position);

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);

        InitializePlayerHUD();

    }

    void InitializePlayerHUD()
    {
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        GameObject go = Resources.Load<GameObject>(PlayerHUDPath);
        GameObject goInstrance = Instantiate<GameObject>(go, inGameSceneMain.DamageManager.CanvasTransform);
        PlayerHUD playerHUD = goInstrance.GetComponent<PlayerHUD>();
        playerHUD.Initialize(this);
    }

    protected override void UpdateActor()
    {
        UpdateInput();
        UpdateMove();
    }

    [ClientCallback] //내 클라이언트에서만 실행된다
    protected override void UpdateInput()
    {
        inputController.UpdateInput();
    }

    void UpdateMove()
    {
        /*
         * Actor의 Update 메소드에 의해서 UpdateActor가 실행되는데, Update는 MonoBehaviour 소속이라
         * Network Input 처리에서 오작동이 존재한다. 이를 위한 해결방법
         */
        //MoveVector 벡터의 크기 제곱 == 0? 변환가 없다면 return;
        if (MoveVector.sqrMagnitude == 0)
            return;

        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdMove(MoveVector);

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때의 꼼수
        // 이경우 클라이언트로 접속하면 Command로 보내지지만 자기자신은 CmdMove를 실행 못함
        if (isServer)
        {
            // Host 플레이어인경우 RPC로 보내고
            RpcMove(MoveVector); // 이동고 보정을 한번에 하는 메소드
        }
        else
        {
            // Client 플레이어인경우 Cmd로 호스트로 보낸후 자신을 Self 동작
            CmdMove(MoveVector); // 이동만 하는 메소드
            if (isLocalPlayer)
                transform.position += AdjustMoveVector(MoveVector); // 움직이는 정도인 MoveVector 값이 배경을 넘지 않도록 보정한다.
        }

    }

   

    [Command] //클라이언트가 서버에게 보내는 동작
    public void CmdMove(Vector3 moveVector)
    {
        // 보정된 MoveVector를 현재 객체의 포지션에 계속 더해준다.
        this.MoveVector = moveVector;
        transform.position += moveVector;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcMove(Vector3 moveVector)
    {
        this.MoveVector = moveVector;
        transform.position += AdjustMoveVector(this.MoveVector);
        base.SetDirtyBit(1);
        this.MoveVector = Vector3.zero; // 타 플레이어가 보낸경우 Update를 통해 초기화 되지 않으므로 사용후 바로 초기화
    }

    public void ProcessInput(Vector3 moveDirection)
    {
        //초당(Time.deltaTime) moveDirection 방향으로(InputController 스크립트에서 매개변수 전달) MoveSpeed의 속도로
        //매프레임마다 InputController 스크립트에 의해서 MoveVector가 수정된다.
        //MoveVector는 매 프레임 마다 움직이는 정도를 나타내는 변수
        MoveVector = moveDirection * MoveSpeed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 MoveVector)
    {
        Transform MainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;
        Vector3 result = Vector3.zero;

        //Debug.Log("MainBGQuadTransform.localScale.x: " + MainBGQuadTransform.localScale.x);
        //Debug.Log("MainBGQuadTransform.localScale.y: " + MainBGQuadTransform.localScale.y);

        //현재 박스 콜라이더의 포지션에 MoveVector를 더한다.
        result = boxCollider.transform.position + MoveVector;

        // why +-boxCollider.size.x * 0.5f?
        // 현재 result가 박스 콜라이더의 센터값 기준으로 설정되기 때문에 그 크기 반만큼 가감해줌.(그래야 박스의 모서리)

        if (result.x - boxCollider.size.x * 0.5f < -MainBGQuadTransform.localScale.x *0.5f
            || result.x + boxCollider.size.x * 0.5f > MainBGQuadTransform.localScale.x * 0.5f)
        {
            MoveVector.x = 0;
        }
        else if (result.y - boxCollider.size.y * 0.5f < -MainBGQuadTransform.localScale.y * 0.5f
            || result.y + boxCollider.size.y * 0.5f > MainBGQuadTransform.localScale.y * 0.5f)
        {
            MoveVector.y = 0;
        }

        return MoveVector;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player-OnTriggerEnter: " + other.name);

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy && !enemy.IsDead) // enemy에게 부딪쳤다를 통보
        {
            //비행기끼리의 충돌은 crashPos를 다시 계산해야한다.
            BoxCollider boxCollider = other as BoxCollider;
            Vector3 crashPos = enemy.transform.position + boxCollider.center;
            crashPos.x += boxCollider.size.x * 0.5f;

            enemy.OnCrash(crashDamage, crashPos);
        }
            
    }

    public void Fire()
    {
        if(Host)
        {
            /*
        //Bullet 자체가 GameObject 이므로 형변환 불필요
        GameObject go = Instantiate(Bullet);
        //위에서 프래팹을 통해 생성한 go 오브젝트에서 Bullet스크립트를 가져온다.
        Bullet bullet = go.GetComponent<Bullet>();
        */

            // -> 위 코드가 아래로 한줄로 대체됨
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.GenerateBullet((int)BulletManager.BulletIndex.Player);
            bullet.Fire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        else
        {
            //호스트가 아니라면 Command를 통해서 발사
            CmdFire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        


        
    }

    [Command] //클라에서 서버로
    public void CmdFire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.GenerateBullet((int)BulletManager.BulletIndex.Player);
        bullet.Fire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        SetDirtyBit(1);
    }

    protected override void DecreaseHP(int damage, Vector3 damagePos)
    {
        base.DecreaseHP(damage, damagePos);

        //damagePoint: 데이미지 발생한 지점 (충돌이 발생한 지점)을 중심으로 적당히 연산
        //Random.insideUnitSphere -> 3차원 공간에서 반지름이 1인 구체안의 임의의 값을 리턴(2차원이라면 insideUnitCircle)
        Vector3 DamagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.GenerateDamage((int)DamageManager.DamageIndex.Player, DamagePoint, damage, Color.red);
    }


    protected override void OnDead()
    {
        base.OnDead();
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetHost()
    {
        Host = true;
        base.SetDirtyBit(1);
    }


}
