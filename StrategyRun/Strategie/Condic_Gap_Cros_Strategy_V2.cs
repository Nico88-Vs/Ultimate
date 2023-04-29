using StrategyRun.Class_Lybrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.Interfacce;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Utils;

namespace StrategyRun.Strategie
{
    /// <summary>
    /// TARGET : Creare una strategia bidirezzionale Definita da una condizione generale Arbitratia o Programmatica, che cerca sotto a nuvole sostanziose
    /// ______ _ ingessi in corrispondenva dei gap di mercato utilizzando i cros di tempo inferiore come validazione
    /// </summary>
    public class Condic_Gap_Cros_Strategy_V2 : ITradeTicket
    {
        public bool gapFilter { get; set; }
        public double thickFilter { get; set; } 
        public  CloudSeries Series { get ; set; }
        public string Status { get; private set; } = "null";
        public string CondictionName { get; set; } = "Gap-Cross V1";
        public  string Description { get; } = "Trading Strategy based on Mid-gap and fast cross";
        public TF.TimeFrame currentTF { get; }

        public event EventHandler<TradeTiket> TradeTicketCreated;

        private List<int> gappedCloudID;
        private List<Cloud> tradeCloud;
        private List<Gaps> tradeGap;
        private Gaps? currentGap;
        private CloudColor gappedCloudColor = CloudColor.white;
        private TradeTiket ticket;
        
        public Condic_Gap_Cros_Strategy_V2(CloudSeries serie , TF.TimeFrame midorsloTf= TF.TimeFrame.Mid,
            bool gapFilter = false, double thickfilter = 0.5)
        {
            this.currentTF = midorsloTf;
            this.gapFilter = gapFilter;
            this.thickFilter = thickfilter;
            this.ticket = new TradeTiket();

            tradeCloud = new List<Cloud>();
            tradeGap = new List<Gaps>();
            this.gappedCloudID = new List<int>();

            if(currentTF == TF.TimeFrame.Fast)
            {
                Log("Current TF wrong value assignment", LoggingLevel.Trading);
                return;
            }

            else
                Series.Cross += this.Series_Cross;
        }

        public void Update()
        {
            GetFastTradableCloud();
            GetEntry();
        }

        private void GetFastTradableCloud()
        {
            if(currentTF != TF.TimeFrame.Mid && currentTF != TF.TimeFrame.Slow)
            {
                Log("Current TF wrong value assignment", LoggingLevel.Trading);
                return;
            }

            // Vrifico se ci sono gap da tradare
            if (!tradeGap.Any())
                return;

            // Get Last Cloud
            Cloud c = currentTF == TF.TimeFrame.Mid ? Series.CurrentCloud : Series.CurrentMidCloud;

            int position = c.Buffer + c.Time_F.GetCorrectBuffer(Series.TenkanPeriod);
            int midtfperio = currentTF == TF.TimeFrame.Mid ? Series.MidTF.GetCorrectBuffer(Series.TenkanPeriod) : Series.SlowTF.GetCorrectBuffer(Series.TenkanPeriod);
            int debug = position - c.Time_F.GetCorrectBuffer(Series.TenkanPeriod);

            Gaps? g = null;

            //Getting Gap
            try
            {
                g = tradeGap.FindAll(i => i.Buffer + midtfperio > position).First();
                if(g != null)
                {
                    Gaps x = (Gaps)g;
                    int indexg = tradeGap.IndexOf(x);
                    if(this.currentTF == TF.TimeFrame.Mid)
                        gappedCloudColor = Series.Clouds[gappedCloudID[indexg]].Color;
                    else if(this.currentTF == TF.TimeFrame.Slow)
                        gappedCloudColor = Series.CloudsMid[gappedCloudID[indexg]].Color;
                }


                if (g is not null)
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

            //Getting Cloud
            else
            { 
                if(currentGap != null)
                {
                    int delayBuffer = c.Buffer + c.Length - Series.TenkanPeriod;
                    Gaps v = (Gaps)currentGap;

                    if (position + c.Length > v.Buffer + midtfperio)
                    {
                        Cloud selected = currentTF == TF.TimeFrame.Mid ? Series.Clouds.Last(c => c.Buffer <= delayBuffer) : Series.CloudsMid.Last(c => c.Buffer <= delayBuffer);

                        if (selected.Color != gappedCloudColor)
                            currentGap = null;
                    }
                }
                else
                {
                    tradeCloud.Clear();
                    this.Status = "Waiting"; 
                }
            }
        }

        private void GetEntry()
        {
            if (!tradeCloud.Any() || gappedCloudColor == CloudColor.white)
            {
                return;
            }
                

            Cloud x = currentTF == TF.TimeFrame.Mid ? Series.CurrentCloud : Series.CurrentMidCloud;
            CloudColor cloudSelectedColor = gappedCloudColor == CloudColor.red ? CloudColor.green : CloudColor.red;
            CloudColor tradeCloudColor = gappedCloudColor == CloudColor.red ? CloudColor.red : CloudColor.green;

            if (x.Thickness > x.AverageList.Last() && x.Color == cloudSelectedColor)
            {
                try
                {
                    double min = double.MaxValue;
                    Cloud w = tradeCloud.Last(x => x.Color == tradeCloudColor);

                    switch (w.RoofList.Any())
                    {
                        case true:
                            Bases b = gappedCloudColor == CloudColor.red ?  w.RoofList.OrderBy(x => x.Value).First() : w.RoofList.OrderBy(x => x.Value).Last();
                            min = b.Value;
                            //Log("Conta Anche Le Basi Da 2", LoggingLevel.Error);
                            break;

                        case false:
                            min = gappedCloudColor == CloudColor.red ? w.MinimaFast.Last().Value : w.MaximaFast.Last().Value;
                            break;
                    }

                    double val = 0;
                    switch (x.MinimaSlow.Any())
                    {
                        case true:
                            val = gappedCloudColor == CloudColor.red ? x.MinimaSlow.Last().Value : x.MaximaSlow.Last().Value;
                            break;

                        case false:
                            val =  x.OriginPrice;
                            break;
                    }

                    //Log("Last Condiction Cek", LoggingLevel.Error);

                    switch (gappedCloudColor)
                    {
                        case CloudColor.red:
                            if (val > min)
                                CeckTradeTiket(w);
                            else
                                return;
                            break;

                        case CloudColor.green:
                            if (val < min)
                                CeckTradeTiket(w);
                            else
                                return;
                            break;

                        case CloudColor.white:
                            return;
                    }
                   
                }
                catch (Exception)
                {
                    return; ;
                }
            }
            else
            {
                return;
            }
        }

        #region Events Subscription
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
                    if (gapFilter && e.Gap.GapReason == Gaps.Reason.bases)
                        return;
                    if (e.Gap.type == Gaps.Type.inverse)
                        return;

                    tradeGap.Add(e.Gap);
                    gappedCloudID.Add(e.CludId);

                    break;
                case TF.TimeFrame.Slow:
                    if (c.Time_F.Timeframe != TF.TimeFrame.Slow)
                        return;
                    if (c.Color == CloudColor.white)
                        return;
                    if (gapFilter && e.Gap.GapReason == Gaps.Reason.bases)
                        return;
                    if (e.Gap.type == Gaps.Type.inverse)
                        return;

                    tradeGap.Add(e.Gap);
                    gappedCloudID.Add(e.CludId);
                    break;
            }
        }
        #endregion

        #region Services
        private void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }

        private void CeckTradeTiket(Cloud c)
        {
            TradeTiket t = new TradeTiket();

            switch (currentTF)
            {
                case TF.TimeFrame.Fast:
                    Log("Wrong Tf selected", LoggingLevel.Trading);
                    break;

                case TF.TimeFrame.Mid:
                    t.Dyctionary = DicManager(c, Series.MidCloudDictionary);
                    break;

                case TF.TimeFrame.Slow:
                    t.Dyctionary = DicManager(c, Series.MidInSlowDictionary);
                    break;
            }

            t.TradeSentiment = c.Color == CloudColor.red ? Sentiment.Buy : c.Color == CloudColor.green ? Sentiment.Sell : Sentiment.Wait;
            t.ClaudId = c.Id;
            t.CondictionName = this.CondictionName;

            if (t.Equals(ticket))
            {
                this.OnTiketCreated(t);
                ticket = t;
            }
        }

        private Dictionary<Cloud, List<Cloud>> DicManager(Cloud c, Dictionary<Cloud , List<Cloud>> dico)
        {
            bool exist = dico.Any(x => x.Equals(c));
            Dictionary<Cloud, List<Cloud>> output = new Dictionary<Cloud, List<Cloud>>();

            if (exist)
            {
                foreach (KeyValuePair<Cloud, List<Cloud>> pair in dico)
                {
                    if(pair.Value.Contains(c))
                        output.Add(pair.Key, pair.Value);
                }
            }
            else
            {
                this.Status = "Unable to build Dictionary";
            }
           return output;
        }

        public virtual void OnTiketCreated(TradeTiket e)
        {
            TradeTicketCreated?.Invoke(this, e);
        }
        #endregion
    }
}
