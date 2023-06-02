using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Class_Lybrary
{
    public enum ManagerStatus { Waiting, WaitingMain, WaitingCovers, Running }

    public interface IPositionManager<S , T , H> where S : ISentiment where T : ITradeTicket where H : IHeadgeStrategy
    {

        public Sentiment Sentimento{ get; set; }
        public S SentimentObj { get; set; }
        public List<T> TradeTickets { get; set; }
        public H HeadgeStrategy { get; set; }
        public CloudSeries Series { get; set; }
        public List<TradeTiket> CurrenTiket { get; set; }
        public ManagerStatus Status { get; set; }

        const string name = "";
        const string description = "";

        public void Update ();
    }
}
