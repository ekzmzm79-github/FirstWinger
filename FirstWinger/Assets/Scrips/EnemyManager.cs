using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    EnemyFactory enemyFactory;

    List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> Enemies { get { return enemies; } }

    [SerializeField]
    PrefabCacheData[] enemyFiles;

    // Start is called before the first frame update
    void Start()
    {
        //Prepare();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public bool GenerateEnemy(SquadronMemberStruct data)
    {
        // 서버(호스트)가 아니라면 프리팹을 Generate하지 않고 모두 전송받아야한다.
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return true;

        /*
        enemyFactory 스크립트에서 Load메소르를 통하여 적 오브젝트를 go에 받음
        GameObject go = enemyFactory.Load(EnemyFactory.EnemyPath);
        */

        //-> 윗 코드가 아래로 대체됨

        // 해당 경로로 캐쉬 꺼내기
        string filePath = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID).FilePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Archive(filePath); 
   
        if(!go) //생성 실패
        {
            Debug.LogError("GenerateEnemy error!");
            return false;
        }

        //매개변수로 받은 position으로 go 위치 초기화
        go.transform.position = new Vector3(data.GeneratePointX, data.GeneratePointY, 0);

        //go 게임 오브젝트는 Enemy 프리팹이므로 Enemy 스크립트를 불러와서 객체로 저장
        Enemy enemy = go.GetComponent<Enemy>();
        //enemy.FilePath = filePath; // enemy 내부의 파일 경로 지정 -> PrefabCacheSystem스크립트로 옮김

        enemy.SetPosition(new Vector3(data.GeneratePointX, data.GeneratePointY, 0));
        enemy.Reset(data);
        enemies.Add(enemy);
        return true;
    }

    
    public bool RemoveEnemy(Enemy enemy)
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return true;

        if (!enemies.Contains(enemy))
        {
            Debug.LogError("No exist Enemy");
            return false;
        }

        enemies.Remove(enemy);
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Restore(enemy.FilePath, enemy.gameObject);

        return true;
    }
    

    public void Prepare()
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return;

        for (int i = 0; i < enemyFiles.Length; i++)
        {
            //enemyFactory에서 enemyFiles각 인덱스의 파일경로로 프리팹을 로드
            GameObject go = enemyFactory.Load(enemyFiles[i].filePath);

            //로드된 go를 경로, 카운트와 함께 EnemyCacheSystem에 캐쉬로 생성(Instantiate 이후 딕셔너리에 추가)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.GenerateCache(enemyFiles[i].filePath, go, enemyFiles[i].cacheCount, this.transform);
        }
    }

}
