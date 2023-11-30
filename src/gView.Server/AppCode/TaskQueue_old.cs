using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class TaskQueue_old<T>
    {
        public delegate Task QueuedTask(T parameter);

        private long ProcessId = 0;
        private long ProcessedTasks = 0;
        private int RunningTasks = 0;
        private int MaxParallelTasks = 50;
        private object locker = new object();

        public TaskQueue_old(int maxParallelTask, int maxQueueLength)
        {
            this.MaxParallelTasks = maxParallelTask;
        }

        public long CurrentProccessId => ProcessId;
        public long CurrentProcessedTasks => ProcessedTasks;
        public long CurrentQueuedTasks => CurrentProccessId - CurrentProcessedTasks;
        public int CurrentRunningTasks => RunningTasks;

        private long NextProcessId()
        {
            lock (locker)
            {
                /*
                if(ProcessId>100)
                {
                    ProcessId -= ProcessedTasks;
                    ProcessedTasks = 0;
                }
                */
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

        private void IncreaseRunningTasks()
        {
            lock(locker)
            {
                RunningTasks = ++RunningTasks;
            }
        }

        public void DecreaseRunningTasks()
        {
            lock (locker)
            {
                RunningTasks = --RunningTasks;
            }
        }

        private bool IsReadyToRumble(long currentProcessId)
        {
            lock (locker)
            {
                return currentProcessId - ProcessedTasks <= MaxParallelTasks;
            }
        }

        async public Task<bool> AwaitRequest(TaskQueue<T>.QueuedTask method, T parameter)
        {
            try
            {
                long currentProcessId = NextProcessId();

                while (true)
                {
                    if (IsReadyToRumble(currentProcessId))
                    {
                        break;
                    }

                    await Task.Delay(10);
                    //return false;
                }

                IncreaseRunningTasks();
                await method?.Invoke(parameter);

                return true;
            }
            catch (Exception /*ex*/)
            {
                //await Logger.LogAsync(
                //    parameter as IServiceRequestContext,
                //    Framework.system.loggingMethod.error,
                //    "Thread Error: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);

                return false;
            }
            finally
            {
                DecreaseRunningTasks();
                IncreaseProcessedTasks();
            }
        }
    }
}
