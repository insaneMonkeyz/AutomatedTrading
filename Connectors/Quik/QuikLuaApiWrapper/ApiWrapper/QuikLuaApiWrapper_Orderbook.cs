using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApi;

namespace QuikLuaApi
{
    //public partial class QuikLuaApiWrapper
    //{
    //    internal OrderbookParsingResult UpdateOrderBook(IOptimizedOrderBook orderbook, string classCode, string ticker)
    //    {
    //        OrderbookParsingResult result = default;

    //        if (_localState.ExecFunction(QuikLuaApi.QuikApi.QuikApi.GET_ORDERBOOK_METHOD, LuaApi.TYPE_TABLE, classCode, ticker))
    //        {
    //            static bool processQuotes(string countField, string quotesField, Action<OneSideQuotesReader> reader)
    //            {
    //                if (_localState.TryFetchLongFromTable(countField, out long qCount) && qCount > 0)
    //                {
    //                    _localState.PushColumnValueTable(quotesField);
    //                    reader(ReadQuotes);
    //                    _localState.PopFromStack();
    //                    return true;
    //                }
    //                else
    //                {
    //                    return false;
    //                }
    //            }

    //            if (processQuotes("bid_count", "bid", orderbook.UseBids))
    //            {
    //                result |= OrderbookParsingResult.BidsParsed;
    //            }
    //            if (processQuotes("offer_count", "offer", orderbook.UseAsks))
    //            {
    //                result |= OrderbookParsingResult.AsksParsed;
    //            }
    //        }

    //        // pop getQuoteLevel2 result
    //        _localState.PopFromStack();
    //        return result;
    //    }

    //    private static void ReadQuotes(Quote[] quotes, Operations operation, long marketDepth)
    //    {
    //        const int LAST_ITEM = -1;

    //        var dataLen = (long)LuaApi.lua_rawlen(_localState, LAST_ITEM);
    //        var quotesSize = Math.Min(dataLen, marketDepth);

    //        if (quotesSize > 0)
    //        {
    //            long passed = 0;
    //            long thisIndex = 0;
    //            long luaIndex = 1;
    //            long increment = 1;

    //            if (operation == Operations.Buy && quotesSize != 1)
    //            {
    //                luaIndex = dataLen - quotesSize - 1;
    //                thisIndex = quotesSize - 1;
    //                increment = -1;
    //            }

    //            while (passed < quotesSize)
    //            {
    //                if (LuaApi.lua_rawgeti(_localState, LAST_ITEM, luaIndex++) != LuaApi.TYPE_TABLE)
    //                {
    //                    _localState.PopFromStack();
    //                    throw new QuikApiException("Array of quotes ended prior than expected. ");
    //                }

    //                if (_localState.TryFetchDecimalFromTable("price", out Decimal5 price) &&
    //                    _localState.TryFetchLongFromTable("quantity", out long size))
    //                {
    //                    quotes[thisIndex] = new Quote
    //                    {
    //                        Price = price,
    //                        Size = size,
    //                        Operation = operation
    //                    };

    //                    _localState.PopFromStack();

    //                    thisIndex += increment;
    //                    passed++;
    //                }
    //                else
    //                {
    //                    _localState.PopFromStack();

    //                    break;
    //                }
    //            }
    //        }
    //    }
    //}
}
