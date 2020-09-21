using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PrefabCacheData //각각의 프리팹 캐시의 경로와 개수
{
    public string filePath;
    public int cacheCount;
}

//
//
//
//

/*
 * 게임에서 사용할 Instantiate를 미리 모두 해두고 딕셔너리에 저장해두는 것이 목적
 * 한 순간에 사용이 완료된 Prefab들을 다시 캐시 딕셔너리에 반환하여서 필요할때마다 다시 꺼내서 사용
 * -> 즉, 파괴된다고 하더라도 이전에 Instantiate 해둔 오브젝트를 다시 재사용함
 * (한 순간에 사용될 최대숫자만 고려하면됨)
 * 
 * 필요 이상의 부하가 생기는 상황
 * 1. 파일로딩: (게임이 실제로 실행되기 이전에 미리 로딩 -> 캐싱)
 * 2. Instantiate를 호출: (게임이 실제로 실행되기 이전에 미리 로딩 -> 캐싱)
 * 3. Active 될 때: Active를 끄지않고 켠 상태로 실제 카메라에 보이지 않는 장소로 이동시켜서 대기
 *                  (단, 이 경우 update를 정지시키거나 다른 작업을 멈추도록 처리해야함)
 * 
 * 
 * 추가 : 모든 캐시 데이터는 서버에서만 최초로 생성하고 클라이언트는 단순히 그걸 받아서 사용한다.
*/
public class PrefabCacheSystem
{
    //<파일 경로, 게임오브젝트 큐> 형태의 딕셔너리(각 파일경로마다 게임오브젝트를 큐로 모아서 저장)
    Dictionary<string, Queue<GameObject>> Caches = new Dictionary<string, Queue<GameObject>>();

    //가장 오른쪽 인자를 추가하고 기본 매개변수값을 설정하면 기존에 사용하던 다른 코드를 수정하지 않아도 됨
    public void GenerateCache(string filePath, GameObject gameObject, int cacheCount, Transform parentTransform = null)
    {
        if(Caches.ContainsKey(filePath)) // 해당 키(파일 경로)가 이미 존재한다
        {
            Debug.LogWarning("Already cache generated! filePath = " + filePath);
            return;
        }
        else
        {
            //해당 filePath(key)에 맞는 큐를 생성하고 cacheCount만큼 인자로 전달된 gameObject를 false 상태로 큐에 저장
            Queue<GameObject> queue = new Queue<GameObject>();
            for(int i=0; i<cacheCount; i++)
            {
                GameObject go = Object.Instantiate<GameObject>(gameObject, parentTransform);
                go.SetActive(false);
                queue.Enqueue(go);

                //해당 go 오브젝트가 enemy인지 판단하고 파일 경로 설정 및 스폰(Spawn)
                Enemy enemy = go.GetComponent<Enemy>(); 
                if (enemy != null)
                {
                    enemy.FilePath = filePath;
                    NetworkServer.Spawn(go);
                    //enemy.RpcSetActive(false);
                }

                //해당 go 오브젝트가 bullet인지 판단하고 파일 경로 설정 및 스폰(Spawn)
                Bullet bullet = go.GetComponent<Bullet>(); 
                if (bullet != null)
                {
                    bullet.FilePath = filePath;
                    NetworkServer.Spawn(go);
                    //enemy.RpcSetActive(false);
                }

                /*
                //해당 go 오브젝트가 effect인지 판단하고 파일 경로 설정 및 스폰(Spawn)
                AutoCachableEffect effect = go.GetComponent<AutoCachableEffect>();
                if (effect != null)
                {
                    effect.FilePath = filePath;
                    NetworkServer.Spawn(go);
                    //enemy.RpcSetActive(false);
                }
                */
            }

            //새로 생성한 큐를 filePath와 함께 딕셔너리에 추가
            Caches.Add(filePath, queue);

        }

    }

    //해당 filePath의 프리팹 캐쉬 사용 요청 (캐시에서 꺼내기)
    public GameObject Archive(string filePath)
    {
        if(!Caches.ContainsKey(filePath))
        {
            Debug.LogError("Archive Erorr! no Cache generated! filePath =" + filePath);
            return null;
        }

        if(Caches[filePath].Count == 0)
        {
            Debug.LogError("Archive problem! This chach have 0 count filePath =" + filePath);
            return null;
        }

        GameObject go = Caches[filePath].Dequeue();
        go.SetActive(true);

        if(((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
        {
            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy != null)
                enemy.RpcSetActive(true);

            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet != null)
                bullet.RpcSetActive(true);
        }

        

        /*
        AutoCachableEffect effect = go.GetComponent<AutoCachableEffect>();
        if (effect != null)
            effect.RpcSetActive(true);
        */

        return go;

    }

    //해당 filePath로 gameObject를 다시 캐시에 반환
    public bool Restore(string filePath, GameObject gameObject)
    {

        if(!Caches.ContainsKey(filePath)) // 반환하려는 파일 경로가 없다
        {

            Debug.LogError("Restore Error! no Cache generated! filePath = " + filePath);
            return false;
        }

        gameObject.SetActive(false);

        if (((FWNetwortkManager)FWNetwortkManager.singleton).isServer)
        {
            Enemy enemy = gameObject.GetComponent<Enemy>();
            if (enemy != null)
                enemy.RpcSetActive(false);

            Bullet bullet = gameObject.GetComponent<Bullet>();
            if (bullet != null)
                bullet.RpcSetActive(false);
        }
        

        /*
        AutoCachableEffect effect = gameObject.GetComponent<AutoCachableEffect>();
        if (effect != null)
            effect.RpcSetActive(false);
        */


        Caches[filePath].Enqueue(gameObject);
        return true;

    }

    public void Add(string filePath, GameObject gameObject)
    {
        Queue<GameObject> queue;
        if (Caches.ContainsKey(filePath))
            queue = Caches[filePath];
        else
        {
            queue = new Queue<GameObject>();
            Caches.Add(filePath, queue);
        }

        queue.Enqueue(gameObject);
    }

}
