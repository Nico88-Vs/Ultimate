using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.Interfacce;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun
{
    public class Condic_Gap_Cros_Strategy : GetCondiction
    {
        public bool useAlgoSentiment { get; set; }

        [InputParameter("Sentiment", 2, variants: new object[]
        {
            Sentiment.Buy, Sentiment.Sell,Sentiment.Wait
        })]
        private Sentiment sentimentInput = Sentiment.Wait;

        [InputParameter("GapsFilter", 3)]
        private bool gapFilter = false;

        [InputParameter("ThickFilter", 4)]

        private double thickFilter = 0.5;

        public int TradableCloudID { get; private set; } = -1;
        public override CloudSeries Series { get ; set; }
        public string Status { get; private set; } = "null";
        public override string Name { get; } = "Gap-Cross";
        public override string Description { get; } = "Trading Strategy based on Mid-gap and fast cross";
        public override int Buffer { get; } = 0;
        public override Sentiment Sentiment { get; set; }

        private List<Cloud> tradeCloud;
        private List<Gaps> tradeGap;
        private Gaps? currentGap;

        public Condic_Gap_Cros_Strategy(CloudSeries serie , Sentiment sent, bool usealgo = false) : base(serie)
        {
            this.sentimentInput = sent;

            this.useAlgoSentiment = usealgo;

            tradeCloud = new List<Cloud>();
            tradeGap = new List<Gaps>();

            SetSentiment(); 

            Series.Cross += this.Series_Cross;
            Log("Non Implementa Shapes, Thick_filter & Gaps_Filter", LoggingLevel.Trading);
        }

        public void GetFastTradableCloud()
        {
            SetSentiment();

            if(this.Sentiment == Sentiment.Wait)
                return;

            if (!tradeGap.Any())
                return;

            Cloud c = Series.CurrentCloud;

            int position = c.Buffer + c.Time_F.GetCorrectBuffer(Series.TenkanPeriod);
            int midtfperio = Series.MidTF.GetCorrectBuffer(Series.TenkanPeriod);
            int debug = position - c.Time_F.GetCorrectBuffer(Series.TenkanPeriod);

            Gaps? g = null;
            try
            {
                g = tradeGap.FindAll(i => i.Buffer + midtfperio > position).First();
                if(g is not null)
                {
                    Gaps d = (Gaps)g;
                    if (d.type == Gaps.Type.inverse)
                        g = null;
                }
            }
            catch (Exception)
            {
                g = null;
            }
            
            if (g != null)
            {
                if (currentGap == null)
                {
                    tradeCloud.Add(c);
                    currentGap = g;
                }

                this.Status = "Running";
            }
            else
            { 
                if(currentGap != null)
                {
                    int delayBuffer = c.Buffer + c.Length - Series.TenkanPeriod;
                    Gaps v = (Gaps)currentGap;

                    if (position + c.Length > v.Buffer + midtfperio)
                    {
                        Cloud selected = Series.Clouds.Last(c => c.Buffer <= delayBuffer);
                        CloudColor cloudSelectedColor = this.Sentiment == Sentiment.Buy ? CloudColor.red : CloudColor.green;

                        if (selected.Color != cloudSelectedColor)
                            currentGap = null;
                    }
                }
                else
                {
                    tradeCloud.Clear();
                    this.Status = "Waiting"; // dovro gestire la persistanza della lista >> fino a +26 va bn oltre i 26 va bene solo se e una rossa con buffer minore
                }
            }

            GetEntry();
        }

        private void GetEntry()
        {
            if (!tradeCloud.Any())
            {
                TradableCloudID = -1;
                return;
            }
                

            Cloud x = Series.CurrentCloud;
            CloudColor cloudSelectedColor = this.Sentiment == Sentiment.Buy ? CloudColor.green : CloudColor.red;
            CloudColor tradeCloudColor = this.Sentiment == Sentiment.Buy ? CloudColor.red : CloudColor.green;

            if (x.Thickness > x.AverageList.Last() && x.Color == cloudSelectedColor)
            {
                try
                {
                    double min = double.MaxValue;
                    Cloud w = tradeCloud.Last(x => x.Color == tradeCloudColor);

                    switch (w.RoofList.Any())
                    {
                        case true:
                            Bases b = this.Sentiment == Sentiment.Buy ?  w.RoofList.OrderBy(x => x.Value).First() : w.RoofList.OrderBy(x => x.Value).Last();
                            min = b.Value;
                            //Log("Conta Anche Le Basi Da 2", LoggingLevel.Error);
                            break;

                        case false:
                            min = this.Sentiment == Sentiment.Buy ? w.MinimaFast.Last().Value : w.MaximaFast.Last().Value;
                            break;
                    }

                    double val = 0;
                    switch (x.MinimaSlow.Any())
                    {
                        case true:
                            val = this.Sentiment == Sentiment.Buy ? x.MinimaSlow.Last().Value : x.MaximaSlow.Last().Value;
                            break;

                        case false:
                            val =  x.OriginPrice;
                            break;
                    }

                    //Log("Last Condiction Cek", LoggingLevel.Error);

                    switch (this.Sentiment)
                    {
                        case Sentiment.Buy:
                            if (val > min)
                                TradableCloudID = w.Id;
                            else
                                TradableCloudID = -1;
                            break;

                        case Sentiment.Sell:
                            if (val < min)
                                TradableCloudID = w.Id;
                            else
                                TradableCloudID = -1;
                            break;

                        case Sentiment.Wait:
                            TradableCloudID = -1;
                            break;
                    }
                   
                }
                catch (Exception)
                {
                    TradableCloudID = -1; ;
                }
            }
            else
            {
                TradableCloudID = -1;
            }
        }

        #region Events
        private void Series_Cross(object sender, CrossEvent e)
        {
            if (Status == "Running" & e.CurrentCloud.Time_F.Timeframe == TF.TimeFrame.Fast)
            {
                if (!tradeCloud.Contains(e.CurrentCloud))
                    tradeCloud.Add(e.CurrentCloud);
            }
            if (e.CurrentCloud.Time_F.Timeframe != TF.TimeFrame.Mid)
                return;
            this.Series.CurrentMidCloud.GapsAdded += this.StoringCloud_GapsAdded;
        }

        public void StoringCloud_GapsAdded(object sender, GapAddedArgs e)
        {
            Cloud c = sender as Cloud;

            if (c.Time_F.Timeframe != TF.TimeFrame.Mid)
                return;
            if (c.Color == CloudColor.white)
                return;
            if (c.Color == CloudColor.red & Sentiment == Sentiment.Sell)
                return;
            if (c.Color == CloudColor.green & Sentiment == Sentiment.Buy)
                return;
            if (gapFilter && e.Gap.GapReason == Gaps.Reason.bases)
                return;
            if (e.Gap.type == Gaps.Type.inverse)
                return;
            //Log("Qua Ci Sono Filtri Da Settare, In particolarmodo allow inverse", LoggingLevel.Error);

            tradeGap.Add(e.Gap);
        }
        #endregion

        #region Services
        private void SetSentiment()
        {
            if (!useAlgoSentiment)
                this.Sentiment = sentimentInput;

            else if (useAlgoSentiment)
            {
                switch (Series.Scenario)
                {
                    case IchimokuCloudScenario.STRONG_BULLISH:
                        this.Sentiment = Sentiment.Sell;
                        break;
                    case IchimokuCloudScenario.STRONG_BEARISH:
                        this.Sentiment = Sentiment.Buy;
                        break;
                    case IchimokuCloudScenario.MODERATELY_BULLISH:
                        this.Sentiment = Sentiment.Buy;
                        break;
                    case IchimokuCloudScenario.MODERATELY_BEARISH:
                        this.Sentiment = Sentiment.Sell;
                        break;
                    case IchimokuCloudScenario.CONSOLIDATION_BULLISH:
                        this.Sentiment = Sentiment.Sell;
                        break;
                    case IchimokuCloudScenario.CONSOLIDATION_BEARISH:
                        this.Sentiment = Sentiment.Buy;
                        break;
                    case IchimokuCloudScenario.UNDEFINED:
                        this.Sentiment = Sentiment.Wait;
                        break;
                    default:
                        this.Sentiment = Sentiment.Wait;
                        break;
                }
            }
        }

        public override void DisplayCondiction(TF.TimeFrame tf)
        {
            if (tf != TF.TimeFrame.Fast)
            {
                Core.Instance.Loggers.Log("This Strategy Works on Fast TF", LoggingLevel.Error);
                return;
            }

            foreach (Cloud item in tradeCloud)
            {
                item.Condiction.Clear();

                for (int i = 0; i < item.LenghtList.Count; i++)
                {
                    item.Condiction.Add(item.LenghtList[i]);
                }
            }
        }

        private void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }
        #endregion
    }
}
