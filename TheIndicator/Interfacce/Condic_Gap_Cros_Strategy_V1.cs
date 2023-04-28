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
    /// <summary>
    /// TARGET : Creare una strategia bidirezzionale Definita da una condizione generale Arbitratia o Programmatica, che cerca sotto a nuvole sostanziose
    /// ______ _ ingessi in corrispondenva dei gap di mercato utilizzando i cros di tempo inferiore come validazione
    /// </summary>
    public class Condic_Gap_Cros_Strategy_V1 : GetCondiction
    {
        public bool useAlgoSentiment { get; set; }
        public Sentiment sentimentInput { get; set; } 
        public bool gapFilter { get; set; }
        public double thickFilter { get; set; } 
        public int TradableCloudID { get; private set; } = -1;
        public override CloudSeries Series { get ; set; }
        public string Status { get; private set; } = "null";
        public override string Name { get; } = "Gap-Cross V1";
        public override string Description { get; } = "Trading Strategy based on Mid-gap and fast cross";
        public override int Buffer { get; } = 0;
        public override Sentiment Sentiment { get; set; }
        public TF.TimeFrame currentTF { get; }


        private List<Cloud> tradeCloud;
        private List<Gaps> tradeGap;
        private Gaps? currentGap;

        public Condic_Gap_Cros_Strategy_V1(CloudSeries serie , bool usealgo = true, TF.TimeFrame midorsloTf= TF.TimeFrame.Mid,
            Sentiment sent = Sentiment.Wait, bool gapFilter = false, double thickfilter = 0.5) : base(serie)
        {
            this.sentimentInput = sent;
            this.currentTF = midorsloTf;
            this.useAlgoSentiment = usealgo;
            this.gapFilter = gapFilter;
            this.thickFilter = thickfilter;

            tradeCloud = new List<Cloud>();
            tradeGap = new List<Gaps>();

            SetSentiment();

            if(currentTF == TF.TimeFrame.Fast)
            {
                Log("Current TF wrong value assignment", LoggingLevel.Trading);
                return;
            }

            else
                Series.Cross += this.Series_Cross;
        }

        public void GetFastTradableCloud()
        {
            SetSentiment();

            if(this.Sentiment == Sentiment.Wait)
                return;

            if(currentTF != TF.TimeFrame.Mid && currentTF != TF.TimeFrame.Slow)
            {
                Log("Current TF wrong value assignment", LoggingLevel.Trading);
                return;
            }

            if (!tradeGap.Any())
                return;

            // Get Correct Cloud
            Cloud c = currentTF == TF.TimeFrame.Mid ? Series.CurrentCloud : Series.CurrentMidCloud;

            int position = c.Buffer + c.Time_F.GetCorrectBuffer(Series.TenkanPeriod);
            int midtfperio = currentTF == TF.TimeFrame.Mid ? Series.MidTF.GetCorrectBuffer(Series.TenkanPeriod) : Series.SlowTF.GetCorrectBuffer(Series.TenkanPeriod);
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
                        Cloud selected = currentTF == TF.TimeFrame.Mid ? Series.Clouds.Last(c => c.Buffer <= delayBuffer) : Series.CloudsMid.Last(c => c.Buffer <= delayBuffer);
                        CloudColor cloudSelectedColor = this.Sentiment == Sentiment.Buy ? CloudColor.red : CloudColor.green;

                        if (selected.Color != cloudSelectedColor)
                            currentGap = null;
                    }
                }
                else
                {
                    tradeCloud.Clear();
                    this.Status = "Waiting"; 
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
                

            Cloud x = currentTF == TF.TimeFrame.Mid ? Series.CurrentCloud : Series.CurrentMidCloud;
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
            switch (currentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Wrong Tf selected", LoggingLevel.Trading);
                    return;

                case TF.TimeFrame.Mid:
                    if (Status == "Running" & e.CurrentCloud.Time_F.Timeframe == TF.TimeFrame.Fast)
                    {
                        if (!tradeCloud.Contains(e.CurrentCloud))
                            tradeCloud.Add(e.CurrentCloud);
                    }
                    if (e.CurrentCloud.Time_F.Timeframe != TF.TimeFrame.Mid)
                        return;

                    this.Series.CurrentMidCloud.GapsAdded += this.StoringCloud_GapsAdded;
                    break;

                case TF.TimeFrame.Slow:
                    if (Status == "Running" & e.CurrentCloud.Time_F.Timeframe == TF.TimeFrame.Mid)
                    {
                        if (!tradeCloud.Contains(e.CurrentCloud))
                            tradeCloud.Add(e.CurrentCloud);
                    }
                    if (e.CurrentCloud.Time_F.Timeframe != TF.TimeFrame.Slow)
                        return;

                    this.Series.CurrentSlowCloud.GapsAdded += this.StoringCloud_GapsAdded;
                    break;
            }
           
        }

        public void StoringCloud_GapsAdded(object sender, GapAddedArgs e)
        {
            Cloud c = sender as Cloud;

            switch (currentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Wrong Tf selected", LoggingLevel.Trading);
                    return;

                case TF.TimeFrame.Mid:
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

                    tradeGap.Add(e.Gap);

                    break;
                case TF.TimeFrame.Slow:
                    if (c.Time_F.Timeframe != TF.TimeFrame.Slow)
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

                    tradeGap.Add(e.Gap);
                    break;
            }
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
