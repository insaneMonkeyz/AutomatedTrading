using System;

namespace Strategies
{
    public sealed class FrontrunningStrategy : Strategy
    {
        public FrontrunningOrderOperator SellOrder { get; }
        public FrontrunningOrderOperator BuyOrder { get; }
    }

    public sealed class FrontrunningOrderOperator 
        : SingleOrderOperator
    {
        public decimal PriceOffsetLimit 
        {
            get => _priceOffset;
            set => _priceOffset = value;
        }
        public decimal PriceGapToIgnoreSmallSizes { get; set; }
        public decimal GroupPriceRange { get; set; }
        public int FrontrunTriggerSize { get; set; }
        public bool FrontrunMyPrice { get; set; }
        public bool DontStepBack { get; set; }
        public TimeSpan ReactionTimeThreshold { get; set; }
        public TimeSpan StepBackTimeLimit { get; set; }
    }
}
