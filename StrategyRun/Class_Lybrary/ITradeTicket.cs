using System;
using System.Collections.Generic;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace StrategyRun.Class_Lybrary
{
    public class TradeTiket : EventArgs
    {
        public Sentiment TradeSentiment { get; set; }
        public int ClaudId { get; set; }
        public Dictionary<Cloud, List<Cloud>> Dyctionary { get; set; }
        public TF TFrame{ get; set; }

        public TradeTiket(Sentiment sent, int clId, Dictionary<Cloud, List<Cloud>> dicty, TF tF)
        {
            this.TradeSentiment = sent;
            this.Dyctionary = dicty;
            this.ClaudId = clId;
            this.TFrame = tF;
        }

        public TradeTiket()
        {
            
        }
    }
    public interface ITradeTicket
    {
        public string CondictionName { get; set; }
        public CloudSeries Series { get; set; }
        public event EventHandler<TradeTiket> TradeTicketCreated;
    }
}
