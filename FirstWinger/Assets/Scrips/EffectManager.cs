using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public enum EffectIndexName : int
    {
        hit = 0,
        dead = 1
    }

    [SerializeField]
    PrefabCacheData[] effectFiles;

    Dictionary<string, GameObject> FileCache = new Dictionary<string, GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        Prepare();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateEffect(EffectIndexName effect, Vector3 position)
    {
        int index = (int)effect;

        if (index < 0 || (int)index >= effectFiles.Length)
        {
            Debug.LogError("GenerateEffect : out of range! index = " + index);
            return null;
        }

        string filePath = effectFiles[index].filePath;

        //해당 filePath로 EffectCacheSystem에서 캐시를 가져옴
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Archive(filePath);
        if (!go) //생성 실패
        {
            Debug.LogError("GenerateEffect error!");
            return null;
        }

        go.transform.position = position;
        AutoCachableEffect autoCachableEffect = go.GetComponent<AutoCachableEffect>();
        autoCachableEffect.FilePath = filePath;
        return go;
    }

    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        if (FileCache.ContainsKey(resourcePath)) //캐시 확인
        {
            go = FileCache[resourcePath];
        }
        else
        {
            //없다면 새로 로드
            go = Resources.Load<GameObject>(resourcePath);
            if (!go)
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
        for (int i = 0; i < effectFiles.Length; i++)
        {
            //EnemyManager 클래스의 Prepare 메소드와 같음
            GameObject go = Load(effectFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.GenerateCache(effectFiles[i].filePath, go, effectFiles[i].cacheCount, this.transform);
        }
    }

    public bool RemoveEffect(AutoCachableEffect effect)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Restore(effect.FilePath, effect.gameObject);
        return true;
    }



}
