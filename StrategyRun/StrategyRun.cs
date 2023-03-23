// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;

namespace StrategyRun
{
    /// <summary>
    /// An example of strategy for working with one symbol. Add your code, compile it and run via Strategy Runner panel in the assigned trading terminal.
    /// Information about API you can find here: http://api.quantower.com
    /// </summary>
    /// 
	public class StrategyRun : Strategy
    {
        [InputParameter("Symbol", 10)]
        private Symbol symbol;

        [InputParameter("Account", 20)]
        public Account account;

        [InputParameter("Period", 2)]
        private Period PerioBase = Period.MIN1;

        private bool IsLoaded = false;
        private Indicator indi;
        private HistoricalData historicalData;

        //debug
        DateTime quotetime;


        public override string[] MonitoringConnectionsIds => new string[] { this.symbol?.ConnectionId };

        public StrategyRun()
            : base()
        {
            // Defines strategy's name and description.
            this.Name = "StrategyRun";
            this.Description = "My strategy's annotation";

            
        }

        /// <summary>
        /// This function will be called after creating a strategy
        /// </summary>
        protected override void OnCreated()
        {
        }

        /// <summary>
        /// This function will be called after running a strategy
        /// </summary>
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
                this.symbol.NewQuote += SymbolOnNewQuote;
                this.symbol.NewLast += SymbolOnNewLast;
            }


            // Add your code here
            this.historicalData = symbol.GetHistory(this.PerioBase, fromTime: Core.TimeUtils.DateTimeUtcNow.AddDays(-365));

            this.Log($"{this.State.ToString()}", StrategyLoggingLevel.Trading);

            //SetIndicator();

            this.historicalData.NewHistoryItem += this.HistoricalData_NewHistoryItem;
            this.Log($"{this.historicalData.Count}", StrategyLoggingLevel.Trading);

            string serverTimeOfset = Core.Instance.TimeUtils.ServerTimeOffset.ToString();
            string StartTerminalDateTimeUtc = Core.Instance.TimeUtils.StartTerminalDateTimeUtc.ToString();
            string DateTimeUtcNow = Core.Instance.TimeUtils.DateTimeUtcNow.ToString();

            this.Log($"{serverTimeOfset}", StrategyLoggingLevel.Trading);
            this.Log($"{StartTerminalDateTimeUtc}", StrategyLoggingLevel.Trading);
            this.Log($"{DateTimeUtcNow}", StrategyLoggingLevel.Trading);


        }

        private void HistoricalData_NewHistoryItem(object sender, HistoryEventArgs e)
        {
            if (!IsLoaded)
                return;

            this.Log($"Indi Count{historicalData.Count}", StrategyLoggingLevel.Trading);

        }



        /// <summary>
        /// This function will be called after stopping a strategy
        /// </summary>
        protected override void OnStop()
        {
            if (this.symbol != null)
            {
                this.symbol.NewQuote -= SymbolOnNewQuote;
                this.symbol.NewLast -= SymbolOnNewLast;
            }

            // Add your code here
        }

        /// <summary>
        /// This function will be called after removing a strategy
        /// </summary>
        protected override void OnRemove()
        {
            this.symbol = null;
            this.account = null;
            // Add your code here
        }

        /// <summary>
        /// Use this method to provide run time information about your strategy. You will see it in StrategyRunner panel in trading terminal
        /// </summary>
        protected override List<StrategyMetric> OnGetMetrics()
        {
            List<StrategyMetric> result = base.OnGetMetrics();

            // An example of adding custom strategy metrics:
            // result.Add("Opened buy orders", "2");
            // result.Add("Opened sell orders", "7");

            return result;
        }

        private void SymbolOnNewQuote(Symbol symbol, Quote quote)
        {

            if (!IsLoaded)
            {
                quotetime = this.symbol.QuoteDateTime.AddDays(-3);
                this.Log($"quotetime: {quotetime}", StrategyLoggingLevel.Trading);
                DateTime starDateTime = CalculateSpan(this.PerioBase, quotetime);
                this.historicalData = symbol.GetHistory(this.PerioBase, fromTime: quotetime);


                IsLoaded = true;
                LoadIndicator(quotetime);
                historicalData.AddIndicator(indi);
            }

        }

        private void SymbolOnNewLast(Symbol symbol, Last last)
        {
            // Add your code here
        }

        /// <summary>
        /// Logic Area
        /// </summary>

        private void LoadIndicator(DateTime endData)
        {
            IndicatorInfo indiInfo = Core.Instance.Indicators.All.FirstOrDefault(x => x.Name == "TheIndicator");
            indi = Core.Instance.Indicators.CreateIndicator(indiInfo);

            this.indi.Settings = new List<SettingItem>()
            {
                new SettingItemPeriod("Period", PerioBase),
                new SettingItemDateTime("End Data", endData)
            };
        }

        private DateTime CalculateSpan(Period thisPeriod, DateTime enData)
        {
            Period newPeriod = new Period(thisPeriod.BasePeriod, 80);
            DateTime output = enData.Subtract(newPeriod.Duration);
            return output;
        }


        ////
        /// Test Area
        /// 

        //private void SetIndicator()
        //{
        //    IndicatorInfo indicatorMT = Core.Instance.Indicators.All.First(info => info.Name == "TheIndicator");
        //    this.indi = Core.Instance.Indicators.CreateIndicator(indicatorMT);

        //    //Settings Interprete3tempi
        //    this.indi.Settings = new List<SettingItem>()
        //    {
        //        new SettingItemPeriod("PerioBase",PerioBase),
        //    };
        //    this.historicalData.AddIndicator(indi);
        //}

        //private string[] test()
        //{
        //    string[] st = new string[this.id.Settings.Count];
        //    for (int i = 0; i < this.Settings.Count; i++)
        //    {
        //        st[i] = this.id.Settings[i].Name.ToString();
        //    }
        //    int ct = this.Settings.Count;
        //    string output = ct.ToString();
        //    return st;
        //}

        //private string stet()
        //{
        //    string c = this.id. ToString();

        //    return c.ToString();
        //}

        //private Indicator id = Core.Instance.Indicators.BuiltIn.SMA(1, PriceType.Close);
        //// symbol.GetHistory(this.PerioBase, fromTime: Core.TimeUtils.DateTimeUtcNow.AddDays(-365));
        }
}
