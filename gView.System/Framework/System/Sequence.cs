using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.IO;

namespace gView.Framework.system
{
    public class IntegerSequence : IPersistable
    {
        private int _number = 0, _inc = 1;
        private object lockThis = new object();

        public IntegerSequence()
        {
        }
        public IntegerSequence(int startValue)
        {
            _number = startValue;
        }
        public IntegerSequence(int startValue, int increment)
            : this(startValue)
        {
            _inc = increment;
        }

        public int Number
        {
            get
            {
                lock (lockThis)
                {
                    _number += _inc;
                    return _number;
                }
            }
        }

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            _number = (int)stream.Load("number", 0);
            _inc = (int)stream.Load("increment", 0);

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("number", _number);
            stream.Save("increment", _inc);

            return Task.FromResult(true);
        }

        #endregion
    }
}
