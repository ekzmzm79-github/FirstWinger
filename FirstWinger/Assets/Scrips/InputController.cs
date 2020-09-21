using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController
{

    public void UpdateInput()
    {
        //게임이 running 중이 아니면 인풋을 막는다
        if (SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().CurrentGameState != GameState.Running)
            return;

        UpdateKeyboard();
        UpateMouse();
    }

    void UpdateKeyboard()
    {
        // Vector3.zero로 초기화하기 때문에 인풋이 없다면 계속해서 0벡터가 전달됨
        Vector3 moveDirection = Vector3.zero;

        //Input.GetKey : 입력되는 동안 계속해서 감지
        //Input.GetKeyDown : 키를 누를 때 한번만 감지, Input.GetKeyUp : 키를 뗄 때 감지
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y = -1;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x = -1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x = 1;
        }

        //SystemManager에 싱글톤 변수인 Instance로 접근해서 
        //해당 스크립트에서 Player 스크립트에 접근하는 변수 프로퍼티 Hero를 통해서
        //Player 스크립트에 접근하여 ProcessInput 메소드를 moveDirection 매개변수로 호출
        //즉, 다른 스크립트로 접근하기 위한 통로로서 싱글톤 SystemManager를 활용
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero.ProcessInput(moveDirection);

    }

    void UpateMouse()
    {
        //마우스 왼쪽 버튼을 누르면 싱글톤 SystemManager의 Player -> Fire() 메소드 호출
        if (Input.GetMouseButtonDown(0))
        {
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero.Fire();
        }
    }

}
