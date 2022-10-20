using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;
using QuikLuaApi.Entities;
using QuikLuaApiWrapper;
using QuikLuaApi.QuikApi;
using static System.Formats.Asn1.AsnWriter;
using QuikLuaApiWrapper.Entities;

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
                      name: QuikLuaApi.QuikApi.QuikApi.IS_CONNECTED_METHOD, 
                returnType: LuaApi.TYPE_NUMBER, 
                  callback: _localState.ReadAsBool);
        }

        internal static IEnumerable<QuikTable> ParseTables(string name)
        {
            var numAccounts = (int)_localState.ExecFunction(Methods.GET_NUMBER_OF_ITEMS, LuaApi.TYPE_NUMBER, _localState.ReadAsNumber, name);
            var result = new List<QuikTable>(numAccounts);

            for (int i = 0; i < numAccounts; i++)
            {
                if (_localState.ExecFunction(Methods.GET_ITEM, LuaApi.TYPE_TABLE, name, i))
                {
                    var table = new QuikTable(name);

                    LuaApi.lua_pushnil(_localState);

                    while (LuaApi.lua_next(_localState, -2) != LuaApi.OK_RESULT)
                    {
                        var value = _localState.ReadValueSafe(LuaTypes.String, _localState, -1);
                        var title = _localState.ReadValueSafe(LuaTypes.String, _localState, -2);

                        table[title] = value;

                        _localState.PopFromStack();
                    }

                    result.Add(table);
                }

                _localState.PopFromStack();
            }

            return result;
        }

        public static T[][] ParseTable<T>(string name, Func<T> parser)
        {
            var numAccounts = _localState.ExecFunction(Methods.GET_NUMBER_OF_ITEMS, LuaApi.TYPE_NUMBER, _localState.ReadAsNumber, name);

            var result = new T[numAccounts][];

            for (int i = 0; i < numAccounts; i++)
            {
                if (_localState.ExecFunction(Methods.GET_ITEM, LuaApi.TYPE_TABLE, name, i))
                {
                    result[i] = TableIterator(parser);
                }

                _localState.PopFromStack();
            }

            return result;
        }

        public static T[] TableIterator<T>(Func<T> parser)
        {
            var dataLen = (long)LuaApi.lua_rawlen(_localState, -1);
            var result = new T[dataLen];

            LuaApi.lua_pushnil(_localState);

            for (int i = 0; i < dataLen && LuaApi.lua_next(_localState, -2) != LuaApi.OK_RESULT; i++)
            {
                result[i] = parser();
            }

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
                LuaApi.lua_pushstring(_localState, "one");
                LuaApi.lua_pushstring(_localState, "two");
                LuaApi.lua_pushstring(_localState, "three");

                //var firms = ParseTable("firms").ToArray();
                //var money_limits = ParseTables("money_limits").ToArray();
                //var trade_accounts = ParseTables("trade_accounts").ToArray();
                //var account_positions = ParseTables("account_positions").ToArray();
                //var futures_client_limits = ParseTables("futures_client_limits").ToArray();

                //var futlims = ParseTables("futures_client_limits")
                //    .Select(t => t.Deserialize<DerivativesTradingAccount>());

                var accounts = GetDerivativesExchangeAccounts();

                System.Diagnostics.Debugger.Launch();

                //var accounts = GetAccounts().ToArray();

                //GetDerivativesMarketFunds("U7A0016", "FORTS9202", Account.CLEARING_LIMIT_TYPE, QuikApi.Account.RUB_CURRENCY);
                //GetDerivativesMarketFunds("U7A0016", "FORTS9202", Account.MONEY_LIMIT_TYPE, QuikApi.Account.RUB_CURRENCY);

                var classes = GetClasses();
                var options = GetSecuritiesOfAClass(QuikApi.QuikApi.OPTIONS_CLASS_CODE);
                var opt = GetSecurity("SPBOPT", options.First());
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }
            return 1;
        }
    }
}