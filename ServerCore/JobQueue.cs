using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
   
    }


    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;


        public void Push(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false) //가장 첫번째 실행
                    flush = _flush = true;

            }

            if (flush)
                Flush();
        }

         void Flush()
        {
            while (true)
            {
                Action job = null;

                lock (_lock)
                {
                    if(_jobQueue.Count == 0)
                    {
                        _flush = false;
                        return;
                    }
                    job = _jobQueue.Dequeue();
                }

                job?.Invoke();
            }
        }

        Action Pop()
        {
            lock (_lock)
            {
                if(_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                    

                return _jobQueue.Dequeue();
            }
        }
    }
}
