namespace Quik.EntityProviders.QuikApiWrappers
{
    [Flags]
    internal enum OrderFlags : long
    {
        IsAlive = 1,
        //  IsFilled => !IsCancelled && !IsAlive
        IsCancelled = 1 << 1,
        IsSellOrder = 1 << 2,
        IsLimitOrder = 1 << 3,
        // ??? 1 << 4
        IsFillOrKill = 1 << 5,
        IsMarketMaker = 1 << 6,
        IsHidden = 1 << 7,
        IsCancelRest = 1 << 8,
        IsIceberg = 1 << 9,
        IsRejected = 1 << 10,
    }
}
