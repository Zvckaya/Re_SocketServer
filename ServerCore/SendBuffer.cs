using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535*100; //청크사이즈는 조절이 가능하다.

        static Stack<SendBuffer> _bufferPool = new Stack<SendBuffer>();
        static object _lock = new object();

        public static ArraySegment<byte> Open(int reserverSize)
        {
            lock (_lock)
            {
                if (_bufferPool.Count > 0)
                    CurrentBuffer.Value = _bufferPool.Pop();
                else
                    CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            return CurrentBuffer.Value.Open(reserverSize);
            
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = CurrentBuffer.Value.Close(usedSize);

            if (CurrentBuffer.Value.FreeSize == 0) // 다쓰면 반난 
            {
                lock (_lock)
                {
                    _bufferPool.Push(CurrentBuffer.Value);
                    CurrentBuffer.Value = null;
                }
            }
            return segment;
        }
            

    }

    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize]; //chunk는 뭉탱이 느낌 
        }

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveSize) // 버퍼 대여
        {
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
;        }

        public ArraySegment<byte> Close (int usedSize) // 버퍼최종 confirm(반환)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer,_usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
