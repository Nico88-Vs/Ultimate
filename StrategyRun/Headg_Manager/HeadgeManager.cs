using System;
using System.Collections.Generic;
using System.Linq;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TheIndicator.Enum;
using StrategyRun.Class_Lybrary;
using TradingPlatform.BusinessLayer.Integration;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace StrategyRun.Headg_Manager
{
    public class HeadgeManager<T , X> where T : ITradeTicket where X : ISentiment 
    {
        private enum ManagerStatus { WaitingMain, WaitingCovers, OpeningMain, OpeningCovers, Running}

        private Covers Cover { get; set; }
        private MainTrades MainTrade { get; set; }
        private List<T> TiketObjList{ get; set; }
        private X SentimentObj { get; set; }
        public bool UseAlgo { get; private set; }
        public double CurrentExposition_Perce { get; private set; }
        public string Name { get; private set; } = "First_Headge_Manager";
        public string Description { get; private set; } = "Non implementa sistemi a piu serie";

        private double totalAmmount;
        private ManagerStatus status;
        private Account account;
        private Symbol symbol;
        public CloudSeries Series;
        private Sentiment sentiment;
        private int tradersId = 0;


        private const string mainComment = IPosizioni.mainComment;
        private const string coverComment = IPosizioni.coverComment;

        public HeadgeManager(Account account, double totalAmmo, List<T> cond, X sentObj, bool usealgo = true)
        {
            this.totalAmmount = totalAmmo;
            this.TiketObjList = cond;
            this.Series = cond[0].Series;
            this.account = account; 
            this.symbol = cond[0].Series.FastTF.Indicatore.Symbol;
            this.MainTrade = new MainTrades(tradersId, Series);
            this.Cover = new Covers(tradersId, Series);
            this.SentimentObj = sentObj;
            this.sentiment = Sentiment.Wait; //sentObj.CurrentSent.NewSentiment;
            this.UseAlgo = usealgo;

            if(UseAlgo)
                this.SentimentObj.CurrentSent.SentimentChanged += this.CurrentSent_SentimentChanged;

            foreach (var item in this.TiketObjList)
            {
                item.TradeTicketCreated += this.TiketObj_TradeTicketCreated;
            }
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
            foreach (var item in this.TiketObjList)
            {
                item.Update();
            }
            SentimentObj.SetSentiment();
            CalculateExposition();
            SetStatus();

            if (MainTrade.CurrenTick != null && this.Series.CurrentCloud.Color == CloudColor.green)
            {
                List<double>[] lvlLists = this.MainTrade.GetIn_Out_Prices(MainTrade.CurrenTick);
                var req = GetRequestParameters(lvlLists);
                Core.Instance.PlaceOrder(req);
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();

                //While debugging 
                this.MainTrade.CurrenTick = null;
            }
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
        private bool CekCompatibility(List<double>[] lvlLists )
        {
            if(lvlLists.Count() != 2)
            {
                this.Log("The Levels List are not compatible with this Headge Manager", LoggingLevel.Trading);
                return false;
            }
            else
            {
                this.Log("Actualy just cek if targets are more than 1% different", LoggingLevel.Trading);

                double inAvarage = lvlLists[0].Average();
                double outAvarage = lvlLists[1].Average();

                if(outAvarage > inAvarage*1.01 || outAvarage < inAvarage*0.99)
                    return true;
                else
                    return false;
            }
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
        /// <param StructList="Trades"></param>
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
            ITradeTicket Mid = TiketObjList.Find(x => x.CurrentTF == TF.TimeFrame.Mid);
            ITradeTicket Slow = TiketObjList.Find(x => x.CurrentTF == TF.TimeFrame.Slow);

            if (e.TradeSentiment == Sentiment.Wait)
            {
                if(e.TradeSentiment == SentimentObj.CurrentSent.NewSentiment && e.TFrame.Timeframe == TF.TimeFrame.Slow)
                {
                    this.status = ManagerStatus.WaitingMain;
                }
            }

            if(this.status == ManagerStatus.WaitingMain && sender.Equals(Mid))
            {
                List<double>[] targets = MainTrade.GetIn_Out_Prices(e);
                if(!CekCompatibility(targets))
                    return;

                if(e.TFrame.Timeframe != TF.TimeFrame.Slow && e.TradeSentiment == sentiment)
                {
                    if(MainTrade.CurrenTick == null)
                        MainTrade.CurrenTick = e;

                    else if(MainTrade.CurrenTick != e)
                    {
                        MainTrade.TiketsList.Add(MainTrade.CurrenTick);

                        double oldEndPrice = IPosizioni.Get_tiketCloud(MainTrade.CurrenTick, Series).EndPrice;
                        double newEndPrice = IPosizioni.Get_tiketCloud(e, Series).EndPrice;

                        if(newEndPrice > oldEndPrice)
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
        // List Operations
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
        // Menge options
        private PlaceOrderRequestParameters GetRequestParameters(List<double>[] targets)
        {
            PlaceOrderRequestParameters rq = new PlaceOrderRequestParameters();
            List<SlTpHolder> tp = new List<SlTpHolder>();
            double slPrice = this.sentiment== Sentiment.Buy ? targets[0].First()*0.995 : targets[0].First()*1.005;

            SlTpHolder sl = SlTpHolder.CreateSL(slPrice, quantityPercentage : 100);

            foreach(var target in targets[1])
            {
                SlTpHolder _Tp = SlTpHolder.CreateTP(target, quantityPercentage : 100 / (totalAmmount / (3 * targets[1].Count)));
                tp.Add(_Tp);
            }

            string orderTypeId = Core.Instance.OrderTypes.FirstOrDefault(x => x.ConnectionId == symbol.ConnectionId && x.Behavior == OrderTypeBehavior.Limit).Id;

            if(MainTrade.CurrenTick != null)
            {
                var request = new PlaceOrderRequestParameters
                {
                    Account = this.account,
                    Symbol = this.symbol,
                    OrderTypeId = orderTypeId,
                    Side = this.sentiment == Sentiment.Buy ? Side.Buy : Side.Sell,
                    Quantity = this.totalAmmount / 3,
                    TakeProfit = tp[0],
                    Price = targets[0].First(),
                    StopLoss = sl,
                    Comment = mainComment
                };

                rq = request;
            }

            return rq;
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
            public CloudSeries Serie { get; set; }

            public const string Name = "Basic_Main_Trader";


            public MainTrades(int id, CloudSeries serie)
            {
                this.Id = id;
                this.Posizioni = new List<Position>();
                this.Orders = new List<Order>();
                this.TiketsList = new List<TradeTiket>();
                this.Serie = serie;
            }
            //Set In and Out Targets
            public List<double>[] GetIn_Out_Prices(TradeTiket tiket)
            {
                List<double>[] prices = new List<double>[2];
                Cloud cloud = IPosizioni.Get_tiketCloud(tiket, this.Serie);

                List<double> In = new List<double>();
                double baseLast = cloud.BasesList.Last().Value;
                double endPrice = cloud.EndPrice;
                In.Add((baseLast + endPrice) / 2);

                List<double> Out = IPosizioni.GetOutLevels_UpperTF(tiket, mainComment, Serie);

                prices[0] = In;
                prices[1] = Out;

                return prices;
            }
           
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
            public CloudSeries Serie { get; set; }


            public const string Name = "Basic_Cover_Trader";


            public Covers(int id, CloudSeries serie)
            {
                this.Id = id;
                this.Posizioni = new List<Position>();
                this.Orders = new List<Order>();
                this.TiketsList = new List<TradeTiket>();
                this.Serie = serie;
            }

            public List<double> GetPrice(Cloud cloud) => throw new NotImplementedException();
            public RequestParameters GetRequestParameters(List<double> targets) => throw new NotImplementedException();
            public bool Buy_Sel() => throw new NotImplementedException();
        }
        #endregion
    }
}
