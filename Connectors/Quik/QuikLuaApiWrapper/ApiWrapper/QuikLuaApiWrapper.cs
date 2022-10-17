using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;
using QuikLuaApi.Entities;
using QuikLuaApiWrapper;
using static System.Formats.Asn1.AsnWriter;

namespace QuikLuaApi
{

    public partial class QuikLuaApiWrapper
    {
        [Flags]
        internal enum OrderbookParsingResult : long
        {
            BidsParsed = 1,
            AsksParsed = 2
        }

        private static LuaState _quikState;
        private static LuaState _localState;

        public static bool IsConnected
        {
            get => _localState.ExecFunction(
                      name: QuikApi.IS_CONNECTED_METHOD, 
                returnType: LuaApi.TYPE_NUMBER, 
                  callback: _localState.TryPopNumberAsBool);
        }


        internal OrderbookParsingResult UpdateOrderBook(IOptimizedOrderBook orderbook, string classCode, string ticker)
        {
            OrderbookParsingResult result = default;

            if (_localState.ExecFunction(QuikApi.GET_ORDERBOOK_METHOD, LuaApi.TYPE_TABLE, classCode, ticker))
            {
                static bool processQuotes(string countField, string quotesField, Action<OneSideQuotesReader> reader)
                {
                    if (_localState.TryFetchLongFromTable(countField, out long qCount) && qCount > 0)
                    {
                        _localState.PushColumnValueTable(quotesField);
                        reader(ReadQuotes);
                        _localState.PopFromStack();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (processQuotes("bid_count", "bid", orderbook.UseBids))
                {
                    result |= OrderbookParsingResult.BidsParsed;
                }
                if (processQuotes("offer_count", "offer", orderbook.UseAsks))
                {
                    result |= OrderbookParsingResult.AsksParsed;
                }
            }

            // pop getQuoteLevel2 result
            _localState.PopFromStack();
            return result;
        }

        /// <summary>
        /// Entry Point. This method gets called from the lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to the Lua state object</param>
        /// <returns></returns>
        public unsafe int Initialize(void* L)
        {
            LuaState lua = L;

            try
            {
                lua.TieProxyLibrary("NativeToManagedProxy");
                lua.RegisterCallback(Main, "main");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }

            return 0;
        }

        private static int Main(IntPtr state)
        {
            _localState = state;

            try
            {
                System.Diagnostics.Debugger.Launch();

                LuaApi.lua_pushstring(_localState, "one");
                LuaApi.lua_pushstring(_localState, "two");
                LuaApi.lua_pushstring(_localState, "three");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }
            return 1;
        }

        private static void ReadQuotes(Quote[] quotes, Operations operation, long marketDepth)
        {
            const int LAST_ITEM = -1;
            const int SECOND_ITEM = -2;

            var dataLen = (long)LuaApi.lua_rawlen(_localState, LAST_ITEM);
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
                    if (LuaApi.lua_rawgeti(_localState, LAST_ITEM, luaIndex++) != LuaApi.TYPE_TABLE)
                    {
                        _localState.PopFromStack();
                        throw new QuikApiException("Array of quotes ended prior than expected. ");
                    }

                    if (_localState.LastItemIsTable() &&
                        _localState.TryFetchDecimalFromTable("price", out Decimal5 price) &&
                        _localState.TryFetchLongFromTable("quantity", out long size))
                    {
                        quotes[thisIndex] = new Quote
                        {
                            Price = price,
                            Size = size,
                            Operation = operation
                        };

                        _localState.PopFromStack();

                        thisIndex += increment;
                        passed++;
                    }
                    else
                    {
                        _localState.PopFromStack();

                        break;
                    }
                }
            }
        }
    }
}