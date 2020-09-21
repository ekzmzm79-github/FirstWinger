using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : BasePanel
{
    [SerializeField]
    Text scoreValue;

    [SerializeField]
    Gage HPgage;

    Player hero = null;
    Player Hero
    {
        get
        {
            if (hero == null)
                hero = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero;
            return hero;
        }
    }

    public void SetScore(int value)
    {
        Debug.Log("SetScore value = " + value);

        scoreValue.text = value.ToString();
    }

    public override void InitializePanel()
    {
        base.InitializePanel();
        HPgage.SetHP(100, 100);
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if(Hero != null)
        {
            HPgage.SetHP(Hero.currentHP, Hero.maxHP);
        }
    }
}
