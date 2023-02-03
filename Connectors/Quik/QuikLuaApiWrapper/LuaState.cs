using System.Diagnostics;
using System.Runtime.InteropServices;
using BasicConcepts;
using KeraLua;

namespace Quik
{
    internal class LuaState
    {
        private const int LAST_ITEM = -1;
        private const int SECOND_ITEM = -2;
        private readonly IntPtr _state;

        #region Initialization
        private unsafe LuaState(void* ptr)
        {
            _state = new IntPtr(ptr);
        }
        private LuaState(IntPtr ptr)
        {
            _state = ptr;
        }

        public static implicit operator IntPtr(LuaState state) => state._state;
        public static implicit operator LuaState(IntPtr pointer) => new(pointer);
        public static bool operator ==(LuaState state1, LuaState state2) => state1 == state2;
        public static bool operator !=(LuaState state1, LuaState state2) => state1 != state2;
        public static unsafe implicit operator LuaState(void* pointer) => new(pointer); 
        #endregion

        internal string ReadValueSafe(LuaTypes type, IntPtr pStack, int i)
        {
            LuaApi.lua_pushnil(pStack);
            LuaApi.lua_copy(pStack, i - 1, LAST_ITEM);

            var value = type switch
            {
                LuaTypes.Boolean => (LuaApi.lua_toboolean(pStack, LAST_ITEM) == 1).ToString(),
                LuaTypes.Number => LuaApi.lua_tonumberx(pStack, LAST_ITEM, IntPtr.Zero).ToString(),
                LuaTypes.String => Marshal.PtrToStringAnsi(LuaApi.lua_tolstring(pStack, LAST_ITEM, out ulong strlen)),
                LuaTypes.Table => LuaApi.lua_rawlen(pStack, LAST_ITEM).ToString(),
                _ => throw new NotSupportedException(),
            };

            LuaApi.lua_settop(pStack, SECOND_ITEM);

            return value;
        }
        internal void PrintStack(string comment = null)
        {
            return;
            Debug.Print(comment != null
                ? $"========= {_state} {comment} ========="
                : $"========= {_state} ===========================");


            for (int i = -1; i > -4; i--)
            {
                var type = (LuaTypes)LuaApi.lua_type(_state, i);

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
        /// <summary>
        /// Gets a table from the stack, goes to the specified column, retrieves a number from there and pushes it onto the stack
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal bool TryFetchDecimalFromTable(string columnName, out Decimal5 result)
        {
            PrintStack("Beginning TryFetchDecimalFromTable " + columnName);

            // lua_rawget заменяет значение в ячейке где lua_pushstring положила ключ.
            // 2 строчки создадут только 1 ячейку памяти
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            if (TryPopDecimal(out result))
            {
                PrintStack("Completed TryFetchDecimalFromTable success " + columnName);
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
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

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
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

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
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

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
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

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

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = LuaApi.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero);
                LuaApi.lua_settop(_state, SECOND_ITEM);
                return true; 
            }

            return false;
        }
        internal bool TryPopDecimal(out Decimal5 value)
        {
            value = 0L;

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = LuaApi.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
                LuaApi.lua_settop(_state, -2);
                return true; 
            }

            return false;
        }
        internal bool TryPopChar(out char value)
        {
            value = default;

            if (LuaApi.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

                if (len > 0)
                {
                    unsafe
                    {
                        value = *(char*)pstr.ToPointer();
                    }
                }

                LuaApi.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryPopString(out string value)
        {
            value = null;

            if (LuaApi.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

                if (len > 0)
                {
                    value = Marshal.PtrToStringAnsi(pstr, (int)len);
                }
                else
                {
                    value = string.Empty;
                }

                LuaApi.lua_settop(_state, SECOND_ITEM);
                return true;
            }

            return false;
        }
        internal bool TryReadString(int i, out string value)
        {
            value = null;

            if (LuaApi.lua_isstring(_state, i) > 0)
            {
                var pstr = LuaApi.lua_tolstring(_state, i, out ulong len);

                value = len > 0 ? Marshal.PtrToStringAnsi(pstr, (int)len) : string.Empty;

                return true;
            }

            return false;
        }

        internal string PopString()
        {
            var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

            var value = len > 0 
                ? Marshal.PtrToStringAnsi(pstr, (int)len) 
                : string.Empty;

            LuaApi.lua_settop(_state, SECOND_ITEM);

            return value;
        }
        internal long PopNumber()
        {
            var value = 0L;

            PrintStack();

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) > 0)
            {
                value = LuaApi.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero);

                PrintStack();
            }

            LuaApi.lua_settop(_state, SECOND_ITEM);

            PrintStack();

            return value;
        }


        #region Tested
        internal void TieProxyLibrary(string dllname)
        {
            var reg = new LuaRegister[]
            {
                new()
            };

            LuaApi.lua_createtable(_state, 0, 0);
            LuaApi.luaL_setfuncs(_state, reg, 0);
            LuaApi.lua_pushvalue(_state, LAST_ITEM);
            LuaApi.lua_setglobal(_state, dllname);
        }
        internal void RegisterCallback(LuaFunction function, string alias)
        {
            LuaApi.lua_pushcclosure(_state, function, 0);
            LuaApi.lua_setglobal(_state, alias);
        }

        internal long ReadAsNumber()
        {
            return LuaApi.lua_isnumber(_state, LAST_ITEM) > 0
                 ? LuaApi.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero)
                 : 0;
        }
        internal bool ReadAsBool()
        {
            return LuaApi.lua_isnumber(_state, LAST_ITEM) > 0 &&
                   LuaApi.lua_tointegerx(_state, LAST_ITEM, IntPtr.Zero) == LuaApi.TRUE;
        }
        internal string? ReadAsString()
        {
            if (LuaApi.lua_isstring(_state, LAST_ITEM) > 0)
            {
                var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

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
            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            switch (LuaApi.lua_type(_state, LAST_ITEM))
            {
                case LuaApi.TYPE_STRING:
                    {
                        var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

                        char result = default;

                        if (len > 0)
                        {
                            unsafe { result = *(char*)pstr.ToPointer(); }
                        }

                        PopFromStack();

                        return result;

                    }
                case LuaApi.TYPE_NULL:
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

            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            switch (LuaApi.lua_type(_state, LAST_ITEM))
            {
                case LuaApi.TYPE_STRING:
                    {
                        var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

                        var result = len > 0
                            ? Marshal.PtrToStringAnsi(pstr, (int)len)
                            : string.Empty;

                        PrintStack("Completed ReadRowValueString success" + columnName);

                        PopFromStack();

                        return result;

                    }
                case LuaApi.TYPE_NULL:
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

            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            long result;

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) == LuaApi.TRUE)
            {
                result = (long)LuaApi.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
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

            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) == LuaApi.TRUE)
            {
                var pstr = LuaApi.lua_tolstring(_state, LAST_ITEM, out ulong len);

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
            PrintStack("ReadRowValueNumber completed. " + columnName);

            return null;
        }
        /// <summary>
        /// Reads value of a field of the table on top of the stack
        /// </summary>
        /// <exception cref="QuikApiException"/>
        internal Decimal5 ReadRowValueDecimal5(string columnName)
        {
            PrintStack("ReadRowValueDecimal5 " + columnName);

            LuaApi.lua_pushstring(_state, columnName);
            LuaApi.lua_rawget(_state, SECOND_ITEM);

            Decimal5 result;

            if (LuaApi.lua_isnumber(_state, LAST_ITEM) == LuaApi.TRUE)
            {
                result = LuaApi.lua_tonumberx(_state, LAST_ITEM, IntPtr.Zero);
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

        internal bool ExecDoubleReturnFunction(string name, int returnType1, int returnType2, string arg0, string arg1)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushnumber(_state, double.Parse(arg1));

            return LuaApi.lua_pcallk(_state, 2, 2, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM)   == returnType2
                && LuaApi.lua_type(_state, SECOND_ITEM) == returnType1;
        }
        internal bool ExecFunction(string name, int returnType)
        {
            LuaApi.lua_getglobal(_state, name);

            return LuaApi.lua_pcallk(_state, 0, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType;
        }
        internal bool ExecFunction(string name, int returnType, string arg0)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);

            return LuaApi.lua_pcallk(_state, 1, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);

            return LuaApi.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, long arg1)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushinteger(_state, arg1);

            return LuaApi.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, string arg2)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);
            LuaApi.lua_pushstring(_state, arg2);

            return LuaApi.lua_pcallk(_state, 3, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, string arg2, string arg3)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);
            LuaApi.lua_pushstring(_state, arg2);
            LuaApi.lua_pushstring(_state, arg3);

            return LuaApi.lua_pcallk(_state, 4, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType; ;
        }
        internal bool ExecFunction(string name, int returnType, string arg0, string arg1, long arg2, string arg3)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);
            LuaApi.lua_pushnumber(_state, arg2);
            LuaApi.lua_pushstring(_state, arg3);

            return LuaApi.lua_pcallk(_state, 4, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT
                && LuaApi.lua_type(_state, LAST_ITEM) == returnType; ;
        }

        internal T ExecFunction<T>(string name, int returnType, Func<T> callback)
        {
            LuaApi.lua_getglobal(_state, name);

            T result = default;

            if (LuaApi.lua_pcallk(_state, 0, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT &&
                LuaApi.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            LuaApi.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);

            T result = default;

            // pcallk заменит все аргументы которые потребовались для его вызова на результат или nil
            if (LuaApi.lua_pcallk(_state, 1, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT &&
                LuaApi.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            LuaApi.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0, string arg1)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);

            T result = default;

            // pcallk заменит все аргументы которые потребовались для его вызова на результат или nil
            if (LuaApi.lua_pcallk(_state, 2, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT &&
                LuaApi.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            LuaApi.lua_settop(_state, SECOND_ITEM);

            return result;
        }
        internal T ExecFunction<T>(string name, int returnType, Func<T> callback, string arg0, string arg1, string arg2)
        {
            LuaApi.lua_getglobal(_state, name);
            LuaApi.lua_pushstring(_state, arg0);
            LuaApi.lua_pushstring(_state, arg1);
            LuaApi.lua_pushstring(_state, arg2);

            T result = default;

            if (LuaApi.lua_pcallk(_state, 3, 1, 0, IntPtr.Zero, LuaApi.EmptyKFunction) == LuaApi.OK_RESULT &&
                LuaApi.lua_type(_state, LAST_ITEM) == returnType)
            {
                result = callback();
            }

            LuaApi.lua_settop(_state, SECOND_ITEM);

            return result;
        }

        /// <summary>
        /// Pops last item from the stack
        /// </summary>
        internal void PopFromStack()
        {
            LuaApi.lua_settop(_state, SECOND_ITEM);
        }
        /// <summary>
        /// Pops last two items from the stack
        /// </summary>
        internal void PopTwoFromStack()
        {
            LuaApi.lua_settop(_state, -3);
        }
        /// <summary>
        /// Pops n items from the stack
        /// </summary>
        internal void PopFromStack(int numItems)
        {
            LuaApi.lua_settop(_state, -numItems - 1);
        }

        internal bool LastItemIsTable()
        {
            return LuaApi.lua_type(_state, LAST_ITEM) == LuaApi.TYPE_TABLE;
        }
        internal bool LastItemIsString()
        {
            return LuaApi.lua_type(_state, LAST_ITEM) == LuaApi.TYPE_STRING;
        }
        #endregion
    }
}