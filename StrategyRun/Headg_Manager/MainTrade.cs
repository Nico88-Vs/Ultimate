using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TheIndicator.LibreriaDiClassi;
using TheIndicator.Enum;

namespace StrategyRun.Headg_Manager
{
    public struct MainTrades
    {
        public List<Position> Positions { get; set; }
        public List<Order> Orders { get; set; }
        public int ID { get; set; }

        public Sentiment Sentiment { get; set; }
        public event EventHandler<SwitchSentiment> SentimentChanged;

        private Sentiment currentSent;
        private CloudSeries CloudSeries;

        public MainTrades(CloudSeries cloudSeries, int id, Sentiment current)
        {
            CloudSeries = cloudSeries;
            this.ID = id;
            Positions = new List<Position>();
            Orders = new List<Order>();
            this.currentSent = current;

            if(currentSent != this.Sentiment)
            {

            }
        }
    }
}
    