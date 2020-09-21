using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public enum BulletIndex : int
    {
        Player = 0,
        Enemy = 1
    }

    [SerializeField]
    PrefabCacheData[] bulletFiles;

    //에너미 팩토리 + 에너미 매니저의 형태
    Dictionary<string, GameObject> FileCache = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Prepare();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        if(FileCache.ContainsKey(resourcePath)) //캐시 확인
        {
            go = FileCache[resourcePath];
        }
        else
        {
            //없다면 새로 로드
            go = Resources.Load<GameObject>(resourcePath);
            if(!go)
            {
                Debug.LogError("Load error! path = " + resourcePath);
                return null;
            }

            //로드 성공 이후에 적재
            FileCache.Add(resourcePath, go);
        }

        return go;
    }

    public void Prepare()
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return;

        for (int i = 0; i<bulletFiles.Length; i++)
        {
            //EnemyManager 클래스의 Prepare 메소드와 같음
            GameObject go = Load(bulletFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.GenerateCache(bulletFiles[i].filePath, go, bulletFiles[i].cacheCount, this.transform);
        }
    }

    public Bullet GenerateBullet(int index)
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return null;

        string filePath = bulletFiles[index].filePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Archive(filePath);
        if (!go) //생성 실패
        {
            Debug.LogError("GenerateBullet error!");
            return null;
        }

        Bullet bullet = go.GetComponent<Bullet>();
        // bullet.FilePath = filePath; -> PrefabCacheSystem에서 지정함

        return bullet;

    }

    public bool RemoveBullet(Bullet bullet)
    {
        if (!((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
            return true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Restore(bullet.FilePath, bullet.gameObject);
        return true;
    }

}
