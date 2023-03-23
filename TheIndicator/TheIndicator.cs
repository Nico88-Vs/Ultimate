// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;

namespace TheIndicator
{
	public class TheIndicator : Indicator
    {
        // il periodo del grafico potrebbe non coincidere
        [InputParameter("Period", 2)]
        private Period PerioBase = Period.MIN1;

        [InputParameter("End Data", 5)]
        private DateTime EndData = Core.TimeUtils.DateTimeUtcNow;

        private Indicator hs;
        public HistoricalData hd;
        private bool isPeriodSync = true;

        //debug
        private int debug = 0;

        public TheIndicator()
            : base()
        {
            Name = "TheIndicator";
            Description = "My indicator's annotation";

            AddLineSeries("line1", Color.CadetBlue, 1, LineStyle.Solid);
            AddLineSeries("line1", Color.Red, 1, LineStyle.Solid);

            SeparateWindow = true;
        }

        protected override void OnInit()
        {
            DateTime startData = CalculateSpan(HistoricalData.Period, EndData);
            hd = this.Symbol.GetHistory(PerioBase, fromTime: startData , toTime : EndData);
            hs = Core.Instance.Indicators.BuiltIn.ICH(9, 26, 52);

            // SELEZIONO L'HD MAGGIORE
            if (HistoricalData.Count > hd.Count)
                hd = HistoricalData;

            hd.AddIndicator(hs);
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            if (HistoricalData.Period != PerioBase)
                isPeriodSync = false;

                SetHistoryValue();
            SetValue(debug, 1);
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            var gr = args.Graphics;
            Font f = new Font("Arial", 10);
            Font w = new Font("Arial", 65);

            gr.DrawString($"HD.count {hd.Count}", f, Brushes.Azure, 10, 125);
            gr.DrawString($"this.count {this.Count}", f, Brushes.Azure, 10, 150);

            if (!isPeriodSync)
                gr.DrawString($"Periods ARE NOT Sync", w, Brushes.Red, 10, 60);
        }

        private void SetHistoryValue()
        {
            for (int i = 0; i< this.HistoricalData.Count; i++)
            {
                double value = hs.GetValue(lineIndex: 0, offset: i);
                SetValue(value, offset: i);
            }

        }

        private DateTime CalculateSpan(Period thisPeriod, DateTime enData)
        {
            Period newPeriod = new Period(thisPeriod.BasePeriod, 80);
            DateTime output = enData.Subtract(newPeriod.Duration);
            return output;
        }
    }
}
