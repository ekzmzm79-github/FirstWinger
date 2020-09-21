using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//모든 패널들이 기본적으로 상속하게 되는 베이스 패널
public class BasePanel : MonoBehaviour
{
    private void Awake()
    {
        InitializePanel();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePanel();
    }

    private void OnDestroy()
    {
        DestroyPanel();
    }

    private void OnGUI() // 매 프레임마다 체크하는 메소드
    {

    }

    public virtual void InitializePanel()
    {
        //GetType(), this는 상속되는 클래스에 따라서 다른 값을 전달
        PanelManager.RegistPanel(GetType(), this);
    }

    public virtual void UpdatePanel()
    {

    }

    public virtual void DestroyPanel()
    {
        PanelManager.UnregistPanel(GetType());
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
