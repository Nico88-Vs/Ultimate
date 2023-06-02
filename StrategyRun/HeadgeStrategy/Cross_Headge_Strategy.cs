using StrategyRun.Class_Lybrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;


namespace StrategyRun.HeadgeStrategyi
{
    public class Cross_Headge_Strategy : IHeadgeStrategy
    {
        public enum Position_Order_Trade { position, order}
        public HadgedPositions Mains { get; set; }
        public HadgedPositions Cover { get; set; }
        public double Covers_Percent { get; set; }
        public IHeadgeStrategy.HeadgeStatus Status { get; set; } = IHeadgeStrategy.HeadgeStatus.Waiting;
        public string Name { get; set; } = "HeadgeManager_Uno";
        public string Description { get; set; } = "Non Implementa Multi Orders";
        public double TotalAmmount { get; set; }
        public TradeTiket LastTiket { get; set; }
        public CloudSeries Serie { get; set; }

        private bool closerunning;
        private double last_Close;

        public Cross_Headge_Strategy(double totalAmm , CloudSeries serie)
        {
            this.TotalAmmount = totalAmm;
            Mains = new HadgedPositions();
            Cover = new HadgedPositions();

            this.Serie = serie;
        }

        // Punto di paretenza per tutti i metodi
        public void EvaluateTiket(TradeTiket t)
        {
            //resetto il bool per la verifica del main runner;
            closerunning = false;

            LastTiket = t;
            EvaluateStatus();

            List<double> entrilvl = FindEntryLvl(this.Serie.MidInSlowDictionary, t.TradeSentiment);

            if (entrilvl.Contains(0))
                ReactNoLvl(entrilvl, t);

            MenageOrders(entrilvl);

        }

        /// <summary>
        /// Modificato sulla lista
        /// </summary>
        private void EvaluateStatus()
        {
            if (Cover.Orders.Any() && Mains.Orders.Any())
            {
                bool filledCover = Cover.Posizioni.Any(); // Cover.Orders.Last().Status != OrderStatus.Opened ? true : false;
                bool filledmain = Mains.Posizioni.Any(); // Mains.Orders.Last().Status != OrderStatus.Opened ? true : false;

                if (filledmain)
                {
                    if (filledCover)
                        this.Status = IHeadgeStrategy.HeadgeStatus.FullyExpo;
                    else
                        this.Status = IHeadgeStrategy.HeadgeStatus.Exposed;
                }
                else if (!filledmain)
                {
                    if (!filledCover)
                        this.Status = IHeadgeStrategy.HeadgeStatus.Waiting;
                    else
                        this.Status = IHeadgeStrategy.HeadgeStatus.Covered;
                }
            }
            else 
                this.Status = IHeadgeStrategy.HeadgeStatus.Waiting;
           

            #region old
            //double main_expositions = Core.Instance.Positions.Where(x => x.Comment == IHeadgeStrategy.mainComment).ToList().Sum(x => x.Quantity);
            //double covers_expositions = Core.Instance.Positions.Where(x => x.Comment == IHeadgeStrategy.coverComment).ToList().Sum(x => x.Quantity);

            ////double ceking expositions 
            //double main_expositions_list = Mains.Posizioni.Where(x => x.Comment == IHeadgeStrategy.mainComment).ToList().Sum(x => x.Quantity);
            //double covers_expositions_list = Cover.Posizioni.Where(x => x.Comment == IHeadgeStrategy.coverComment).ToList().Sum(x => x.Quantity);

            //if (main_expositions != main_expositions_list || covers_expositions != covers_expositions_list)
            //    Log("List Orders or Positions Dont Match", LoggingLevel.Trading);

            //double expositions = Math.Abs(main_expositions_list) - Math.Abs(covers_expositions_list);

            //if(!Cover.Posizioni.Any())
            //{
            //    if(Mains.Posizioni.Any())
            //        this.Status = IHeadgeStrategy.HeadgeStatus.Exposed;
            //    else this.Status = IHeadgeStrategy.HeadgeStatus.Waiting;
            //}
            //else
            //{
            //    if(!Mains.Posizioni.Any())
            //        this.Status = IHeadgeStrategy.HeadgeStatus.Covered;
            //    else this.Status = IHeadgeStrategy.HeadgeStatus.FullyExpo;
            //}
            #endregion
        }
        private void ReactNoLvl(List<double> entrilvl, TradeTiket t)
        {
            if (entrilvl[1] == 0)
                this.Serie.Cross += this.Serie_Cross_Cover;

            if (entrilvl[0] == 0)
                this.Serie.Cross += this.Serie_Cross_Main;

        }
        private void MenageOrders(List<double> list)
        {
            Position main = null;
            Position cover = null;

            //Tento lo shift in base allo status
            switch (this.Status)
            {
                case IHeadgeStrategy.HeadgeStatus.Waiting:
                    if(Cover.Orders.Any())
                        Core.Instance.CancelOrder(Cover.Orders.Last());
                    if(Mains.Orders.Any())
                        Core.Instance.CancelOrder(Mains.Orders.Last());

                    //Apro Nuove Posizioni
                    if (list[0] != 0)
                        OpenOrder(list[0], LastTiket.TradeSentiment, IHeadgeStrategy.mainComment);
                    if (list[1] != 0)
                        OpenOrder(list[1], LastTiket.TradeSentiment, IHeadgeStrategy.coverComment);
                    break;

                case IHeadgeStrategy.HeadgeStatus.Exposed:
                    Core.Instance.CancelOrder(Cover.Orders.Last());
                    main = Mains.Posizioni.Last();
                    Mains.Posizioni.Remove(main);
                    Cover.Posizioni.Add(main);
                    Cover.Orders.Add(Mains.Orders.Last());
                    Mains.Orders.Remove(Mains.Orders.Last());

                    //Apro e chiudo posizioni
                    if (list[0] != 0)
                        OpenOrderTP(list[0], Cover.Orders.Last());
                    else if (list[0] == 0)
                        closerunning = true;

                    if (list[1] != 0)
                        OpenOrder(list[1], LastTiket.TradeSentiment, IHeadgeStrategy.coverComment);
                    break;

                case IHeadgeStrategy.HeadgeStatus.Covered:
                    Core.Instance.CancelOrder(Mains.Orders.Last());
                    cover = Cover.Posizioni.Last();
                    Cover.Posizioni.Remove(cover);
                    Mains.Posizioni.Add(cover);
                    Mains.Orders.Add(Cover.Orders.Last());
                    Cover.Orders.Remove(Cover.Orders.Last());

                    if (list[1] != 0)
                        OpenOrder(list[1], LastTiket.TradeSentiment, IHeadgeStrategy.coverComment);
                    break;

                case IHeadgeStrategy.HeadgeStatus.FullyExpo:
                    cover = Cover.Posizioni.Last();
                    main = Mains.Posizioni.Last();
                    Order mainorder = Mains.Orders.Last();
                    Order coverorder = Cover.Orders.Last();
                    Mains.Posizioni[Mains.Posizioni.Count-1] = cover;
                    Cover.Posizioni[Cover.Posizioni.Count - 1] = main;
                    Mains.Orders[Mains.Orders.Count - 1] = coverorder;
                    Cover.Orders[Cover.Orders.Count - 1] = mainorder;

                    if (list[0] != 0)
                        OpenOrderTP(list[0], Cover.Orders.Last());
                    else
                        closerunning = true;
                    break;
            }

            #region// shifto in caso di posizioni aperte
            //if (Cover.Orders.Any() || Mains.Orders.Any())
            //{
            //    if (Cover.Orders.Last().Status == OrderStatus.Opened)
            //        Core.Instance.CancelOrder(Cover.Orders.Last());

            //    if (Mains.Orders.Last().Status == OrderStatus.Opened)
            //        Core.Instance.CancelOrder(Mains.Orders.Last());

            //    bool thersMain = Mains.Posizioni.Any() ? true : false;
            //    bool thersCover = Cover.Posizioni.Any() ? true : false;

            //    if (thersCover)
            //        cover = Cover.Posizioni.Last();

            //    if (thersMain)
            //        main = Mains.Posizioni.Last();

            //    if (main != null)
            //        Cover.Posizioni[Cover.Posizioni.Count - 1] = main;

            //    if (cover != null)
            //        Mains.Posizioni[Mains.Posizioni.Count - 1] = cover;
            //}
            #endregion

            #region// ci sono posizioni aperte
            //if (Mains.Posizioni.Count == 0 || list[0] != 0)
            //    OpenOrder(list[0], LastTiket.TradeSentiment, IHeadgeStrategy.mainComment);
            
            //if(Cover.Posizioni.Count == 0 || list[1] != 0)
            //{
            //    Sentiment oppo = LastTiket.TradeSentiment == Sentiment.Buy ? Sentiment.Sell : Sentiment.Buy;
            //    OpenOrder(list[1], oppo, IHeadgeStrategy.coverComment);
            //}
            //else if (Cover.Posizioni.Count == 0 || list[1] == 0)
            //{
            //    closerunning = true;
            //}
            #endregion


        }
        private List<double> FindEntryLvl(Dictionary<Cloud, List<Cloud>> dict, Sentiment s)
        {
            List<double> result = new List<double> { 0, 0 };

            CloudColor color = s == Sentiment.Sell? CloudColor.green : CloudColor.red;

            Cloud lastSlow = dict.Keys.Where(x=> x.Color == color).Last();
            Cloud pre_lastSlow = dict.Keys.Where(x => x.Color == color || x.Id < lastSlow.Id).Last();
            //Cek log
            Log($"LastSlowID {lastSlow.Id}", LoggingLevel.Trading);

            //verifico se usare pre
            bool usepre =  lastSlow.Length <= pre_lastSlow.Length*0.3334 ? true : false;

            if (!usepre)
            {
                double top = 0;
                double bottom = 0;
                try
                {
                    double v = dict[lastSlow].OrderByDescending(x => x.OriginPrice).First().OriginPrice;
                    top = v > last_Close ? v : 0;

                } catch { }

                try
                {
                    double val = dict[lastSlow].OrderBy(x => x.OriginPrice).First().OriginPrice;
                    bottom = val < last_Close ? val : 0;

                } catch { }


                switch (s)
                {
                    case Sentiment.Buy:
                        result[0] = bottom;
                        result[1] = top;
                        break;

                    case Sentiment.Sell:
                        result[0] = top;
                        result[1] = bottom;
                        break;

                        case Sentiment.Wait:
                        return result;
                }

                return result;
            }

            else
            {
                double top = 0;
                double bottom = 0;
                try
                {
                    top = dict[pre_lastSlow].OrderByDescending(x => x.OriginPrice).First().OriginPrice;
                }
                catch { }

                try
                {
                    bottom = dict[pre_lastSlow].OrderBy(x => x.OriginPrice).First().OriginPrice;
                }
                catch { }


                switch (s)
                {
                    case Sentiment.Buy:
                        result[0] = bottom;
                        result[1] = top;
                        break;

                    case Sentiment.Sell:
                        result[0] = top;
                        result[1] = bottom;
                        break;

                    case Sentiment.Wait:
                        return result;
                }

                return result;
            }


            #region old
            //double output = 0;
            //switch(s)
            //{
            //    case Sentiment.Buy:
            //        output = double.MaxValue;
            //        break;
            //    case Sentiment.Sell:
            //        output = double.MinValue;
            //        break;
            //    case Sentiment.Wait:
            //        Log("Wrong Sentiment", LoggingLevel.Trading);
            //        break;
            //}

            //foreach(List<Cloud> item in dict.Values)
            //{
            //    foreach(Cloud c in item)
            //    {
            //        switch(s)
            //        {
            //            case Sentiment.Buy:
            //                if (c.OriginPrice < output)
            //                    output = c.OriginPrice;
            //                break;

            //            case Sentiment.Sell:
            //                if (c.OriginPrice > output)
            //                    output = c.OriginPrice;
            //                break;

            //            case Sentiment.Wait:
            //                Log("Wrong Sentiment", LoggingLevel.Trading);
            //                break;
            //        }
            //    }
            //}

            //return output;
            #endregion
        }
        public void SetCovers(PlaceOrderRequestParameters p, TradeTiket t)
        {

            double openPrice = t.Dyctionary.Keys.Last(x=> x.Id == t.ClaudId-1).OriginPrice;
            double slPrice = t.TradeSentiment == Sentiment.Buy ? 1.01 * openPrice : 0.99 * openPrice;

            SlTpHolder sl = SlTpHolder.CreateSL(slPrice);

            var placeOrderReq_sl = new PlaceOrderRequestParameters()
            {
                Price = openPrice,
                Quantity = TotalAmmount / 2,
                Comment = IHeadgeStrategy.coverComment,
                OrderTypeId = OrderType.Limit,
                Side = t.TradeSentiment == Sentiment.Buy ? Side.Sell : Side.Buy,
                Symbol = t.Symbol,
                Account = t.Account,
                TimeInForce = TimeInForce.GTC,
                StopLoss = sl,
            };

            var place = Core.Instance.PlaceOrder(placeOrderReq_sl);
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            while (place.Status == TradingOperationResultStatus.Failure)
            {
                place = Core.Instance.PlaceOrder(placeOrderReq_sl);
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
        }
        public void ModifyOrder(double lastclose)
        {

            //if (Mains.Trades.Count > 1)
            //    return;

            //Position p = Mains.Trades.Last();

            //string orderId = GetId(Position_Order_Trade.order, p);
            //Order o = GetOrder(orderId);
            //double slPrice = 0;

            //if (p.OpenPrice == p.StopLoss.Price)
            //    return;

            //Cloud c = LastTiket.Dyctionary.Keys.Last(x => x.Id == LastTiket.ClaudId -1);

            //double min = double.MaxValue;

            //switch (p.Side)
            //{
            //    case Side.Buy:
            //        min = c.MinimaFast.OrderBy(x=> x.Value).First().Value;
            //        if (lastclose <= min)
            //            return;
            //        if(min > p.OpenPrice)
            //            slPrice = p.OpenPrice;
                        
            //        break;
            //    case Side.Sell:
            //        min = c.MaximaFast.OrderByDescending(x => x.Value).First().Value;
            //        if (lastclose <= min)
            //            return;
            //        if(min < p.OpenPrice)
            //            slPrice = p.OpenPrice;
            //        break;
            //}

            //if(slPrice > 0)
            //{
            //    SlTpHolder sl = SlTpHolder.CreateSL(slPrice);
            //    ModifyOrderRequestParameters modifyOrderReq = new ModifyOrderRequestParameters(o)
            //    {
            //        StopLoss = sl
            //    };
            //    var modify = Core.Instance.ModifyOrder(modifyOrderReq);
            //    while (modify.Status != TradingOperationResultStatus.Success)
            //        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            //}

        }
        private void OpenOrder(double lvel, Sentiment sent, string constan)
        {
            if (sent == Sentiment.Wait)
                return;

            if (constan == IHeadgeStrategy.mainComment)
                if (Mains.Posizioni.Any())
                    return;

            if (constan == IHeadgeStrategy.coverComment)
                if (Cover.Posizioni.Any())
                    return;

            Sentiment s;

            switch (constan)
            {
                case IHeadgeStrategy.coverComment:
                    s = sent == Sentiment.Buy ? Sentiment.Sell : Sentiment.Buy;
                    break;

                case IHeadgeStrategy.mainComment:
                    s = sent;
                    break;

                default:
                    Log("INvalid comment on open position", lvl: LoggingLevel.Trading);
                    return;
            }

            double slPrice = s == Sentiment.Buy ? 0.99 * lvel : 1.01 * lvel;

            SlTpHolder sl = SlTpHolder.CreateSL(slPrice);

            var placeOrderReq = new PlaceOrderRequestParameters()
            {
                Price = lvel,
                Quantity = TotalAmmount / 2,
                GroupId = constan,
                OrderTypeId = OrderType.Limit,
                Side = s == Sentiment.Buy ? Side.Buy : Side.Sell,
                Symbol = LastTiket.Symbol,
                Account = LastTiket.Account,
                TimeInForce = TimeInForce.GTC,
                StopLoss = sl,
            };

            var place = Core.Instance.PlaceOrder(placeOrderReq);


            while (place.Status != TradingOperationResultStatus.Success)
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }
        private void OpenOrderTP(double lvel, Order closingOrder)
        {
            if (closingOrder.Comment != IHeadgeStrategy.mainComment)
                return;

            Order o;

            try
            {
                o = Mains.Orders.Where(x => x.Id == closingOrder.Id).First();
                if (o.Side == Side.Buy)
                    if (o.StopLoss.Price > lvel)
                        return;

                if (o.Side == Side.Sell)
                    if (o.StopLoss.Price < lvel)
                        return;
            }
            catch
            {
                return;
            }

            SlTpHolder tp = SlTpHolder.CreateTP(lvel);

            var placeOrderReq = new ModifyOrderRequestParameters(order: closingOrder)
            {
                TakeProfit = tp
            };

            var place = Core.Instance.ModifyOrder(placeOrderReq);

            while (place.Status != TradingOperationResultStatus.Success)
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }
        private void Serie_Cross_Main(object sender, CrossEvent e)
        {
            if (e.Args == EventCrosArg.Gold_midt || e.Args == EventCrosArg.Dead_mid)
            {
                switch (e.Args)
                {
                    case EventCrosArg.Gold_midt:
                        if (LastTiket.TradeSentiment == Sentiment.Buy)
                            OpenOrder(e.Price, LastTiket.TradeSentiment, IHeadgeStrategy.mainComment);
                        break;
                    case EventCrosArg.Dead_mid:
                        if (LastTiket.TradeSentiment == Sentiment.Sell)
                            OpenOrder(e.Price, LastTiket.TradeSentiment, IHeadgeStrategy.mainComment);
                        break;
                }
                this.Serie.Cross -= this.Serie_Cross_Main;
            }
        }
        private void Serie_Cross_Cover(object sender, CrossEvent e) 
        {
            if (e.Args == EventCrosArg.Gold_midt || e.Args == EventCrosArg.Dead_mid)
            {
                if(!closerunning)
                {
                    switch (e.Args)
                    {
                        case EventCrosArg.Gold_midt:
                            if (LastTiket.TradeSentiment == Sentiment.Sell)
                                OpenOrder(e.Price, LastTiket.TradeSentiment, IHeadgeStrategy.coverComment);
                            break;
                        case EventCrosArg.Dead_mid:
                            if (LastTiket.TradeSentiment == Sentiment.Buy)
                                OpenOrder(e.Price, LastTiket.TradeSentiment, IHeadgeStrategy.coverComment);
                            break;
                    }

                    // Elimino la sottoscrizione all evento in caso siano presenti i livelli                    
                    this.Serie.Cross -= this.Serie_Cross_Cover;

                }
                else
                {
                    Order o;
                    try
                    {
                        o = Cover.Orders.Where(x => x.Id == Cover.Posizioni.Last().Id).First();
                    }
                    catch
                    {
                        Log("Order to close Not Found", LoggingLevel.Trading);
                        return;
                    }

                    switch (e.Args)
                    {
                        
                        case EventCrosArg.Gold_midt:
                            if (LastTiket.TradeSentiment == Sentiment.Sell)
                                OpenOrderTP(e.Price, o);
                            break;
                        case EventCrosArg.Dead_mid:
                            if (LastTiket.TradeSentiment == Sentiment.Buy)
                                OpenOrderTP(e.Price, o);
                            break;
                    }
                    // Elimino la sottoscrizione all evento in caso siano presenti i livelli
                    this.Serie.Cross -= this.Serie_Cross_Cover;
                }
            }
        }
        
        public void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }

        public void CloseOrder() => throw new NotImplementedException();

        //old
        private void OpenOrder(TradeTiket t)
        {
            double openPrice = 22222; // FindEntryLvl(t.Dyctionary, t.TradeSentiment);
            double slPrice = t.TradeSentiment == Sentiment.Buy ? 0.99 * openPrice : 1.01 * openPrice;

            SlTpHolder sl = SlTpHolder.CreateSL(slPrice);

            var placeOrderReq = new PlaceOrderRequestParameters()
            {
                Price = openPrice,
                Quantity = TotalAmmount / 2,
                GroupId = IHeadgeStrategy.mainComment,
                OrderTypeId = OrderType.Limit,
                Side = t.TradeSentiment == Sentiment.Buy ? Side.Buy : Side.Sell,
                Symbol = t.Symbol,
                Account = t.Account,
                TimeInForce = TimeInForce.GTC,
                StopLoss = sl,
            };

            var place = Core.Instance.PlaceOrder(placeOrderReq);


            while (place.Status != TradingOperationResultStatus.Success)
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            SetCovers(placeOrderReq, t);
        }
        public void Update(double lastClose)
        {
            last_Close = lastClose;
        }
    }
}
