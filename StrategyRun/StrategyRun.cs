// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TheIndicator;
using TheIndicator.LibreriaDiClassi;
using StrategyRun.Strategie;
using System.Threading.Tasks;
using StrategyRun.Order_Placer;
using StrategyRun.Headg_Manager;
using StrategyRun.Sentiments_Strategies;
using StrategyRun.HeadgeStrategy;
using StrategyRun.Class_Lybrary;
using StrategyRun.HeadgeStrategyi;

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

        private Indicator Indi;
        private HistoricalData historicalData;
        private int minutesInHd = 0;
        private TF Fast;
        private TF Mid;
        private TF Slow;    
        public CloudSeries Serie;

        private Cros_Sentiment_Strategy sentStrategy;
        private Test_Cross_Strategy crossStrategy;
        private Cross_Strategy_Position_Manager positionManager;
        private Cross_Headge_Strategy headgeStrategy;

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
        }

        protected override void OnRun()
        {
            #region IchiBasics
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
            #endregion

            if(this.Serie != null)
            {
                this.sentStrategy = new Cros_Sentiment_Strategy(this.Serie, TF.TimeFrame.Slow);
                this.crossStrategy = new Test_Cross_Strategy(this.sentStrategy);
                this.headgeStrategy = new Cross_Headge_Strategy(this.ammount, this.Serie);
                List<ITradeTicket> lista = new List<ITradeTicket>() { this.crossStrategy };
                this.positionManager = new Cross_Strategy_Position_Manager(lista, this.headgeStrategy, this.Serie, this.account );
            }
        }

        #region Events
        protected override List<StrategyMetric> OnGetMetrics()
        {
            List<StrategyMetric> result = base.OnGetMetrics();

            if (historicalData != null)
                result.Add("Serie", $"{historicalData.Count}");
            if (headgeStrategy != null)
            {
                result.Add("Order_Main", headgeStrategy.Mains.Orders.Count);
                result.Add("Posizioni_Main", headgeStrategy.Mains.Posizioni.Count);
            }
            return result;
        }
       
       
        private void HistoricalData_NewHistoryItem(object sender, HistoryEventArgs e)
        {
            double close = this.historicalData[0][PriceType.Open];

            Serie.Update(Fast);
            Serie.Update(Mid);
            Serie.Update(Slow);
            headgeStrategy.Update(close);
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

