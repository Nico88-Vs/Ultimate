using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace TheIndicator.Interfacce
{
    [Serializable]
    public abstract class GetCondiction : IGetCondition
    {
        public abstract CloudSeries  Series { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int Buffer{ get; }
        public abstract Sentiment Sentiment { get; set; }

        private static int Id = 0;

        public GetCondiction(CloudSeries serie)
        {
            this.Series = serie;
            Id = Id ++;
        }
       
        public  Condiction.Stato ReturnStato()
        {
            Condiction objCondiction = new Condiction();
            return objCondiction.StatusProp;
        }
        public Condiction.Strutture ReturnStrutture()
        {
            Condiction objCondiction = new Condiction();
            return objCondiction.StruttureProp;
        }
        public Condiction.Trend ReturnTrend()
        {
            Condiction objCondiction = new Condiction();
            return objCondiction.TrendProp;
        }
        public Condiction.Type ReturnType()
        {
            Condiction objCondiction = new Condiction();
            return objCondiction.TypeProp;
        }

        public abstract void DisplayCondiction(TF.TimeFrame tf );


        public int GetId()
        {
            return Id;
        }
        public void ResetId()
        {
            Id = 0;
        }
    }
}
