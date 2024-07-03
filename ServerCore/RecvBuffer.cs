using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        //[][][][][][][][][][][]
        //버퍼가 완전 비어있는 경우 r,w 모두 0에 위치
        //클라가 5byte 송신(성공) -> w가 5번 인덱스에 위치(4번이 아님)-> 다음에 5번부터 이어서 받겠다
        //r은 왜 존재할까? 컨텐츠 코드에서 사용 
        //5byte가 완전체일 경우-> 바로 5로 이동,
        //8byte가 완전체일 경우-> 대기 


        ArraySegment<byte> _buffer;
        int _readPos; // 읽는 cursor
        int _writePos; //작성하는 cursor

        public RecvBuffer(int bufferSize)
        {
            //ReadBuff로 사용할 버퍼 
            _buffer = new ArraySegment<byte>(new byte[bufferSize],0,bufferSize);
        }
    }
}
