using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;

namespace StrategyRun.Headg_Manager
{
    public class Covers
    {
        public int ID { get; set; }
        public MainTrades Main { get; set; }
        public List<Order> Orders { get; set; }
        public List<Position> Positions { get; set; }
        public Sentiment Sentiment { get; set; }

        public Covers(MainTrades main)
        {
            this.Main = main;
            this.ID = main.ID;
            this.Orders = new List<Order>();
            this.Positions = new List<Position>();

            main.SentimentChanged += this.Main_SentimentChanged;
        }

        private void Main_SentimentChanged(object sender, SwitchSentiment e)
        {
            switch (e.NewSentiment)
            {
                case Sentiment.Buy:
                    this.Sentiment = Sentiment.Sell;
                    break;
                case Sentiment.Sell:
                    this.Sentiment = Sentiment.Buy;
                    break;
                case Sentiment.Wait:
                    this.Sentiment = Sentiment.Wait;
                    break;
            }
        }
    }
}
