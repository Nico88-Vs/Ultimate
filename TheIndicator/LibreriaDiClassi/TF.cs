using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;

namespace TheIndicator.LibreriaDiClassi
{
    public class TF
    {
        public int Delay { get; set; }
        public TimeFrame Timeframe { get; set; }
        public Indicator Indicatore { get; private set; }
        public int FastSeries { get; private set; }
        public int SlowSeries { get; private set; }

        public TF()
        {

        }

        public TF(TimeFrame timeframe, int delay, Indicator indi, int fast, int slow)
        {
            Delay = delay;
            Timeframe = timeframe;
            this.Indicatore = indi;
            this.FastSeries = fast;
            this.SlowSeries = slow;
        }
        public enum TimeFrame
        {
            Fast,
            Mid,
            Slow,
        }

        public int GetCorrectBuffer(int tenkanperiod, int buffer)
        {
            return buffer + (tenkanperiod * Delay);
        }

        public int GetCorrectBuffer(int tenkanperiod)
        {
            return tenkanperiod * Delay;
        }
    }
}
