using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheIndicator.Enum
{
    [Serializable]
    public class Min_Max
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int Length { get; set; }
        public int Buffer { get; set; }



        public Min_Max(int id, double value, int lenght, int buffer)
        {
            this.Id = id;
            this.Value = value;
            this.Length = lenght;
            this.Buffer = buffer;
        }
    }
}
