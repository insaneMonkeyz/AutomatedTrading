﻿namespace BasicConcepts.SecuritySpecifics
{
    public interface IExpiring
    {
        DateTimeOffset Expiry { get; }
        TimeSpan TimeToExpiry { get; }
    }
}