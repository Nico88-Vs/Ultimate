using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;

namespace TheIndicator.Interfacce
{
    public abstract class GetCondiction
    {
        public abstract CloudSeries  Series { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int Buffer{ get; }

        private static int Id = 0;

        public GetCondiction(CloudSeries serie)
        {
            this.Series = serie;
            Id = Id ++;
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
