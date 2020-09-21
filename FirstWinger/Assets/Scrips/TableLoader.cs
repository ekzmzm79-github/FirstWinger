using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TableLoader<TMarshalStruct> : MonoBehaviour
{
    [SerializeField]
    protected string FilePath;

    TableRecordParser<TMarshalStruct> tableRecordParser = new TableRecordParser<TMarshalStruct>();

    public bool Load()
    {
        //textAsset 형태로 해당 경로에서 로드
        TextAsset textAsset = Resources.Load<TextAsset>(FilePath);
        if(!textAsset)
        {
            Debug.LogError("Load Failed! FilePath = " + FilePath);
            return false;
        }

        ParseTable(textAsset.text);

        return true;
    }

    void ParseTable(string text)
    {
        // Sysyem.IO.StringReader
        StringReader reader = new StringReader(text);

        string line = null;
        bool fieldRead = false;

        while((line = reader.ReadLine()) != null)
        {
            if(!fieldRead) // 첫째줄은 패스시키기 위해서(필드명으로만 이루어진 줄)
            {
                fieldRead = true;
                continue;
            }

            //각 한줄을 데이터로 파싱
            TMarshalStruct data = tableRecordParser.ParseRecordLine(line);
            AddData(data);
        }
    }

    protected virtual void AddData(TMarshalStruct data)
    {

    }

}
