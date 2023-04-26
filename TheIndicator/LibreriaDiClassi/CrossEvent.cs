using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;

namespace TheIndicator.LibreriaDiClassi
{
    [Serializable]
    public class CrossEvent : EventArgs
    {
        public EventCrosArg Args { get; set; }
        public double Price { get; set; }
        public int BarIndex { get; set; }
        public Cloud CurrentCloud { get; private set; }

        public CrossEvent(EventCrosArg arg, double price, int barIndex, Cloud currentCloud)
        {
            this.Price = price;
            this.Args = arg;
            this.BarIndex = barIndex;
            this.CurrentCloud = currentCloud;
        }
    }
}
