namespace Core.Tools
{
    public static class TransactionIdGenerator
    {
        private static readonly object _sync = new();

        public static long CreateId()
        {
            lock (_sync)
            {
                return DateTime.Now.Ticks & 0xFFFFFF;
            }
        }
    }
}
