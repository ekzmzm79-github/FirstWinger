using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Actor : NetworkBehaviour
{
    //Player와 Enemy 스크립트는 모두 Start, Update를 정의하지 않고 여기서 상속받음

    [SerializeField]
    [SyncVar]
    protected int MaxHP = 100;
    public int maxHP { get { return MaxHP; } }
    [SerializeField]
    [SyncVar]
    protected int CurrentHP;
    public int currentHP { get { return CurrentHP; } }

    [SerializeField]
    [SyncVar]
    protected int Damage = 1;

    [SerializeField]
    [SyncVar]
    protected int crashDamage = 100;

    // isDead는 private이고 get밖에 정의를 안했기 때문에
    // OnDead 메소드를 통해서만 수정 가능
    [SerializeField]
    [SyncVar]
    protected bool isDead = false; 
    public bool IsDead { get { return isDead; } }

    [SyncVar]
    protected int actorInstanceID = 0;
    public int ActorInstanceID { get { return actorInstanceID; } }
    // Start is called before the first frame update
    void Start()
    {
        Intialize();
    }

    protected virtual void Intialize()
    {
        CurrentHP = MaxHP;

        if(isServer)
        {
            //서버일 경우에만 actorInstanceID에 값 설정 -> 여기서 Actor 인스턴스ID 최초 설정됨
            actorInstanceID = GetInstanceID();
            RpcSetActorInstanceID(actorInstanceID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateActor();
    }

    protected virtual void UpdateActor()
    {

    }

    protected virtual void UpdateInput()
    {

    }

    //비행기가 총알에 충돌했을 때를 처리하는 메소드
    public virtual void OnBulletHited(int damage, Vector3 hitPos)
    {
        DecreaseHP(damage, hitPos);
    }

    //비행기끼리의 충돌을 처리하는 메소드
    public virtual void OnCrash(int damage, Vector3 crashPos)
    {
        DecreaseHP(damage, crashPos);
    }
    
    protected virtual void DecreaseHP(int damage, Vector3 damagePos)
    {
        if(isDead) return;

        if(isServer)
        {
            RpcDecreaseHP(damage, damagePos);
        }
        else
        {
            CmdDecreaseHP(damage, damagePos);
            if (isLocalPlayer)
                InternalDecreaseHP(damage, damagePos);
        }

    }
    
    protected virtual void InternalDecreaseHP(int damage, Vector3 damagePos)
    {
        if (isDead)
            return;

        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            OnDead();
        }
    }

    [Command]
    public void CmdDecreaseHP(int damage, Vector3 damagePos)
    {
        InternalDecreaseHP(damage, damagePos);
        base.SetDirtyBit(1);
    }
    [ClientRpc]
    public void RpcDecreaseHP(int damage, Vector3 damagePos)
    {
        InternalDecreaseHP(damage, damagePos);
        base.SetDirtyBit(1);
    }

    //isDead 변수를 true로 바꾸는 위한 유일한 메소드
    protected virtual void OnDead()
    {
        isDead = true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.EffectIndexName.dead, transform.position);
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

    [ClientRpc] //서버 사이드에서만 호출가능
    public void RpcSetActive(bool value)
    {
        this.gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetActorInstanceID(int instID)
    {
        this.actorInstanceID = instID;
        if (this.actorInstanceID != 0)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.Regist(this.actorInstanceID, this);

        base.SetDirtyBit(1);
    }









    /*
    public void UpdateNetworkActor()
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdUpdateNetworkActor();

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때의 꼼수
        if (isServer)
        {
            // Host 플레이어인경우 RPC로 보내고
            RpcUpdateNetworkActor();
        }
        else
        {
            // Client 플레이어인경우 Cmd로 호스트로 보낸후 자신을 Self 동작
            CmdUpdateNetworkActor();

        }

    }

    [Command]
    public void CmdUpdateNetworkActor()
    {
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcUpdateNetworkActor()
    {
        base.SetDirtyBit(1);
    }

    */

}
