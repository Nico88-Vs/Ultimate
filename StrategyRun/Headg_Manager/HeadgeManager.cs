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

namespace StrategyRun.Headg_Manager
{
    public class HeadgeManager
    {
        private enum Status { WaitingCover, WaitingMain, FullFilled }
        public Covers Covers { get; set; }
        public MainTrades MainTrades { get; set; }

        private Account account;
        private Symbol symbol;
        private Status status;
        private CloudSeries Series;
        private GetCondiction condiction;
        private Type conType;

        public HeadgeManager(CloudSeries serie, Account account, Symbol symbol, GetCondiction cond)
        {
            this.condiction = cond;
            this.conType = cond.GetType();
            this.Series = serie;
            this.account = account;
            this.symbol = symbol;
            this.MainTrades = new MainTrades(0, cond.Sentiment);
            this.Covers = new Covers(this.MainTrades);
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
