using System;
using gView.Framework.system;

namespace gView.Framework.system
{
	/// <summary>
	/// Zusammenfassung f�r CancelTracker.
	/// </summary>
	public class CancelTracker : ICancelTracker
	{
		private bool _continue;
        private bool _paused;

		public CancelTracker()
		{
			_continue=true;
            _paused = false;
		}
	
		public void Reset() 
		{
			_continue=true;
            _paused = false;
		}

		#region ICancelTracker Member

		public void Cancel()
		{
            _continue = _paused = false;
		}

		public bool Continue
		{
			get
			{
				return _continue;
			}
		}

        public void Pause()
        {
            _continue = false;
            _paused = true;
        }

        public bool Paused
        {
            get { return _paused; }
        }

        #endregion

        public static bool Canceled(ICancelTracker cancelTracker) 
        {
            if (cancelTracker != null && !cancelTracker.Continue)
                return true;

            return false;
        }
    }
}
