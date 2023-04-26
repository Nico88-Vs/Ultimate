// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TheIndicator.Enum;
using TheIndicator.Interfacce;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace TheIndicator
{
	public class TheIndicator : Indicator 
    {
        [InputParameter("GapsFiltered", 1)]
        private bool gapFilter = true;

        private TF Fast;
        private TF Mid;
        private TF Slow;
        private CloudSeries Series;
        private Indicator Indi;
        private XmlSerializer Serializzatore;

        //Test
        Displayer condizione;
        Shapes shapeCondiction;
        Shapes opposiShape;

        public TheIndicator()
            : base()
        {
            Name = "TheIndicator";
            Description = "My indicator's annotation";

            AddLineSeries("line1", Color.CadetBlue, 1, LineStyle.Solid);
            AddLineSeries("line1", Color.Red, 1, LineStyle.Solid);
            AddLineSeries("line1", Color.Violet, 1, LineStyle.Solid);
            AddLineSeries("line1", Color.White, 1, LineStyle.Solid);

            SeparateWindow = true;
        }

        protected override void OnInit()
        {
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

            this.HistoricalData.AddIndicator(Indi);

            Fast = new TF(TF.TimeFrame.Fast, 1, Indi, 13, 14);
            Mid = new TF(TF.TimeFrame.Mid, 5, Indi, 3, 4);
            Slow = new TF(TF.TimeFrame.Slow, 30, Indi, 8, 9);

            Series = new CloudSeries(this.HistoricalData, Fast, Mid, Slow);

            List<TF> tfList = new List<TF> {Fast, Mid, Slow};
            Series.GenerateCloud(tfList);

            //test
            condizione = new Displayer(Series);
            shapeCondiction = new Shapes(Series, gapFilter);
            bool b = gapFilter == true ? false : true;
            opposiShape = new Shapes(Series, b);
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            if (args.Reason == UpdateReason.NewBar)
            {
                Series.Update(Fast);
                Series.Update(Mid);
                Series.Update(Slow);
            }

            ////SetValue(1);
            //List<Cloud> li = Series.CloudsMid;
            //List<Cloud> di = GetFromD(li[li.Count-4]);

            //this.LinesSeries[0].Width = 3;


            //DysplaySeries(di, TypeOfValue.id, this.LinesSeries[0]);
            //DysplaySeries(di, TypeOfValue.lenght, this.LinesSeries[1]);
            //DysplaySeries(Series.Clouds, TypeOfValue.id, this.LinesSeries[0]);
            //Series.GetBuffer(this.LinesSeries[1]);

            //cond.DisplayCondiction(this.LinesSeries[0]);
            //DysplaySeries(Series.CloudSlow, condizione, this.LinesSeries[2]);
            //DysplaySeries(Series.CloudSlow, shapeCondiction, this.LinesSeries[1]);
            //DysplaySeries(Series.Clouds, TypeOfValue.minFast, this.LinesSeries[1]);
            //DysplaySeries(Series.Clouds, TypeOfValue.id, this.LinesSeries[0]);
            DysolayScenario(this.LinesSeries[0]);
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            var gr = args.Graphics;
            Font f = new Font("Arial", 10);
            Font w = new Font("Arial", 65);

            //int x = Series.Clouds.Count;
            //List<Cloud> li = Series.CloudsMid;
            gr.DrawString($"GapsFilter  {gapFilter}", f, Brushes.YellowGreen, 10, 200);
            //gr.DrawString($"FIrst Fast Buffer {GetFromD(li[li.Count - 2]).First().Buffer}", f, Brushes.Azure, 10, 225);
            //gr.DrawString($"mancante buffer {this.Count - Series.Clouds.Find(x => x.Id == 106).Buffer}", f, Brushes.Azure, 10, 250);
            //gr.DrawString($"Last Buffer slow {Series.Clouds[-2].MinimaFast[0].Buffer}", f, Brushes.Azure, 10, 250);
            //gr.DrawString($"penultima buffer {Series.Clouds[-2].Buffer}", f, Brushes.Azure, 10, 275);
            //gr.DrawString($"penultima buffer -- {Series.Clouds.Last().MinimaFast[0].Value}", f, Brushes.Azure, 10, 300);
            //gr.DrawString($"penultima buffer -- {Series.Clouds.Last().MinimaFast.Count}", f, Brushes.Azure, 10, 325);





            //if (!isPeriodSync)
            //    gr.DrawString($"Periods ARE NOT Sync", w, Brushes.Red, 10, 60);
        }


        private void DysplaySeries(List<Cloud> clouds, TypeOfValue type, LineSeries lineSeries)
        {
            if (!clouds.Any())
                return;

            int delay = clouds.FirstOrDefault().Time_F.GetCorrectBuffer(Series.TenkanPeriod);

            for (int i = 0; i < clouds.Count; i++)
            {
                Cloud c = clouds[i];
                int[] x = c.BufferArray();
                for (int w = 0; w < x.Length; w++)
                {
                    if (this.Count > x[w])
                    {
                        lineSeries.TimeShift = delay;
                        double output = GetTypeOfValue(c, type, w);
                        lineSeries.SetValue(output, offset: this.Count - x[w]);
                    }
                    
                }

            }
        }

        private void DysolayScenario(LineSeries line)
        {
            line.TimeShift = Series.TenkanPeriod;
            int x = 0;

            for (int i = Series.ScenarioList.Count; i > 0; i--)
            {
                IchimokuCloudScenario s = Series.ScenarioList[this.Series.ScenarioList.Count - i];
                x++;

                line.SetValue((int)s, offset: i-1);
            }
        }

        private void DysplaySeries(List<Cloud> clouds, GetCondiction con, LineSeries lineSeries)
        {
            if (!clouds.Any())
                return;

            con.DisplayCondiction(clouds[0].Time_F.Timeframe);

            int delay = clouds.FirstOrDefault().Time_F.GetCorrectBuffer(Series.TenkanPeriod);

            for (int i = 0; i < clouds.Count; i++)
            {
                Cloud c = clouds[i];
                int[] x = c.BufferArray();
                for (int w = 0; w < x.Length; w++)
                {
                    if (this.Count > x[w])
                    {
                        lineSeries.TimeShift = delay;
                        double output = c.Condiction[w];
                        if (output != 0)
                            lineSeries.SetValue(output, offset: this.Count - x[w]);
                    }

                }

            }
        }

        private void DysplaySeries(List<Cloud> clouds, Displayer con, LineSeries lineSeries, TypeOfMin_Max ty)
        {
            if (!clouds.Any())
                return;

            con.DisplayCondiction(clouds[0].Time_F.Timeframe, ty);

            int delay = clouds.FirstOrDefault().Time_F.GetCorrectBuffer(Series.TenkanPeriod);

            for (int i = 0; i < clouds.Count; i++)
            {
                Cloud c = clouds[i];
                int[] x = c.BufferArray();
                for (int w = 0; w < x.Length; w++)
                {
                    if (this.Count > x[w])
                    {
                        lineSeries.TimeShift = delay;
                        double output = c.Condiction[w];
                        if (output != 0)
                        {
                            lineSeries.SetValue(output, offset: this.Count - x[w]);
                        }
                    }

                }

            }
        }

        private List<Cloud> GetFromD(Cloud cloud)
        {
            List<Cloud> list = new List<Cloud>();

            switch (cloud.Time_F.Timeframe)
            {
                case TF.TimeFrame.Fast:
                    return list;
                case TF.TimeFrame.Mid:
                    try
                    {
                        list = Series.MidCloudDictionary[cloud];
                        
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    break;
                    
                case TF.TimeFrame.Slow:
                    try
                    {
                        list = Series.SlowCloudDictionary[cloud];
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    break;
            }
            return list;
        }

        private double GetTypeOfValue( Cloud c, TypeOfValue type, int index)
        {
            switch (type)
            {
                case TypeOfValue.lenght:
                    return c.LenghtList[index];
                case TypeOfValue.id:
                    return c.Id;
                case TypeOfValue.buffer:
                    return c.Buffer;
                case TypeOfValue.buffer_plus_lenght:
                    return c.Buffer + c.LenghtList[index];
                case TypeOfValue.minFast:
                    //c.DysplayObj(c.MaximaFast);
                    return 0;
                case TypeOfValue.color:
                    double x = 3;
                    switch (c.Color)
                    {
                        case CloudColor.green:
                            x = 1;
                            break;
                        case CloudColor.red:
                            x = -1;
                            break;
                        case CloudColor.white:
                            x = 0;
                            break;
                    }
                    return x;
                default:
                    return 0;

            }
        }

        private void Serializza(TF oggetto, string name )
        {
            Serializzatore = new XmlSerializer(typeof(TF));

            using (StreamWriter writer = new StreamWriter(name))
            {
                Serializzatore.Serialize(writer, oggetto);
            }
        }
    }
}
