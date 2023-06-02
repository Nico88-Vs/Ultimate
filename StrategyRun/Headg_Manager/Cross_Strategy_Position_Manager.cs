using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrategyRun.Class_Lybrary;
using StrategyRun.HeadgeStrategy;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Headg_Manager
{
    public class Cross_Strategy_Position_Manager : IPositionManager<ISentiment, ITradeTicket, IHeadgeStrategy>
    {
        public Sentiment Sentimento { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISentiment SentimentObj { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<ITradeTicket> TradeTickets { get; set; }
        public IHeadgeStrategy HeadgeStrategy { get; set; }
        public CloudSeries Series { get; set; }
        public List<TradeTiket> CurrenTiket { get; set; }
        public ManagerStatus Status { get; set; } = ManagerStatus.Waiting;
        public List<Trade> Debug_trades { get; set; }

        private const string name = "Gestore Trades Per CrosStry";
        private const string description = "Inserisce ordini al cross";
        private Account account;
        private Symbol symbol;

        /// <summary>
        /// Calcolo I Target per i tick in entrate
        /// </summary>
        /// <param name="sent"></param>
        /// <param name="tiketlist"></param>
        /// <param name="headgeStrategy"></param>
        /// <param name="serie"></param>
        public Cross_Strategy_Position_Manager(List<ITradeTicket> tiketlist, IHeadgeStrategy headgeStrategy, CloudSeries serie, Account a )
        {
            this.Debug_trades = new List<Trade>();
            this.TradeTickets = tiketlist;
            this.HeadgeStrategy = headgeStrategy;
            this.Series = serie;
            this.account = a;
            this.symbol = serie.FastTF.Indicatore.Symbol;

            foreach (var t in TradeTickets)
            {
                t.TradeTicketCreated += this.T_TradeTicketCreated;
            }

            Core.Instance.OrderAdded += this.Instance_OrderAdded;
            Core.Instance.PositionAdded += this.Instance_PositionAdded;
            Core.Instance.PositionRemoved += this.Instance_PositionRemoved;
            Core.Instance.OrderRemoved += this.Instance_OrderRemoved;
        }

        private void Instance_OrderRemoved(Order obj)
        {
            if(HeadgeStrategy.Mains.Orders.Contains(obj))
                HeadgeStrategy.Mains.Orders.Remove(obj);

            if (HeadgeStrategy.Cover.Orders.Contains(obj))
                HeadgeStrategy.Cover.Orders.Remove(obj);
        }

        private void Instance_PositionRemoved(Position obj)
        {
            if (HeadgeStrategy.Cover.Posizioni.Contains(obj))
                HeadgeStrategy.Cover.Posizioni.Remove(obj);

            if (HeadgeStrategy.Mains.Posizioni.Contains(obj))
                HeadgeStrategy.Mains.Posizioni.Remove(obj);
        }

        private void Instance_PositionAdded(Position obj)
        {
            if (obj.Account != this.account || obj.Symbol != this.symbol)
                return;

            foreach(Order or in HeadgeStrategy.Mains.Orders)
                if(or.Id == obj.Id)
                    if(!HeadgeStrategy.Mains.Posizioni.Contains(obj))
                        HeadgeStrategy.Mains.Posizioni.Add(obj);

            foreach (Order or in HeadgeStrategy.Cover.Orders)
                if (or.Id == obj.Id)
                    if (or.Id == obj.Id)
                        if (!HeadgeStrategy.Cover.Posizioni.Contains(obj))
                            HeadgeStrategy.Cover.Posizioni.Add(obj);


        }

        //Aggiorno le liste oggetti 
        private void Instance_OrderAdded(Order obj)
        {
            if (this.symbol != obj.Symbol)
                return;
            if (this.account != obj.Account)
                return;

            if (obj.GroupId == IHeadgeStrategy.mainComment)
                HeadgeStrategy.Mains.Orders.Add(obj);

            else if (obj.GroupId == IHeadgeStrategy.coverComment)
                HeadgeStrategy.Cover.Orders.Add(obj);
        }

        public void Stop()
        {
            foreach (var t in TradeTickets)
            {
                t.TradeTicketCreated -= this.T_TradeTicketCreated;
            }
            Core.Instance.OrderAdded -= this.Instance_OrderAdded;
            Core.Instance.PositionRemoved -= this.Instance_PositionRemoved;
            Core.Instance.OrderRemoved -= this.Instance_OrderRemoved;
        }

        public void Strat()
        {
            foreach (var t in TradeTickets)
            {
                t.TradeTicketCreated += this.T_TradeTicketCreated;
            }
            Core.Instance.OrderAdded += this.Instance_OrderAdded;
            Core.Instance.PositionRemoved += this.Instance_PositionRemoved;
            Core.Instance.OrderRemoved += this.Instance_OrderRemoved;
        }


        private void T_TradeTicketCreated(object sender, TradeTiket e)
        {
            e.Symbol = this.symbol;
            e.Account = this.account;
            HeadgeStrategy.LastTiket = e;
            HeadgeStrategy.EvaluateTiket(e);
            
        }

        public void Update() => throw new NotImplementedException();

        private void Log(string message)
        {
            Core.Instance.Loggers.Log(message, LoggingLevel.Trading);
        }

    }
}
