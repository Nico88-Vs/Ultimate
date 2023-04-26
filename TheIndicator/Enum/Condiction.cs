using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheIndicator.Enum
{
    [Serializable]
    public struct Condiction
    {
        public enum Trend
        {
            None,
            Flat,
            Up,
            Down,
            Impulso,
            Ritraccaiamento
        }

        public enum Stato
        {
            WaitingLong,
            WaitingShort,
            Long,
            Short,
            WaitingCloseLong,
            WaitingCloseShort
        }

        public enum Type
        {
            Position,
            Trade,
            Cover
        }

        public enum Strutture
        {
            PivotPoint,
            Support,
            Resistence,
            Min,
            Max
        }

        public Trend TrendProp { get; set; }
        public Stato StatusProp { get; set; }
        public Type TypeProp { get; set; }
        public Strutture StruttureProp { get; set; }
    }
}
