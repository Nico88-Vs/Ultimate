using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Modules;

namespace PlaceOrder
{
    public class Limit_PlaceOrder : OrderPlacingStrategy
    {
        [InputParameter("NumberClose", 1, increment: 1, maximum: 5)]
        private int numberOfCloses = 3;

        [InputParameter("TP%", 2, increment: 0.001)]
        private double tp = 0.01;

        [InputParameter("SL%", 2, increment: 0.001)]
        private double sl = 0.01;

        [InputParameter("Ammount", 4)]
        private double ammount;

        public string Status { get; private set; } = "Not Started";
        private bool cancel = false;

        public Limit_PlaceOrder()
            :base()
        {
            this.Name = "First Limit_Order Strategy";
            this.Description = "It's a test _ price type not implemented";
            this.Status = "Started";

            Core.Instance.OrderAdded += this.Instance_OrderAdded;
        }


        protected override void OnPlaceOrder(PlaceOrderRequestParameters placeOrderRequest)
        {
            var positions = Core.Instance.Positions.Where(p => p.Symbol == placeOrderRequest.Symbol && p.Account == placeOrderRequest.Account).ToList();
            var poSum = Math.Abs(positions.Sum(x => x.Quantity));
            placeOrderRequest.SendingSource = this.Name;

            double orderquantity = placeOrderRequest.Quantity;
            var remaining = this.ammount - poSum;

            if (poSum >= remaining)
            {
                this.Status = "Totaly Exposed";
                return;
            }

            if (orderquantity > remaining)
                placeOrderRequest.Quantity = remaining;

            var resoul = Core.Instance.PlaceOrder(placeOrderRequest);

            //Time.Wait __ moved in place order
            while (this.State != OrderPlacingStrategyState.Processing)
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();


            this.Status = resoul.Status.ToString();
            if (resoul.Status == TradingOperationResultStatus.Failure)
                throw new Exception(resoul.Message);

            if (this.cancel)
                return;
        }

        private void Instance_OrderAdded(Order obj)
        {

        }

        protected override void OnCancel()
        {
            this.cancel = true;
        }
    }
}
