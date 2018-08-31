using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace gView.Framework.system
{
    public abstract class SmartDatabaseConnection<T>
        where T : DatabaseConnection
    {
        private int _holdConnection = 20;
        private T _connection;
        private bool _locked = false, _disposed = false;
        private object _lockThis = new object();
        private DateTime _lastAccess;
        
        public SmartDatabaseConnection()
        {
           
        }

        private void StartTimer()
        {
            Timer t = new Timer(new TimerCallback(TimerProc));
            t.Change(1000, 0);
        }

        private void TimerProc(object state)
        {
            Timer t = (Timer)state;

            // Keine Connection mehr -> Timer sterben lassen
            if (_connection == null)
            {
                t.Dispose();
                return;
            }

            TimeSpan sp = DateTime.Now - _lastAccess;
            if (sp.TotalSeconds > _holdConnection && !_locked)
            {
                lock (_lockThis)
                {
                    t.Dispose();
                    if (_locked) // wenn in der Zwischenzeit angeforder wurde
                    {
                        return;
                    }
                    CloseConnection(_connection);
                    _connection = null;  
                }
            }
            else
            {
                t.Change(1000, 0);
            }
        }

        public T AllocConnection(gView.Framework.Data.IDataset dataset)
        {
            lock (_lockThis)
            {
                if (_disposed) return null;

                _lastAccess = DateTime.Now;

                if (_locked) return null; // Sollte eigentlich nicht vorkommen
                                          // Aufrufende Routinen sollen nicht parallel aufrufen

                if (_connection == null)
                {
                    _connection = OpenConnection(dataset);
                    if (_connection != null)
                    {   
                        //
                        // erst beim wieder Freigeben starten...
                        //
                        //_lastAccess = DateTime.Now;
                        //StartTimer();
                    }
                    else
                    {
                        return null;
                    }

                }

                _locked = true;
                return _connection;
            }
        }

        public void FreeConnection()
        {
            _locked = false;
            _lastAccess = DateTime.Now;
            StartTimer();
        }

        public void Dispose()
        {
            _disposed = true;
            CloseConnection(_connection);
            _connection = null;
        }

        protected abstract void CloseConnection(T connection);
        protected abstract T OpenConnection(gView.Framework.Data.IDataset dataset);
    }

    public class DatabaseConnection
    {
    }
}
