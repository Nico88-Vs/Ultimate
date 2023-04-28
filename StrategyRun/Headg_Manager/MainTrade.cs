using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TheIndicator.LibreriaDiClassi;
using TheIndicator.Enum;

namespace StrategyRun.Headg_Manager
{
    public class MainTrades
    {
        public List<Position> Positions { get; set; }
        public List<Order> Orders { get; set; }
        public int ID { get; set; }

        public Sentiment Sentiment { get; set; }
        public event EventHandler<SwitchSentiment> SentimentChanged;

        private Sentiment currentSent = Sentiment.Wait;

        public MainTrades(int id, Sentiment current)
        {
            this.ID = id;
            Positions = new List<Position>();
            Orders = new List<Order>();
            this.currentSent = current;

            if(currentSent != this.Sentiment)
            {
                SwitchSentiment arg = new SwitchSentiment(currentSent, this.Sentiment);
                this.OnSentimentChanged(arg);
            }
        }

        public virtual void OnSentimentChanged(SwitchSentiment e)
        {
            SentimentChanged?.Invoke(this, e);
            this.currentSent = e.NewSentiment;
        }
    }
}
    