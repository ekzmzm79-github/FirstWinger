using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNameConstants
{
    public static string TitleScene = "Title";
    public static string LoadingScene = "LoadingScene";
    public static string Ingame = "Ingame";

}


public class SceneController : MonoBehaviour
{
    private static SceneController instance = null;
    public static SceneController Instance
    {
        get
        {
            if(instance == null)
            {
                //최초 사용시 클래명과 같은 이름의 게임오브젝트 생성해서 어태치
                GameObject go = GameObject.Find("SceneController");
                if (!go)//못찾음 -> 생성
                {
                    go = new GameObject("SceneController");

                    SceneController sceneController = go.AddComponent<SceneController>();
                    return sceneController;
                }
                else // 찾았다면 go의 SceneController를 연결
                    instance = go.GetComponent<SceneController>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Can't have two instance of singletone");
            DestroyImmediate(this);
            return;
        }

        instance = this; // 실질적으로 모두 여기서 연결된다.
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        //SceneManager의 각상황에 따라 실행되는 이벤트에서 
        //각각의 씬들을 확인하기 위해서 메소드들을 매핑시킴(매개변수 맞춰서)
        SceneManager.activeSceneChanged += OnActiveSceneChanged; // 씬의 교체
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬의 로드
        SceneManager.sceneUnloaded += OnSceneUnloaded; // 씬의 언로드

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //LoadSceneMode: 씬을 불러올때, Single(이전 씬을 파괴하고 새로운 씬 로드) 
    //              혹은 Additive(현재 씬에 지정한 씬을 추가해서 로드) 중 선택

    /// <summary>
    /// 이전 씬을 언로드하고 새 씬을 로딩(Single)
    /// </summary>
    /// <param name="sceneName"> 로딩 씬의 이름</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsys(sceneName, LoadSceneMode.Single));
    }
    /// <summary>
    /// 이전 씬을 그대로 두고 새로운 씬을 추가해서 로딩(Additive)
    /// </summary>
    /// <param name="sceneName"> 로딩 씬의 이름</param>
    public void LoadSceneAdditive(string sceneName)
    {
        StartCoroutine(LoadSceneAsys(sceneName, LoadSceneMode.Additive));
    }

    IEnumerator LoadSceneAsys(string sceneName, LoadSceneMode loadSceneMode)
    {
        //비동기 형식이라 아래 코드에서 걸려있든 아니든 isDone이 변경된다
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        while (!asyncOperation.isDone)
            yield return null;

        Debug.Log("LoadSceneAsys is complete");
    }

    public void OnActiveSceneChanged(Scene scene0, Scene scene1)
    {
        Debug.Log("OnActiveSceneChanged is called! scene0= " + scene0.name + ", scene1= " + scene1.name);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("OnSceneLoaded is called scene = " + scene.name  + ", loadSceneMode = " + loadSceneMode.ToString());

        BaseSceneMain baseSceneMain = GameObject.FindObjectOfType<BaseSceneMain>();
        Debug.Log("OnSceneLoaded! baseSceneMain.name = " + baseSceneMain.name);
        SystemManager.Instance.CurrentSceneMain = baseSceneMain;
    }

    public void OnSceneUnloaded(Scene scene)
    {
        Debug.Log("OnSceneUnloaded is called! scene = " + scene.name);
    }

}
