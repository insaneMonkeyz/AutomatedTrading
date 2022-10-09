using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LuaGate
{
    public enum IconType
    {
        Info = 1,
        Warning = 2,
        Error = 3
    }
    public class Security
    {

    }
    public class LuaGate
    {
        private static LuaState currentState;
        private static IntPtr state;

        public static bool IsConnected
        {
            get
            {
                LuaApi.lua_getglobal(state, "isConnected");
                LuaApi.lua_pcallk(state, 0, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction);
                return LuaApi.lua_tointegerx(state, 0, IntPtr.Zero) != 0;
            }
        }

        public static void Message(string message)
        {
            LuaApi.lua_getglobal(state, "message");
            LuaApi.lua_pushstring(state, message);
            LuaApi.lua_pcallk(state, 1, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction);
        }
        public static void Message(string message, IconType iconType)
        {
            LuaApi.lua_getglobal(state, "message");
            LuaApi.lua_pushstring(state, message);
            LuaApi.lua_pushnumber(state, (double)iconType);
            LuaApi.lua_pcallk(state, 2, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction);
        }
        public static string GetScriptPath()
        {
            LuaApi.lua_getglobal(state, "getScriptPath");
            LuaApi.lua_pcallk(state, 0, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction);
            return Marshal.PtrToStringAnsi(LuaApi.lua_tolstring(state, 0, out ulong len), (int)len);
        }
        public static void GetTradeDate()
        {
            if (ApiHelper.ExecFunction(state, "getTradeDate", LuaApi.TYPE_TABLE))
            {
                var date = ApiHelper.ReadRowValueString(state, "date");
                var year = ApiHelper.ReadRowValueLong(state, "year");
                var month = ApiHelper.ReadRowValueLong(state, "month");
                var day = ApiHelper.ReadRowValueLong(state, "day"); 
            }
        }

        public static void GetSecurityInfo(string classCode, string ticker)
        {
            if (ApiHelper.ExecFunction(state, "getSecurityInfo", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                var faceUnit = ApiHelper.ReadRowValueString(state, "face_unit");
                var shortName = ApiHelper.ReadRowValueString(state, "short_name");
            }
        }
        public static void GetOrderBook(string classCode, string ticker)
        {
            if (ApiHelper.ExecFunction(state, "getQuoteLevel2", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                // stack: orderbooks
                LuaApi.lua_pushnil(state);
                // stack: orderbooks, nil
                while (LuaApi.lua_next(state, -2) != LuaApi.OK_RESULT)
                {
                    // stack: orderbooks, orderbook_n_key,  orderbook_n_value,
                    var bidsCount = int.TryParse(ApiHelper.ReadRowValueString(state, "bid_count"), out int nBids) ? nBids : default(int?);
                    var asksCount = int.TryParse(ApiHelper.ReadRowValueString(state, "offer_count"), out int nAsks) ? nBids : default(int?);

                    static (long, long)[] ReadRowValueQuotes(IntPtr stack, string columnName)
                    {
                        var quotes = Array.Empty<(long, long)>();

                        if (ApiHelper.ResolveRowValueTable(stack, columnName))
                        {
                            // stack: orderbooks, orderbook_n_key,  orderbook_n_value, bids
                            var quotesSize = LuaApi.lua_rawlen(stack, -1);

                            if (quotesSize > 0)
                            {
                                quotes = new (long, long)[quotesSize];
                                var i = 0uL;

                                LuaApi.lua_pushnil(stack);
                                // stack: orderbooks, orderbook_n_key,  orderbook_n_value, bids, nil
                                while (LuaApi.lua_next(stack, -2) != LuaApi.OK_RESULT && i < quotesSize)
                                {
                                    // stack: orderbooks, orderbook_n_key,  orderbook_n_value, bids, bid_n_key, bid_n_value
                                    var price = long.TryParse(ApiHelper.ReadRowValueString(stack, "price"), out long prc) ? prc : default(long?);
                                    var size = long.TryParse(ApiHelper.ReadRowValueString(stack, "price"), out long sz) ? prc : default(long?);

                                    if (price.HasValue && size.HasValue)
                                    {
                                        quotes[i++] = (price.Value, size.Value);
                                    }

                                    ApiHelper.PopFromStack(stack);
                                }

                                // stack: orderbooks, orderbook_n_key,  orderbook_n_value, bids, bid_n_key
                                ApiHelper.PopFromStack(stack);
                            }

                            // stack: orderbooks, orderbook_n_key,  orderbook_n_value, bids
                            ApiHelper.PopFromStack(stack);
                        }

                        return quotes;
                    }

                    var bids = ReadRowValueQuotes(state, "bid");
                    var asks = ReadRowValueQuotes(state, "offer");

                    ApiHelper.PopFromStack(state);
                }

                ApiHelper.PopFromStack(state);
            }
        }

        public static int Main(IntPtr state)
        {
            currentState = state;
            LuaGate.state = state;

            try
            {
                System.Diagnostics.Debugger.Launch();

                GetTradeDate();
                //GetSecurityInfo("SPBFUT", "BRZ2");

                // пока что уберем, т.к. квик падает
                //LuaApi.lua_close(state);  
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }
            return 1;
        }

        /// <summary>
        /// Entry Point. This method gets called from the lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to the Lua state object</param>
        /// <returns></returns>
        public unsafe int Initialize(void* L)
        {
            var lua = new LuaState(L);

            try
            {
                lua.TieProxyLibrary("ClrBootstrap");
                lua.RegisterCallback(Main, "main");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }

            return 0;
        }

    }
}