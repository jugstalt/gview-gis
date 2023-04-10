using gView.Framework.system;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace gView.Server.AppCode;

public class TaskQueue<T>
{
    public delegate Task QueuedTask(T parameter);

    private int _idleDuration = 100;
    private ConcurrentCounter _runningTasks;
    private int _maxRunningTasks = 50;
    private readonly ConcurrentQueue<QueueSemaphore> _queueSemaphores;

    public TaskQueue(int maxParallelTask, int maxQueueLength)
    {
        this._maxRunningTasks = maxParallelTask;
        _runningTasks = new ConcurrentCounter();
        _queueSemaphores = new ConcurrentQueue<QueueSemaphore>();
    }

    public long IdleDuration => _idleDuration;
    public long CurrentQueuedTasks { get; private set; } = 0;
    public int CurrentRunningTasks => _runningTasks.GetValue();

    #region Helper

    private bool ContinueWithoutQueueing()
    {
        lock (_runningTasks)
        {
            if (_runningTasks.GetValue() >= _maxRunningTasks)
            {
                return false;
            }

            _runningTasks.Increment();
            return true;
        }
    }

    private void ReleaseTask()
    {
        _runningTasks.Decrement();
    }

    private bool IsReadyToRumble(QueueSemaphore queueSemaphore)
    {
        return queueSemaphore.IsDequeued == true;
    }

    #endregion

    async public Task<bool> AwaitRequest(TaskQueue<T>.QueuedTask method, T parameter)
    {
        try
        {
            if (!ContinueWithoutQueueing())
            {
                var queueSemaphore = new QueueSemaphore();
                _queueSemaphores.Enqueue(queueSemaphore);

                while (true)
                {
                    if (IsReadyToRumble(queueSemaphore))
                    {
                        break;
                    }

                    await Task.Delay(10);
                }
            }

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
            ReleaseTask();
        }
    }

    public int Dequeue()
    {
        int dequeueCount = 0;

        lock (_runningTasks)
        {
            dequeueCount = _maxRunningTasks - _runningTasks.GetValue();

            for (int i = 0; i < dequeueCount; i++)
            {
                if (_queueSemaphores.TryDequeue(out var queueSemaphore))
                {
                    _runningTasks.Increment();
                    queueSemaphore.IsDequeued = true;
                }
            }
        }

        this.CurrentQueuedTasks = _queueSemaphores.Count;

        if(this.CurrentQueuedTasks > 0 )
        {
            _idleDuration = Math.Max(10, _idleDuration - 10);
        } 
        else
        {
            _idleDuration = Math.Min(100, _idleDuration + 10);
        }

        return _idleDuration;
    }

    #region Classes

    public class QueueSemaphore
    {
        public bool IsDequeued { get; set; } = false;
    }

    #endregion
}
