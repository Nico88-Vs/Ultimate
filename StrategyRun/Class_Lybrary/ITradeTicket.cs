using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace StrategyRun.Class_Lybrary
{
    public class TradeTiket : EventArgs
    {
        public Sentiment TradeSentiment { get; set; }
        public int ClaudId { get; set; }
        public Dictionary<Cloud, List<Cloud>> Dyctionary { get; set; }
        public string CondictionName { get; set; }

        public TradeTiket(Sentiment sent, int clId, Dictionary<Cloud, List<Cloud>> dicty, string condName)
        {
            this.TradeSentiment = sent;
            this.Dyctionary = dicty;
            this.ClaudId = clId;
            this.CondictionName = condName;
        }

        public TradeTiket()
        {
            
        }
    }
    public interface ITradeTicket
    {
        public string CondictionName { get; set; }
        public event EventHandler<TradeTiket> TradeTicketCreated;
    }
}
