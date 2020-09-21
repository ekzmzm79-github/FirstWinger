using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct SquadronMemberStruct
{
    public int index;
    public int EnemyID;
    public float GeneratePointX;
    public float GeneratePointY;
    public float AppearPointX;
    public float AppearPointY;
    public float DisappearPointX;
    public float DisappearPointY;
}
public class SquadronTable : TableLoader<SquadronMemberStruct>
{
    //<인덱스, 스쿼드 멤버 정보> 형식의 List
    List<SquadronMemberStruct> tableDatas = new List<SquadronMemberStruct>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    protected override void AddData(SquadronMemberStruct data)
    {
        tableDatas.Add(data);
    }


    //해당 인덱스의 SquadronMemberStruct 형태의 정보 반환
    public SquadronMemberStruct GetSquadronMember(int index)
    {
        if (index < 0 || index >= tableDatas.Count)
        {
            Debug.Log("SquadronMemberStruct Error! index = " + index);
            //SquadronMemberStruct의 모든 멤버가 0으로 채워지는 기본값
            //class라면 참조값이기 때문에 null을 리턴하면 되지만 struct는 불가
            return default(SquadronMemberStruct);
        }

        return tableDatas[index];
    }

    public int GetCount()
    {
        return tableDatas.Count;
    }

}
