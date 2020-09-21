using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

public struct EnemyStruct
{
    public int index;
    //string 크기 강제
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MarshalTableConstant.charBufferSize)]
    public string FilePath;
    public int MaxHP;
    public int Damage;
    public int CrashDamage;
    public float BulletSpeed;
    public int FireRemainCount;
    public int GamePoint;

}

public class EnemyTable : TableLoader<EnemyStruct>
{
    Dictionary<int, EnemyStruct> tableDatas = new Dictionary<int, EnemyStruct>();

    // Start is called before the first frame update
    void Start()
    {
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void AddData(EnemyStruct data)
    {
        tableDatas.Add(data.index, data);
    }


    //해당 인덱스의 EnemyStruct 형태의 정보 반환
    public EnemyStruct GetEnemy(int index)
    {
        if(!tableDatas.ContainsKey(index))
        {
            Debug.Log("GetEnemy Error! index = " + index);
            //EnemyStruct 모든 멤버가 0으로 채워지는 기본값
            //class라면 참조값이기 때문에 null을 리턴하면 되지만 struct는 불가
            return default(EnemyStruct);
        }

        return tableDatas[index];
    }

}
