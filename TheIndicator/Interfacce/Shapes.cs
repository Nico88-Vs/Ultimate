using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace TheIndicator.Interfacce
{
    [Serializable]
    public class Shapes : GetCondiction
    {
        public enum shape
        {
            Flat = 1,
            Trend = 2,
            Gapped = 3,
            GappedReverse = 4,
            Uknown = 5
        }
        public override string Name { get; }
        public override int Buffer { get; }
        public override string Description { get; }
        public override CloudSeries Series { get; set; }
        public bool GapFilter { get; set; }
        public override Sentiment Sentiment { get; set; }


        public Shapes( CloudSeries serie, bool filter) : base(serie)
        {
            this.GapFilter = filter;
        }

        public shape SetShape(Cloud c)
        {
            shape output = shape.Uknown;

            List<Gaps> filtred = new List<Gaps>();

            switch (GapFilter)
            {
                case true:
                    filtred = c.Gap.FindAll(b => b.GapReason == Gaps.Reason.lines || b.GapReason == Gaps.Reason.both);
                    break;

                case false:
                    filtred = c.Gap;
                    break;
            }

            if (!filtred.Any())
            {
                double maxdelta = c.ThickList.Max();
                double cldelta = Math.Abs(c.OriginPrice - c.EndPrice);
                output = cldelta < maxdelta? shape.Flat : shape.Trend;
            }
            else if (filtred.Any())
            {
                output = ChekReverse(c, filtred) == true ? shape.GappedReverse : shape.Gapped;
            }

            return output;
        }

        private bool ChekReverse(Cloud cl, List<Gaps> listgaps)
        {
            bool basi = false;

            int initBuffer = listgaps.Last().Buffer;
            int lenght = initBuffer - cl.Buffer;

            List<double> partial = cl.FastValue.Skip(lenght).ToList();

            List<Bases> ordinata = new List<Bases>();
            List<Bases> listbases = cl.BasesList.FindAll(x => x.Buffer >= initBuffer);

            switch (cl.Color)
            {
                case CloudColor.green:
                    if(!listbases.Any())
                        basi = true;
                    if (listbases.Any())
                    {
                        ordinata = listbases.OrderBy(x => x.Value).ToList();
                        if (ordinata == listbases)
                            basi = true;
                    }
                    break;

                case CloudColor.red:
                    if (!listbases.Any())
                        basi = true;
                    if (listbases.Any())
                    {
                        ordinata = listbases.OrderByDescending(x => x.Value).ToList();
                        if (ordinata == listbases)
                            basi = true;
                    }
                    break;

                case CloudColor.white:
                    break;
            }

            if (partial.Count > 4)
            {
                double max = partial.Min();
                int posizione = partial.IndexOf(max);

                List<double> li = cl.ThickList.Skip(posizione).ToList();
                List<double> medie = new List<double>();

                for (int i = 0; i < li.Count - 3; i++)
                {
                    double av = (li[i] + li[i + 1] + li[i + 2]) / 3;
                    medie.Add(av);
                }

                int count = 0;
                for (int i = medie.Count-2; i >= 0; i--)
                {
                    if (medie[i] >= medie[i + 1])
                        count++;
                    else if (medie[i] < medie[i + 1])
                        break;
                }

                if(count < medie.Count/3)
                    basi = false;
                
            }
            else if (partial.Count <= 4)
                basi = false;

            return basi;
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
                    clouds = Series.CloudsMid;
                    break;
                case TF.TimeFrame.Slow:
                    clouds = Series.CloudSlow;
                    break;
            }

            foreach (Cloud cloud in clouds)
            {
                cloud.Condiction.Clear();

                shape x = SetShape(cloud);

                for (int i = 0; i < cloud.LenghtList.Count; i++)
                {
                    cloud.Condiction.Add((double)x);
                }
            }
        }
    }
}
