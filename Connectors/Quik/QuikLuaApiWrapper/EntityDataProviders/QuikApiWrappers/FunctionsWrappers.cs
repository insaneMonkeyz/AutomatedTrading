using Quik.Lua;
using static Quik.Quik;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal static class FunctionsWrappers
    {
        public struct ReadCallbackArgs<TArg0, TArg1, TResult>
        {
            public LuaWrap LuaProvider;
            public Func<TArg0, TArg1, TResult> Callback;
        }
        public struct VoidCallback1Param<TReturn>
        {
            public TReturn Arg0;
        }
        public struct Callback1Param<TArg, TReturn>
        {
            public TArg Arg;
            public Func<TArg, TReturn> Invoke;
            public TReturn DefaultValue;
        }
        public struct VoidCallback2Params<TCallbackArg0, TCallbackArg1>
        {
            public TCallbackArg0 Arg0;
            public TCallbackArg1 Arg1;
            public Action<TCallbackArg0, TCallbackArg1> Invoke;
        }

        public struct VoidMultiGetMethod2Params<TActionArg0, TActionArg1>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType1;
            public int ReturnType2;
            public VoidCallback2Params<TActionArg0, TActionArg1> Callback;
        }
        public struct MultiGetMethod2Params<TCallbackArg, TCallbackReturn>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType1;
            public int ReturnType2;
            public Callback1Param<TCallbackArg, TCallbackReturn> Callback;
        }
        public struct Method2ParamsNoReturn<TActionArg>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType;
            public VoidCallback1Param<TActionArg> ActionParams;
            public Action<TActionArg> Action;
        }
        public struct VoidMethod4Params<TCallbackArg0, TCallbackArg1>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public string Arg2;
            public string Arg3;
            public int ReturnType;
            public VoidCallback2Params<TCallbackArg0, TCallbackArg1> Callback;
        }
        public struct Method4Params<TCallbackArg, TCallbackReturn>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public string Arg2;
            public string Arg3;
            public int ReturnType;
            public Callback1Param<TCallbackArg, TCallbackReturn> Callback;
        }
        public struct Method2Params<T>
        {
            public string Method;
            public string Arg0;
            public string Arg1;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }
        public struct Method1Param<T>
        {
            public string Method;
            public string Arg0;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }
        public struct MethodNoParams<T>
        {
            public string Method;
            public int ReturnType;
            public Func<T> Action;
            public T DefaultValue;
        }

        public static void ReadSpecificEntry<TCallbackArg0, TCallbackArg1>(ref VoidMethod4Params<TCallbackArg0, TCallbackArg1> param)
        {
            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType,
                    param.Arg0, param.Arg1, param.Arg2, param.Arg3))
                {
                    param.Callback.Invoke(param.Callback.Arg0, param.Callback.Arg1);
                }

                Quik.Lua.PopFromStack();
            }
        }
        public static TCallbackReturn ReadSpecificEntry<TCallback, TCallbackReturn>(ref Method4Params<TCallback, TCallbackReturn> param)
        {
            lock (SyncRoot)
            {
                var result = param.Callback.DefaultValue;

                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType,
                    param.Arg0, param.Arg1, param.Arg2, param.Arg3))
                {
                    result = param.Callback.Invoke(param.Callback.Arg);
                }

                Quik.Lua.PopFromStack();

                return result;
            }
        }
        public static void ReadSpecificEntry<T0, T1>(ref VoidMultiGetMethod2Params<T0, T1> param)
        {
            lock (SyncRoot)
            {
                if (Quik.Lua.ExecMultyReturnFunction(param.Method,
                    param.ReturnType1, param.ReturnType2,
                    param.Arg0, param.Arg1))
                {
                    param.Callback.Invoke(param.Callback.Arg0, param.Callback.Arg1);
                }

                Quik.Lua.PopTwoFromStack();
            }
        }
        public static TCallbackReturn ReadSpecificEntry<TCallbackArg, TCallbackReturn>(ref MultiGetMethod2Params<TCallbackArg, TCallbackReturn> param)
        {
            lock (SyncRoot)
            {
                var result = param.Callback.DefaultValue;

                Quik.Lua.PrintStack("entered ReadSpecificEntry MultiGetMethod2Params");

                if (Quik.Lua.ExecMultyReturnFunction(param.Method,
                    param.ReturnType1, param.ReturnType2,
                    param.Arg0, param.Arg1))
                {
                    Quik.Lua.PopFromStack();
                    Quik.Lua.PrintStack("calling callback from MultiGetMethod2Params");

                    result = param.Callback.Invoke(param.Callback.Arg);

                    Quik.Lua.PrintStack("completed callback in MultiGetMethod2Params");
                }
                else
                {
                    Quik.Lua.PopFromStack();
                    Quik.Lua.PrintStack("calling callback from MultiGetMethod2Params");
                }

                Quik.Lua.PopFromStack();

                Quik.Lua.PrintStack("completed ReadSpecificEntry MultiGetMethod2Params");

                return result;
            }
        }
        public static void ReadSpecificEntry<T>(ref Method2ParamsNoReturn<T> param)
        {
            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType, param.Arg0, param.Arg1))
                {
                    param.Action(param.ActionParams.Arg0);
                }

                Quik.Lua.PopFromStack();
            }
        }
        public static T ReadSpecificEntry<T>(ref Method2Params<T> param)
        {
            param.Method.DebugPrintQuikFunctionCall(param.Arg0, param.Arg1);

            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType, param.Arg0, param.Arg1))
                {
                    result = param.Action();
                }

                Quik.Lua.PopFromStack();
            }

            return result;
        }
        public static T ReadSpecificEntry<T>(ref Method1Param<T> param)
        {
            param.Method.DebugPrintQuikFunctionCall(param.Arg0);

            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType, param.Arg0))
                {
                    result = param.Action();
                }

                Quik.Lua.PopFromStack();
            }

            return result;
        }
        public static T ReadSpecificEntry<T>(ref MethodNoParams<T> param)
        {
            param.Method.DebugPrintQuikFunctionCall();

            T result = param.DefaultValue;

            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(param.Method, param.ReturnType))
                {
                    result = param.Action();
                }

                Quik.Lua.PopFromStack();
            }

            return result;
        }
        public static TResult ReadCallbackArguments<TResult>(ref ReadCallbackArgs<string?, string?, TResult> callbackReaderParams)
        {
            lock (SyncRoot)
            {
                // order of reading is important.
                // the last item is on top of the stack
                var arg1 = callbackReaderParams.LuaProvider.ReadAsString();
                var arg0 = callbackReaderParams.LuaProvider.ReadAsString(LuaWrap.SECOND_ITEM);
                //var arg1 = callbackReaderParams.LuaProvider.SafeReadString();
                //var arg0 = callbackReaderParams.LuaProvider.SafeReadString(LuaWrap.SECOND_ITEM);

                return callbackReaderParams.Callback(arg0, arg1);
            }
        }
    }
}
