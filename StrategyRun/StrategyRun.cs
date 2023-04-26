// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TheIndicator;
using TheIndicator.LibreriaDiClassi;
using TheIndicator.Enum;
using PlaceOrder;
using System.Threading.Tasks;

namespace StrategyRun
{
    public class StrategyRun : Strategy
    {
        [InputParameter("Symbol", 10)]
        private Symbol symbol;

        [InputParameter("Account", 20)]
        public Account account;

        [InputParameter("Period", 2)]
        private Period PerioBase = Period.MIN1;

        [InputParameter("Multiplaier", 2)]
        private int Multiplaier = 5;

        [InputParameter("Multiplaier_2", 2)]
        private int Multiplaier2 = 30;

        [InputParameter("Start_Data", 2)]
        private DateTime StarrtData;

        [InputParameter("GapFilter", 3)]
        private bool gapFilter = true;

        // Placer Imput________________________
        [InputParameter("NumberClose", 1, increment: 1, maximum: 5)]
        private int numberOfCloses = 3;

        [InputParameter("TP%", 2, increment: 0.001)]
        private double tp = 0.01;

        [InputParameter("SL%", 2, increment: 0.001)]
        private double sl = 0.01;

        [InputParameter("Ammount", 4)]
        private double ammount = 33;

        private List<Trade> trades;
        private List<Position> positions;
        private Indicator Indi;
        private HistoricalData historicalData;
        private int minutesInHd = 0;
        private TF Fast;
        private TF Mid;
        private TF Slow;    
        private Condic_Gap_Cros_Strategy_V1 crosStrategy;
        public CloudSeries Serie;
        private CustomOrderPlacer customPlacer;

        public override string[] MonitoringConnectionsIds => new string[] { this.symbol?.ConnectionId };

        public StrategyRun()
            : base()
        {
            this.Name = "StrategyRun";
            this.Description = "My strategy's annotation";
        }


        protected override void OnCreated()
        {
            int m = this.Multiplaier2 * 53;
            TimeSpan span = PerioBase.Duration * m;
            minutesInHd = span.Minutes;

            this.trades = new List<Trade>();
            this.positions = new List<Position>();
        }

        protected override void OnRun()
        {
            if (symbol == null || account == null || symbol.ConnectionId != account.ConnectionId)
            {
                Log("Incorrect input parameters... Symbol or Account are not specified or they have diffent connectionID.", StrategyLoggingLevel.Error);
                return;
            }

            this.symbol = Core.GetSymbol(this.symbol?.CreateInfo());

            if (this.symbol != null)
            {
                this.symbol.NewQuote += this.Symbol_NewQuote;
                this.symbol.NewLast += this.Symbol_NewLast;
            }

            this.historicalData = symbol.GetHistory(PerioBase, fromTime: StarrtData.AddMinutes(-minutesInHd * 60));
            this.historicalData.NewHistoryItem += this.HistoricalData_NewHistoryItem;

            IndicatorInfo iInfo = Core.Instance.Indicators.All.First(info => info.Name == "IchiMTreTempi V.1");
            Indi = Core.Instance.Indicators.CreateIndicator(iInfo);

            this.Indi.Settings = new List<SettingItem>()
            {
               new SettingItemInteger(name: "Tenkan Sen", value: 9),
               new SettingItemInteger(name: "Kijoun Sen", value: 26),
               new SettingItemInteger(name: "SekuSpanB", value: 52),
               new SettingItemInteger(name: "Multiplaier", value: 5),
               new SettingItemInteger(name: "MultiplaierSecondo", value: 30),
            };

            this.historicalData.AddIndicator(Indi);

            this.Fast = new TF(TF.TimeFrame.Fast, 1, this.Indi, 13, 14);
            this.Mid = new TF(TF.TimeFrame.Mid, Multiplaier, this.Indi, 3, 4);
            this.Slow = new TF(TF.TimeFrame.Slow, Multiplaier2, this.Indi, 8, 9);

            List<TF> list = new List<TF>() { Fast, Mid, Slow };

            Serie = new CloudSeries(this.historicalData, Fast, Mid, Slow);
            Serie.GenerateCloud(list);

            crosStrategy = new Condic_Gap_Cros_Strategy_V1(Serie, usealgo: true, midorsloTf: TF.TimeFrame.Slow);

            customPlacer = new CustomOrderPlacer(this.symbol, this.account, this.ammount);
        }

        
        private void Place(Cloud c)
        {
            List<PlaceOrderRequestParameters> y = customPlacer.SetTarget(c, 0.01, MyTradeType.Cover);

            foreach (var item in y)
            {
                Core.Instance.PlaceOrder(item);
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }
        }

        #region Old_Set_Target
        // Works Only Whith Limit Order Placer
        //private List<PlaceOrderRequestParameters> SetTargets(Cloud cloud)
        //{
        //    List<PlaceOrderRequestParameters> output = new List<PlaceOrderRequestParameters>();

        //    if (cloud.Color == CloudColor.white)
        //        return output;

        //    List<double> levelsList = new List<double>();
        //    List<double> tpList = new List<double>();
        //    List<double> sList = new List<double>();

        //    if (levelsList.Any())
        //        levelsList.Clear();

        //    double clMin = 0;

        //    switch (cloud.Color)
        //    {
        //        case CloudColor.green:
        //            clMin = cloud.MaximaFast.Any() ? cloud.MaximaFast.Last().Value : cloud.EndPrice;
        //            break;
        //        case CloudColor.red:
        //            clMin = cloud.MinimaFast.Any() ? cloud.MinimaFast.Last().Value : cloud.EndPrice;
        //            break;
        //    }

        //    double clEnd = cloud.EndPrice;

        //    levelsList.Add(clMin);
        //    levelsList.Add(clEnd);
        //    List<Bases> orderedBaseList = new List<Bases>();

        //    if (cloud.BasesList.Any() & cloud.BasesList.Count > numberOfCloses - 2)
        //    {
        //        orderedBaseList = cloud.Color == CloudColor.red ? cloud.BasesList.OrderBy(b => b.Value).ToList() : cloud.BasesList.OrderByDescending(b => b.Value).ToList();

        //        for (int i = 0; i < numberOfCloses - 2; i++)
        //            levelsList.Add(orderedBaseList[i].Value);
        //    }
        //    else
        //    {
        //        levelsList.Clear();

        //        double top = clEnd - clMin + clEnd;
        //        double delta = top - clMin;

        //        double step = delta / numberOfCloses;

        //        for (int i = 0; i < numberOfCloses; i++)
        //        {
        //            levelsList.Add(clMin + (step * i));
        //        }
        //    }

        //    if (!levelsList.Any())
        //    {
        //        Core.Instance.Loggers.Log("Emply Level List", LoggingLevel.Error);
        //        return output;
        //    }

        //    if (tpList.Any() || sList.Any())
        //    {
        //        tpList.Clear();
        //        sList.Clear();
        //    }

        //    else
        //    {
        //        for (int i = 0; i < levelsList.Count; i++)
        //        {
        //            switch (cloud.Color)
        //            {
        //                case CloudColor.green:
        //                    tpList.Add(levelsList[i] - (levelsList[i] * tp));
        //                    sList.Add(levelsList[i] + (levelsList[i] * sl));
        //                    break;

        //                case CloudColor.red:
        //                    tpList.Add(levelsList[i] + (levelsList[i] * tp));
        //                    sList.Add(levelsList[i] - (levelsList[i] * sl));
        //                    break;
        //            }
        //        }
        //    }

        //    if (levelsList != null)
        //    {
        //        List<SlTpHolder> sls = new List<SlTpHolder>();
        //        List<SlTpHolder> tps = new List<SlTpHolder>();

        //        for (int i = 0; i < tpList.Count; i++)
        //        {
        //            SlTpHolder _sl = SlTpHolder.CreateSL(price: sList[i], PriceMeasurement.Absolute, quantityPercentage: 100);
        //            sls.Add(_sl);

        //            SlTpHolder _tp = SlTpHolder.CreateTP(price: tpList[i], PriceMeasurement.Absolute, quantityPercentage: 100);
        //            tps.Add(_tp);
        //        }

        //        string orderTypeId = Core.OrderTypes.FirstOrDefault(x => x.ConnectionId == this.symbol.ConnectionId && x.Behavior == OrderTypeBehavior.Limit).Id;

        //        for (int i = 0; i < levelsList.Count; i++)
        //        {
        //            var request = new PlaceOrderRequestParameters
        //            {
        //                Symbol = symbol,
        //                OrderTypeId = orderTypeId,
        //                Account = account,
        //                Side = cloud.Color == CloudColor.red ? Side.Buy : Side.Sell,
        //                Quantity = ammount / levelsList.Count,
        //                Price = levelsList[i],
        //                StopLoss = sls[i],
        //                TakeProfit = tps[i],
        //            };

        //            output.Add(request);
        //        }
        //    }
        //    return output;
        //}
        #endregion

        #region Events
        protected override List<StrategyMetric> OnGetMetrics()
        {
            List<StrategyMetric> result = base.OnGetMetrics();

            if (historicalData != null)
                result.Add("Serie", $"{historicalData.Count}");
            result.Add("Positions_Count", $"{positions.Count}");
            result.Add("Trades_Count", $"{trades.Count}");
            result.Add("GapFilter", gapFilter);
            if(crosStrategy != null)
            {
                result.Add("Sentiment", crosStrategy.Sentiment);
                result.Add("Cloud TF ", crosStrategy.currentTF);
                result.Add("Cloud  ID ", crosStrategy.TradableCloudID);

            }
            return result;
        }
       
       
        private void HistoricalData_NewHistoryItem(object sender, HistoryEventArgs e)
        {
            Serie.Update(Fast);
            Serie.Update(Mid);
            Serie.Update(Slow);

            crosStrategy.GetFastTradableCloud();

            if (crosStrategy.TradableCloudID != -1)
            {
                Cloud c = crosStrategy.currentTF == TF.TimeFrame.Mid ? Serie.Clouds.Find(x => x.Id == crosStrategy.TradableCloudID) : Serie.CloudsMid.Find(x => x.Id == crosStrategy.TradableCloudID);

                Place(c);
            }
        }
        protected override void OnStop()
        {
            this.Log($"clouds.count {Serie.Hd.Count}", StrategyLoggingLevel.Trading);
            this.symbol.NewQuote -= this.Symbol_NewQuote;
            this.symbol.NewLast -= this.Symbol_NewLast;
        }
        private void Symbol_NewLast(Symbol symbol, Last last)
        {

        }
        private void Symbol_NewQuote(Symbol symbol, Quote quote)
        {
        }
        protected override void OnRemove()
        {
            this.symbol = null;
            this.account = null;
        }
        #endregion
    }
}

