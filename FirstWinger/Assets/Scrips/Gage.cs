using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gage : MonoBehaviour
{
    Slider slider;
    // Start is called before the first frame update

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetHP(float currentValue, float maxValue)
    {
        //최대값 이하로만 조절
        currentValue = currentValue > maxValue ? maxValue : currentValue;

        slider.value = currentValue / maxValue; // 비율
    }
}
