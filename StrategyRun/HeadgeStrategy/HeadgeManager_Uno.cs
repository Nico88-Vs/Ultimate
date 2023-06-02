using StrategyRun.Class_Lybrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.HeadgeStrategy
{
    public class HeadgeManager_Uno : IHeadgeStrategy
    {
        public HadgedPositions Mains { get; set; }
        public List<Trade> Trades { get; set; }
        public HadgedPositions Cover { get; set; }
        public double Covers_Percent { get; set; }
        public IHeadgeStrategy.HeadgeStatus Status { get; set; } = IHeadgeStrategy.HeadgeStatus.Waiting;
        public string Name { get; set; } = "HeadgeManager_Uno";
        public string Description { get; set; } = "Non Implementa Multi Orders";
        public double  TotalAmmount { get; set; }
        public CloudSeries Series { get; set; }
        public TradeTiket LastTiket { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public HeadgeManager_Uno(double totalAmm, double coverPerc, CloudSeries series)
        {
            this.TotalAmmount = totalAmm;
            this.Covers_Percent = coverPerc;
            this.Series = series;
            Mains = new HadgedPositions();
            Cover = new HadgedPositions();
        }

        public void EvaluateTiket(TradeTiket t)
        {
            EvaluateStatus();

            if (Status == IHeadgeStrategy.HeadgeStatus.FullyExpo)
                return;

            if (Cover.Posizioni.Sum(x => x.Quantity) >= TotalAmmount * Covers_Percent && t.Comment == IHeadgeStrategy.coverComment)
                return;

            if(Mains.Posizioni.Sum(x => x.Quantity) >= TotalAmmount * 1-Covers_Percent && t.Comment == IHeadgeStrategy.mainComment)
                return;

            if (t.Comment == IHeadgeStrategy.mainComment)
            {
                Cloud cl = Get_tiketCloud(t, this.Series);
            }
            else if (t.Comment == IHeadgeStrategy.coverComment && this.Status != IHeadgeStrategy.HeadgeStatus.Waiting)
            {
                Cloud cl = Get_tiketCloud(t, this.Series);
            }   
        }
        public void CloseOrder() => throw new NotImplementedException();
        public void ModifyOrder(double lastclose) => throw new NotImplementedException();
        public void OpenOrder(Cloud c, string comment)
        {
            double openPrice = c.OriginPrice;

            var placeOrderReq = new PlaceOrderRequestParameters()
            {
                Price = openPrice,
                Comment = comment,
            };
        }
        public void SetCovers(PlaceOrderRequestParameters p, TradeTiket t) => throw new NotImplementedException();

        //Static Optional Methods
        public static Cloud Get_tiketCloud(TradeTiket tiket, CloudSeries Series)
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

        private void EvaluateStatus()
        {
            double main_expositions = Core.Instance.Positions.Where(x => x.Comment == IHeadgeStrategy.mainComment).ToList().Sum(x => x.Quantity);
            double covers_expositions = Core.Instance.Positions.Where(x => x.Comment == IHeadgeStrategy.coverComment).ToList().Sum(x => x.Quantity);

            //double ceking expositions 
            double main_expositions_list = Mains.Posizioni.Where(x => x.Comment == IHeadgeStrategy.mainComment).ToList().Sum(x => x.Quantity);
            double covers_expositions_list = Cover.Posizioni.Where(x => x.Comment == IHeadgeStrategy.coverComment).ToList().Sum(x => x.Quantity);

            if (main_expositions != main_expositions_list || covers_expositions != covers_expositions_list)
                Log("List Orders or Positions Dont Match", LoggingLevel.Trading);

            double expositions = Math.Abs(main_expositions_list) + Math.Abs(covers_expositions_list);

            switch (expositions)
            {
                case 0:
                    if (Mains.Posizioni.Count != 0 || Cover.Posizioni.Count != 0)
                        Status = IHeadgeStrategy.HeadgeStatus.Waiting;
                    else
                        Status = IHeadgeStrategy.HeadgeStatus.FullyExpo;
                    break;

                case > 0:
                    if(!Cover.Posizioni.Any())
                        Status = IHeadgeStrategy.HeadgeStatus.Exposed;
                    else
                        Status = IHeadgeStrategy.HeadgeStatus.Covered;
                    break;

                case < 0:
                    Log("Over Covered", LoggingLevel.Trading);
                    break;
            }
        }   

        public void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }

    }
}
