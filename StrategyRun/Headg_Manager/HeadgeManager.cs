using System;
using System.Collections.Generic;
using System.Linq;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;
using StrategyRun.Class_Lybrary;

namespace StrategyRun.Headg_Manager
{
    public class HeadgeManager<T , X> where T : ITradeTicket where X : ISentiment 
    {
        private  enum TypeOfPosition { Cover, Main }
        private enum ManagerStatus { WaitingMain, WaitingCovers, OpeningMain, OpeningCovers, Running}

        private Covers Cover { get; set; }
        private MainTrades MainTrade { get; set; }
        private T TiketObj{ get; set; }
        private X SentimentObj { get; set; }
        public bool UseAlgo { get; private set; }
        public double CurrentExposition_Perce { get; private set; }
        public string Name { get; private set; } = "First_Headge_Manager";
        public string Description { get; private set; } = "Non implementa sistemi a piu serie";

        private double totalAmmount;
        private ManagerStatus status;
        private Account account;
        private Symbol symbol;
        private CloudSeries Series;
        private Sentiment sentiment;
        private int tradersId = 0;


        private const string mainComment = "Main";
        private const string coverComment = "Cover";

        public HeadgeManager(Account account, double totalAmmo, T cond, X sentObj, bool usealgo = true)
        {
            this.totalAmmount = totalAmmo;
            this.TiketObj = cond;
            this.Series = cond.Series;
            this.account = account; 
            this.symbol = cond.Series.FastTF.Indicatore.Symbol;
            this.MainTrade = new MainTrades(tradersId);
            this.Cover = new Covers(tradersId);
            this.SentimentObj = sentObj;
            this.sentiment = sentObj.CurrentSent.NewSentiment;

            if(UseAlgo)
                this.SentimentObj.CurrentSent.SentimentChanged += this.CurrentSent_SentimentChanged;

            this.TiketObj.TradeTicketCreated += this.TiketObj_TradeTicketCreated;
            Core.Instance.OrderAdded += this.Instance_OrderAdded;
            Core.Instance.OrderRemoved += this.Instance_OrderRemoved;
            Core.Instance.PositionAdded += this.Instance_PositionAdded;
            Core.Instance.PositionRemoved += this.Instance_PositionRemoved;
        }
        /// <summary>
        /// Chiamare In Strategy Run  ____ Non necessario se funzionano gli eventi
        /// </summary>
        public void Update()
        {
            CalculateExposition();
            SetStatus();
        }

        #region Methods
        //Calculating the total exposition
        private void CalculateExposition()
        {
            double total = 0;
            foreach (var pos in this.MainTrade.Posizioni)
            {
                total += pos.Quantity;
            }
            foreach (var pos in this.Cover.Posizioni)
            {
                total += pos.Quantity;
            }

            this.CurrentExposition_Perce = (total / this.totalAmmount) * 100;
        }

        /// <summary>
        /// Controlla la compatibilita target e sentiment
        /// </summary>
        /// <param Sentiment="currentSentObj"></param>
        private void CekCompatibility()
        {

        }

        /// <summary>
        /// Swich Structure Status
        /// </summary>
        /// <param Status="StructStatus"></param>
        private void SwichStruct()
        {

        }

        /// <summary>
        /// Switch Current Status
        /// </summary>
        /// <param StructList="Posizioni"></param>
        private void SetStatus()
        {
            if(this.MainTrade.Posizioni.Count == 0 && this.Cover.Posizioni.Count == 0)
            {
                if(this.MainTrade.Orders.Count == 0 && this.MainTrade.CurrenTick == null)
                    this.status = ManagerStatus.WaitingMain;
                else
                    this.status = ManagerStatus.OpeningMain;
            }
            else if(this.MainTrade.Posizioni.Count > 0 && this.Cover.Posizioni.Count == 0)
            {
                if(this.Cover.Orders.Count == 0 && this.Cover.CurrenTick == null)
                    this.status = ManagerStatus.WaitingCovers;
                else
                    this.status = ManagerStatus.OpeningCovers;

            }
            else if(this.MainTrade.Posizioni.Count > 0 && this.Cover.Posizioni.Count > 0)
            {
               this.status = ManagerStatus.Running;
            }
            else
                Log("We Are Exposed Only With Covers", LoggingLevel.Trading);
        }
        #endregion

        #region Event Subscriptions
        private void Instance_PositionRemoved(Position obj)
        {
            this.RemoveTradingObj(obj);
        }
        private void Instance_PositionAdded(Position obj)
        {
            this.AddTradingObj(obj);
        }

        private void Instance_OrderRemoved(Order obj)
        {
            this.RemoveTradingObj(obj);
        }

        private void Instance_OrderAdded(Order obj)
        {
            this.AddTradingObj(obj);
        }

        private void TiketObj_TradeTicketCreated(object sender, TradeTiket e)
        {
            if (e.TradeSentiment == Sentiment.Wait)
                return;

            if(this.status == ManagerStatus.WaitingMain)
            {
                if(e.TFrame.Timeframe != TF.TimeFrame.Slow && e.TradeSentiment == sentiment)
                {
                    if(MainTrade.CurrenTick == null)
                        MainTrade.CurrenTick = e;

                    else if(MainTrade.CurrenTick != e)
                    {
                        MainTrade.TiketsList.Add(MainTrade.CurrenTick);
                        MainTrade.CurrenTick = e;
                    }
                }
            }
            else if(this.status == ManagerStatus.WaitingCovers)
            {
                if(e.TFrame.Timeframe == TF.TimeFrame.Slow && e.TradeSentiment != sentiment)
                {
                    if(Cover.CurrenTick == null)
                        Cover.CurrenTick = e;

                    else if (Cover.CurrenTick != e)
                    {
                        Cover.TiketsList.Add(Cover.CurrenTick);
                        Cover.CurrenTick = e;
                    }
                }
            }
        }

        private void CurrentSent_SentimentChanged(object sender, SwitchSentiment e)
        {
            this.sentiment = e.NewSentiment;
        }
        #endregion

        #region Service
        private void AddTradingObj(object tradeObj)
        {
            if(tradeObj == null)
            {
                Log("Null Objecet in FindTradingObj", LoggingLevel.Trading);
                return;
            }

            if(tradeObj is Position)
            {
                Position pos = (Position)tradeObj;
                if(pos.Comment == mainComment)
                {
                    if(!this.MainTrade.Posizioni.Contains(pos))
                        this.MainTrade.Posizioni.Add(pos);
                    else
                        Log("Position Already Exist", LoggingLevel.Trading);
                }
                else if(pos.Comment == coverComment)
                {
                    if(!this.Cover.Posizioni.Contains(pos))
                        this.Cover.Posizioni.Add(pos);
                    else
                        Log("Position Already Exist", LoggingLevel.Trading);
                }
                else
                {
                    Log("Incoerent Comment", LoggingLevel.Trading);
                }
            }
            else if(tradeObj is Order)
            {
                Order ord = (Order)tradeObj;
                if(ord.Comment == mainComment)
                {
                    if(!this.MainTrade.Orders.Contains(ord))
                        this.MainTrade.Orders.Add(ord);
                    else
                        Log("Order Already Exist", LoggingLevel.Trading);
                }
                else if(ord.Comment == coverComment)
                {
                    if(!this.Cover.Orders.Contains(ord))
                        this.Cover.Orders.Add(ord);
                    else
                        Log("Order Already Exist", LoggingLevel.Trading);
                }
                else
                {
                    Log("Incoerent Comment", LoggingLevel.Trading);
                }
            }
            else
            {
                Log("Incoerent Type", LoggingLevel.Trading);
            }   
        }
        private void RemoveTradingObj(object tradeObj)
        {
            if(tradeObj == null)
            {
                Log("Null Objecet in FindTradingObj", LoggingLevel.Trading);
                return;
            }
            if(tradeObj is Position)
            {
                Position pos = (Position)tradeObj;
                if(pos.Comment == mainComment)
                {
                    if(this.MainTrade.Posizioni.Contains(pos))
                        this.MainTrade.Posizioni.Remove(pos);
                    else
                        Log("Position Not Exist", LoggingLevel.Trading);
                }
                else if(pos.Comment == coverComment)
                {
                    if(this.Cover.Posizioni.Contains(pos))
                        this.Cover.Posizioni.Remove(pos);
                    else
                        Log("Position Not Exist", LoggingLevel.Trading);
                }
                else
                {
                    Log("Incoerent Comment", LoggingLevel.Trading);
                }
            }
            else if(tradeObj is Order)
            {
                Order ord = (Order)tradeObj;
                if(ord.Comment == mainComment)
                {
                    if(this.MainTrade.Orders.Contains(ord))
                        this.MainTrade.Orders.Remove(ord);
                    else
                        Log("Order Not Exist", LoggingLevel.Trading);
                }
                else if(ord.Comment == coverComment)
                {
                    if(this.Cover.Orders.Contains(ord))
                        this.Cover.Orders.Remove(ord);
                    else
                        Log("Order Not Exist", LoggingLevel.Trading);
                }
                else
                {
                    Log("Incoerent Comment", LoggingLevel.Trading);
                }
            }
            else
            {
                Log("Incoerent Type", LoggingLevel.Trading);
            }   
        }   
        public void ActiveAlgo()
        {
            if (UseAlgo)
                Log("Manager arlady use AlgoSentiment", LoggingLevel.Trading);
            else
            {
                this.UseAlgo = true;
                this.SentimentObj.CurrentSent.SentimentChanged += this.CurrentSent_SentimentChanged;    
            }
        }
        public void DectiveAlgo()
        {
            if (!UseAlgo)
                Log("Manager arlady use DiscretionarySentiment", LoggingLevel.Trading);
            else
            {
                this.UseAlgo = false;
                this.SentimentObj.CurrentSent.SentimentChanged -= this.CurrentSent_SentimentChanged;
            }
        }
        private void Log(string txt, LoggingLevel lgLvl)
        {
            Core.Instance.Loggers.Log(txt, lgLvl);
        }
        #endregion

        #region Sub_Classes
        public class MainTrades : IPosizioni
        {
            public List<Position> Posizioni { get; set; }
            public List<Order> Orders { get; set; }
            public int Id { get; set; }
            public IPosizioni.IPosi_Status Status { get; set; } = IPosizioni.IPosi_Status.Waiting;
            public List<TradeTiket> TiketsList { get; set; }
            public TradeTiket CurrenTick { get; set; }

            public const string Name = "Basic_Main_Trader";


            public MainTrades(int id)
            {
                this.Id = id;
                this.Posizioni = new List<Position>();
                this.Orders = new List<Order>();
                this.TiketsList = new List<TradeTiket>();
            }

            public List<double> GetPrice(Cloud cloud) => throw new NotImplementedException();
            public RequestParameters GetRequestParameters(List<double> targets) => throw new NotImplementedException();
            public bool Buy_Sel() => throw new NotImplementedException();
        }

        internal class Covers
        {
            public List<Position> Posizioni { get; set; }
            public List<Order> Orders { get; set; }
            public int Id { get; set; }
            public IPosizioni.IPosi_Status Status { get; set; } = IPosizioni.IPosi_Status.Waiting;
            public List<TradeTiket> TiketsList { get; set; }
            public TradeTiket CurrenTick { get; set; }

            public const string Name = "Basic_Cover_Trader";


            public Covers(int id)
            {
                this.Id = id;
                this.Posizioni = new List<Position>();
                this.Orders = new List<Order>();
                this.TiketsList = new List<TradeTiket>();
            }

            public List<double> GetPrice(Cloud cloud) => throw new NotImplementedException();
            public RequestParameters GetRequestParameters(List<double> targets) => throw new NotImplementedException();
            public bool Buy_Sel() => throw new NotImplementedException();
        }
        #endregion
    }
}
