using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class TaskQueue<T>
    {
        public delegate Task QueuedTask(T parameter);

        private long ProcessId = 0;
        private long ProcessedTasks = 0;
        private int MaxParallelTasks = 50;
        private object locker = new object();

        public TaskQueue(int maxParallelTask, int maxQueueLength)
        {
            this.MaxParallelTasks = maxParallelTask;
        }

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
                return currentProcessId - ProcessedTasks <= MaxParallelTasks;
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
            catch (Exception ex)
            {
                //await Logger.LogAsync(
                //    parameter as IServiceRequestContext,
                //    Framework.system.loggingMethod.error,
                //    "Thread Error: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);

                return false;
            }
            finally
            {
                IncreaseProcessedTasks();
            }
        }
    }
}
