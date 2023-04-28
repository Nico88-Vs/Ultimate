using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Interfacce;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace StrategyRun.Strategie
{
    public class TrendStrategy : GetCondiction
    {
        public override CloudSeries Series { get; set; }
        public override string Name { get; }
        public override string Description { get; }
        public override int Buffer { get; }
        public bool GapsFiltered { get; set; }

        private Shapes shDetector;

        public List<Shapes.shape> shapesFastArray;
        public List<Shapes.shape> shapesMidArray;
        public List<Shapes.shape> shapesSlowArray;

        public TrendStrategy(CloudSeries serie, bool gapFilter) : base(serie)
        {
            GapsFiltered = gapFilter;
            shDetector = new Shapes(Series, GapsFiltered);

            GetShape(TF.TimeFrame.Fast);
            GetShape(TF.TimeFrame.Mid);
            GetShape(TF.TimeFrame.Slow);

            Series.Cross += Series_Cross;
        }

        private void Series_Cross(object sender, CrossEvent e)
        {
            switch (e.CurrentCloud.Time_F.Timeframe)
            {
                case TF.TimeFrame.Fast:
                    Shapes.shape shape = shDetector.SetShape(e.CurrentCloud);
                    if (e.CurrentCloud.Id == shapesFastArray.Count)
                        shapesFastArray.Add(shape);
                    else
                        GetShape(TF.TimeFrame.Fast);
                    break;

                case TF.TimeFrame.Mid:
                    Shapes.shape shapeM = shDetector.SetShape(e.CurrentCloud);
                    if (e.CurrentCloud.Id == shapesMidArray.Count)
                        shapesMidArray.Add(shapeM);
                    else
                        GetShape(TF.TimeFrame.Mid);
                    break;
                case TF.TimeFrame.Slow:
                    Shapes.shape shapeS = shDetector.SetShape(e.CurrentCloud);
                    if (e.CurrentCloud.Id == shapesSlowArray.Count)
                        shapesSlowArray.Add(shapeS);
                    else
                        GetShape(TF.TimeFrame.Slow);
                    break;
            }
        }

        private void GetShape(TF.TimeFrame tf)
        {
            List<Cloud> list = new List<Cloud>();
            switch (tf)
            {
                case TF.TimeFrame.Fast:
                    list = Series.Clouds;
                    if (!list.Any())
                        return;
                    shapesFastArray = new List<Shapes.shape>();
                    foreach (Cloud item in list)
                    {
                        Shapes.shape shape = shDetector.SetShape(item);
                        shapesFastArray.Add(shape);
                    }
                    break;

                case TF.TimeFrame.Mid:
                    list = Series.CloudsMid;
                    if (!list.Any())
                        return;
                    shapesMidArray = new List<Shapes.shape>();
                    foreach (Cloud item in list)
                    {
                        Shapes.shape shape = shDetector.SetShape(item);
                        shapesMidArray.Add(shape);
                    }
                    break;

                case TF.TimeFrame.Slow:
                    list = Series.CloudSlow;
                    if (!list.Any())
                        return;
                    shapesSlowArray = new List<Shapes.shape>();
                    foreach (Cloud item in list)
                    {
                        Shapes.shape shape = shDetector.SetShape(item);
                        shapesSlowArray.Add(shape);
                    }
                    break;
            }
        }

        public override void DisplayCondiction(TF.TimeFrame tf) => throw new NotImplementedException();

    }
}
