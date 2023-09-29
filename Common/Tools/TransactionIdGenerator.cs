namespace Tools
{
    public static class TransactionIdGenerator
    {
        private static readonly DateTime LastDayUntilOverflow = new(2056, 01, 24);
        private static readonly object _sync = new();
        private static long _counter;

        static TransactionIdGenerator()
        {
            if (DateTime.Now > LastDayUntilOverflow)
            {
                throw new Exception("Congratulations! This shitty app has survived 30 years. " +
                    "But unfortunarely the algorithm used to generate unique numbers can no longer work. " +
                    "It generates numbers based on the tick value of the current time, " +
                    "and from now on, the value of the current date will cause an overflow");
            }

            _counter = (DateTime.Now.Ticks >> 23) & int.MaxValue;
        }

        /// <summary>
        /// Guaranteed to generate unique numbers no more than 102996 times per day
        /// </summary>
        /// <returns></returns>
        public static long CreateId()
        {
            lock (_sync)
            {
                return ++_counter;
            }
        }
    }
}
