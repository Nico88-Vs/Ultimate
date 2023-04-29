using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TheIndicator.LibreriaDiClassi;
using TheIndicator.Enum;

namespace StrategyRun.Class_Lybrary
{
    public struct MainTrades
    {
        public List<Position> Positions { get; set; }
        public List<Order> Orders { get; set; }
        public int ID { get; set; }
        public string Name { get; private set; }
        public Sentiment Sent { get; set; } = Sentiment.Wait;


        public MainTrades(int id, string name)
        {
            ID = id;
            Positions = new List<Position>();
            Orders = new List<Order>();
            Name = name;
        }
    }
}
