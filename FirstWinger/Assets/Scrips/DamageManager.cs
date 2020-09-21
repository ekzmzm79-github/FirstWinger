using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageManager : MonoBehaviour
{
    public enum DamageIndex : int
    {
        Enemy = 0,
        Player = 0,

    }

    /* 위의 enum으로 대체했음
    public const int EnemyDamageIndex = 0;
    public const int PlayerDamageIndex = 0;
    */

    [SerializeField]
    Transform canvasTransform;
    public Transform CanvasTransform { get { return canvasTransform; } }


    [SerializeField]
    PrefabCacheData[] DamageFiles;

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
        for (int i = 0; i < DamageFiles.Length; i++)
        {
            //EnemyManager 클래스의 Prepare 메소드와 같음
            GameObject go = Load(DamageFiles[i].filePath);
            //uidamage의 경우 생성과 동시에 부모(canvas)가 정해져야한다.
            //그렇기 때문에 GenerateCache의 마지막 인자를 사용하여서 부모를 설정해줌
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.GenerateCache(DamageFiles[i].filePath, go, DamageFiles[i].cacheCount, canvasTransform);
        }
    }


    public GameObject GenerateDamage(int index, Vector3 position, int damageValue, Color textColor)
    {
        if (index < 0 || (int)index >= DamageFiles.Length)
        {
            Debug.LogError("GenerateDamage : out of range! index = " + index);
            return null;
        }

        string filePath = DamageFiles[index].filePath;

        //해당 filePath로 DamageCacheSystem에서 캐시를 가져옴
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Archive(filePath);
        if (!go) //생성 실패
        {
            Debug.LogError("GenerateDamage error!");
            return null;
        }

        //go.transform.position = position;
        //위 코드를 아래로 대체 -> 3차원 공간상을 기준으로 전달된 position인자 이기때문에 2차원상으로 변환
        go.transform.position = Camera.main.WorldToScreenPoint(position);

        UIDamage uIDamage = go.GetComponent<UIDamage>();
        uIDamage.FilePath = filePath;
        uIDamage.ShowDamage(damageValue, textColor);

        return go;
    }

    public bool Remove(UIDamage uIDamage)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Restore(uIDamage.FilePath, uIDamage.gameObject);
        return true;
    }

}
