using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneMain : BaseSceneMain
{
    const float NextSceneInterval = 3.0f;
    const float TextUpdateInterval = 0.15f;
    const string LoadingTextValue = "Loading...";

    [SerializeField]
    Text LoadingText;

    int TextIndex = 0;
    float LastUpdateTime;

    float SceneStartTime;
    bool NextSceneCall = false;

    protected override void OnStart()
    {
        SceneStartTime = Time.time;
    }

    protected override void UpdateScene()
    {
        base.UpdateScene();

        float curTime = Time.time;
        //TextUpdateInterval이 될때마다
        if (curTime - LastUpdateTime > TextUpdateInterval)
        {
            //로딩 텍스트를 미리 지정해둔 상수 스트링의 0~ (TextIndex +1)까지 잘라서 표시
            LoadingText.text = LoadingTextValue.Substring(0, TextIndex + 1);

            TextIndex++;
            if (TextIndex >= LoadingTextValue.Length)
                TextIndex = 0;

            //curTime의 업데이트
            LastUpdateTime = curTime;
        }

        //현재는 임시적으로 그저 일정 시간(NextSceneInterval)이 지나면 인게임 씬으로 넘어감
        if (curTime - SceneStartTime > NextSceneInterval)
        {
            //GotoNextScene 메소드가 여러번 실행되는 것을 방지하기 위한 플래그 변수
            if (!NextSceneCall)
                GotoNextScene();
        }

    }
    
    void GotoNextScene()
    {
        NetworkConnectionInfo info = SystemManager.Instance.ConnectionInfo;
        if(info.Host) // 현재 연결이 호스트라면
        {
            Debug.Log("FW Start with Host!");
            //singleton은 NetwortkManager 형태이기 때문에 FWNetwortkManager 메소드를 원한다면 캐스팅
            FWNetwortkManager.singleton.StartHost();
        }
        else // 현재 연결이 클라이언트라면
        {
            Debug.Log("FW Start with Client!");

            if (!string.IsNullOrEmpty(info.IPAddress)) //Title 씬에서 이미 설정되어있다
                FWNetwortkManager.singleton.networkAddress = info.IPAddress;

            if (info.Port != FWNetwortkManager.singleton.networkPort) //SystemManager와 FWNetwortkManager 포트가 다르다면
                FWNetwortkManager.singleton.networkPort = info.Port;

            FWNetwortkManager.singleton.StartClient();
        }

        
        NextSceneCall = true;
    }

}
