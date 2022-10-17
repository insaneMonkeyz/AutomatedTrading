﻿using BasicConcepts;

namespace QuikLuaApi
{
    internal class SecurityParamsContainer
    {
        public string Ticker;
        public string ClassCode;
        public long PricePrecisionScale;
        public long ContractSize;
        public string Description;
        public Decimal5 MinPriceStep;
        public Currencies DenominationCurrency;

        public bool StateIsCorrect
        {
            get
            {
                return
                    !string.IsNullOrEmpty(Ticker) &&
                    !string.IsNullOrEmpty(ClassCode) &&
                    PricePrecisionScale >= 0 &&
                    ContractSize > 0 &&
                    Description != null &&
                    MinPriceStep > 0;
            }
        }
    }
}
