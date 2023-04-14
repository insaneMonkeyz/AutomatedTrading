using Quik.Entities;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders.Resolvers
{
    internal class TransactionCache
    {
        private readonly int _cacheSize;
        private readonly object _changeLock = new();
        private readonly Order?[] _orders;

        private readonly Log _log = LogManagement.GetLogger(typeof(TransactionCache));

        public int Count { get; private set; }

        public Order? Pop(long transactionId)
        {
            for (int i = 0; i < _cacheSize; i++)
            {
                var candidate = _orders[i];

                if (candidate?.TransactionId == transactionId)
                {
                    lock (_changeLock)
                    {
                        _orders[i] = null;
                        Count--;
#if DEBUG
                        _log.Debug($"Order found in cache: {candidate}"); 
#endif
                    }
                    return candidate;
                }
            }
#if DEBUG
            _log.Debug($"Order with transaction id {transactionId} is not found in cache");
#endif

            return null;
        }
        public Order? Pop(ref TransactionRequestContainer description)
        {
            for (int i = 0; i < _cacheSize; i++)
            {
                var candidate = _orders[i];

                if (description.IsMatching(candidate))
                {
                    lock (_changeLock)
                    {
                        _orders[i] = null;
                        Count--; 
                    }

                    return candidate;
                }
            }

            return null;
        }
        public void Push(Order order)
        {
            lock (_changeLock)
            {
                for (int i = 0; i < _cacheSize; i++)
                {
                    if (_orders[i] == null)
                    {
                        _orders[i] = order;
                        Count++;
#if DEBUG
                        if (Count > _cacheSize * 0.8d)
                        {
                            _log.Debug($@"==============================================================================================
    Attention! Transaction cache is almost full. Cache size: {_cacheSize}. Elements num: {Count}
  ==============================================================================================");
                        } 
#endif
                        return;
                    }
                }

                throw new OverflowException("Cache is full. Consider increasing its size");
            }
        }

        public TransactionCache(int cacheSize)
        {
            if (cacheSize <= 0 || cacheSize > 100)
            {
                throw new ArgumentException($"Size of transaction cache must be between 1 and 100. Requested {cacheSize}", nameof(cacheSize));
            }

            _cacheSize = cacheSize;
            _orders = new Order[cacheSize];
        }
    }
}