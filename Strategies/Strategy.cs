using Strategies.StrategySwitching;
using System;

namespace Strategies
{
    public abstract class Strategy: ISettingsProvider, ISwitchable
    {
        public virtual int CurrentPosition { get; }
        public virtual int PositionSellLimit { get; set; }
        public virtual int PositionBuyLimit { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual bool IsActive { get; set; }
        public bool IsOn
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        protected SingleOrderOperator[] orderOperators;

        public virtual string GetSettings()
        {
            throw new NotImplementedException();
        }
        public virtual void LoadFromSettings(string settings)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class SingleOrderOperator : ISettingsProvider, ISwitchable
    {
        public virtual PriceProvider QuotesProvider { get; set; }
        public virtual PriceDifferenceTracker PriceDifferenceTracker { get; }
        public virtual Security Security { get; }
        public virtual Operations Operation { get; }
        public virtual int OrderSize { get; set; }
        public virtual bool IsEnabled { get; set; }
        public bool IsOn
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        protected decimal _priceOffset;

        public virtual string GetSettings()
        {
            throw new NotImplementedException();
        }
        public virtual void LoadFromSettings(string settings)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsOffsetValidToBuy
        {
            get => throw new NotImplementedException();
        }
        protected virtual bool IsOffsetValidToSell
        {
            get => throw new NotImplementedException();
        }
    }
}
