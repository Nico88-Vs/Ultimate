using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;

namespace StrategyRun.Class_Lybrary
{
    public struct Covers
    {
        public int ID { get; set; }
        public MainTrades Main { get; set; }
        public List<Order> Orders { get; set; }
        public List<Position> Positions { get; set; }
        public Sentiment Sent { get; set; } = Sentiment.Wait;

        public Covers(MainTrades main)
        {
            Main = main;
            ID = main.ID;
            Orders = new List<Order>();
            Positions = new List<Position>();
        }
    }
}
