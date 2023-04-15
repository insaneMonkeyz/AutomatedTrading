using System.Runtime.CompilerServices;
using TradingConcepts;
using Quik.Entities;
using Quik.Lua;
using TradingConcepts.CommonImplementations;

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

        public static readonly object Lock = new();

        public static bool UpdateOrderBook(OrderBook book)
        {
            lock (Quik.SyncRoot)
            {
                GET_METOD.LogQuikFunctionCall(book.Security.ClassCode, book.Security.Ticker);

                bool updated = false;

                if (Quik.Lua.ExecFunction(GET_METOD, Api.TYPE_TABLE, book.Security.ClassCode, book.Security.Ticker))
                {
                    if (Quik.Lua.ReadRowValueInteger(BIDS_COUNT) > 0 &&
                        Quik.Lua.PushColumnValueTable(BIDS))
                    {
                        book.UseBids(_quotesReader);
                        Quik.Lua.PopFromStack();
                        updated = true;
                    }

                    if (Quik.Lua.ReadRowValueInteger(ASKS_COUNT) > 0 &&
                        Quik.Lua.PushColumnValueTable(ASKS))
                    {
                        book.UseAsks(_quotesReader);
                        Quik.Lua.PopFromStack();
                        updated = true;
                    }
                }

                Quik.Lua.PopFromStack();

                return updated;
            }
        }

        private static readonly OneSideQuotesReader _quotesReader = ReadQuotes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadQuotes(Quote[] quotes, Operations operation, long marketDepth)
        {
            const int LAST_ITEM = -1;

            var dataLen = (long)Api.lua_rawlen(Quik.Lua, LAST_ITEM);
            var quotesSize = Math.Min(dataLen, marketDepth);

            if (quotesSize <= 0)
            {
                return;
            }

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
                if (Api.lua_rawgeti(Quik.Lua, LAST_ITEM, luaIndex++) != Api.TYPE_TABLE)
                {
                    Quik.Lua.PopFromStack();
                    throw new QuikApiException("Array of quotes ended prior than expected. ");
                }

                if (Quik.Lua.TryFetchDecimalFromTable(PRICE, out Decimal5 price) &&
                    Quik.Lua.TryFetchIntegerFromTable(SIZE, out long size))
                {
                    quotes[thisIndex] = new Quote
                    {
                        Price = price,
                        Size = size,
                        Operation = operation
                    };

                    Quik.Lua.PopFromStack();

                    thisIndex += increment;
                    passed++;
                }
                else
                {
                    Quik.Lua.PopFromStack();

                    break;
                }
            }
        }
    }
}
