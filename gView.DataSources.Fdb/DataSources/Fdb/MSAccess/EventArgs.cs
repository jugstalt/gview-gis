using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.Fdb.MSAccess
{
    public class UpdateSIDefEventArgs : EventArgs
    {
        public bool Finished;

        public UpdateSIDefEventArgs()
        {
            Finished = true;
        }
    }

    public class UpdateSICalculateNodes : EventArgs
    {
        public int Pos;
        public int Count;

        public UpdateSICalculateNodes(int pos, int count)
        {
            Pos = pos;
            Count = count;
        }
    }

    public class UpdateSIUpdateNodes : EventArgs
    {
        public int Pos;
        public int Count;

        public UpdateSIUpdateNodes(int pos, int count)
        {
            Pos = pos;
            Count = count;
        }
    }

    public class RepairSICheckNodes : EventArgs
    {
        public int Pos;
        public int Count;
        public int WrongNIDs = 0;

        public RepairSICheckNodes(int pos, int count)
        {
            Pos = pos;
            Count = count;
        }
    }

    public class RepairSIUpdateNodes : EventArgs
    {
        public int Pos;
        public int Count;

        public RepairSIUpdateNodes(int pos, int count)
        {
            Pos = pos;
            Count = count;
        }
    }
}
