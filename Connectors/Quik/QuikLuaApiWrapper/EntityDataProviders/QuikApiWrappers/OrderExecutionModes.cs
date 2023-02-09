namespace Quik.EntityProviders.QuikApiWrappers
{
    internal enum MoexOrderExecutionModes
    {
        Undefined = 0,
        FillOrKill,
        Queue,
        CancelRest,
        GoodTillCanceled,
        GoodTillDate,
        GoodTillSessionEnd,
        Open,
        Close,
        Cross,
        GoodTillNextSession,
        CancelOnDisconnect,
        GoodTillTime,
        NextAuction,
    }
}
