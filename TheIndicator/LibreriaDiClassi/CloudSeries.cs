using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;
using static TheIndicator.LibreriaDiClassi.TF;

namespace TheIndicator.LibreriaDiClassi
{
    public class CloudSeries
    {
        #region Variabili
        public string StatusLog { get; private set; } = "Inizilaisin";
        public List<Cloud> Clouds { get; set; }
        public List<Cloud> CloudsMid { get; set; }
        public List<Cloud> CloudSlow { get; set; }
        public Cloud CurrentCloud { get; set; }
        public Cloud CurrentMidCloud { get; set; }
        public Cloud CurrentSlowCloud { get; set; }
        public HistoricalData Hd { get; set; }
        public Dictionary<Cloud, List<Cloud>> MidCloudDictionary { get; set; }
        public Dictionary<Cloud, List<Cloud>> SlowCloudDictionary { get; set; }
        public Dictionary<Cloud, List<Cloud>> MidInSlowDictionary { get; set; }

       
        public int FastSeriesStart { get; private set; } = 0;
        public int MidSeriesStart { get; private set; } = 0;
        public int SlowSeriesStart { get; private set; } = 0;
        public int TenkanPeriod { get; set; } = 26;


        private int FastS_Fast;
        private int SlowS_Fast;
        private int FastS_Mid;
        private int SlowS_Mid;
        private int FastS_Slow;
        private int SlowS_Slow;



        public event EventHandler<CrossEvent> Cross;
        private Indicator Indi;
        #endregion

        public CloudSeries(HistoricalData hd, TF Fast, TF Mid, TF Slow)
        {
            this.Hd= hd;
            this.FastS_Fast = Fast.FastSeries;
            this.SlowS_Fast= Fast.SlowSeries;
            this.FastS_Mid = Mid.FastSeries;
            this.SlowS_Mid = Mid.SlowSeries;
            this.FastS_Slow = Slow.FastSeries;
            this.SlowS_Slow= Slow.SlowSeries;

            if (Fast.Indicatore == Mid.Indicatore && Mid.Indicatore == Slow.Indicatore)
            {
                this.Indi = Fast.Indicatore;
                this.Hd.AddIndicator(Indi);
            }
            else
            {
                this.StatusLog = "Gli indicatori non corrispondono";
                return;
            }

            
            this.Clouds = new List<Cloud>();
            this.CloudsMid = new List<Cloud>();
            this.CloudSlow= new List<Cloud>();
            this.MidCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.SlowCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.MidInSlowDictionary = new Dictionary<Cloud, List<Cloud>>();

        }

        // Attenzione Hd non aggiunge Indi
        public CloudSeries(HistoricalData hd, Indicator indi)
        {
            this.Hd = hd;
            this.Indi = indi;
            this.FastS_Fast = 13;
            this.SlowS_Fast = 14;
            this.FastS_Mid = 3;
            this.SlowS_Mid = 4;
            this.FastS_Slow = 8;
            this.SlowS_Slow = 9;
           

            this.Clouds = new List<Cloud>();
            this.CloudsMid = new List<Cloud>();
            this.CloudSlow = new List<Cloud>();
            this.MidCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.SlowCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.MidInSlowDictionary = new Dictionary<Cloud, List<Cloud>>();

        }

        // Attenzione Hd non aggiunge Indi
        public CloudSeries(int fastS_Fast, int slowS_Fast, int fastS_Mid, int slowS_Mid, int fastS_Slow, int slowS_Slow, Indicator indi, 
            HistoricalData hd)
        {
            this.FastS_Fast = fastS_Fast;
            this.SlowS_Fast = slowS_Fast;
            this.FastS_Mid = fastS_Mid;
            this.SlowS_Mid = slowS_Mid;
            this.FastS_Slow = fastS_Slow;
            this.SlowS_Slow = slowS_Slow;
            this.Hd = hd;
            this.Indi = indi;

            this.Clouds = new List<Cloud>();
            this.MidCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.SlowCloudDictionary = new Dictionary<Cloud, List<Cloud>>();
            this.MidInSlowDictionary= new Dictionary<Cloud, List<Cloud>>();
        }

        public  void GenerateCloud(TF timeFrame)
        {
            switch (timeFrame.Timeframe)
            {
                case TF.TimeFrame.Fast:
                    this.CurrentCloud = new Cloud(0, 0, timeFrame);
                    this.StatusLog = "CurrentCloud Created";

                    //inizio il ciclo sulla storia
                    for (int i = this.Hd.Count; i >= 0; i--)
                    {
                        double fastValue = this.Indi.GetValue(lineIndex: FastS_Fast, offset: i);
                        double slowvalue = this.Indi.GetValue(lineIndex: SlowS_Fast, offset: i);
                        double fastValueShift = this.Indi.GetValue(lineIndex: FastS_Fast, offset: i + 1);
                        double slowvalueShift = this.Indi.GetValue(lineIndex: SlowS_Fast, offset: i + 1);

                        //verifico la presenza delle linee
                        if (fastValueShift > 0 && slowvalueShift > 0)
                        {
                            this.CurrentCloud.UpdateCloud(i);

                            bool goldenCross = (slowvalueShift >= fastValueShift) && (fastValue > slowvalue) ? true : false;
                            bool deadCross = (slowvalueShift <= fastValueShift) && (fastValue < slowvalue) ? true : false;

                            if (goldenCross || deadCross)
                            {
                                double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                                CrossEvent ar = new CrossEvent(CalculateArg(goldenCross, this.CurrentCloud.Time_F), endPrice, this.Hd.Count - i, this.CurrentCloud);
                                OnCross(ar);
                            }
                        }
                        else if (fastValue > 0 && fastValueShift !> 0)
                        {
                            if (CurrentCloud.Id == 0)
                            {
                                CurrentCloud.Buffer = i;
                                this.Clouds.Add(CurrentCloud);
                            }
                        }
                        else
                        {
                            this.FastSeriesStart++;
                        }

                    }
                    break;

                case TF.TimeFrame.Mid:
                    //setto la current media
                    this.CurrentMidCloud = new Cloud(0, 0, timeFrame);

                    //inizio il ciclo
                    for (int i = this.Hd.Count; i >= 0; i--)
                    {
                        double fastValue = this.Indi.GetValue(lineIndex: FastS_Mid, offset: i);
                        double slowvalue = this.Indi.GetValue(lineIndex: SlowS_Mid, offset: i);
                        double fastValueShift = this.Indi.GetValue(lineIndex: FastS_Mid, offset: i + 1);
                        double slowvalueShift = this.Indi.GetValue(lineIndex: SlowS_Mid, offset: i + 1);

                        //verifico la presenza delle linee
                        if (fastValueShift > 0 && slowvalueShift > 0)
                        {
                            this.CurrentMidCloud.UpdateCloud(i);

                            bool goldenCross = (slowvalueShift >= fastValueShift) && (fastValue > slowvalue) ? true : false;
                            bool deadCross = (slowvalueShift <= fastValueShift) && (fastValue < slowvalue) ? true : false;

                            if (goldenCross || deadCross)
                            {
                                double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                                CrossEvent ar = new CrossEvent(CalculateArg(goldenCross, CurrentMidCloud.Time_F), endPrice, this.Hd.Count - i, CurrentMidCloud);
                                OnCross(ar);
                            }
                        }
                        else if (fastValue > 0 && fastValueShift! > 0)
                        {
                            if (CurrentMidCloud.Id == 0)
                            {
                                CurrentMidCloud.Buffer = i;
                                this.CloudsMid.Add(CurrentMidCloud);
                            }
                        }
                        else
                        {
                            this.MidSeriesStart++;
                        }
                    }
                    break;

                case TF.TimeFrame.slow:
                    //setto la current media
                    this.CurrentSlowCloud = new Cloud(0, 0, timeFrame);

                    //inizio il ciclo
                    for (int i = this.Hd.Count; i >= 0; i--)
                    {
                        double fastValue = this.Indi.GetValue(lineIndex: FastS_Slow, offset: i);
                        double slowvalue = this.Indi.GetValue(lineIndex: SlowS_Slow, offset: i);
                        double fastValueShift = this.Indi.GetValue(lineIndex: FastS_Slow, offset: i + 1);
                        double slowvalueShift = this.Indi.GetValue(lineIndex: SlowS_Slow, offset: i + 1);

                        //verifico la presenza delle linee
                        if (fastValueShift > 0 && slowvalueShift > 0)
                        {
                            this.CurrentSlowCloud.UpdateCloud(i);

                            bool goldenCross = (slowvalueShift >= fastValueShift) && (fastValue > slowvalue) ? true : false;
                            bool deadCross = (slowvalueShift <= fastValueShift) && (fastValue < slowvalue) ? true : false;

                            if (goldenCross || deadCross)
                            {
                                double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                                CrossEvent ar = new CrossEvent(CalculateArg(goldenCross, CurrentCloud.Time_F), endPrice, this.Hd.Count - i, CurrentSlowCloud);
                                OnCross(ar);
                            }
                        }
                        else if (fastValue > 0 && fastValueShift! > 0)
                        {
                            if (CurrentSlowCloud.Id == 0)
                            {
                                CurrentSlowCloud.Buffer = i;
                                this.CloudSlow.Add(CurrentSlowCloud);
                            }
                        }
                        else
                        {
                            this.SlowSeriesStart++;
                        }
                    }
                    break;
            }
        }

        public void Update(TF timeFrame)
        {
            double fastValue = this.Indi.GetValue(lineIndex: timeFrame.FastSeries, offset: 0);
            double slowvalue = this.Indi.GetValue(lineIndex: timeFrame.SlowSeries, offset: 0);
            double fastValueShift = this.Indi.GetValue(lineIndex: timeFrame.FastSeries, offset: 1);
            double slowvalueShift = this.Indi.GetValue(lineIndex: timeFrame.SlowSeries, offset: 1);

            bool goldenCross = (slowvalueShift >= fastValueShift) && (fastValue > slowvalue) ? true : false;
            bool deadCross = (slowvalueShift <= fastValueShift) && (fastValue < slowvalue) ? true : false;

            switch (timeFrame.Timeframe)
            {
                case TimeFrame.Fast:
                    
                    if (fastValueShift > 0 && slowvalueShift > 0)
                    {
                        this.CurrentCloud.UpdateCloud(0);

                        if (goldenCross || deadCross)
                        {
                            double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                            CrossEvent ar = new CrossEvent(CalculateArg(goldenCross, CurrentCloud.Time_F), endPrice, this.Hd.Count, CurrentCloud);
                            OnCross(ar);
                        }

                        // Qua c'era logica dei gap
                    }
                    break;

                case TimeFrame.Mid:

                    if (fastValueShift > 0 && slowvalueShift > 0)
                    {
                        this.CurrentMidCloud.UpdateCloud(0);

                        if (goldenCross || deadCross)
                        {
                            double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                            CrossEvent ar = new CrossEvent(CalculateArg(goldenCross, CurrentMidCloud.Time_F), endPrice, this.Hd.Count, CurrentMidCloud);
                            OnCross(ar);
                        }
                    }
                    break;

                case TimeFrame.slow:
                   
                    if (fastValueShift > 0 && slowvalueShift > 0)
                    {
                        this.CurrentSlowCloud.UpdateCloud(0);

                        bool goldenCrossMid = (slowvalue >= fastValueShift) && (fastValue > slowvalue) ? true : false;
                        bool deadCrossMid = (slowvalue >= fastValueShift) && (fastValue > slowvalue) ? true : false;

                        if (goldenCrossMid || deadCrossMid)
                        {
                            double endPrice = (fastValue + slowvalue + fastValueShift + slowvalueShift) / 4;
                            CrossEvent ar = new CrossEvent(CalculateArg(goldenCrossMid, CurrentSlowCloud.Time_F), endPrice, this.Hd.Count, this.CurrentSlowCloud);
                            OnCross(ar);
                        }
                    }
                    break;
            }
        }

        private EventCrosArg CalculateArg(bool isGolden, TF tf)
        {
            EventCrosArg x = EventCrosArg.Unkown;

            switch (tf.Timeframe)
            {
                case TF.TimeFrame.Fast:
                    x = isGolden == true ? EventCrosArg.Gold_fast : EventCrosArg.Dead_fast;
                    break;

                case TF.TimeFrame.Mid:
                    x = isGolden == true ? EventCrosArg.Gold_midt : EventCrosArg.Dead_mid;
                    break;

                case TF.TimeFrame.slow:
                    x = isGolden == true ? EventCrosArg.Gold_slow : EventCrosArg.Dead_slow;
                    break;
            }
            return x;
        }

        public virtual void OnCross(CrossEvent e)
        {
            Cross?.Invoke(this, e);

            CloseCloud(e);
            DictionaryManager(e.CurrentCloud);
            SwitCloud(e);
        }

        private void DictionaryManager(Cloud cl)
        {
            List<Cloud> li = new List<Cloud>();

            switch (cl.Time_F.Timeframe)
            {
                case TimeFrame.Fast:
                    if (MidCloudDictionary.Any())
                    {
                        try
                        {
                            MidCloudDictionary[MidCloudDictionary.Keys.Last(key => key.Buffer < cl.Buffer)].Add(cl);
                        }
                        catch (Exception)
                        {
                            this.StatusLog = $"Errore Nel Dizionario  ... TF {CurrentMidCloud.Time_F.Timeframe.ToString()}";
                        }
                    }

                    if (SlowCloudDictionary.Any())
                    {

                        try
                        {
                            SlowCloudDictionary[SlowCloudDictionary.Keys.Last(key => key.Buffer < cl.Buffer)].Add(cl);
                        }
                        catch (Exception)
                        {
                            this.StatusLog = $"Errore Nel Dizionario  ... TF {CurrentSlowCloud.Time_F.Timeframe.ToString()}";
                        }
                    }
                   
                    break;
                case TimeFrame.Mid:
                    if (!MidCloudDictionary.Any())
                    {
                        MidCloudDictionary.Add(cl,li);
                    }
                    else if (MidInSlowDictionary.Any())
                    {
                        try
                        {
                            MidInSlowDictionary[MidInSlowDictionary.Keys.Last(key => key.Buffer < cl.Buffer)].Add(cl);
                        }
                        catch (Exception)
                        {
                            this.StatusLog = $"Errore Nel Dizionario  ... TF {CurrentSlowCloud.Time_F.Timeframe.ToString()}";
                        }
                    }

                    break;
                case TimeFrame.slow:
                    if (!SlowCloudDictionary.Any())
                        SlowCloudDictionary.Add(cl,li);
                    if (!MidInSlowDictionary.Any())
                        MidInSlowDictionary.Add(cl, li);
                    return;
                
            }
        }

        private void SwitCloud(CrossEvent e)
        {
            switch (e.CurrentCloud.Time_F.Timeframe)
            {
                case TimeFrame.Fast:
                    Cloud x = new Cloud(this.CurrentCloud.Id+1, e.BarIndex, e.CurrentCloud.Time_F);
                    this.CurrentCloud = x;
                    break;
                case TimeFrame.Mid:
                    Cloud y = new Cloud(this.CurrentMidCloud.Id + 1, e.BarIndex, e.CurrentCloud.Time_F);
                    this.CurrentMidCloud = y;
                    break;
                case TimeFrame.slow:
                    Cloud z = new Cloud(this.CurrentSlowCloud.Id + 1, e.BarIndex, e.CurrentCloud.Time_F);
                    break;
            }
        }

        private void CloseCloud(CrossEvent e)
        {
            switch (e.CurrentCloud.Time_F.Timeframe)
            {
                case TimeFrame.Fast:
                    this.CurrentCloud.CloudIsClosed(e.Price);
                    this.Clouds.Add(e.CurrentCloud);
                    break;
                case TimeFrame.Mid:
                    this.CurrentMidCloud.CloudIsClosed(e.Price);
                    this.CloudsMid.Add(e.CurrentCloud);
                    break;
                case TimeFrame.slow:
                    this.CurrentSlowCloud.CloudIsClosed(e.Price);
                    this.CloudSlow.Add(e.CurrentCloud);
                    break;
            }
        }
    }
}
