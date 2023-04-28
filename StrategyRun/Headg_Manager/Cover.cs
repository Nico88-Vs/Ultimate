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
        public Sentiment Sent { get; set; } = Sentiment.Wait;


        public Covers(MainTrades main)
        {
            this.Main = main;
            this.ID = main.ID;
            this.Orders = new List<Order>();
            this.Positions = new List<Position>();
        }
    }
}
