using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;

namespace StrategyRun.Class_Lybrary
{
    public interface ISentiment
    {
        public string Name { get; set; }
        public SwitchSentiment CurrentSent { get; set; }
    }
}
