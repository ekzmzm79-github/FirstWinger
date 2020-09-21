using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetWorkConfigPanel : BasePanel
{
    const string DefaultIPAddress = "localhost";
    const string DefaultPort = "7777";

    [SerializeField]
    InputField IPAddressInputField;

    [SerializeField]
    InputField PortInputField;

    public override void InitializePanel()
    {
        base.InitializePanel();
        // IP와 Port 입력을 기본 값으로 세팅한다.
        IPAddressInputField.text = DefaultIPAddress;
        PortInputField.text = DefaultPort;
        Close(); //최초에는 닫아둔다
    }

    //호스트로 연결 버튼을 클릭
    public void OnHostButton()
    {
        //호스트라면 기본 ip, host를 사용한다.
        SystemManager.Instance.ConnectionInfo.Host = true;
        TitleSceneMain sceneMain = SystemManager.Instance.GetCurrentSceneMain<TitleSceneMain>();
        sceneMain.GotoNextScene();
    }
    
    public void OnClientButton()
    {
        //클라이언트라면 입력된 ip, host를 사용한다.
        SystemManager.Instance.ConnectionInfo.Host = false;
        TitleSceneMain sceneMain = SystemManager.Instance.GetCurrentSceneMain<TitleSceneMain>();

        //IPAddressInputField의 입력값이 널이거나 비어있지 않거나(||) 디폴트 값이 아니라면 
        if (!string.IsNullOrEmpty(IPAddressInputField.text) || IPAddressInputField.text != DefaultIPAddress)
        {
            SystemManager.Instance.ConnectionInfo.IPAddress = IPAddressInputField.text;
        }

        //PortInputField 입력값이 널이거나 비어있지 않거나(||) 디폴트 값이 아니라면 
        if (!string.IsNullOrEmpty(PortInputField.text) || PortInputField.text != DefaultPort)
        {
            //입력받은 포트를 TryParse해서 성공한다면(true 리턴) 그대로 세팅
            //아니라면(false 리턴) 에러로그
            int port = 0;
            if (int.TryParse(PortInputField.text, out port))
                SystemManager.Instance.ConnectionInfo.Port = port;
            else
            {
                Debug.LogError("OnClientButton Error! port = " + PortInputField.text);
                return;
            }
        }


        sceneMain.GotoNextScene();

    }

}
