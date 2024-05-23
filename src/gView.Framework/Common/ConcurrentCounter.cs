namespace gView.Framework.Common
{
    public class ConcurrentCounter
    {
        private int count = 0;
        private readonly object lockObject = new object();

        public void Increment()
        {
            lock (lockObject)
            {
                count++;
            }
        }

        public void Decrement()
        {
            lock (lockObject)
            {
                count--;
            }
        }

        public int GetValue()
        {
            lock (lockObject)
            {
                return count;
            }
        }
    }
}
