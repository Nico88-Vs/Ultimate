using System;
using System.Collections.Generic;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Modules;
using TheIndicator;
using TheIndicator.LibreriaDiClassi;
using System.Linq;
using TheIndicator.Enum;
using System.Threading;
using TradingPlatform.BusinessLayer.LocalOrders;

namespace PlaceOrder
{
    public class PlaceOrder : OrderPlacingStrategy
    {
        [InputParameter("NumberClose", 1, increment: 1, maximum: 5)]
        private int numberOfCloses = 3;

        [InputParameter("TP%", 2 , increment: 0.001)]
        private double tp = 0.01;

        [InputParameter("SL%", 2, increment: 0.001)]
        private double sl = 0.01;

        [InputParameter("Ammount", 4)]
        private double ammount;

        [InputParameter("Account", 5)]
        private Account account;

        [InputParameter("Symbol", 6)]
        private Symbol symbol;

        [InputParameter("ConnectionID", 7)]
        private string connectionID;

        [InputParameter("History Type", 8, variants: new object[]
        {
            "HistoryType.Last", HistoryType.Last,
            "HistoryType.Bid", HistoryType.Bid,
            "HistoryType.Ask", HistoryType.Ask,
        })]
        private HistoryType hyTy;

        private CancellationTokenSource cts;
        private bool finisch;
        private PlaceOrderRequestParameters placerOrdeReq;

        public PlaceOrder()
          : base()
        {
            this.Name = "First Order Strategy";
            this.Description = "It's a test _ price type not implemented";

            //Core.OrdersHistoryAdded += this.Core_OrdersHistoryAdded;
            //Core.PositionAdded += this.Core_PositionAdded;
            //Core.OrderAdded += this.Core_OrderAdded;
            //Core.PositionRemoved += this.Core_PositionRemoved;
            //Core.TradeAdded += this.Core_TradeAdded;
            //Core.ClosedPositionAdded += this.Core_ClosedPositionAdded;
        }

        private void Core_OrderAdded(Order obj) => throw new NotImplementedException();
        private void Core_PositionAdded(Position obj) => throw new NotImplementedException();


        protected override void OnPlaceOrder(PlaceOrderRequestParameters placeOrderRequest)
        {
            string localorderId = string.Empty;

            try
            {
                this.placerOrdeReq = placeOrderRequest;
                this.cts= new CancellationTokenSource();

                var localOrder = new LocalOrder
                {
                    Symbol = placeOrderRequest.Symbol,
                    Account = placeOrderRequest.Account,
                    Side = placeOrderRequest.Side,
                    TotalQuantity = placeOrderRequest.Quantity,
                    OrderType = new CustomOrderType(this.Name, placeOrderRequest.OrderType),
                    TimeInForce = placeOrderRequest.TimeInForce,
                    Price = placeOrderRequest.Price,
                    TriggerPrice = placeOrderRequest.TriggerPrice,
                    TrailOffset = placeOrderRequest.TrailOffset

                };

                localorderId = Core.Instance.LocalOrders.AddOrder(localOrder);
                Core.Instance.LocalOrders.Updated += LocalOrders_Updated;
                placeOrderRequest.Symbol.NewQuote += this.Symbol_NewQuote;

                //while (!this.finisch && !this.cts.IsCancellationRequested) 
                    //Thread.Sleep(1);
            }
            finally
            {
                Core.Instance.LocalOrders.Updated -= LocalOrders_Updated;
                Core.Instance.LocalOrders.RemoveOrder(localorderId);
            }

            void LocalOrders_Updated(object sender, LocalOrderEventArgs e)
            {
                var localOrder = e.LocalOrder;

                if (localorderId != localOrder.Id)
                    return;

                if (e.Lifecycle == EntityLifecycle.Removed)
                {
                    this.cts?.Cancel();
                    return;
                }

                placerOrdeReq.Price = localOrder.Price;
                placerOrdeReq.TriggerPrice = localOrder.TriggerPrice;
                placerOrdeReq.TrailOffset = localOrder.TrailOffset;
                placerOrdeReq.Quantity = localOrder.TotalQuantity;
                placerOrdeReq.TimeInForce = localOrder.TimeInForce;
                var placeOrderAdditionalParameters = placeOrderRequest.AdditionalParameters;
                placeOrderAdditionalParameters.UpdateValues(localOrder.AdditionalInfo);
                placerOrdeReq.AdditionalParameters = placeOrderAdditionalParameters;
            }
        }

        private void Symbol_NewQuote(Symbol symbol, Quote quote) => this.Processprice(quote.Bid);

        private void Processprice(double price)
        {
            if(this.finisch || this.cts.IsCancellationRequested)
                return;

            //Log("Manca Implementazione Per Acquisti Intelligenti", LoggingLevel.Trading);

            //try
            //{
            //    PlaceOrderRequestParameters markRequest = placerOrdeReq;
            //    markRequest.SendingSource = "source";
            //}
            //finally
            //{
            //    this.finisch = false;
            //}
        }



        protected override void OnCancel() => this.cts?.Cancel();

        private void Log(string message, LoggingLevel lvl)
        {
            Core.Instance.Loggers.Log(message, lvl);
        }

        private void Core_TradeAdded(Trade obj) => throw new NotImplementedException();
        private void Core_PositionRemoved(Position obj) => throw new NotImplementedException();
        private void Core_ClosedPositionAdded(ClosedPosition obj) => throw new NotImplementedException();
        private void Core_OrdersHistoryAdded(OrderHistory obj) => throw new NotImplementedException();


    }
}