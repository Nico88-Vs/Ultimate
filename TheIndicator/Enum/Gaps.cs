using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;

namespace TheIndicator.Enum
{
    [Serializable]
    public struct Gaps
    {
        public enum Status
        {
            Waiting,
            Running
        }
        public enum Type
        {
            regular,
            inverse,
        }

        public enum Reason
        {
            lines,
            bases,
            both
        }


        public Type type { get; set; }
        public int Lenght { get; set; }
        public double Momentum { get; set; }
        public int Id { get; set; }
        public int Buffer { get; set; }
        public Status GapStatus { get; set; } = Status.Running;
        public int GapThick { get; private set; }
        public Reason GapReason { get; set; }
        public double StartPrice { get; set; }
        public double EndPrice{ get; set; }


        private int thk = 3;

        public Gaps(int id, int buffer, int TfMultiplaier)
        {
            SetThick(thk, TfMultiplaier);
            Id = id;
            Buffer = buffer;

            if (Lenght > 0)
            {
                this.Momentum = Math.Atan2(this.Lenght, EndPrice - StartPrice);
            }
        }

        public void Close()
        {
            this.GapStatus = Status.Waiting;
        }

        public void SetThick(int thick, int multiplaier)
        {
            this.GapThick = thick * multiplaier;
        }

       
    }
}
