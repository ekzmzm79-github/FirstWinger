using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum GameState : int
{
    None=0,
    Ready,
    Running,
    End,
}
[System.Serializable]
public class InGameNetworkTransfer : NetworkBehaviour
{
    /*
       [Command] : 클라이언트에서 호출되어서 서버에서 수행되는 Command(클라이언트 -> 서버)
       [ClientRpc] : 서버에서 호출되어서 클라이언트에서 수행되는 ClientRpc(서버 -> 클라이언트)
    */
    const float GameReadyInterval = 3.0f;

    [SyncVar]
    GameState currentGameState = GameState.None;
    public GameState CurrentGameState { get { return currentGameState; } }

    [SyncVar]
    float CountingStartTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        //GameReadyInterval 시간이 자나면 자동으로 SquadronManager의 GameStart메소드 실행

        float currentTime = Time.time;

        if (currentGameState == GameState.Ready)
        {
            if (currentTime - CountingStartTime > GameReadyInterval)
            {
                SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().SquadronManager.StartGame();
                currentGameState = GameState.Running;
            }
        }

        
    }

    [ClientRpc]
    public void RpcGameStart()
    {
        //서버에서 게임 시작하고 클라이언트에도 자동으로 전달됨.
        //게임 시작과 동시에 적, 총알, 이펙트 사전 준비(캐시 생성)

        Debug.Log("RpcGameStart");
        CountingStartTime = Time.time;
        currentGameState = GameState.Ready;

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.EnemyManager.Prepare();
        inGameSceneMain.BulletManager.Prepare();
    }
}
