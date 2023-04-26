using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheIndicator.Enum
{
    [Serializable]
    public struct Bases
    {
        public enum Status
        {
            Waiting,
            Running
        }
        public int Lenght { get; set; }
        public Status BStatus { get; private set; }
        public int Id { get; set; }
        public double Value { get; set; }
        public int LineSeries { get; private set; }
        public int Buffer { get; set; }

        public Bases(int id, int lineseries, double value, int buffer)
        {
            this.Lenght = 0;
            this.BStatus = Status.Running;
            this.LineSeries = lineseries;
            this.Id = id;
            this.Value = value;
            this.Buffer = buffer;
        }

        public void Close()
        {
            this.BStatus = Status.Waiting;
        }
    }
}
