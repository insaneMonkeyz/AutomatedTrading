using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using KeraLua;

namespace LuaGate
{
    internal class ApiHelper
    {
        internal static bool LastItemIsTable(IntPtr stack)
        {
            return LuaApi.lua_type(stack, -1) == LuaApi.TYPE_TABLE;
        }
        internal static bool IsTable(IntPtr state, int i)
        {
            return LuaApi.lua_type(state, i) == LuaApi.TYPE_TABLE;
        }

        internal static (string, string)[] ReadStringsTableRow(IntPtr stack, string columnName)
        {
            // stack: table
            LuaApi.lua_pushstring(stack, columnName);
            // stack: table, columnName
            LuaApi.lua_rawget(stack, -2);
            // stack: table, columnName, fieldValue

            if (LastItemIsTable(stack))
            {
                var result = new List<(string, string)>(50);

                LuaApi.lua_pushnil(stack);
                // stack: table, columnName, fieldValue, nil
                while (LuaApi.lua_next(stack, -2) != LuaApi.OK_RESULT)
                {
                    // stack: table, columnName, fieldValue, key, value
                    //result
                }
            }

            PopFromStack(stack, 2);
            return Array.Empty<(string, string)>();
        }
        internal static string GetString(IntPtr state, int tableIndex, int itemIndex)
        {
            if (LuaApi.lua_rawgeti(state, tableIndex, itemIndex) == LuaApi.OK_RESULT &&
                LuaApi.lua_isstring(state, 0) > 0)
            {
                return Marshal.PtrToStringAnsi(LuaApi.lua_tolstring(state, 0, out ulong len0), (int)len0);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Tries to retrieve a string from the top of the stack
        /// </summary>
        internal static bool TryGetString(IntPtr state, int tableIndex, int itemIndex, out string result)
        {
            result = null;

            if (LuaApi.lua_rawgeti(state, tableIndex, itemIndex) == LuaApi.OK_RESULT &&
                LuaApi.lua_isstring(state, 0) > 0)
            {
                var pstr = LuaApi.lua_tolstring(state, 0, out ulong strlen);

                if (strlen > 0)
                {
                    result = Marshal.PtrToStringAnsi(pstr, (int)strlen);
                    return true;
                }
            }

            return false;
        }

        //internal static IEnumerable<(TColumn0, TColumn1)> ReadTable<TColumn0, TColumn1>(IntPtr stack, string columnName0, string columnName1)
        //{
        //    (TColumn0, TColumn1) result;
        //    // stack: table
        //    LuaApi.lua_pushstring(stack, columnName0);
        //    // stack: table, columnName
        //    LuaApi.lua_rawget(stack, -2);
        //    // stack: table, columnName, fieldValue
        //}

        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a table from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal static bool ResolveRowValueTable(IntPtr stack, string columnName)
        {
            LuaApi.lua_pushstring(stack, columnName);
            LuaApi.lua_rawget(stack, -2);

            return ApiHelper.LastItemIsTable(stack);
        }

        #region Tested. Works fine
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        internal static string ReadRowValueString(IntPtr state, string columnName)
        {
            LuaApi.lua_pushstring(state, columnName);
            LuaApi.lua_rawget(state, -2);

            string result = null;

            if (LuaApi.lua_isstring(state, -1) > 0)
            {
                var pstr = LuaApi.lua_tolstring(state, -1, out ulong len);

                if (len > 0)
                {
                    result = Marshal.PtrToStringAnsi(pstr, (int)len);
                }
            }

            PopTwoFromStack(state);
            return result;
        }
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        internal static long? ReadRowValueLong(IntPtr state, string columnName)
        {
            LuaApi.lua_pushstring(state, columnName);
            LuaApi.lua_rawget(state, -2);

            long? result = null;

            if (LuaApi.lua_type(state, -1) == LuaApi.TYPE_NUMBER)
            {
                result = (long)LuaApi.lua_tonumberx(state, -1, IntPtr.Zero);
            }

            PopTwoFromStack(state);
            return result;
        }

        internal static bool ExecFunction(IntPtr stack, string name, int returnType)
        {
            LuaApi.lua_getglobal(stack, name);

            return LuaApi.lua_pcallk(stack, 0, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(stack, -1) == returnType;
        }
        internal static bool ExecFunction(IntPtr stack, string name, int returnType, string arg0)
        {
            LuaApi.lua_getglobal(stack, name);
            LuaApi.lua_pushstring(stack, arg0);

            return LuaApi.lua_pcallk(stack, 1, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(stack, -1) == returnType;
        }
        internal static bool ExecFunction(IntPtr stack, string name, int returnType, string arg0, string arg1)
        {
            LuaApi.lua_getglobal(stack, name);
            LuaApi.lua_pushstring(stack, arg0);
            LuaApi.lua_pushstring(stack, arg1);

            return LuaApi.lua_pcallk(stack, 2, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(stack, -1) == returnType; ;
        }
        internal static bool ExecFunction(IntPtr stack, string name, int returnType, string arg0, string arg1, string arg2)
        {
            LuaApi.lua_getglobal(stack, name);
            LuaApi.lua_pushstring(stack, arg0);
            LuaApi.lua_pushstring(stack, arg1);
            LuaApi.lua_pushstring(stack, arg2);

            return LuaApi.lua_pcallk(stack, 3, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(stack, -1) == returnType; ;
        }
        #endregion

        /// <summary>
        /// Pops last item from the stack
        /// </summary>
        internal static void PopFromStack(IntPtr state)
        {
            LuaApi.lua_settop(state, -1);
        }

        /// <summary>
        /// Pops last two items from the stack
        /// </summary>
        internal static void PopTwoFromStack(IntPtr state)
        {
            LuaApi.lua_settop(state, -2);
        }

        /// <summary>
        /// Pops n items from the stack
        /// </summary>
        internal static void PopFromStack(IntPtr state, int numItems)
        {
            LuaApi.lua_settop(state, -numItems);
        }
    }
}
