using System.Runtime.CompilerServices;
using BasicConcepts;
using Quik;
using Quik.Entities;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal static class OrderbookWrapper
    {
        public const string GET_METOD = "getQuoteLevel2";
        public const string CALLBACK_METHOD = "OnQuote";

        private const string BIDS_COUNT = "bid_count";
        private const string ASKS_COUNT = "offer_count";
        private const string BIDS = "bid";
        private const string ASKS = "offer";
        private const string PRICE = "price";
        private const string SIZE = "quantity";
        private const string CLASS_CODE = "class_code";
        private const string TICKER = "sec_code";

        private static LuaState _stack;

        public static string? ClassCode
        {
            get => _stack.ReadRowValueString(CLASS_CODE);
        }
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }

        public static void Set(LuaState stack)
        {
            _stack = stack;
        }

        public static void UpdateOrderBook(OrderBook book)
        {
            lock (Quik.QuikProxy.SyncRoot)
            {
                if (_stack.ExecFunction(GET_METOD, LuaApi.TYPE_TABLE, book.Security.ClassCode, book.Security.Ticker))
                {
                    if (_stack.ReadRowValueLong(BIDS_COUNT) > 0 &&
                        _stack.PushColumnValueTable(BIDS))
                    {
                        book.UseBids(ReadQuotes);
                        _stack.PopFromStack();
                    }

                    if (_stack.ReadRowValueLong(ASKS_COUNT) > 0 &&
                        _stack.PushColumnValueTable(ASKS))
                    {
                        book.UseAsks(ReadQuotes);
                        _stack.PopFromStack();
                    }
                }

                _stack.PopFromStack();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadQuotes(Quote[] quotes, Operations operation, long marketDepth)
        {
            const int LAST_ITEM = -1;

            var dataLen = (long)LuaApi.lua_rawlen(_stack, LAST_ITEM);
            var quotesSize = Math.Min(dataLen, marketDepth);

            if (quotesSize > 0)
            {
                long passed = 0;
                long thisIndex = 0;
                long luaIndex = 1;
                long increment = 1;

                if (operation == Operations.Buy && quotesSize != 1)
                {
                    luaIndex = dataLen - quotesSize - 1;
                    thisIndex = quotesSize - 1;
                    increment = -1;
                }

                while (passed < quotesSize)
                {
                    if (LuaApi.lua_rawgeti(_stack, LAST_ITEM, luaIndex++) != LuaApi.TYPE_TABLE)
                    {
                        _stack.PopFromStack();
                        throw new QuikApiException("Array of quotes ended prior than expected. ");
                    }

                    if (_stack.TryFetchDecimalFromTable(PRICE, out Decimal5 price) &&
                        _stack.TryFetchLongFromTable(SIZE, out long size))
                    {
                        quotes[thisIndex] = new Quote
                        {
                            Price = price,
                            Size = size,
                            Operation = operation
                        };

                        _stack.PopFromStack();

                        thisIndex += increment;
                        passed++;
                    }
                    else
                    {
                        _stack.PopFromStack();

                        break;
                    }
                }
            }
        }
    }
}
