using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.Entities
{
    internal class OrderBook : IOptimizedOrderBook
    {
        private const long DEFAULT_MARKET_DEPTH = 10;
        private const long MAX_MARKET_DEPTH = 50;
        private const long MIN_MARKET_DEPTH = 1;
        private const long LAST_ITEM_INDEX = MAX_MARKET_DEPTH - 1;

        private readonly object _bidsLock = new();
        private readonly object _asksLock = new();

        private readonly Quote[] _bids = new Quote[MAX_MARKET_DEPTH];
        private readonly Quote[] _asks = new Quote[MAX_MARKET_DEPTH];

        private long _marketDepth = DEFAULT_MARKET_DEPTH;

        public long MarketDepth 
        { 
            get => _marketDepth; 
            set 
            {
                if (_marketDepth > MAX_MARKET_DEPTH ||
                    _marketDepth < MIN_MARKET_DEPTH)
                {
                    throw new ArgumentOutOfRangeException(nameof(MarketDepth), 
                        $"Value must be between {nameof(MAX_MARKET_DEPTH)} and {MIN_MARKET_DEPTH} constants value");
                }

                _marketDepth = value;

                lock (_asksLock)
                {
                    for (long i = value - 1; i < LAST_ITEM_INDEX; i++)
                    {
                        _asks[i] = default;
                    }
                }

                lock (_bidsLock)
                {
                    for (long i = value - 1; i < LAST_ITEM_INDEX; i++)
                    {
                        _bids[i] = default;
                    }
                }
            }
        }

        public Security Security { get; init; }
        ISecurity IOrderBook.Security => Security;

        public Quote[] Bids
        {
            get
            {
                lock (_bidsLock)
                {
                    return Clone(_bids);
                }
            }
        }
        public Quote[] Asks
        {
            get
            {
                lock (_asksLock)
                {
                    return Clone(_asks);
                }
            }
        }

        private Quote[] Clone(Quote[] src)
        {
            var copy = new Quote[src.Length];

            var srchandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            var copyhandle = GCHandle.Alloc(copy, GCHandleType.Pinned);

            try
            {
                unsafe
                {
                    var pcopy = (Quote*)copyhandle.AddrOfPinnedObject().ToPointer();
                    var psrc  = (Quote*)srchandle .AddrOfPinnedObject().ToPointer();

                    // no, this is not the same to iterating
                    // through a regular array

                    for (int i = 0; i < _marketDepth; i++)
                    {
                        pcopy[i] = psrc[i];
                    }

                    return copy;
                }
            }
            finally
            {
                srchandle.Free();
                copyhandle.Free();
            }
        }

        public void UseQuotes(QuotesReader reader, long marketDepth)
        {
            lock (_bidsLock)
            {
                lock (_asksLock)
                {
                    reader(_bids, _asks, _marketDepth);
                }
            }
        }
        public void UseBids(OneSideQuotesReader reader)
        {
            lock (_bidsLock)
            {
                reader(_bids, Operations.Buy, _marketDepth);
            }
        }
        public void UseAsks(OneSideQuotesReader reader)
        {
            lock (_asksLock)
            {
                reader(_asks, Operations.Sell, _marketDepth);
            }
        }
    }
}
