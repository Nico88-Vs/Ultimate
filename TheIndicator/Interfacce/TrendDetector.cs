using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Utils;

namespace TheIndicator.Interfacce
{
    [Serializable]
    public class TrendDetector : GetCondiction
    {
        public enum Trend
        {
            Uptrend,
            Downtrend,
            Flat
        }

        public override int Buffer { get; }
        public override CloudSeries Series { get; set; }
        public override string Description { get; }
        public override string Name { get; }
        public override Sentiment Sentiment { get; set; }

        private Dictionary<Cloud, List<Cloud>> Fast_Mid;
        private Dictionary<Cloud, List<Cloud>> Fast_Slow;
        private Dictionary<Cloud, List<Cloud>> Mid_Slow;


        public TrendDetector(CloudSeries serie) : base(serie)
        {
            Fast_Mid = Series.MidCloudDictionary;
            Fast_Slow = Series.SlowCloudDictionary;
            Mid_Slow= Series.MidInSlowDictionary;

        }
        
        public void Evalutate( Cloud slowCloud, List<Cloud> fastList)
        {
            CloudColor color = slowCloud.Color;

            List<Bases> listLenta = slowCloud.BasesList.FindAll(p => p.LineSeries == fastList[0].SlowSeries );
            List<Bases> listVeloce = slowCloud.BasesList.FindAll(p => p.LineSeries == fastList[0].FastSeries);

            Bases lungaLenta = listLenta.OrderByDescending(p => p.Lenght).FirstOrDefault();
            Bases lungaveloce = listVeloce.OrderByDescending(p => p.Lenght).FirstOrDefault();
        }

        public override void DisplayCondiction(TF.TimeFrame tf) => throw new NotImplementedException();
    }
}
