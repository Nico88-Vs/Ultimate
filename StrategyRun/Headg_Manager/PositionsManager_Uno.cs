//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using StrategyRun.Class_Lybrary;
//using TheIndicator.Enum;
//using TheIndicator.LibreriaDiClassi;
//using TradingPlatform.BusinessLayer;

//namespace StrategyRun.Headg_Manager
//{
//    public class PositionsManager_Uno : IPositionManager<ISentiment, ITradeTicket, IHeadgeStrategy>
//    {
//        public ISentiment SentimentObj { get; set; }
//        public List<ITradeTicket> TradeTickets { get; set; }
//        public IHeadgeStrategy HeadgeStrategy { get; set; }
//        public CloudSeries Series { get; set; }
//        public Sentiment Sentimento { get; set; } = Sentiment.Wait;
//        public List<TradeTiket> CurrenTiket { get; set; }
//        public ManagerStatus Status { get; set; } = ManagerStatus.Waiting;

//        private const string name = "Primo Gestore Trades";
//        private const string description = "Utilizza Solo Le Trades e non gli Ordini";
//        private Account account;
//        private Symbol symbol;

//        public PositionsManager_Uno(ISentiment sent, List<ITradeTicket> tiketlist, IHeadgeStrategy headgeStrategy, CloudSeries serie)
//        {
//            this.SentimentObj = sent;
//            this.TradeTickets = tiketlist;
//            this.HeadgeStrategy = headgeStrategy;
//            this.Series = serie;
//            this.account = serie.FastTF.Indicatore.CurrentChart.Account;
//            this.symbol = serie.FastTF.Indicatore.Symbol;

//            this.SentimentObj.CurrentSent.SentimentChanged += this.CurrentSent_SentimentChanged;
//            foreach (var t in TradeTickets)
//            {
//                t.TradeTicketCreated += this.T_TradeTicketCreated;
//            }

//            this.Series.Cross += this.Series_Cross;
//            Core.Instance.PositionAdded += this.Instance_PositionAdded;
//            Core.Instance.PositionRemoved += this.Instance_PositionRemoved;
//            Core.Instance.OrderAdded += this.Instance_OrderAdded;
//            Core.Instance.OrderRemoved += this.Instance_OrderRemoved;
//        }

//        // Aggiorno Tutto
//        public void Update()
//        {
//            foreach (var t in TradeTickets)
//                t.Update();
//            SentimentObj.SetSentiment();
//        }

//        //Aggiorno le liste oggetti 
//        private void Instance_OrderRemoved(Order obj)
//        {
//            if(HeadgeStrategy.Cover.Orders.Contains(obj))
//                HeadgeStrategy.Cover.Orders.Remove(obj);
//            else if (HeadgeStrategy.Mains.Orders.Contains(obj))
//                HeadgeStrategy.Mains.Orders.Remove(obj);
//        }
//        private void Instance_OrderAdded(Order obj)
//        {
//            if(this.symbol != obj.Symbol)
//                return;
//            if(this.account != obj.Account)
//                return;

//            if(obj.Comment == IHeadgeStrategy.mainComment)
//                HeadgeStrategy.Mains.Orders.Add(obj);

//            else if (obj.Comment == IHeadgeStrategy.coverComment)
//                HeadgeStrategy.Cover.Orders.Add(obj);
//        }

//        // Setto lo status e reagisco
//        private void T_TradeTicketCreated(object sender, TradeTiket e)
//        {
//            ITradeTicket send = sender as ITradeTicket;

//            if (CurrenTiket.Contains(e))
//                return;

//            CurrenTiket.Add(e);

//            if (e.TradeSentiment == this.Sentimento)
//                e.Comment = IHeadgeStrategy.mainComment;
//            else
//                e.Comment = IHeadgeStrategy.coverComment;

//            switch (this.Status)
//            {
//                case ManagerStatus.Waiting:
//                    if (send.CurrentTF != TF.TimeFrame.Slow)
//                        return;
//                    else if (e.Comment == IHeadgeStrategy.mainComment)
//                        this.Status = ManagerStatus.WaitingMain;
//                    break;

//                    // Entra solo su TF inferiori
//                case ManagerStatus.WaitingMain:
//                    if(send.CurrentTF == TF.TimeFrame.Slow)
//                        return;
//                    else if (e.Comment == IHeadgeStrategy.mainComment)
//                        SendTick(e);
//                    break;

//                case ManagerStatus.WaitingCovers:
//                    if(send.CurrentTF != TF.TimeFrame.Slow)
//                        SendTick(e);
//                    break;

//                case ManagerStatus.Running:
//                    break;
//            }
//        }

//        // Setto il sentimento
//        private void CurrentSent_SentimentChanged(object sender, TheIndicator.Enum.SwitchSentiment e) => this.Sentimento = e.NewSentiment;

//        // Setto lo status sul cross slow
//        private void Series_Cross(object sender, CrossEvent e)
//        {
//            if(this.Status != ManagerStatus.Waiting)
//                return;

//            if(e.Args == EventCrosArg.Dead_slow && this.Sentimento == Sentiment.Sell)
//                this.Status = ManagerStatus.WaitingMain;

//            if(e.Args == EventCrosArg.Gold_slow && this.Sentimento == Sentiment.Buy)
//                this.Status = ManagerStatus.WaitingMain;
//        }

//        // Setto lo status sulle posizioni aperte
//        private void Instance_PositionAdded(Position obj)
//        {
//            //Filling the list
//            if(obj.Symbol != this.symbol || obj.Account != this.account)
//                return;

//            if(obj.Comment == IHeadgeStrategy.mainComment)
//                HeadgeStrategy.Mains.Trades.Add(obj);

//            else if (obj.Comment == IHeadgeStrategy.coverComment)
//                HeadgeStrategy.Cover.Trades.Add(obj);

//            if(this.Status == ManagerStatus.WaitingMain)
//            {
//                if (obj.Comment == IHeadgeStrategy.mainComment)
//                    this.Status = ManagerStatus.WaitingCovers;
//                else
//                    Log("Wrong Comment", LoggingLevel.Trading);
//            }
                
//            if(this.Status == ManagerStatus.WaitingCovers)
//            {
//                double mainExposition = -1;
//                double coverExposition = -1;

//                foreach(var p in Core.Instance.Positions)
//                {
//                    if (p.Comment == IHeadgeStrategy.mainComment)
//                        mainExposition += Math.Abs(p.Quantity);
//                    else if (p.Comment == IHeadgeStrategy.coverComment)
//                        coverExposition += Math.Abs(p.Quantity);
//                }

//                if (mainExposition == coverExposition) 
//                    this.Status = ManagerStatus.Running;
//                else if(mainExposition < coverExposition)
//                {
//                    this.Status = ManagerStatus.Running;
//                    Log("Main Exposition < Cover Exposition", LoggingLevel.Trading);
//                }
//            }
//        }

//        // Setto lo status sulle posizioni chiuse
//        private void Instance_PositionRemoved(Position obj)
//        {
//            if(obj.Symbol != this.symbol || obj.Account != this.account)
//                return;

//            if(obj.Comment == IHeadgeStrategy.mainComment)
//                HeadgeStrategy.Mains.Trades.Remove(obj);

//            else if (obj.Comment == IHeadgeStrategy.coverComment)
//                HeadgeStrategy.Cover.Trades.Remove(obj);

//            if (this.Status != ManagerStatus.Running)
//                return;

//            int conto = 0;

//            foreach(var p in Core.Instance.Positions)
//            {
//                if (p.Comment == IHeadgeStrategy.mainComment)
//                    conto++;
//                else if (p.Comment == IHeadgeStrategy.coverComment)
//                    conto++;
//            }

//            if(conto == 0)
//                this.Status = ManagerStatus.Waiting;
//        }   

//        // Invio i Tick
//        private void SendTick(TradeTiket t)
//        {
//            switch (t.Comment)
//            {
//                case IHeadgeStrategy.mainComment:
//                    break;

//                case IHeadgeStrategy.coverComment:
//                    break;

//                default:
//                    Log("Wrong Comment", LoggingLevel.Trading);
//                    break;
//            }
//        }

//        private void Log(string message, LoggingLevel lvl)
//        {
//            Core.Instance.Loggers.Log(message, lvl);
//        }

//    }
//}
