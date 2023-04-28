using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Utils;

namespace TheIndicator.LibreriaDiClassi
{
    public enum TypeOfMin_Max
    {
        MaximaFast,
        MinimaFast,
        MaximaSlow,
        MinimaSlow,
        bases
    }

    public enum CloudColor
    {
        green = 1,
        red = -1,
        white = 0
    }

    public class Cloud
    {
        public int Id { get; set; }
        public int Buffer { get; set; }
        public TF Time_F { get; set; }
        public bool IsOpen { get; set; }
        public int Length { get; set; }
        public double EndPrice { get; private set; }
        public List<double> LenghtList { get; private set; }
        public double OriginPrice { get; set; }
        public List<double> AverageList { get; set; }
        public List<double> ThickList { get; set; }
        public List<double> MomentumList { get; set; }
        public double Thickness { get; private set; }
        public CloudColor Color { get; private set; }
        public List<Bases> RoofList { get; set; } 
        public List<Bases> BasesList { get; set; } 
        public List<Min_Max> MaximaFast { get; set; }
        public List<Min_Max> MinimaFast { get; set; }
        public List<Min_Max> MaximaSlow { get; set; }
        public List<Min_Max> MinimaSlow { get; set; }

        //test
        public List<double> Condiction { get; set; }


        #region on going
        public event EventHandler<GapAddedArgs> GapsAdded;
        public List<Gaps> Gap { get; set; }
        #endregion

        public int FastSeries { get; set; }
        public int SlowSeries { get; set; }
        public List<double> FastValue { get; private set; }
        public List<double> SlowValue { get; private set; }

        private Indicator Indicator;

        public Cloud(int id, int buffer, TF tF , double originPrice)
        {
            this.Id = id;
            this.Time_F = tF;
            this.FastSeries = this.Time_F.FastSeries;
            this.SlowSeries = this.Time_F.SlowSeries;
            this.Indicator = this.Time_F.Indicatore;
            this.Buffer = buffer;
            this.IsOpen= true;
            this.Length= 1;
            this.OriginPrice = originPrice;

            LenghtList= new List<double>();
            this.LenghtList.Add(this.Length);

            AverageList= new List<double>();
            this.AverageList.Add(0);

            ThickList = new List<double>();
            this.ThickList.Add(0);

            this.MomentumList= new List<double>();
            this.MomentumList.Add(0);

            this.Gap = new List<Gaps>();

            this.RoofList = new List<Bases>();
            this.BasesList = new List<Bases>();

            this.MinimaFast = new List<Min_Max>();
            this.MinimaSlow= new List<Min_Max>();
            this.MaximaFast = new List<Min_Max>();
            this.MaximaSlow = new List<Min_Max> ();

            this.Condiction = new List<double>();

            this.SlowValue= new List<double>();
            this.FastValue= new List<double>();
            this.FastValue.Add(0);
            this.SlowValue.Add(0);
        }

        public void UpdateCloud(int ofset)
        {
            if (IsOpen)
            {

                double fastserie = this.Indicator.GetValue(lineIndex: this.FastSeries, offset: ofset);
                this.FastValue.Add(fastserie);

                double slowserie = this.Indicator.GetValue(lineIndex: this.SlowSeries, offset: ofset);
                this.SlowValue.Add(slowserie);

                if (this.Length == 1)
                {
                    switch (fastserie - slowserie)
                    {
                        case > 0:
                            this.Color = CloudColor.green;
                            break;

                        case < 0:
                            this.Color = CloudColor.red;
                            break;

                        default:
                            this.Color = CloudColor.white;
                            break;
                    }
                }

                this.Length++;
                LenghtList.Add(this.Length);

                this.Thickness = Math.Abs(fastserie - slowserie);
                this.ThickList.Add(this.Thickness);

                this.AverageList.Add(GetAverage(this.ThickList));

                this.MomentumList.Add(Math.Atan2(this.Length, this.Thickness));

                this.EndPrice = (fastserie + slowserie) / 2;

                BasesUpdate(RoofList, ofset, this.FastSeries);
                BasesUpdate(BasesList, ofset, this.SlowSeries);

                UpdateMinMax(this.FastSeries, ofset);
                UpdateMinMax(this.SlowSeries, ofset);

                MenageGaps(ofset);
            }
        }

        private void UpdateMinMax(int lineseries, int offset)
        {
            double line = this.Indicator.GetValue(lineIndex: lineseries, offset: offset);
            int b = this.Buffer+this.Length;

            // Fast
            if (lineseries == this.FastSeries)
            {
                Min_Max min = new Min_Max(this.MinimaFast.Count, line, this.Length, b);
                Min_Max max = new Min_Max(this.MaximaFast.Count, line, this.Length, b);

                // minimi
                if (MinimaFast.Count < 1)
                {
                    if (min.Value < OriginPrice)
                        MinimaFast.Add(min);
                }
                else if (MinimaFast.Count >= 1)
                {
                    if (min.Value < MinimaFast[MinimaFast.Count - 1].Value)
                        MinimaFast.Add(min);
                }

                //massimi
                if (MaximaFast.Count < 1)
                {
                    if (max.Value > OriginPrice)
                        MaximaFast.Add(max);
                }
                else if (MaximaFast.Count >= 1)
                {
                    if (max.Value > MaximaFast[MaximaFast.Count - 1].Value)
                        MaximaFast.Add(min);
                }
            }

            // Slow
            if (lineseries == this.SlowSeries)
            {
                Min_Max min = new Min_Max(this.MinimaSlow.Count, line, this.Length, b);
                Min_Max max = new Min_Max(this.MaximaSlow.Count, line, this.Length, b);

                // minimi
                if (MinimaSlow.Count < 1)
                {
                    if (min.Value < OriginPrice)
                        MinimaSlow.Add(min);
                }
                else if (MinimaSlow.Count >= 1)
                {
                    if (min.Value < MinimaSlow[MinimaSlow.Count - 1].Value)
                        MinimaSlow.Add(min);
                }

                //massimi
                if (MaximaSlow.Count < 1)
                {
                    if (max.Value > OriginPrice)
                        MaximaSlow.Add(max);
                }
                else if (MaximaSlow.Count >= 1)
                {
                    if (max.Value > MaximaSlow[MaximaSlow.Count - 1].Value)
                        MaximaSlow.Add(min);
                }
            }

            else return;
        }

        public int[] BufferArray()
        {
            int[] output = new int[this.Length];
            for (int i = 0; i < this.Length; i++)
            {
                output[i] = this.Buffer + i;
            }
            return output;
        }

        public void CloudIsClosed( double endPrice, double endbarIndex)
        {
            this.IsOpen = false;
            this.EndPrice = endPrice;
        }

        public double GetAverage(List<double> array)
        {
            double sum = 0;

            foreach (double item in array)
            {
                sum += item;
            }

            return sum/array.Count;
        }

        private void MenageGaps(int ofset)
        {
            if ( Gap.Any() && Gap.Last().GapStatus == Gaps.Status.Running)
            {
                Gaps g = LokingForGaps(ofset);
                Gaps last = Gap.Last();

                switch (g.GapStatus)
                {
                    case Gaps.Status.Waiting:
                        last.GapStatus = Gaps.Status.Waiting;
                        Gap[Gap.Count- 1] = last;
                        break;
                    case Gaps.Status.Running:
                        if(last.type != g.type)
                            last.type = g.type;
                        last.Lenght++;
                        last.EndPrice = g.EndPrice;
                        Gap[Gap.Count - 1] = last;
                        break;
                }
            } 
            else if (!Gap.Any() || Gap.Last().GapStatus == Gaps.Status.Waiting)
            {
                Gaps x = LokingForGaps(ofset);
                Gaps least = new Gaps();

                if (x.GapStatus == Gaps.Status.Running)
                {
                    switch (Gap.Any() == true)
                    {
                        case true:
                            least = Gap.Last();
                            x.Id = least.Id++;
                            x.Lenght = 1;
                            x.Buffer = this.Buffer + this.Length;
                            GapAddedArgs arg1 = new GapAddedArgs(this.Id, x);
                            Gap.Add(x);
                            OnGapAdded(arg1);
                            break;

                        case false:
                            x.Id = 0;
                            x.Lenght = 1;
                            x.Buffer = this.Buffer + this.Length;
                            GapAddedArgs arg = new GapAddedArgs(this.Id, x);
                            Gap.Add(x);
                            OnGapAdded(arg);
                            break;
                    }
                }
               
            }
        }

        public virtual void OnGapAdded(GapAddedArgs e)
        {
            GapsAdded?.Invoke(this, e);
        }

        private Gaps LokingForGaps(int ofset)
        {
            Gaps g = new Gaps(0, ofset, this.Time_F.Delay);
            g.GapStatus = Gaps.Status.Waiting;

            int thk = g.GapThick;

            Gaps b = new Gaps(0, ofset, this.Time_F.Delay);
            b.GapStatus = Gaps.Status.Waiting;

            if (thk >= this.Length)
                return g;

            double fastNow = this.Indicator.GetValue(lineIndex: this.FastSeries, offset: ofset);
            double slowNow = this.Indicator.GetValue(lineIndex: this.SlowSeries, offset: ofset);
            double fastBack = this.Indicator.GetValue(lineIndex: this.FastSeries, offset: ofset + thk);
            double slowBack = this.Indicator.GetValue(lineIndex: this.SlowSeries, offset: ofset + thk);

            switch (this.Color)
            {
                case CloudColor.green:
                    if (slowNow > fastBack || fastNow < slowBack)
                    {
                        g.GapStatus = Gaps.Status.Running;
                        g.GapReason = Gaps.Reason.lines;
                        g.StartPrice = slowBack;
                        g.EndPrice = fastNow;
                        g.type = slowNow > fastBack ? Gaps.Type.regular : Gaps.Type.inverse;
                    }

                    if (BasesList.Any() && RoofList.Any())
                    {
                        if (BasesList.Last().Value >= RoofList.Last().Value)
                        {
                            b.GapStatus = Gaps.Status.Running;
                            b.GapReason = Gaps.Reason.bases;
                            b.type = Gaps.Type.regular;
                            b.StartPrice = slowBack;
                            b.EndPrice = fastNow;
                        }
                    }
                    break;
                case CloudColor.red:
                    if (slowNow < fastBack || fastNow > slowBack)
                    {
                        g.GapStatus = Gaps.Status.Running;
                        g.GapReason = Gaps.Reason.lines;
                        g.StartPrice = slowBack;
                        g.EndPrice = fastNow;
                        g.type = slowNow < fastBack ? Gaps.Type.regular : Gaps.Type.inverse;
                    }

                    if (BasesList.Any() && RoofList.Any())
                    {
                        if (BasesList.Last().Value <= RoofList.Last().Value)
                        {
                            b.GapStatus = Gaps.Status.Running;
                            b.GapReason = Gaps.Reason.bases;
                            b.type = Gaps.Type.regular;
                            b.StartPrice = slowBack;
                            b.EndPrice = fastNow;
                        }
                    }
                    break;

                case CloudColor.white:
                    break;
            }

            Gaps output = new Gaps();

            if (g.GapStatus == Gaps.Status.Waiting && b.GapStatus == Gaps.Status.Waiting)
            {
                output = g;
            }


            if (g.GapStatus == Gaps.Status.Running && b.GapStatus == Gaps.Status.Running)
            {
                g.GapReason = Gaps.Reason.both;
                output = g;
            }
            else if (g.GapStatus == Gaps.Status.Running || b.GapStatus == Gaps.Status.Running)
            {
                if (g.GapStatus != Gaps.Status.Running)
                    output = b;

                if (b.GapStatus != Gaps.Status.Running)
                {
                    output = g;
                }
            }

            return output;
        }

        private void BasesUpdate(List<Bases> list, int ofset, int serie)
        {
            double now = this.Indicator.GetValue(lineIndex: serie, offset: ofset);
            double before = this.Indicator.GetValue(lineIndex: serie, offset: ofset + 1);

            if (!list.Any() || list.Last().BStatus == Bases.Status.Waiting)
            {
                if (now == before)
                {
                    int id = 0;
                    if (list.Any())
                        id = list.Last().Id;
                    Bases bases = new Bases(id, serie, now, this.Buffer+this.Length-1);
                    bases.Lenght = 1;
                    list.Add(bases);
                }
            }
            else if (list.Any() && list.Last().BStatus == Bases.Status.Running )
            {
                if (now == list.Last().Value)
                {
                    Bases b = list.Last();
                    b.Lenght += 1;
                    list[list.Count - 1] = b;
                }
                else if (now != list.Last().Value)
                {
                    Bases b = list.Last();
                    b.Close();
                    list[list.Count - 1] = b;
                }

            }
        }

        public double? GetArea()
        {
            double? output = null;

            if (this.ThickList.Any())
                output = this.ThickList.Sum()/this.Length;

            return output;
        }

        public List<double> DysplayObj( TypeOfMin_Max typeminmax )
        {
            List<double> list = new List<double>();
            List<Min_Max> cList = new List<Min_Max>();

            switch (typeminmax)
            {
                case TypeOfMin_Max.MaximaFast:
                    cList = this.MaximaFast;
                    break;
                case TypeOfMin_Max.MinimaFast:
                    cList = this.MinimaFast;
                    break;
                case TypeOfMin_Max.MaximaSlow:
                    cList = this.MinimaSlow;
                    break;
                case TypeOfMin_Max.MinimaSlow:
                    cList = this.MinimaSlow;
                    break;
            }

            if (!cList.Any())
            {
                for (int i = 0; i < this.LenghtList.Count; i++)
                {
                    list.Add(0);
                }
            }
            else if (cList.Any())
            {
                int[] buff = new int[cList.Count];
                int[] leng = new int[cList.Count];
                double[] val = new double[cList.Count];

                for (int i = 0; i < cList.Count; i++)
                {
                    buff[i] = cList[i].Buffer;
                    leng[i] = cList[i].Length;
                    val[i] = cList[i].Value;
                }

                for (int i = 0; i < this.LenghtList.Count; i++)
                {
                    double x = 0;
                    for (int z = 0; z < buff.Length; z++)
                    {
                        if (this.Buffer + i >= buff[z] && this.Buffer + i < buff[z] + leng[z])
                        {
                            x = val[z];
                        }
                    }

                    list.Add(x);
                }
            }

            return list;
        }

        public List<double> DysplayObj(List<Bases> cList)
        {
            List<double> list = new List<double>();

            if (!cList.Any())
            {
                for (int i = 0; i < this.LenghtList.Count; i++)
                {
                    list.Add(0);
                }
            }
            else if (cList.Any())
            {
                int[] buff = new int[cList.Count];
                int[] leng = new int[cList.Count];
                double[] val = new double[cList.Count];

                for (int i = 0; i < cList.Count; i++)
                {
                    buff[i] = cList[i].Buffer;
                    leng[i] = cList[i].Lenght;
                    val[i] = cList[i].Value;
                }

                for (int i = 0; i < this.LenghtList.Count; i++)
                {
                    double x = 0;
                    for (int z = 0; z < buff.Length; z++)
                    {
                        if (this.Buffer + i >= buff[z] && this.Buffer + i < buff[z] + leng[z])
                        {
                            x = val[z];
                        }
                    }

                    list.Add(x);
                }
            }

            return list;
        }

        //crea un enum che definisca tre scenari nuvola che s'nspessisce, si assottiglia o rimane invariata
        public enum CloudBehavior
        {
            Thickening,
            Thinning,
            Unchanged
        }

       //cre un enum che rappresenti tre colori rosso, verde , giallo
       public enum colori
        {
            Red,
            Green,
            Yellow
        }
    }
}
