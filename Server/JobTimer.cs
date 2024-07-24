using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; //실행시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick; //실행시간 적을수록 선순위;
        }
    }

    class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0) //당장 실행하고 싶으면 인자 x
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter; //실행 타이밍 
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }

        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job; // 일감 추출 

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek(); //살펴보기 
                    if (job.execTick > now)
                        break;

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
