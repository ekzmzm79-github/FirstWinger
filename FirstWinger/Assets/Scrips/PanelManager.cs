using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    // static으로 선언되는 딕셔너리와 메소드는 
    // 이 스크립트가 프로젝트에서 유일하게 있는 Canvas에 Attach될 것이기 때문

    static Dictionary<Type, BasePanel> Panels = new Dictionary<Type, BasePanel>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //패널을 등록하는 메소드
    public static bool RegistPanel(Type PanelClassType, BasePanel basePanel)
    {
        if(Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("RegistPanel error, Already exist Type! PanelClassType = " + PanelClassType.ToString());
            return false;
        }

        Debug.Log("RegistPanel Called! Type = " + PanelClassType.ToString() + ", basePanel = " + basePanel.name);

        //각각의 Panel 창은 유일해야한다.
        Panels.Add(PanelClassType, basePanel);
        return true;
    }

    //패널 등록을 해제하는 메소드
    public static bool UnregistPanel(Type PanelClassType)
    {
        if(!Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("UnregistPanel error, Can't Find Type! PanelClassType = " + PanelClassType.ToString());
            return false;
        }

        //PanelClassType 키 값 삭제
        Panels.Remove(PanelClassType);
        return true;
    }

    // PanelClassType 타입의 패널을 반환(BasePanel 형식)
    public static BasePanel GetPanel(Type PanelClassType)
    {
        if (!Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("GetPanel error, Can't Find Type! PanelClassType = " + PanelClassType.ToString());
            return null;
        }

        return Panels[PanelClassType];
    }

}
