using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

/*
 * Marshalling : 한 객체의 메모리에서 표현방식을 저장 또는 전송에 적합한 
 *              다른 데이터 형식으로 변환하는 과정
 *              
 * csv 테이블의 레코드 문자열(한 줄)을 구조체로 변환하고
 * 필요한 형태의 자료 저장소에 기록할 예정
*/

//상수 정의용 클래스
public class MarshalTableConstant
{
    public const int charBufferSize = 256;
}



public class TableRecordParser<TMarshalStruct>
{
    public TMarshalStruct ParseRecordLine(string line)
    {
        //TMarshalStruct 크기에 맞춰서 Byte 배열 할당
        Type type = typeof(TMarshalStruct); // TMarshalStruct의 타입

        //System.Runtime.InteropServices.Marshal
        int structsize = Marshal.SizeOf(type); // TMarshalStruct타입의 크기
        byte[] structBytes = new byte[structsize]; //TMarshalStruct타입 데이터 크기만큼 바이트 할당
        int structBytesIndex = 0;

        // line 문자열을 spliter로 자름
        const string spliter = ",";
        string[] fieldDataList = line.Split(spliter.ToCharArray()); // ','로 잘려서 각가 배열에 들어감
        // 타입을 보고 바이너리에 파싱하여 삽입
        Type dataType;
        string splited;
        byte[] fieldByte;
        byte[] keyBytes;

        //템플릿으로 전달된 TMarshalStruct의 필드들을 배열로 반환 
        FieldInfo[] fieldInfos = type.GetFields(); // System.Reflection.FieldInfo
        for (int i = 0; i< fieldInfos.Length; i++) // 하나의 TMarshalStruct의 모든 필드만큼 수행
        {
            //','로 잘린 각 문자열이 원래 무슨 타입인가(int, float, char 등..)
            dataType = fieldInfos[i].FieldType; //각 필드의 타입
            splited = fieldDataList[i]; //','로 잘린 각 문자열

            //fieldByte에 알맞은 크기 할당
            MakeBytesByFieldType(out fieldByte, dataType, splited);

            // fieldByte의 값을 structBytes에 누적
            for (int index = 0; index <fieldByte.Length; index++)
            {
                structBytes[structBytesIndex++] = fieldByte[index];
            }

            /* 위 for 와 같은 소스
            Buffer.BlockCopy(fieldByte, 0, structBytes, structBytesIndex, fieldByte.Length);
            structBytesIndex += fieldByte.Length;
            */

            //첫번째 필드를 Key값으로 사용하기 위해 백업
            //if (i == 0)
            //    keyBytes = fieldByte;
        }

        //marshaling
        TMarshalStruct tStuct = MakeStructFromBytes<TMarshalStruct>(structBytes);
        //AddData(keyBytes, tStruct);
        return tStuct;
    }

    /// <summary>
    /// 문자열 splite를 주어진 dataType 에 맞게 fieldByte 배열에 변환해서 반환
    /// </summary>
    /// <param name="fieldByte">결과 값을 받을 배열</param>
    /// <param name="dataType">splite를 변환할때 사용될 자료형</param>
    /// <param name="splite">변환할 값이 있는 문자열</param>
    protected void MakeBytesByFieldType(out byte[] fieldByte, Type dataType, string splite)
    {
        fieldByte = new byte[1]; // 임시 할당

        // dataType의 각 형태에 맞춰서 분기
        if (typeof(int) == dataType)
        {
            fieldByte = BitConverter.GetBytes(int.Parse(splite));    // System.BitConverter
        }
        else if (typeof(float) == dataType)
        {
            fieldByte = BitConverter.GetBytes(float.Parse(splite));
        }
        else if (typeof(bool) == dataType)
        {
            /*
            bool value = bool.Parse(splite);
            int temp = value ? 1 : 0;
            //bool은 int로 변환해서 int로 바이트 할당
            fieldByte = BitConverter.GetBytes((int)temp);
            */

            fieldByte = BitConverter.GetBytes(bool.Parse(splite));

        }
        else if (typeof(string) == dataType)
        {
            fieldByte = new byte[MarshalTableConstant.charBufferSize];      // 마샬링을 하기위해서 고정크기 버퍼를 생성
            byte[] byteArr = Encoding.UTF8.GetBytes(splite);                // System.Text.Encoding;
            // 변환된 byte 배열을 고정크기 버퍼에 복사 (byteArr -> fieldByte)
            Buffer.BlockCopy(byteArr, 0, fieldByte, 0, byteArr.Length);     // System.Buffer;
        }
    }



    /// <summary>
    /// 마샬링을 통한 byte 배열의 T형 구조체 변환
    /// </summary>
    /// <typeparam name="T">마샬링에 적합하게 정의된 구조체의 타입</typeparam>
    /// <param name="bytes">마샬링할 데이터가 저장된 배열</param>
    /// <returns>변환된 T형 구조체</returns>
    public static T MakeStructFromBytes<T>(byte[] bytes)
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size);    // 마샬 메모리 할당

        Marshal.Copy(bytes, 0, ptr, size);          // 복사

        T tStruct = (T)Marshal.PtrToStructure(ptr, typeof(T));  // 메모리로부터 T형 구조체로 변환
        Marshal.FreeHGlobal(ptr);       // 할당된 메모리 해제
        return tStruct; // 변환된 값 반환
    }

}
