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

    /// <summary>
    /// Wrapper общих таблиц
    /// </summary>
    internal static class GenericQuikTable
    {

        //public static List<T> ReadSpecificEntry<T>(LuaState stack, string table, Func<T?> creator, TableReader<T> reader)
        //{
        //    _stack = stack;

        //    var numEntries = 
        //        (int)stack.ExecFunction(
        //              name: GET_NUMBER_OF_ITEMS,
        //        returnType: LuaApi.TYPE_NUMBER,
        //          callback: stack.ReadAsNumber,
        //              arg0: table);

        //    var result = new List<T>(numEntries);

        //    for (int i = 0; i < numEntries; i++)
        //    {
        //        if (stack.ExecFunction(GET_ITEM, LuaApi.TYPE_TABLE, table, i) && reader(stack, creator) is T res)
        //        {
        //            result.Add(res);
        //        }

        //        stack.PopFromStack();
        //    }

        //    return result;
        //}

        //public struct MethodParams4
        //{
        //    public string Method;
        //    public string Arg0;
        //    public string Arg1;
        //    public string Arg2;
        //    public string Arg3;
        //}
        //public struct MethodParams2
        //{
        //    public string Method;
        //    public string Arg0;
        //    public string Arg1;
        //}
        //public struct Reader1Params<TArg, TRes>
        //{
        //    public Func<LuaState, TArg, TRes> Reader;
        //    public TArg Arg0;
        //}

        //public static TRes ReadSpecificEntry<TArg, TRes>(LuaState state, ref MethodParams4 tp, ref Reader1Params<TArg, TRes> rp)
        //{
        //    TRes result = default;

        //    if (state.ExecFunction(tp.Method, LuaApi.TYPE_TABLE, tp.Arg0, tp.Arg1, tp.Arg2, tp.Arg3))
        //    {
        //        result = rp.Reader(state, rp.Arg0);
        //    }

        //    state.PopFromStack();

        //    return result;
        //}
        //public static TRes ReadSpecificEntry<TRes>(LuaState state, ref MethodParams2 tp, Func<TRes> reader)
        //{
        //    TRes result = default;

        //    if (state.ExecFunction(tp.Method, LuaApi.TYPE_TABLE, tp.Arg0, tp.Arg1))
        //    {
        //        result = reader();
        //    }

        //    state.PopFromStack();

        //    return result;
        //}
    }
}
