using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class TaskQueue<T>
    {
        public delegate Task QueuedTask(T parameter);

        private static long ProcessId = 0;
        private static long ProcessedTasks = 0;
        private static int MaxParalellTasks = 50;
        private object locker = new object();

        private long NextProcessId()
        {
            lock (locker)
            {
                return ++ProcessId;
            }
        }

        private void IncreaseProcessedTasks()
        {
            lock (locker)
            {
                ProcessedTasks = ++ProcessedTasks;
            }
        }

        private bool IsReadyToRumble(long currentProcessId)
        {
            lock(locker)
            {
                return currentProcessId - ProcessedTasks <= MaxParalellTasks;
            }
        }

        async public Task<bool> AwaitRequest(TaskQueue<T>.QueuedTask method, T parameter)
        {
            try
            {
                long currentProcessId = NextProcessId();

                while(true)
                {
                    if (IsReadyToRumble(currentProcessId))
                        break;

                    await Task.Delay(10);
                    //return false;
                }

                await method?.Invoke(parameter);

                return true;
            }
            catch (Exception /*ex*/)
            {
                
                return false;
            }
            finally
            {
                IncreaseProcessedTasks();
            }
        }
    }
}
