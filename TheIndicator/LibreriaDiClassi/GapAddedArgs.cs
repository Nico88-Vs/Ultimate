using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;

namespace TheIndicator.LibreriaDiClassi
{
    public class GapAddedArgs : EventArgs
    {
        public int CludId { get; private set; }
        public Gaps Gap { get; private set; }
        public GapAddedArgs(int c, Gaps g)
        {
            CludId = c;
            Gap = g;
        }
    }
}
