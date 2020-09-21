using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enemy, bullet 과 비슷하며 Effect에 해당함

//반드시 ParticleSystem이라는 컴포넌트를 요구하는 스크립트
[RequireComponent(typeof(ParticleSystem))] 
public class AutoCachableEffect : MonoBehaviour
{
    public string FilePath { get; set; }
    void OnEnable()
    {
        StartCoroutine("CheckIfAlive");
    }

    IEnumerator CheckIfAlive()
    {
        while (true)
        {
            //0.5초후에 이 코드 밑 실행 (null이라면 프레임 단위로 실행)
            //0.5초 마다 현재 객체의 ParticleSystem 컴포넌트가 살아있는지 체크하고
            //만약 죽은 상태라면 이 객체 이펙트를 제거하고 반복문 탈출
            yield return new WaitForSeconds(0.5f);
            if (!GetComponent<ParticleSystem>().IsAlive(true))
            {
                SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.RemoveEffect(this);
                break;
            }

        }
    }

}
