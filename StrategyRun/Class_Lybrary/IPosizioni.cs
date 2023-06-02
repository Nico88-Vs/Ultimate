using System;
using System.Collections.Generic;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;
using System.Linq;
using TheIndicator.Enum;

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
        public CloudSeries Serie { get; set; }

        public const string Name = "";
        public const string mainComment = "Main";
        public const string coverComment = "Cover";

        // 0 per Ingressi e 1 per Uscite
        public List<double>[] GetIn_Out_Prices(TradeTiket tiket);
        public bool Buy_Sel();
        //Get the cloud of the tiket
        public static Cloud Get_tiketCloud(TradeTiket tiket , CloudSeries Series)
        {
            switch (tiket.TFrame.Timeframe)
            {
                case TF.TimeFrame.Fast:
                    return Series.Clouds.FirstOrDefault(x => x.Id == tiket.ClaudId);
                case TF.TimeFrame.Mid:
                    return Series.CloudsMid.FirstOrDefault(x => x.Id == tiket.ClaudId);
                case TF.TimeFrame.Slow:
                    return Series.CloudSlow.FirstOrDefault(x => x.Id == tiket.ClaudId);
                default: return null;
            }
        }
        //Get the out level of the Upper Tf cloud??
        public static List<double> GetOutLevels_UpperTF(TradeTiket tiket, string constant, CloudSeries serie)
        {
            List<double> levels = new List<double>();

            Cloud? outCloud = null;
            foreach (KeyValuePair<Cloud, List<Cloud>> item in tiket.Dyctionary)
            {
                if (item.Value.Any(c => c.Id == tiket.ClaudId))
                {
                    outCloud = item.Key;
                    break;
                }
            }

            if (outCloud != null)
            {
                List<Bases> l = outCloud.BasesList;
                switch (constant)
                {
                    case mainComment:
                        if (l.Count > 2)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                levels.Add(l[i].Value);
                            }
                        }
                        else
                        {
                            levels.Add(Get_tiketCloud(tiket, serie).OriginPrice);
                        }
                        break;

                    case coverComment:
                        if (l.Count > 2)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                levels.Add(l[l.Count - 1 - i].Value);
                            }
                        }
                        else
                        {
                            levels.Add(Get_tiketCloud(tiket, serie).EndPrice);
                        }
                        break;

                    default:
                        break;
                }
            }
            return levels;
        }
    }
}
