using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using TradingConcepts;

namespace Quik.Lua
{
    internal struct LuaWrap
    {
        public readonly string ThreadName = "Callback Thread";
        public static readonly object SyncRoot = new();

        public const int LAST_ITEM = -1;
        public const int SECOND_ITEM = -2;

        #region Initialization
        private readonly IntPtr _state;
        public LuaWrap(IntPtr lua, string threadName)
        {
            ThreadName = threadName;
            _state = lua;
        }
        private LuaWrap(IntPtr ptr)
        {
            _state = ptr;
        }

        public static implicit operator IntPtr(LuaWrap state) => state._state;
        public static implicit operator LuaWrap(IntPtr pointer) => new(pointer);
        public static bool operator ==(LuaWrap left, LuaWrap right) => left.Equals(right);
        public static bool operator !=(LuaWrap left, LuaWrap right) => !left.Equals(right);
        #endregion

        public bool Equals(LuaWrap other)
        {
            return _state == other._state;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is LuaWrap other && other._state == _state;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_state, 64425674);
        }
        public override string ToString()
        {
            return _state.ToString();
        }

        internal string? SafeReadString(int i)
        {
            if (Api.lua_isstring(_state, i) <= 0)
            {
                return null;
            }

            Api.lua_pushnil(_state);
            Api.lua_copy(_state, i-1, LAST_ITEM);

            var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

            var result = len > 0
                ? Marshal.PtrToStringAnsi(pstr, (int)len)
                : string.Empty;

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal string? SafeReadString()
        {
            if (Api.lua_isstring(_state, LAST_ITEM) <= 0)
            {
                return null;
            }

            Api.lua_pushnil(_state);
            Api.lua_copy(_state, SECOND_ITEM, LAST_ITEM);

            var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

            var result = len > 0
                ? Marshal.PtrToStringAnsi(pstr, (int)len)
                : string.Empty;

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal long SafeReadLong()
        {
            if (Api.lua_isinteger(_state, LAST_ITEM) <= 0)
            {
                return 0L;
            }

            Api.lua_pushnil(_state);
            Api.lua_copy(_state, SECOND_ITEM, LAST_ITEM);

            long result = Api.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero);
            Api.lua_settop(_state, SECOND_ITEM);
            return result;
        }
        internal Decimal5 SafeReadDecimal5()
        {
            if (Api.lua_isnumber(_state, LAST_ITEM) <= 0)
            {
                return default;
            }

            Api.lua_pushnil(_state);
            Api.lua_copy(_state, SECOND_ITEM, LAST_ITEM);

            Decimal5 result = Api.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
            Api.lua_settop(_state, SECOND_ITEM);
            return result;
        }

        internal string ReadValueSafe(LuaTypes type, IntPtr pStack, int i)
        {
            Api.lua_pushnil(pStack);
            Api.lua_copy(pStack, i - 1, LAST_ITEM);

            var value = type switch
            {
                LuaTypes.Boolean => (Api.lua_toboolean(pStack, LAST_ITEM) == 1).ToString(),
                LuaTypes.Number => Api.lua_tonumberx(pStack, LAST_ITEM, IntPtr.Zero).ToString(),
                LuaTypes.String => Marshal.PtrToStringAnsi(Api.lua_tolstring(pStack, LAST_ITEM, out ulong strlen)),
                LuaTypes.Table => Api.lua_rawlen(pStack, LAST_ITEM).ToString(),
                _ => throw new NotSupportedException(),
            };

            Api.lua_settop(pStack, SECOND_ITEM);

            return value;
        }
        internal void PrintStack(string comment = null)
        {
            if (!GlobalParameters.TraceLuaApiCalls)
            {
                return; 
            }

            lock (SyncRoot)
            {
                Debug.Print(comment != null
                        ? $"========= {ThreadName} {_state} {comment} ========="
                        : $"========= {ThreadName} {_state} =========");


                for (int i = -1; i > -6; i--)
                {
                    var type = (LuaTypes)Api.lua_type(_state, i);

                    var value = type switch
                    {
                        LuaTypes.Boolean => $"Bool {ReadValueSafe(type, _state, i)}",
                        LuaTypes.Number => $"Number {ReadValueSafe(type, _state, i)}",
                        LuaTypes.String => $"String {ReadValueSafe(type, _state, i)}",
                        LuaTypes.Table => $"Table Size:{ReadValueSafe(type, _state, i)}",
                        _ => type.ToString()        //возможно баг связан с попаданием сюда, когда наверху лежит nil
                    };

                    Debug.Print($"[{-i}] [{value}]");
                } 
            }
        }
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a number from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool TryFetchDecimalFromTable(string columnName, out Decimal5 result)
        {
            //PrintStack("Beginning TryFetchDecimalFromTable " + columnName);

            // lua_rawget заменяет значение в ячейке где lua_pushstring положила ключ.
            // 2 строчки создадут только 1 ячейку памяти
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (TryPopDecimal(out result))
            {
                //PrintStack("Completed TryFetchDecimalFromTable success " + columnName);
                return true;
            }
            else
            {
                PrintStack("Completed TryFetchDecimalFromTable fail " + columnName);
                PopFromStack();
                return false;
            }
        }
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a number from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool TryFetchLongFromTable(string columnName, out long result)
        {
            // lua_rawget заменяет значение в ячейке где lua_pushstring положила ключ.
            // 2 строчки создадут только 1 ячейку памяти
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (TryPopLong(out result))
            {
                return true;
            }
            else
            {
                PopFromStack();
                return false;
            }
        }
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a string from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool TryFetchCharFromTable(string columnName, out char result)
        {
            // lua_rawget заменяет значение в ячейке где lua_pushstring положила ключ.
            // 2 строчки создадут только 1 ячейку памяти
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (TryPopChar(out result))
            {
                return true;
            }
            else
            {
                PopFromStack();
                return false;
            }
        }
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a string from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool TryFetchStringFromTable(string columnName, out string result)
        {
            // lua_rawget заменяет значение в ячейке где lua_pushstring положила ключ.
            // 2 строчки создадут только 1 ячейку памяти
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (TryPopString(out result))
            {
                return true;
            }
            else
            {
                PopFromStack();
                return false;
            }
        }
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a table from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool PushColumnValueTable(string columnName)
        {
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (LastItemIsTable())
            {
                return true;
            }
            else
            {
                PopFromStack();
                return false;
            }
        }

        internal bool TryPopLong(out long value)
        {
            value = 0L;

            if (Api.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = Api.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero);
                Api.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryPopDecimal(out Decimal5 value)
        {
            value = 0L;

            if (Api.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = Api.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
                Api.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryPopChar(out char value)
        {
            value = default;

            if (Api.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                if (len > 0)
                {
                    unsafe
                    {
                        value = *(char*)pstr.ToPointer();
                    }
                }

                Api.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryPopString(out string value)
        {
            value = null;

            if (Api.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                if (len > 0)
                {
                    value = Marshal.PtrToStringAnsi(pstr, (int)len);
                }
                else
                {
                    value = string.Empty;
                }

                Api.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryReadString(int i, out string value)
        {
            value = null;

            if (Api.lua_isstring(_state, i) > 0)
            {
                var pstr = Api.lua_tolstring(_state, i, out ulong len);

                value = len > 0 ? Marshal.PtrToStringAnsi(pstr, (int)len) : string.Empty;

                return true;
            }

            return false;
        }

        internal string PopString()
        {
            var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

            var value = len > 0
                ? Marshal.PtrToStringAnsi(pstr, (int)len)
                : string.Empty;

            Api.lua_settop(_state, SECOND_ITEM);

            return value;
        }
        internal long PopNumber()
        {
            var value = 0L;

            PrintStack();

            if (Api.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = Api.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero);

                PrintStack();
            }

            Api.lua_settop(_state, SECOND_ITEM);

            PrintStack();

            return value;
        }
        internal void TieProxyLibrary(string dllname)
        {
            var reg = new LuaRegister[]
            {
                new()
            };

            Api.lua_createtable(_state, 0, 0);
            Api.luaL_setfuncs(_state, reg, 0);
            Api.lua_pushvalue(_state, LAST_ITEM);
            Api.lua_setglobal(_state, dllname);
        }
        internal void RegisterCallback(LuaFunction function, string alias)
        {
            Api.lua_pushcclosure(_state, function, 0);
            Api.lua_setglobal(_state, alias);
        }

        internal long ReadAsNumber()
        {
            return Api.lua_isnumber(_state, LAST_ITEM) > 0
                 ? Api.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero)
                 : 0;
        }
        internal bool ReadAsBool()
        {
            return Api.lua_isnumber(_state, LAST_ITEM) > 0 &&
                   Api.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero) == Api.TRUE;
        }
        internal string? ReadAsString()
        {
            if (Api.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                return len > 0 ? Marshal.PtrToStringAnsi(pstr, (int)len) : string.Empty;
            }

            return null;
        }
        internal string? ReadAsString(int index)
        {
            if (Api.lua_isstring(_state, index) > 0)
            {
                var pstr = Api.lua_tolstring(_state, index, out ulong len);

                return len > 0 ? Marshal.PtrToStringAnsi(pstr, (int)len) : string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal char ReadRowValueChar(string columnName)
        {
            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            switch (Api.lua_type(_state, LAST_ITEM))
            {
                case Api.TYPE_STRING:
                    {
                        var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                        char result = default;

                        if (len > 0)
                        {
                            unsafe { result = *(char*)pstr.ToPointer(); }
                        }

                        PopFromStack();

                        return result;

                    }
                case Api.TYPE_NULL:
                    {
                        PopFromStack();
                        return default;
                    }
                default: throw QuikApiException.ParseExceptionMsg(columnName, "char");
            }
        }

        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal string? ReadRowValueString(string columnName)
        {
            PrintStack("Beginning ReadRowValueString " + columnName);

            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            switch (Api.lua_type(_state, LAST_ITEM))
            {
                case Api.TYPE_STRING:
                    {
                        var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                        var result = len > 0
                            ? Marshal.PtrToStringAnsi(pstr, (int)len)
                            : string.Empty;

                        PrintStack("Completed ReadRowValueString success" + columnName);

                        PopFromStack();

                        return result;

                    }
                case Api.TYPE_NULL:
                    {
                        PrintStack("Completed ReadRowValueString null" + columnName);

                        PopFromStack();
                        return null;
                    }
                default: throw QuikApiException.ParseExceptionMsg(columnName, "string");
            }
        }
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal long ReadRowValueLong(string columnName)
        {
            PrintStack("ReadRowValueLong " + columnName);

            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            long result;

            if (Api.lua_isnumber(_state, LAST_ITEM) == Api.TRUE)
            {
                result = (long)Api.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
                PopFromStack();
            }
            else
            {
                PopFromStack();
                throw QuikApiException.ParseExceptionMsg(columnName, "number");
            }
            PrintStack("ReadRowValueLong completed. " + columnName);

            return result;
        }
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal string? ReadRowValueNumber(string columnName)
        {
            PrintStack("ReadRowValueNumber " + columnName);

            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            if (Api.lua_isnumber(_state, LAST_ITEM) == Api.TRUE)
            {
                var pstr = Api.lua_tolstring(_state, LAST_ITEM, out ulong len);

                var result = len > 0
                    ? Marshal.PtrToStringAnsi(pstr, (int)len)
                    : string.Empty;

                PopFromStack();

                return result;
            }
            else
            {
                PopFromStack();
                throw QuikApiException.ParseExceptionMsg(columnName, "number");
            }
        }
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal Decimal5 ReadRowValueDecimal5(string columnName)
        {
            PrintStack("ReadRowValueDecimal5 " + columnName);

            Api.lua_pushstring(_state, columnName);
            Api.lua_rawget(_state, SECOND_ITEM);

            Decimal5 result;

            if (Api.lua_isnumber(_state, LAST_ITEM) == Api.TRUE)
            {
                result = Api.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
                PopFromStack();
            }
            else
            {
                PopFromStack();
                throw QuikApiException.ParseExceptionMsg(columnName, "decimal5");
            }
            PrintStack("ReadRowValueDecimal5 completed. " + columnName);

            return result;
        }

        internal bool ExecMultyReturnFunction(string name, int returnType1, int returnType2, string arg0, string arg1)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushnumber(_state, double.Parse(arg1));

            return Api.lua_pcallk(_state, 2, 2, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType2
                && Api.lua_type(_state, SECOND_ITEM) == returnType1;
        }
        internal bool ExecFunction(string name, int returnType)
        {
            Api.lua_getglobal(_state, name);

            return Api.lua_pcallk(_state, 0, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType;
        }
        internal bool ExecFunction(string name, int returnType, string arg0)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);

            return Api.lua_pcallk(_state, 1, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);

            return Api.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, long arg1)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushinteger(_state, arg1);

            return Api.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, string arg2)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);
            Api.lua_pushstring(_state, arg2);

            return Api.lua_pcallk(_state, 3, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, string arg2, string arg3)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);
            Api.lua_pushstring(_state, arg2);
            Api.lua_pushstring(_state, arg3);

            return Api.lua_pcallk(_state, 4, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, long arg2, string arg3)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);
            Api.lua_pushnumber(_state, arg2);
            Api.lua_pushstring(_state, arg3);

            return Api.lua_pcallk(_state, 4, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT
                && Api.lua_type(_state, LAST_ITEM) == returnType; ;
        }

        internal T ExecFunction<T>(string name, int returnType, Func<T> callback)
        {
            Api.lua_getglobal(_state, name);

            T result = default;

            if (Api.lua_pcallk(_state, 0, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT &&
                Api.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);

            T result = default;

            // pcallk заменит все аргументы которые потребовались для его вызова на результат или nil
            if (Api.lua_pcallk(_state, 1, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT &&
                Api.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0, string arg1)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);

            T result = default;

            // pcallk заменит все аргументы которые потребовались для его вызова на результат или nil
            if (Api.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT &&
                Api.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0, string arg1, string arg2)
        {
            Api.lua_getglobal(_state, name);
            Api.lua_pushstring(_state, arg0);
            Api.lua_pushstring(_state, arg1);
            Api.lua_pushstring(_state, arg2);

            T result = default;

            if (Api.lua_pcallk(_state, 3, 1, 0, IntPtr.Zero, Api.EmptyKFunction) == Api.OK_RESULT &&
                Api.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            Api.lua_settop(_state, SECOND_ITEM);

            return result;
        }

        /// <summary>
        /// Pops last item from the stack
        /// </summary>
        internal void PopFromStack()
        {
            Api.lua_settop(_state, SECOND_ITEM);
        }
        /// <summary>
        /// Pops last two items from the stack
        /// </summary>
        internal void PopTwoFromStack()
        {
            Api.lua_settop(_state, -3);
        }
        /// <summary>
        /// Pops n items from the stack
        /// </summary>
        internal void PopFromStack(int numItems)
        {
            Api.lua_settop(_state, -numItems - 1);
        }

        internal bool LastItemIsTable()
        {
            return Api.lua_type(_state, LAST_ITEM) == Api.TYPE_TABLE;
        }
        internal bool LastItemIsString()
        {
            return Api.lua_type(_state, LAST_ITEM) == Api.TYPE_STRING;
        }
    }
}