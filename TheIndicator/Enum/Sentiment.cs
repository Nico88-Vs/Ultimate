using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheIndicator.Enum
{
    public enum Sentiment
    {
        Buy, Sell, Wait
    }

    public class SwitchSentiment : EventArgs
    {
        public Sentiment NewSentiment { get; set; }
        public Sentiment OldSentimennt { get; set; }

        public SwitchSentiment(Sentiment oldSentiment, Sentiment newSentiment)
        {
            this.OldSentimennt = oldSentiment;
            this.NewSentiment = newSentiment;
        }
    }
}
