using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    /*
    싱글톤으로 사용되는 게임매니저 스크립트
    title 씬에서부터 시작되며 EnemyTalble와 함께 배치시켜 게임 내에서 사용되는 리로스를 미리 로드시킨다.
    */

    //자기 참조형 변수
    static SystemManager instance = null;
    //instance 프로퍼티 -> set은 필요가 없다.(최초에 자동으로 초기화 되므로)
    public static SystemManager Instance
    {
        get
        {
            return instance;
        }
    }

    //enemyTable은 인게임 중에 사용되는 개념이 아니라 게임 시작전에 사용하는 개념
    [SerializeField]
    EnemyTable enemyTable;
    public EnemyTable EnemyTable { get { return enemyTable;  } }

    BaseSceneMain currentSceneMain;
    public BaseSceneMain CurrentSceneMain { set { currentSceneMain = value; } }

    [SerializeField]
    NetworkConnectionInfo connectionInfo = new NetworkConnectionInfo();
    public NetworkConnectionInfo ConnectionInfo { get { return connectionInfo; } }

    private void Awake()
    {
        if(instance != null)
        {
            //static으로 선언된 instance가 초기값인 null이 아니다?
            //현재 게임 오브젝트는 두번째 이상의 SystemManager이므로 파괴
            Debug.Log("SystemManager: Singletone error!");
            Destroy(gameObject);
            return;
        }

        //instance가 초기값인 null이므로 현재 객체로 매칭
        instance = this;

       
        DontDestroyOnLoad(gameObject);

    }

    

    // Start is called before the first frame update
    void Start()
    {
        //currentSceneMain의 최초 세팅

        BaseSceneMain baseSceneMain = GameObject.FindObjectOfType<BaseSceneMain>();
        Debug.Log("OnSceneLoaded! baseSceneMain.name = " + baseSceneMain.name);
        SystemManager.Instance.CurrentSceneMain = baseSceneMain;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //where T: BaseSceneMain -> 해당 메소드에 전달되는 형식 인수는
    //                          반드시 BaseSceneMain이거나 이것을 상속하는 클래스로 한정
    public T GetCurrentSceneMain<T>() where T: BaseSceneMain
    {
        return currentSceneMain as T;
    }
}
