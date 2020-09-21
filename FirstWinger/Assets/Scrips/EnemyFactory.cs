using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public const string EnemyPath = "Prefabs/Enemy";

    //<경로, 프리팹> Dictionary
    Dictionary<string, GameObject> EnemyFileCache = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        //EnemyFileCache Dictionary가 resourcePath를 Key값으로 가지고 있는가?
        if (EnemyFileCache.ContainsKey(resourcePath)) // 이미 있다.
        {
            //해당 키값으로 GameObject 가져옴
            go = EnemyFileCache[resourcePath];
        }
        else // 없다
        {
            //resourcePath에 해당하는 프리팹 로드
            go = Resources.Load<GameObject>(resourcePath);
            if(!go) // 리소스 로드의 실패
            {
                Debug.LogError("Load error! path = " + resourcePath);
                return null;
            }

            EnemyFileCache.Add(resourcePath, go);
        }


        //한번 새로 받아서 리턴 -> 안전함
        //GameObject InstanceGO = Instantiate<GameObject>(go);


        return go;
    }

}
