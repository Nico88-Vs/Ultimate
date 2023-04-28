using StrategyRun.Headg_Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TheIndicator.Enum;
using TheIndicator.LibreriaDiClassi;
using TradingPlatform.BusinessLayer;

namespace StrategyRun.Order_Placer
{
    public class CustomOrderPlacer
    {
        List<Trade> Trades { get; set; }
        List<Position> ActivePositions { get; set; }
        List<Order> ActiveOrderders { get; set; }
        public string Status { get; private set; } = "Initializing";

        private string Name = "First Custom Order Placer";
        private List<Order> openedOrders;
        private bool closed = false;
        private double mainTradesAmmount;
        private double coverTradesAmmount;
        private double remainingCoverQuantity = 0;
        private double remainingTradesQuantity = 0;
        private Symbol symbol;
        private Account account;
        private double min_Risk;
        private int numberOfCloses;

        public CustomOrderPlacer(Symbol sy, Account ac, double totalExposion, double exposionCoverRapport = 0.5, double min_Risk = 0.5, int numberOfCloses = 3)
        {
            symbol = sy;
            account = ac;
            mainTradesAmmount = totalExposion * (1 - exposionCoverRapport);
            coverTradesAmmount = exposionCoverRapport * totalExposion;
            Trades = new List<Trade>();
            ActivePositions = new List<Position>();
            ActiveOrderders = new List<Order>();
            openedOrders = new List<Order>();
            this.min_Risk = min_Risk;
            this.numberOfCloses = numberOfCloses;

            Core.Instance.OrderAdded += Instance_OrderAdded;
            Core.Instance.PositionAdded += Instance_PositionAdded;
            Core.Instance.TradeAdded += Instance_TradeAdded;
            Core.Instance.PositionRemoved += Instance_PositionRemoved;

            Status = "Started";
        }


        public List<PlaceOrderRequestParameters> SetTarget(Cloud cloud, double target, TypeOfPosition type)
        {
            List<PlaceOrderRequestParameters> output = new List<PlaceOrderRequestParameters>();

            if (cloud.Color == CloudColor.white)
                return null;

            List<double> levelsList = new List<double>();
            List<double> tpList = new List<double>();
            List<double> sList = new List<double>();

            if (levelsList.Any())
                levelsList.Clear();

            double tp = target;
            double sl = target * min_Risk;

            double clMin = 0;

            switch (cloud.Color)
            {
                case CloudColor.green:
                    clMin = cloud.MaximaFast.Any() ? cloud.MaximaFast.Last().Value : cloud.EndPrice;
                    break;
                case CloudColor.red:
                    clMin = cloud.MinimaFast.Any() ? cloud.MinimaFast.Last().Value : cloud.EndPrice;
                    break;
            }
            double clEnd = cloud.EndPrice;

            levelsList.Add(clMin);
            levelsList.Add(clEnd);
            List<Bases> orderedBaseList = new List<Bases>();

            if (cloud.BasesList.Any() & cloud.BasesList.Count > numberOfCloses - 2)
            {
                orderedBaseList = cloud.Color == CloudColor.red ? cloud.BasesList.OrderBy(b => b.Value).ToList() : cloud.BasesList.OrderByDescending(b => b.Value).ToList();
                for (int i = 0; i < numberOfCloses - 2; i++)
                    levelsList.Add(orderedBaseList[i].Value);
            }
            else
            {
                levelsList.Clear();

                double top = clEnd - clMin + clEnd;
                double delta = top - clMin;

                double step = delta / numberOfCloses;

                for (int i = 0; i < numberOfCloses; i++)
                {
                    levelsList.Add(clMin + step * i);
                }
            }

            if (!levelsList.Any())
            {
                Core.Instance.Loggers.Log("Emply Level List", LoggingLevel.Error);
                return null;
            }

            if (tpList.Any() || sList.Any())
            {
                tpList.Clear();
                sList.Clear();
            }

            else
            {
                for (int i = 0; i < levelsList.Count; i++)
                {
                    switch (cloud.Color)
                    {
                        case CloudColor.green:
                            tpList.Add(levelsList[i] - levelsList[i] * tp);
                            sList.Add(levelsList[i] + levelsList[i] * sl);
                            break;

                        case CloudColor.red:
                            tpList.Add(levelsList[i] + levelsList[i] * tp);
                            sList.Add(levelsList[i] - levelsList[i] * sl);
                            break;
                    }
                }
            }

            if (levelsList != null)
            {
                List<SlTpHolder> sls = new List<SlTpHolder>();
                List<SlTpHolder> tps = new List<SlTpHolder>();

                for (int i = 0; i < tpList.Count; i++)
                {
                    SlTpHolder _sl = SlTpHolder.CreateSL(price: sList[i], PriceMeasurement.Absolute, quantityPercentage: 100);
                    sls.Add(_sl);

                    SlTpHolder _tp = SlTpHolder.CreateTP(price: tpList[i], PriceMeasurement.Absolute, quantityPercentage: 100);
                    tps.Add(_tp);
                }

                string orderTypeId = Core.Instance.OrderTypes.FirstOrDefault(x => x.ConnectionId == symbol.ConnectionId && x.Behavior == OrderTypeBehavior.Limit).Id;
                double qt = type == TypeOfPosition.Cover ? coverTradesAmmount : mainTradesAmmount;

                for (int i = 0; i < levelsList.Count; i++)
                {
                    var request = new PlaceOrderRequestParameters
                    {
                        Symbol = symbol,
                        OrderTypeId = orderTypeId,
                        Account = account,
                        Side = cloud.Color == CloudColor.red ? Side.Buy : Side.Sell,
                        Quantity = qt / levelsList.Count,
                        Price = levelsList[i],
                        StopLoss = sls[i],
                        TakeProfit = tps[i],
                    };
                    output.Add(request);
                }
            }

            List<PlaceOrderRequestParameters> x = new List<PlaceOrderRequestParameters>();

            foreach (PlaceOrderRequestParameters item in output)
            {
                item.Comment = TypeOfPosition.Cover.ToString();
                PlaceOrderRequestParameters pc = PlaceOrder(item);
                if (pc != null)
                    x.Add(pc);
            }

            return x;
        }

        private PlaceOrderRequestParameters PlaceOrder(PlaceOrderRequestParameters placeOrderRequest)
        {
            if (closed)
                return null;

            if (placeOrderRequest.Comment != TypeOfPosition.Cover.ToString() || placeOrderRequest.Comment != TypeOfPosition.Main.ToString())
                Log("Uncorrect Comment ... should be close", LoggingLevel.Trading);

            CalculateRamainingQuantity();

            if (placeOrderRequest.Comment == TypeOfPosition.Main.ToString())
                placeOrderRequest.Quantity = placeOrderRequest.Quantity <= remainingTradesQuantity ? placeOrderRequest.Quantity : remainingTradesQuantity;

            if (placeOrderRequest.Comment == TypeOfPosition.Cover.ToString())
                placeOrderRequest.Quantity = placeOrderRequest.Quantity <= remainingCoverQuantity ? placeOrderRequest.Quantity : remainingCoverQuantity;

            if (placeOrderRequest.Quantity <= 0)
            {
                Log("Arlady Exposed", LoggingLevel.Trading);
                return null;
            }

            placeOrderRequest.SendingSource = Name;
            return placeOrderRequest;

            //var resoul = Core.Instance.PlaceOrder(placeOrderRequest);

            //// Waiting Time
            //Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            //if (resoul.Status == TradingOperationResultStatus.Success)
            //    openedOrders.Add(Core.Instance.Orders.Where(x => x.Id == resoul.OrderId).Last());

            //else
            //{
            //    this.Log("Order Failed", LoggingLevel.Trading);
            //    Close();
            //}

        }

        #region Events
        private void Instance_PositionRemoved(Position obj)
        {
            if (ActivePositions.Contains(obj))
                ActivePositions.Remove(obj);

            if (ActivePositions != Core.Instance.Positions.ToList())
                Log("Active position != Positions, should be close", LoggingLevel.Trading);
        }

        private void Instance_TradeAdded(Trade obj)
        {
            if (!Trades.Contains(obj))
                Trades.Add(obj);
        }

        private void Instance_PositionAdded(Position obj)
        {
            if (!ActivePositions.Contains(obj))
                ActivePositions.Add(obj);
        }
        private void Instance_OrderAdded(Order obj)
        {
            if (openedOrders.Contains(obj))
                ActiveOrderders.Remove(obj);
        }
        #endregion

        #region Services
        public void Close()
        {

            Status = "Closed";
            //this.closed = true;
        }

        private void Log(string message, LoggingLevel loglevel)
        {
            Core.Instance.Loggers.Log(message, loglevel);
        }

        private void CalculateRamainingQuantity()
        {
            double tradeQty = Core.Instance.Positions.Where(x => x.Side == Side.Sell).Sum(x => x.Quantity) +
                Core.Instance.Orders.Where(x => x.Comment == TypeOfPosition.Main.ToString() && x.Status == OrderStatus.Opened).Sum(x => x.TotalQuantity);

            double coverQty = Core.Instance.Positions.Where(x => x.Side == Side.Sell).Sum(x => x.Quantity) +
                Core.Instance.Orders.Where(x => x.Comment == TypeOfPosition.Cover.ToString() && x.Status == OrderStatus.Opened).Sum(x => x.TotalQuantity);

            remainingTradesQuantity = mainTradesAmmount - tradeQty;
            remainingCoverQuantity = coverTradesAmmount - coverQty;
        }
        #endregion
    }
}
