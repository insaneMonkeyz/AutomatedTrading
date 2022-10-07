﻿namespace BasicConcepts.SecuritySpecifics.Options
{
    public interface IOptionQuote : IQuote
    {
        IOption ParametersHolder { get; }
        double Volatility { get; }
    }
}
