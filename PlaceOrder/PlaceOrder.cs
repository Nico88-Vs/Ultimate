using System;
using System.Collections.Generic;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Modules;

namespace PlaceOrder
{
    /// <summary>
    /// Information about API you can find here: http://api.quantower.com
    /// Code samples: https://github.com/Quantower/Examples 
    /// </summary>
    public class PlaceOrder : OrderPlacingStrategy
    {
        public PlaceOrder()
            : base()
        {
            // Defines strategy's name and description.
            this.Name = "PlaceOrder";
            this.Description = "My OrderPlacingStrategy's annotation";
        }

        protected override void OnPlaceOrder(PlaceOrderRequestParameters placeOrderRequest)
        {
            throw new NotImplementedException();
        }

        protected override void OnCancel()
        {
            throw new NotImplementedException();
        }
    }
}