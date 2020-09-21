using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 클래스는 기본적으로 public이며 인스펙터에 노출이 안되지만
// System.Serializable를 통해서 가능하다.
[System.Serializable] 
public class BGscrollData
{
    public Renderer RenderForScroll;
    public float Speed;
    public float OffsetX;
}

public class BGScroller : MonoBehaviour
{
    [SerializeField]
    BGscrollData[] ScrollDatas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScroll();
    }

    void UpdateScroll()
    {
        //foreach는 일반 for보다 느리다.
        for(int i = 0; i< ScrollDatas.Length; i++)
        {
            SetTexureOffset(ScrollDatas[i]);
        }
    }

    void SetTexureOffset(BGscrollData scrollData)
    {
        //움직일 x값의 오프셋을 Speed와 Time.deltaTime을 곱해서 개별적으로 구함
        scrollData.OffsetX += (float)(scrollData.Speed) * Time.deltaTime;

        if (scrollData.OffsetX > 1)
            scrollData.OffsetX = scrollData.OffsetX % 1.0f;

        //생성한 오프셋으로 2d 벡터를 생성한뒤,
        Vector2 Offset = new Vector2(scrollData.OffsetX, 0);

        //현재 scrollData의 RenderForScroll(렌더러)의 매터리얼의 텍스쳐 오프셋을 설정.
        //이러면 해당 매터리얼의 텍스쳐가 애니메이션 효과를 주는거처럼 회전한다.
        scrollData.RenderForScroll.material.SetTextureOffset("_MainTex", Offset);
    }
}
