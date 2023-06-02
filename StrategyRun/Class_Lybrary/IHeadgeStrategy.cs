using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Class_Lybrary
{
    public struct HadgedPositions
    {
        public List<Order> Orders { get; set; }
        public List<Position> Posizioni { get; set; }

        public const string Name = "";

        public HadgedPositions()
        {
            this.Posizioni = new List<Position>();
            this.Orders = new List<Order>();
        }
    }
    public interface IHeadgeStrategy
    {
        
        public enum HeadgeStatus { Waiting, Exposed, Covered, FullyExpo }

        public const string mainComment = "Main";
        public const string coverComment = "Cover";

        public double TotalAmmount { get; set; }
        public HadgedPositions Mains { get; }
        public HadgedPositions Cover { get; }
        public double Covers_Percent { get; set; }
        public HeadgeStatus Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TradeTiket LastTiket { get; set; }


        public void EvaluateTiket(TradeTiket t);
        public void SetCovers(PlaceOrderRequestParameters p, TradeTiket t);
        public void ModifyOrder(double LastPrice);
        public void CloseOrder();
      
    }
}
