using System.Diagnostics;
using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

namespace Quik
{
    public delegate void CallbackSubscriber(LuaFunction handler, string quikTableName);

    public class QuikProxy
    {
        internal static readonly object SyncRoot = new();
        internal static LuaState State => _localState;
        private static LuaState _localState;

        public static bool IsConnected
        {
            get => _localState.ExecFunction(
                      name: Quik.QuikApi.QuikApi.IS_CONNECTED_METHOD, 
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
            lock (SyncRoot)
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
        }

        internal static class GetParam
        {
            public const string STRING_VALUE = "param_image";
            public const string VALUE = "param_value";
            public const string METHOD = "getParamEx";
            public const string RESULT_TYPE = "param_type";
            public const string RESULT_STATUS = "result";

            public const int SUCCESS = '1';
            public const int FAIL = '0';

            public enum Values : long
            {
                /// <summary>
                /// param is found, but isn't evaluated (i.e. default)
                /// </summary>
                /// 
                NoValue =  '0',
                Double,
                Long,
                Char,
                Enum,
                Time,
                Date
            }

        }

        internal struct GetItemParams
        {
            public string ClassCode;
            public string Ticker;
            public string Parameter;
            public GetParam.Values ReturnType;
        }
        internal struct VoidCallback1Param<TReturn>
        {
            public TReturn Arg0;
        }
        internal struct Callback1Param<TArg,TReturn>
        {
            public TArg Arg;
            public Func<TArg, TReturn> Invoke;
            public TReturn DefaultValue;
        }
        internal struct VoidCallback2Params<TCallbackArg0, TCallbackArg1>
        {
            public TCallbackArg0 Arg0;
            public TCallbackArg1 Arg1;
            public Action<TCallbackArg0, TCallbackArg1> Invoke;
        }

        internal struct VoidMultiGetMethod2Params<TActionArg0, TActionArg1>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType1;
            public int ReturnType2;
            public VoidCallback2Params<TActionArg0, TActionArg1> Callback;
        }
        internal struct MultiGetMethod2Params<TCallbackArg, TCallbackReturn>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType1;
            public int ReturnType2;
            public Callback1Param<TCallbackArg, TCallbackReturn> Callback;
        }
        internal struct Method2ParamsNoReturn<TActionArg>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType;
            public VoidCallback1Param<TActionArg> ActionParams;
            public Action<TActionArg> Action;
        }
        internal struct VoidMethod4Params<TCallbackArg0,TCallbackArg1>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public string Arg2;
            public string Arg3;
            public int ReturnType;
            public VoidCallback2Params<TCallbackArg0,TCallbackArg1> Callback;
        }
        internal struct Method4Params<TCallbackArg,TCallbackReturn>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public string Arg2;
            public string Arg3;
            public int ReturnType;
            public Callback1Param<TCallbackArg, TCallbackReturn> Callback;
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

        internal static Decimal5? GetDecimal5From_getParamEx(Security security, string param)
        {
            var @params = new GetItemParams
            {
                ReturnType = GetParam.Values.Long,
                ClassCode = security.ClassCode,
                Ticker = security.Ticker,
                Parameter = param
            };

            if (!Decimal5.TryParse(ReadSpecificEntry(ref @params), out Decimal5 value))
            {
                $"Parameter '{param}' of security {security.ClassCode}:{security.Ticker} was not set"
                    .DebugPrintWarning();

                return null;
            }

            return value;
        }
        internal static string? ReadSpecificEntry(ref GetItemParams param)
        {
            string? result = null;

            lock (SyncRoot)
            {
                if (_localState.ExecFunction(GetParam.METHOD, LuaApi.TYPE_TABLE, param.ClassCode, param.Ticker, param.Parameter) &&
                    _localState.ReadRowValueChar(GetParam.RESULT_STATUS) == GetParam.SUCCESS)
                {
                    if (_localState.TryFetchCharFromTable(GetParam.RESULT_TYPE, out char type))
                    {
                        result = type == (long)GetParam.Values.Char
                                ? _localState.ReadRowValueString(GetParam.STRING_VALUE)
                                : _localState.ReadRowValueString(GetParam.VALUE);
                    }
                    else if (type == (long)GetParam.Values.NoValue)
                    {
                        ($"Value of parameter {param.Parameter} was not present for security " +
                            $"'{param.ClassCode}:{param.Ticker}'").DebugPrintWarning();
                    }
                    else
                    {
                        _localState.PopFromStack();

                        throw new ArgumentException($"Provided return type '{param.ReturnType}' of parameter '{param.Parameter}' " +
                            $" for security '{param.ClassCode}:{param.Ticker}' does not match the return type '{type}' of {GetParam.METHOD} method");
                    }
                }

                _localState.PopFromStack(); 
            }

            return result;
        }
        internal static void ReadSpecificEntry<TCallbackArg0,TCallbackArg1>(ref VoidMethod4Params<TCallbackArg0,TCallbackArg1> param)
        {
            lock (SyncRoot)
            {
                if (_localState.ExecFunction(param.Method, param.ReturnType, 
                    param.Arg0, param.Arg1, param.Arg2, param.Arg3))
                {
                    param.Callback.Invoke(param.Callback.Arg0, param.Callback.Arg1);
                }

                _localState.PopFromStack(); 
            }
        }
        internal static TCallbackReturn ReadSpecificEntry<TCallback,TCallbackReturn>(ref Method4Params<TCallback,TCallbackReturn> param)
        {
            lock (SyncRoot)
            {
                var result = param.Callback.DefaultValue;

                if (_localState.ExecFunction(param.Method, param.ReturnType, 
                    param.Arg0, param.Arg1, param.Arg2, param.Arg3))
                {
                    result = param.Callback.Invoke(param.Callback.Arg);
                }

                _localState.PopFromStack();

                return result;
            }
        }
        internal static void ReadSpecificEntry<T0,T1>(ref VoidMultiGetMethod2Params<T0,T1> param)
        {
            lock (SyncRoot)
            {
                if (_localState.ExecDoubleReturnFunction(param.Method, 
                    param.ReturnType1, param.ReturnType2, 
                    param.Arg0, param.Arg1))
                {
                    param.Callback.Invoke(param.Callback.Arg0, param.Callback.Arg1);
                }

                _localState.PopTwoFromStack(); 
            }
        }
        internal static TCallbackReturn ReadSpecificEntry<TCallbackArg, TCallbackReturn>(ref MultiGetMethod2Params<TCallbackArg, TCallbackReturn> param)
        {
            lock (SyncRoot)
            {
                var result = param.Callback.DefaultValue;

                if (_localState.ExecDoubleReturnFunction(param.Method, 
                    param.ReturnType1, param.ReturnType2, 
                    param.Arg0, param.Arg1))
                {
                    result = param.Callback.Invoke(param.Callback.Arg);
                }

                _localState.PopTwoFromStack();

                return result;
            }
        }
        internal static void ReadSpecificEntry<T>(ref Method2ParamsNoReturn<T> param)
        {
            lock (SyncRoot)
            {
                if (_localState.ExecFunction(param.Method, param.ReturnType, param.Arg0, param.Arg1))
                {
                    param.Action(param.ActionParams.Arg0);
                }

                _localState.PopFromStack(); 
            }
        }
        internal static T ReadSpecificEntry<T>(ref Method2Params<T> param)
        {
            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (_localState.ExecFunction(param.Method, param.ReturnType, param.Arg0, param.Arg1))
                {
                    result = param.Action();
                }

                _localState.PopFromStack(); 
            }

            return result;
        }
        internal static T ReadSpecificEntry<T>(ref Method1Param<T> param)
        {
            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (_localState.ExecFunction(param.Method, param.ReturnType, param.Arg0))
                {
                    result = param.Action();
                }

                _localState.PopFromStack(); 
            }

            return result;
        }
        internal static T ReadSpecificEntry<T>(ref MethodNoParams<T> param)
        {
            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (_localState.ExecFunction(param.Method, param.ReturnType))
                {
                    result = param.Action();
                }

                _localState.PopFromStack(); 
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

                //AccountsProvider.Instance.Initialize();
                //AccountsProvider.Instance.SubscribeCallback(lua);
                DerivativesBalanceProvider.Instance.Initialize();
                DerivativesBalanceProvider.Instance.SubscribeCallback(lua);
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

                SecurityWrapper.Set(State);

                System.Diagnostics.Debugger.Launch();

                //foreach (var subscriber in SingletonProvider.GetInstances<IQuikDataSubscriber>())
                //{
                //    subscriber.SubscribeCallback(_localState);
                //}

                // PASSED
                //var accResolver = EntityResolvers.GetAccountsResolver();

                //var account = AccountsProvider.Instance.GetAllEntities().FirstOrDefault(acc => acc.IsMoneyAccount);

                //if (account != null)
                //{
                //    Debug.Print($"-- {DateTime.Now:T} {account}");
                //}

                //AccountsProvider.Instance.EntityChanged = (acc) =>
                //{
                //    Debug.Print($"-- {DateTime.Now:T} ENTITY CHANGED: {acc}");
                //};

                // FAILED

                var pos = DerivativesBalanceProvider.Instance.GetAllEntities();

                foreach (var p in pos)
                {
                    Debug.Print($"-- {DateTime.Now:T} {p}");
                }

                DerivativesBalanceProvider.Instance.EntityChanged = (balance) =>
                {
                    Debug.Print($"-- {DateTime.Now:T} ENTITY CHANGED: {balance}");
                };
                DerivativesBalanceProvider.Instance.NewEntity = (balance) =>
                {
                    Debug.Print($"-- {DateTime.Now:T} NEW ENTITY: {balance}");
                };

                while (true)
                {
                    Thread.Sleep(10);
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