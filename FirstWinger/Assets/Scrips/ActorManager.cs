using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Bullet과 Actor의 충돌에서 Bullet의 Owner가 누구인지 아는것은 서버뿐이다.(생성자만 알기때문에)
/// 때문에 ActorManager를 따로 두어서 생성 이후에 충돌 등의 처리를 위해 Owner를 따로 알려줄 필요가 있다.
/// 즉, 서버와 클라이언트는 서로 다른 참조값을 가지지만 같은 id값(인스턴스?)을 가진 Owner를 동기화시켜서 인지한다.
/// </summary>
public class ActorManager
{
    //<인스턴스 id, 액터>로 이루어진 딕셔너리
    Dictionary<int, Actor> Actors = new Dictionary<int, Actor>();

    public bool Regist(int ActorInstanceID, Actor actor)
    {
        if(ActorInstanceID ==0) // ActorInstanceID가 0이다 -> 에러
        {
            Debug.LogError("Regist error! ActorInstanceID = 0");
            return false;
        }

        if(Actors.ContainsKey(ActorInstanceID)) // ActorInstanceID가 이미 딕셔너리에 존재한다
        {
            if(actor.GetInstanceID() != Actors[ActorInstanceID].GetInstanceID())
            {
                //해당 키는 이미 있지만 둘의 인스턴스 값은 다르다
                Debug.LogError("Regist error! already exist! ActorInstanceID = " + ActorInstanceID);
                return false;
            }
            //키와 인스턴스 값까지 모두 같다
            Debug.Log(ActorInstanceID + "is already registed!");
            return true;

        }

        //등록 성공
        Actors.Add(ActorInstanceID, actor);
        return true;

    }

    public Actor GetActor(int ActorInstanceID)
    {
        if(!Actors.ContainsKey(ActorInstanceID))
        {
            Debug.LogError("GetActor error! no exist! ActorInstanceID = " + ActorInstanceID);
            return null;
        }

        return Actors[ActorInstanceID];

    }

}
