using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;
using QuikLuaApi.Entities;
using QuikLuaApi.QuikApi;
using QuikLuaApiWrapper.Entities;
using Quik.ApiWrapper;
using QuikLuaApiWrapper.ApiWrapper.QuikApi;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi
{
    public delegate void CallbackSubscriber(LuaFunction handler, string quikTableName);

    public class QuikLuaApiWrapper
    {
        internal static LuaState State => _localState;

        [Flags]
        internal enum OrderbookParsingResult : long
        {
            BidsParsed = 1,
            AsksParsed = 2
        }

        private static LuaState _localState;

        public static bool IsConnected
        {
            get => _localState.ExecFunction(
                      name: QuikLuaApi.QuikApi.QuikApi.IS_CONNECTED_METHOD, 
                returnType: LuaApi.TYPE_NUMBER, 
                  callback: _localState.ReadAsBool);
        }

        //internal static IEnumerable<QuikTable> ParseTables(string name)
        //{
        //    var numAccounts = (int)_localState.ExecFunction(Methods.GET_NUMBER_OF_ITEMS, LuaApi.TYPE_NUMBER, _localState.ReadAsNumber, name);
        //    var result = new List<QuikTable>(numAccounts);

        //    for (int i = 0; i < numAccounts; i++)
        //    {
        //        if (_localState.ExecFunction(Methods.GET_ITEM, LuaApi.TYPE_TABLE, name, i))
        //        {
        //            var table = new QuikTable(name);

        //            LuaApi.lua_pushnil(_localState);

        //            while (LuaApi.lua_next(_localState, -2) != LuaApi.OK_RESULT)
        //            {
        //                var value = _localState.ReadValueSafe(LuaTypes.String, _localState, -1);
        //                var title = _localState.ReadValueSafe(LuaTypes.String, _localState, -2);

        //                table[title] = value;

        //                _localState.PopFromStack();
        //            }

        //            result.Add(table);
        //        }

        //        _localState.PopFromStack();
        //    }

        //    return result;
        //}

        //public static T[][] ParseTable<T>(string name, Func<T> parser)
        //{
        //    var numAccounts = _localState.ExecFunction(Methods.GET_NUMBER_OF_ITEMS, LuaApi.TYPE_NUMBER, _localState.ReadAsNumber, name);

        //    var result = new T[numAccounts][];

        //    for (int i = 0; i < numAccounts; i++)
        //    {
        //        if (_localState.ExecFunction(Methods.GET_ITEM, LuaApi.TYPE_TABLE, name, i))
        //        {
        //            result[i] = TableIterator(parser);
        //        }

        //        _localState.PopFromStack();
        //    }

        //    return result;
        //}

        //public static T[] TableIterator<T>(Func<T> parser)
        //{
        //    var dataLen = (long)LuaApi.lua_rawlen(_localState, -1);
        //    var result = new T[dataLen];

        //    LuaApi.lua_pushnil(_localState);

        //    for (int i = 0; i < dataLen && LuaApi.lua_next(_localState, -2) != LuaApi.OK_RESULT; i++)
        //    {
        //        result[i] = parser();
        //    }

        //    return result;
        //}



        private const string GET_NUMBER_OF_ITEMS = "getNumberOf";
        private const string GET_ITEM = "getItem";

        internal static List<T> ReadWholeTable<T>(string table, Func<LuaState, T?> reader)
        {
            var numEntries =
                (int)_localState.ExecFunction(
                      name: GET_NUMBER_OF_ITEMS,
                returnType: LuaApi.TYPE_NUMBER,
                  callback: _localState.ReadAsNumber,
                      arg0: table);

            var result = new List<T>(numEntries);

            for (int i = 0; i < numEntries; i++)
            {
                if (_localState.ExecFunction(GET_ITEM, LuaApi.TYPE_TABLE, table, i) && reader(_localState) is T res)
                {
                    result.Add(res);
                }

                _localState.PopFromStack();
            }

            return result;
        }

        internal struct Method2Params<T>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }
        internal struct Method1Param<T>
        {
            public string Method;
            public string Arg0;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }
        internal struct MethodNoParams<T>
        {
            public string Method;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }

        internal static T ReadSpecificEntry<T>(ref Method2Params<T> param)
        {
            T result = param.DefaultValue;

            _localState.PrintStack("Beginning ReadSpecificEntry");

            if (_localState.ExecFunction(param.Method, param.ReturnType, param.Arg0, param.Arg1))
            {
                result = param.Action();
            }

            _localState.PopFromStack();

            _localState.PrintStack("Completed ReadSpecificEntry");

            return result;
        }
        internal static T ReadSpecificEntry<T>(ref Method1Param<T> param)
        {
            T result = param.DefaultValue;

            _localState.PrintStack("Beginning ReadSpecificEntry");

            if (_localState.ExecFunction(param.Method, param.ReturnType, param.Arg0))
            {
                result = param.Action();
            }

            _localState.PopFromStack();

            _localState.PrintStack("Completed ReadSpecificEntry");

            return result;
        }
        internal static T ReadSpecificEntry<T>(ref MethodNoParams<T> param)
        {
            T result = param.DefaultValue;

            if (_localState.ExecFunction(param.Method, param.ReturnType))
            {
                result = param.Action();
            }

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

                //AccountWrapper.Instance.Subscribe(lua.RegisterCallback);
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

                System.Diagnostics.Debugger.Launch();

                QuikSecurity.Set(state);

                SecurityWrapper.ResolveSecurity = SecurityWrapper.GetSecurity;

                var availableFutures = SecurityWrapper.GetAvailableSecuritiesOfType(typeof(IFutures));
                var availableOptions = SecurityWrapper.GetAvailableSecuritiesOfType(typeof(IOption));
                var availableSpreads = SecurityWrapper.GetAvailableSecuritiesOfType(typeof(ICalendarSpread));

                var fut = SecurityWrapper.GetSecurity(typeof(IFutures), availableFutures.First());
                var opt = SecurityWrapper.GetSecurity(typeof(IOption), availableOptions.First());
                var spr = SecurityWrapper.GetSecurity(typeof(ICalendarSpread), availableSpreads.First());

                while (true)
                {
                    Thread.Sleep(200);
                }
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