using System;
using System.Collections.Generic;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Class_Lybrary
{
    public interface IPosizioni
    {
        public enum IPosi_Status { Waiting, OnOpening, OnClosing };
        public int Id { get; set; }
        public IPosi_Status Status { get; set; }
        public List<Order> Orders { get; set; }
        public List<Position> Posizioni { get; set; }
        public List<TradeTiket> TiketsList { get; set; }
        public TradeTiket CurrenTick { get; set; }

        public const string Name = "";

        public List<double> GetPrice(Cloud cloud);
        public RequestParameters GetRequestParameters(List<double> targets);
        public bool Buy_Sel();
    }
}
