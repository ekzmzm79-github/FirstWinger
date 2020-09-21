using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameSceneMain : BaseSceneMain
{
    public GameState CurrentGameState { get { return NetworkTransfer.CurrentGameState; } }

    //Player 스크립트 접근 변수
    [SerializeField]
    Player player;
    //player 변수 프로퍼티
    public Player Hero
    {
        get
        {
            if (!player) Debug.LogWarning("Main Player is not setted!");
            return player;
        }
        set
        {
            player = value;
        }
    }

    GamePointAccumulator gamePointAccumulator = new GamePointAccumulator();
    public GamePointAccumulator GamePointAccumulator { get { return gamePointAccumulator; } }

    [SerializeField]
    EffectManager effectManager;
    public EffectManager EffectManager { get { return effectManager; } }

    [SerializeField]
    EnemyManager enemyManager;
    public EnemyManager EnemyManager { get { return enemyManager; } }

    [SerializeField]
    BulletManager bulletManager;
    public BulletManager BulletManager { get { return bulletManager; } }

    [SerializeField]
    DamageManager damageManager;
    public DamageManager DamageManager { get { return damageManager; } }


    ActorManager actorManager = new ActorManager();
    public ActorManager ActorManager { get { return actorManager; } }


    PrefabCacheSystem enemyCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem EnemyCacheSystem { get { return enemyCacheSystem; } }

    PrefabCacheSystem bulletCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem BulletCacheSystem { get { return bulletCacheSystem; } }

    PrefabCacheSystem effectCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem EffectCacheSystem { get { return effectCacheSystem; } }

    PrefabCacheSystem damageCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem DamageCacheSystem { get { return damageCacheSystem; } }


    [SerializeField]
    SquadronManager squadronManager;
    public SquadronManager SquadronManager { get { return squadronManager; } }


    [SerializeField]
    Transform mainBGQuadTransform;
    public Transform MainBGQuadTransform { get { return mainBGQuadTransform; } }


    [SerializeField]
    InGameNetworkTransfer inGameNetworkTransfer;
    InGameNetworkTransfer NetworkTransfer { get { return inGameNetworkTransfer; } }

    [SerializeField]
    Transform playerStartTransform1; // 초기 플레이어 자리 지정을 위한 변수
    public Transform PlayerStartTransform1 { get { return playerStartTransform1; } }

    [SerializeField]
    Transform playerStartTransform2; // 초기 플레이어 자리 지정을 위한 변수
    public Transform PlayerStartTransform2 { get { return playerStartTransform2; } }


    public void GameStart()
    {
        NetworkTransfer.RpcGameStart();
    }


}
