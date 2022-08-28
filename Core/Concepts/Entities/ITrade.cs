﻿namespace Core.Concepts.Entities
{
    public interface ITrade
    {
        DateTimeOffset TimeStamp { get; }
        ISecurity Security { get; }
        Decimal5 Price { get; }
        Side Side { get; }
        long Size { get; }
    }
}