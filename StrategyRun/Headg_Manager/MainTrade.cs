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
        public string Name { get; private set; }
        public Sentiment Sent { get; set; } = Sentiment.Wait;


        public MainTrades(int id , string name)
        {
            this.ID = id;
            Positions = new List<Position>();
            Orders = new List<Order>();
            this.Name = name;
        }

       
    }
}
    