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
        //read가 write를 따라가야함 




        ArraySegment<byte> _buffer;
        int _readPos; // 읽는 cursor
        int _writePos; //작성하는 cursor

        public RecvBuffer(int bufferSize)
        {
            //ReadBuff로 사용할 버퍼 
            _buffer = new ArraySegment<byte>(new byte[bufferSize],0,bufferSize);
        }

        public int DataSzie { get { return _writePos-_readPos; } } //실제 데이터 사이즈는 Write에서 ReadPos를 뺀 크기 
        public int FreeSize { get { return _buffer.Count-_writePos; } } //남은 공간 
        
        public ArraySegment<byte> ReadSegment  //읽을 데이터 ,현재까지 읽을 데이터 
        {
            get { return new ArraySegment<byte>(_buffer.Array,_buffer.Offset+_readPos,DataSzie); }
        }

        public ArraySegment<byte> WriteSegment //다음에 Recv할 때 사용할 범위 -
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSzie;
            if(dataSize ==0) // rw가 겹치는 상태(모두 완료한 상태)
            {
                //남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0; 
            }
            else
            {
                //남은 데이터가 있으면 시작 위치로 이동 
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                //원본배열, 버퍼 Offset에서 + readCursor(복사 시작 위치) , 이동시킬 배열, 이동시킬 배열의스타트 위치, 복사할 데이터 크기
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfByte) //컨텐츠 코드에서 성공적으로 처리가 되었는가?
        {
            if(numOfByte > DataSzie)
            {
                return false;
            }
            _readPos += numOfByte;// 처리할 데이터만큼 이동
            return true;

        }

        public bool OnWrite(int numOfByte) // Recv했을때 Write커서를 이동 즉, 클라에서 요청했을때
        {
            if (numOfByte > FreeSize) {                 
                return false;
            }
            _writePos += numOfByte;
            return true; 
        }

    }
}
