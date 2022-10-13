using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml.Linq;
using BasicConcepts;
using QuikLuaApiWrapper.Entities;

namespace QuikLuaApi
{
    public class QuikApiException : Exception
    {
        public QuikApiException(string message) : base(message)
        {

        }
    }

    public class QuikLuaApiWrapper
    {
        private static LuaState _quikState;
        private static LuaState _localState;

        private static Collection<string> _registeredEntities;

        public static QuikLuaApiWrapper Quik;

        public static bool IsConnected
        {
            get
            {
                _localState.ExecFunction("isConnected", LuaApi.TYPE_NUMBER);
                return _localState.TryPopLong(out long value) && value == 1;
            }
        }

        public static IOptimizedOrderBook GetOrderbook(ISecurity security)
        {
            return new OrderBook()
            {
                Security = security
            };
        }

        public void GetSecurityInfo(string classCode, string ticker)
        {
            if (_localState.ExecFunction("getSecurityInfo", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                var faceUnit = _localState.ReadRowValueString("face_unit");
                var shortName = _localState.ReadRowValueString("short_name");
            }
        }
        public bool UpdateOrderBook(IOptimizedOrderBook orderbook, string classCode, string ticker)
        {
            if (_localState.ExecFunction("getQuoteLevel2", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                if (_localState.TryFetchLongFromTable("bid_count", out long bidsCount) && bidsCount > 0)
                {
                    _localState.PushColumnValueTable("bid");
                    _localState.PrintStack("Start processing bids");
                    orderbook.UseBids(_localState.ReadRowValueQuotes);
                    _localState.PopFromStack();
                }
                else
                {
                    // pop getQuoteLevel2 result
                    _localState.PopFromStack();
                    return false;
                }

                _localState.PrintStack("After bids");

                if (_localState.TryFetchLongFromTable("offer_count", out long asksCount) && asksCount > 0)
                {
                    _localState.PushColumnValueTable("offer");
                    _localState.PrintStack("Start processing asks");
                    orderbook.UseAsks(_localState.ReadRowValueQuotes);
                    _localState.PopFromStack();
                }
                else
                {
                    // pop getQuoteLevel2 result
                    _localState.PopFromStack();
                    return false;
                }

                // pop getQuoteLevel2 result
                _localState.PopFromStack();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int Main(IntPtr state)
        {
            _localState = state;

            try
            {
                System.Diagnostics.Debugger.Launch();

                var api = new QuikLuaApiWrapper();
                Quik = api;

                var book = new OrderBook();

                api.UpdateOrderBook(book, "SPBFUT", "BRZ2");

                foreach (var entity in _registeredEntities)
                {
                    LuaApi.lua_pushnil(state);
                    LuaApi.lua_setglobal(state, entity);
                }

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
            LuaState lua = L;
            _registeredEntities = new Collection<string>();

            try
            {
                lua.TieProxyLibrary("NativeToManagedProxy");
                _registeredEntities.Add("NativeToManagedProxy");

                lua.RegisterCallback(Main, "main");
                _registeredEntities.Add("main");
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