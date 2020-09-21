using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

public struct SquadronScheduleDataStruct
{
    public int index;
    public float GenerateTime;
    public int SquardronID;
}

public class SquadronScheduleTable : TableLoader<SquadronScheduleDataStruct>
{
    List<SquadronScheduleDataStruct> tableDatas = new List<SquadronScheduleDataStruct>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void AddData(SquadronScheduleDataStruct data)
    {
        tableDatas.Add(data);
    }


    //해당 인덱스의 SquadronScheduleDataStruct 형태의 정보 반환
    public SquadronScheduleDataStruct GetSquadronSchedule(int index)
    {
        if (index < 0 || index >= tableDatas.Count)
        {
            Debug.Log("SquadronScheduleDataStruct Error! index = " + index);
            //SquadronMemberStruct의 모든 멤버가 0으로 채워지는 기본값
            //class라면 참조값이기 때문에 null을 리턴하면 되지만 struct는 불가
            return default(SquadronScheduleDataStruct);
        }

        return tableDatas[index];
    }

    public int GetDataCount()
    {
        return tableDatas.Count;
    }

}
