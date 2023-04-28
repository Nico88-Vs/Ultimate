using System;
using System.Collections.Generic;
using System.Linq;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace TheIndicator.Interfacce
{
    public class Gap_Displayer : GetCondiction
    {
        public Condiction Condiction { get; set; }
        public override CloudSeries Series { get; set; }
        public override string Description  { get; }
        public override string Name { get; }
        public Period Period { get; private set; }
        public Symbol Symbol { get; private set; }
        public DateTime StartData { get; private set; }
        public DateTime EndData { get; private set; }
        public override int Buffer { get; }

        public Gap_Displayer( CloudSeries series) : base (series)
        {
            this.Condiction = new Condiction();
            this.Series = series;
            this.Buffer = series.Hd.Count;
            this.Description = "Mostra Oggetti, di base i gap";
            this.Name = "Gap Detector";
            this.Period = this.Series.Hd.Period;
            this.Symbol = this.Series.Hd.Symbol;
            this.StartData = this.Series.Hd.FromTime;
            this.EndData = this.Series.Hd.ToTime;
        }

        public Condiction.Stato ReturnStato()
        {
            Condiction.Stato stato = Condiction.Stato.WaitingCloseLong;
            foreach (Cloud item in Series.Clouds)
            {
                if (item.Gap.Any())
                    stato = Condiction.Stato.WaitingLong;

                else stato = Condiction.Stato.WaitingShort;
            }
            return stato;
        }

        public  Condiction.Strutture ReturnStrutture()
        {
            return Condiction.Strutture.PivotPoint;
        }

        public override void DisplayCondiction(TF.TimeFrame tf)
        {
            List<Cloud> clouds = new List<Cloud>();
            switch (tf)
            {
                case TF.TimeFrame.Fast:
                    clouds = Series.Clouds;
                    break;
                case TF.TimeFrame.Mid:
                    clouds= Series.CloudsMid;
                    break;
                case TF.TimeFrame.Slow:
                    clouds = Series.CloudSlow;
                    break;
            }

            foreach (Cloud item in clouds)
            {
                item.Condiction.Clear();
                double x = 0;
                for (int i = 0; i < item.LenghtList.Count; i++)
                {
                    double y = item.Buffer + item.LenghtList[i]-1;
                    foreach (Gaps gaps in item.Gap)
                    {
                        if (gaps.Buffer == y)
                        {
                            x = gaps.Buffer;
                        }
                    }
                    

                    item.Condiction.Add(x);
                }
            }
        }

        public void DisplayCondiction(TF.TimeFrame tf, TypeOfMin_Max ty)
        {
            List<Cloud> clouds = new List<Cloud>();

            switch (tf)
            {
                case TF.TimeFrame.Fast:
                    clouds = Series.Clouds;
                    break;
                case TF.TimeFrame.Mid:
                    clouds = Series.CloudsMid;
                    break;
                case TF.TimeFrame.Slow:
                    clouds = Series.CloudSlow;
                    break;
            }

            

            foreach (Cloud item in clouds)
            {
                item.Condiction.Clear();

                switch (ty)
                {
                    case TypeOfMin_Max.MaximaFast:
                        item.Condiction = item.DysplayObj(ty);
                        break;
                    case TypeOfMin_Max.MinimaFast:
                        item.Condiction = item.DysplayObj(ty);
                        break;
                    case TypeOfMin_Max.MaximaSlow:
                        item.Condiction = item.DysplayObj(ty);
                        break;
                    case TypeOfMin_Max.MinimaSlow:
                        item.Condiction = item.DysplayObj(ty);
                        break;
                    case TypeOfMin_Max.bases:
                        item.Condiction = item.DysplayObj(item.BasesList);
                        break;
                }
            }
        }
    }
}
