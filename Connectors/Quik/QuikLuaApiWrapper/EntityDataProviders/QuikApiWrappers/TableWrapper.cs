using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Quik.Entities;
using Quik.Lua;
using static Quik.Quik;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal static class TableWrapper
    {
        private const string STRING_VALUE = "param_image";
        private const string VALUE = "param_value";
        private const string GET_PARAM_EX_METHOD = "getParamEx";
        private const string GET_NUMBER_OF_ITEMS_METHOD = "getNumberOf";
        private const string GET_ITEM_METHOD = "getItem";
        private const string RESULT_TYPE = "param_type";
        private const string RESULT_STATUS = "result";

        private const int SUCCESS = '1';
        private const int FAIL = '0';

        public const char NO_VALUE = '0';
        public const char DOUBLE_VALUE = '1';
        public const char LONG_VALUE = '2';
        public const char CHAR_VALUE = '3';
        public const char ENUM_VALUE = '4';
        public const char TIME_VALUE = '5';
        public const char DATE_VALUE = '6';

        public struct GetParamExParams
        {
            public string ClassCode;
            public string Ticker;
            public string Parameter;
            public char ReturnType;
        }


        internal static List<T> ReadWholeTable<T>(string table, Func<LuaWrap, T?> reader)
        {
            lock (SyncRoot)
            {
                var numEntries =
                        (int)Quik.Lua.ExecFunction(
                              name: GET_NUMBER_OF_ITEMS_METHOD,
                        returnType: Api.TYPE_NUMBER,
                          callback: Quik.Lua.ReadAsNumber,
                              arg0: table);

                var result = new List<T>(numEntries);

                for (int i = 0; i < numEntries; i++)
                {
                    if (Quik.Lua.ExecFunction(GET_ITEM_METHOD, Api.TYPE_TABLE, table, i) && reader(Quik.Lua) is T res)
                    {
                        result.Add(res);
                    }

                    Quik.Lua.PopFromStack();
                }

                return result;
            }
        }
        public static string? FetchParamEx(ref GetParamExParams param)
        {
            string? result = null;

            lock (SyncRoot)
            {
                if (Quik.Lua.ExecFunction(GET_PARAM_EX_METHOD, Api.TYPE_TABLE, param.ClassCode, param.Ticker, param.Parameter) &&
                    Quik.Lua.ReadRowValueChar(RESULT_STATUS) == SUCCESS)
                {
                    if (Quik.Lua.TryFetchCharFromTable(RESULT_TYPE, out char type))
                    {
                        result = type == CHAR_VALUE
                                ? Quik.Lua.ReadRowValueString(STRING_VALUE)
                                : Quik.Lua.ReadRowValueString(VALUE);
                    }
                    else if (type == NO_VALUE)
                    {
                        ($"Value of parameter {param.Parameter} was not present for security " +
                            $"'{param.ClassCode}:{param.Ticker}'").DebugPrintWarning();
                    }
                    else
                    {
                        Quik.Lua.PopFromStack();

                        throw new ArgumentException($"Provided return type '{param.ReturnType}' of parameter '{param.Parameter}' " +
                            $" for security '{param.ClassCode}:{param.Ticker}' does not match the return type '{type}' of {GET_PARAM_EX_METHOD} method");
                    }
                }

                Quik.Lua.PopFromStack();
            }

            return result;
        }
        public static Decimal5? FetchDecimal5ParamEx(Security security, string param)
        {
            var @params = new GetParamExParams
            {
                ReturnType = TableWrapper.LONG_VALUE,
                ClassCode = security.ClassCode,
                Ticker = security.Ticker,
                Parameter = param
            };

            if (!Decimal5.TryParse(TableWrapper.FetchParamEx(ref @params), out Decimal5 value))
            {
                $"Parameter '{param}' of security {security.ClassCode}:{security.Ticker} was not set"
                    .DebugPrintWarning();

                return null;
            }

            return value;
        }
    }
}
