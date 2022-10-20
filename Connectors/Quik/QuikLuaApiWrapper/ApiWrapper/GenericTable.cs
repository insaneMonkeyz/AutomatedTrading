using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikLuaApi;
using QuikLuaApi.QuikApi;

namespace QuikLuaApiWrapper.ApiWrapper
{
    internal delegate TResult? TableReader<TRequest, TResult>(LuaState stack, ref TRequest request);
    internal delegate TResult? TableReader<TResult>(LuaState stack, Func<TResult?> creator);
    //internal delegate TResult? TableCreator<TRequest, TResult>(LuaState stack, )
    /// <summary>
    /// Wrapper общих таблиц
    /// </summary>
    internal static class GenericQuikTable
    {
        private const string GET_NUMBER_OF_ITEMS = "getNumberOf";
        private const string GET_ITEM = "getItem";

        private static LuaState _stack;

        public static List<TResult> ReadTable<TRequest, TResult>(
            LuaState stack, string table, ref TRequest request, TableReader<TRequest,TResult> reader) 
                where TRequest : struct
        {
            _stack = stack;

            var numEntries = 
                (int)stack.ExecFunction(
                      name: GET_NUMBER_OF_ITEMS,
                returnType: LuaApi.TYPE_NUMBER,
                  callback: stack.ReadAsNumber,
                      arg0: table);

            var result = new List<TResult>(numEntries);

            for (int i = 0; i < numEntries; i++)
            {
                if (stack.ExecFunction(GET_ITEM, LuaApi.TYPE_TABLE, table, i) && reader(stack, ref request) is TResult res)
                {
                    result.Add(res);
                }

                stack.PopFromStack();
            }

            return result;
        }

        public static List<T> ReadTable<T>(LuaState stack, string table, Func<T?> creator, TableReader<T> reader)
        {
            _stack = stack;

            var numEntries = 
                (int)stack.ExecFunction(
                      name: GET_NUMBER_OF_ITEMS,
                returnType: LuaApi.TYPE_NUMBER,
                  callback: stack.ReadAsNumber,
                      arg0: table);

            var result = new List<T>(numEntries);

            for (int i = 0; i < numEntries; i++)
            {
                if (stack.ExecFunction(GET_ITEM, LuaApi.TYPE_TABLE, table, i) && reader(stack, creator) is T res)
                {
                    result.Add(res);
                }

                stack.PopFromStack();
            }

            return result;
        }
    }
}
