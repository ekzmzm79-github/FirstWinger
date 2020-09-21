using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneMain : BaseSceneMain
{

    public void OnStartButton()
    {
        PanelManager.GetPanel(typeof(NetWorkConfigPanel)).Show();
    }

    public void GotoNextScene()
    {
        SceneController.Instance.LoadScene(SceneNameConstants.LoadingScene);
    }
}
