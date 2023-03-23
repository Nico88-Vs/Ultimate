using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;


namespace TheIndicator.LibreriaDiClassi
{
    public struct TF
    {
        public int Delay { get; set; }
        public TimeFrame Timeframe { get; set; }
        public Indicator Indicatore { get; private set; }
        public int FastSeries { get; private set; }
        public int SlowSeries { get; private set; }

        public TF(TimeFrame timeframe, int delay)
        {
            Delay = delay;
            Timeframe = timeframe;
        }
        public enum TimeFrame
        {
            Fast,
            Mid,
            slow,
        }
    }

}
