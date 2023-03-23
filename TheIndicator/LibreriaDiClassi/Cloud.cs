using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;

namespace TheIndicator.LibreriaDiClassi
{
    public class Cloud
    {
        public int Id { get; set; }
        public int Buffer { get; set; }
        public TF Time_F { get; set; }
        public bool IsOpen { get; set; }
        public int Length { get; set; }
        public double EndPrice { get; private set; }

        private int FastSeries;
        private int SlowSeries;
        private Indicator Indicator;



        public Cloud(int id, int buffer, TF tF)
        {
            this.Id = id;
            this.Time_F = tF;
            this.FastSeries = this.Time_F.FastSeries;
            this.SlowSeries = this.Time_F.SlowSeries;
            this.Indicator = this.Time_F.Indicatore;
            this.Buffer = buffer;

            this.IsOpen= true;
        }

        public void UpdateCloud(int ofset)
        {
            if (IsOpen)
            {
                this.Length++;
                //FastList.Add(Indicator.GetValue(lineIndex: this.FastSeries, offset: ofset));
                //SlowList.Add(Indicator.GetValue(lineIndex: this.SlowSeries, offset: ofset));
                //this.LenghtList.Add(this.Length);
                //this.Thickness = GetThickness(ofset);
                //ThickList.Add(this.Thickness);
                //this.AverageThickness = GetAvarage(ofset);
                //AverageList.Add(this.AverageThickness);
                //this.Momentum = CalculateMomentum(SlowSeries, FastSeries, ofset);
                //MomentumList.Add(this.Momentum);
                //UpdateMinMax(this.FastSeries, ofset);
                //UpdateMinMax(this.SlowSeries, ofset);
                //UpdateBases(this.SlowSeries, BasesList, ofset, 1);
                //UpdateBases(this.FastSeries, RoofList, ofset, 1);

                //this.Color = IsGreen();
            }
        }

        public void CloudIsClosed( double endPrice)
        {
            this.IsOpen = false;
            this.EndPrice = endPrice;
        }
    }
}
