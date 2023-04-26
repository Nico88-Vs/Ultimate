using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;


namespace TheIndicator.Interfacce
{
    public interface IGetCondition
    {
        public Condiction.Stato ReturnStato();
        public Condiction.Trend ReturnTrend();
        public Condiction.Type ReturnType();
        public Condiction.Strutture ReturnStrutture();
    }
}
