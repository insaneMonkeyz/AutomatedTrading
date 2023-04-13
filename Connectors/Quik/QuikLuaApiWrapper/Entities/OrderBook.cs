using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.Entities
{
    internal class OrderBook : IOptimizedOrderBook, INotifyEntityUpdated
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
                if (_marketDepth is > MAX_MARKET_DEPTH or < MIN_MARKET_DEPTH)
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

        public Security Security { get; }
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

        public event Action Updated = delegate { };

        public OrderBook(Security security)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
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

        public T UseQuotes<T>(QuotesReader<T> reader)
        {
            lock (_bidsLock) lock (_asksLock)
            {
                return reader(_bids, _asks, _marketDepth);
            }
        }
        public void UseQuotes(QuotesReader reader)
        {
            lock (_bidsLock) lock (_asksLock)
            {
                reader(_bids, _asks, _marketDepth);
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

        public override string ToString()
        {
            return $"{nameof(OrderBook)} {Security} Depth: {MarketDepth}";
        }
        public string Print()
        {
            static string read(Quote[] bids, Quote[] asks, long depth)
            {
                var builder = new StringBuilder();

                static void appendAsks(StringBuilder b, Quote q)
                {
                    b.Append('\t');
                    b.Append(q.Price.ToString((uint)Decimal5.EXPONENT, separateThousands: true));
                    b.Append('\t');
                    b.AppendLine(q.Size.ToString());
                }
                static void appendBids(StringBuilder b, Quote q)
                {
                    b.Append(q.Size);
                    b.Append('\t');
                    b.AppendLine(q.Price.ToString((uint)Decimal5.EXPONENT, separateThousands: true));
                }

                for (int i = (int)depth - 1; i >= 0; i--)
                {
                    appendAsks(builder, asks[i]);
                }
                for (int i = 0; i < depth; i++)
                {
                    appendBids(builder, bids[i]);
                }

                return builder.ToString();
            }

            return UseQuotes<string>(read);
        }
    }
}
