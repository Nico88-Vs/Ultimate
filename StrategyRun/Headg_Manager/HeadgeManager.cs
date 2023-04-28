using PlaceOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;
using TheIndicator.Interfacce;
using StrategyRun.Strategie;

namespace StrategyRun.Headg_Manager
{
    public enum TypeOfPosition
    {
        Main,
        Cover
    }

    public class HeadgeManager
    {
        private enum Status { WaitingCover, WaitingMain, FullFilled }
        public Covers Covers { get; set; }
        public MainTrades MainTrades { get; set; }
        public SwitchSentiment SentimentObj { get; set; }

        private Account account;
        private Symbol symbol;
        private CloudSeries Series;
        private Condic_Gap_Cros_Strategy_V1 condiction;


        public HeadgeManager(Account account, Symbol symbol, Condic_Gap_Cros_Strategy_V1 cond, SwitchSentiment sentObj)
        {
            this.condiction = cond;
            this.Series = cond.Series;
            this.account = account; 
            this.symbol = symbol;
            this.MainTrades = new MainTrades(0, "test");
            this.Covers = new Covers(this.MainTrades);
            this.SentimentObj = sentObj;

            this.SentimentObj.SentimentChanged += this.Sentiment_SentimentChanged; 
        }

        private void Sentiment_SentimentChanged(object sender, SwitchSentiment e)
        {
            if(e.NewSentiment == Sentiment.Wait)
            {

            }
        }

        private void DeterminaStatus()
        {
            List<Position> positions = Core.Instance.Positions.Where(x => x.Symbol == this.symbol && x.Account == this.account).ToList();

            foreach (Position position in positions)
            {
                switch (position.Comment)
                {
                    case "Cover":
                        Covers.Positions.Add(position);
                        break;

                    case "Main":
                        MainTrades.Positions.Add(position);
                        break;

                    default:
                        Core.Instance.Loggers.Log("Errore nel determinare lo status del HeadgeManager", loggingLevel: LoggingLevel.Trading);
                        break;
                }
            }

            List<Order> orders = Core.Instance.Orders.Where(x => x.Symbol == this.symbol && x.Account == this.account).ToList();

            foreach (Order order in orders)
            {
                switch (order.Comment)
                {
                    case "Cover":
                        Covers.Orders.Add(order);
                        break;
                    case "Main":
                        MainTrades.Orders.Add(order);
                        break;
                    default:
                        Core.Instance.Loggers.Log("Errore nel determinare lo status del HeadgeManager", loggingLevel: LoggingLevel.Trading);
                        break;
                }
            }
        }

       
    }
}
