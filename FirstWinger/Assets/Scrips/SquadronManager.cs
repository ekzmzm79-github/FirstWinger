using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadronManager : MonoBehaviour
{
    float GameStartTime;
    int ScheduleIndex;
    bool running = false;

    [SerializeField]
    SquadronTable[] squadronDatas;

    [SerializeField]
    SquadronScheduleTable squadronScheduleTable;


    // Start is called before the first frame update
    void Start()
    {
        //현재 객체 자식에 달린 모든 SquadronTable 컴포넌트를 배열로 반환
        squadronDatas = GetComponentsInChildren<SquadronTable>();
        for (int i = 0; i < squadronDatas.Length; i++)
            squadronDatas[i].Load(); //각각 스쿼드 데이터 로드

        squadronScheduleTable.Load(); // 스쿼드 스케줄 로드
    }

    // Update is called once per frame
    void Update()
    {

        CheckSquardronGeneratings();
    }

    public void StartGame()
    {
        GameStartTime = Time.time;
        ScheduleIndex = 0;
        running = true;
        Debug.Log("Game Started!");
    }

    void CheckSquardronGeneratings()
    {
        if (!running)
            return;

        SquadronScheduleDataStruct data = squadronScheduleTable.GetSquadronSchedule(ScheduleIndex);

        if(Time.time - GameStartTime >= data.GenerateTime)
        {
            
            if(ScheduleIndex >= squadronScheduleTable.GetDataCount())
            {
                AllSquardronGenerated();
                return;
            }

            GenerateSquardron(squadronDatas[data.SquardronID]);
            ScheduleIndex++;

        }
    }

    void GenerateSquardron(SquadronTable table)
    {
        Debug.Log("GenerateSquardron : " + ScheduleIndex);
        for(int i=0;i<table.GetCount(); i++)
        {
            SquadronMemberStruct squadronMember = table.GetSquadronMember(i);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.GenerateEnemy(squadronMember);
        }
    }
    void AllSquardronGenerated()
    {
        Debug.Log("AllSquardronGenerated");

        running = false;
    }

}
