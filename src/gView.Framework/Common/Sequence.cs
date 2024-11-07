using gView.Framework.Core.IO;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace gView.Framework.Common
{
    public class IntegerSequence : IPersistable
    {
        private int _next = 0, _inc = 1;
        private object lockThis = new object();

        public IntegerSequence()
        {
        }
        public IntegerSequence(int startValue)
        {
            _next = startValue;
        }
        public IntegerSequence(int startValue, int increment)
            : this(startValue)
        {
            _inc = increment;
        }

        public int Next
        {
            get
            {
                lock (lockThis)
                {
                    _next += _inc;
                    return _next;
                }
            }
        }

        public void SetToIfLower(int number)
        {
            if (_next < number)
            {
                _next = number;
            }
        }

        public int TakeIfUnique(int canditate, int[] existingNumbers)
        {
            if(existingNumbers.Contains(canditate))
            {
                _next = Math.Max(_next, existingNumbers.Any() ? existingNumbers.Max() : -1);

                return Next;
            }

            _next = Math.Max(_next, Math.Max(canditate, existingNumbers.Any() ? existingNumbers.Max() : -1));

            return canditate;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _next = (int)stream.Load("number", 0);
            _inc = (int)stream.Load("increment", 0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("number", _next);
            stream.Save("increment", _inc);
        }

        #endregion
    }
}
