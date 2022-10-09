using System.Runtime.InteropServices;
using System.Security;
using KeraLua;
using LPStr = System.IntPtr;
using LuaDebug = System.IntPtr;
using LuaHook = System.IntPtr;
using LuaReader = System.IntPtr;
using LuaWriter = System.IntPtr;
using LuaAlloc = System.IntPtr;
using LuaKContext = System.IntPtr;
using LuaKFunction = System.IntPtr;
using LuaWFunction = System.IntPtr;

namespace LuaGate
{
    /// <summary>
    /// LuaRegister stores the name and the delegate to register a native function
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LuaRegister
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name;
        /// <summary>
        /// Function delegate
        /// </summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LuaFunction Function;
    }

    /// <summary>
    /// Type for C# callbacks
    /// In order to communicate properly with Lua, a C function must use the following protocol, which defines
    /// the way parameters and results are passed: a C function receives its arguments from Lua in its stack 
    /// in direct order (the first argument is pushed first). So, when the function starts, lua_gettop(L) returns 
    /// the number of arguments received by the function. The first argument (if any) is at index 1 and its last 
    /// argument is at index lua_gettop(L). To return values to Lua, a C function just pushes them onto the stack, 
    /// in direct order (the first result is pushed first), and returns the number of results. Any other value in 
    /// the stack below the results will be properly discarded by Lua. Like a Lua function, a C function 
    /// called by Lua can also return many results. 
    /// </summary>
    /// <param name="luaState"></param>
    /// <returns></returns>
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaFunction(IntPtr luaState);

    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaKFunction(IntPtr luaState, int status, long ctx);

    /// <summary>
    /// Lua types
    /// </summary>
    /// 
    public enum LuaTypes : int
    {
        /// <summary>
        /// 
        /// </summary>
        None = -1,
        /// <summary>
        /// LUA_TNIL
        /// </summary>
        Nil = 0,
        /// <summary>
        /// LUA_TBOOLEAN
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// LUA_TLIGHTUSERDATA
        /// </summary>
        LightUserData = 2,
        /// <summary>
        /// LUA_TNUMBER
        /// </summary>
        Number = 3,
        /// <summary>
        /// LUA_TSTRING
        /// </summary>
        String = 4,
        /// <summary>
        /// LUA_TTABLE
        /// </summary>
        Table = 5,
        /// <summary>
        /// LUA_TFUNCTION
        /// </summary>
        Function = 6,
        /// <summary>
        /// LUA_TUSERDATA
        /// </summary>
        UserData = 7,
        /// <summary>
        /// LUA_TTHREAD
        /// </summary>
        /// //
        Thread = 8,
    }


    internal struct LuaApi
    {
        internal static readonly LuaKFunction EmptyKFunction = delegate { return 0; };

        internal const string LUA_DLLNAME = "lua54";
        internal const int OK_RESULT = 0;
        internal const int TYPE_NULL = 0;
        internal const int TYPE_BOOL = 1;
        internal const int TYPE_NUMBER = 3;
        internal const int TYPE_STRING = 4;
        internal const int TYPE_TABLE = 5;


        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_createtable(IntPtr luaState, int narr, int nrec);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_setfuncs(IntPtr luaState, [In] LuaRegister[] luaReg, int numUp);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushvalue(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_setglobal(IntPtr luaState, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern LPStr lua_pushstring(IntPtr state, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getglobal(IntPtr state, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_pushnumber(IntPtr state, double n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_pcallk(IntPtr state, int nargs, int nresults, int msgh, LuaKContext ctx, [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushcclosure(IntPtr luaState, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction f, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long lua_tointeger(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_absindex(IntPtr luaState, int idx);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_arith(IntPtr luaState, int op);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_atpanic(IntPtr luaState, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction panicf);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_callk(IntPtr luaState, int nargs, int nresults, LuaKContext ctx, [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_checkstack(IntPtr luaState, int extra);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_close(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_compare(IntPtr luaState, int index1, int index2, int op);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_concat(IntPtr luaState, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_copy(IntPtr luaState, int fromIndex, int toIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_dump(IntPtr luaState, LuaWriter writer, IntPtr data, int strip);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_error(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gc(IntPtr luaState, int what, int data);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gc(IntPtr luaState, int what, int data, int data2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaAlloc lua_getallocf(IntPtr luaState, ref IntPtr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getfield(IntPtr luaState, int index, string k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaHook lua_gethook(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gethookcount(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gethookmask(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_geti(IntPtr luaState, int index, long i);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getinfo(IntPtr luaState, string what, LuaDebug ar);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getiuservalue(IntPtr luaState, int idx, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_getlocal(IntPtr luaState, LuaDebug ar, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getmetatable(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getstack(IntPtr luaState, int level, LuaDebug n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gettable(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gettop(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_getupvalue(IntPtr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_iscfunction(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isinteger(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isnumber(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isstring(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isuserdata(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isyieldable(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_len(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_load
           (IntPtr luaState,
            LuaReader reader,
            IntPtr data,
            string chunkName,
            string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_newstate(LuaAlloc allocFunction, IntPtr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_newthread(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_newuserdatauv(IntPtr luaState, ulong size, int nuvalue);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_next(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushboolean(IntPtr luaState, int value);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushinteger(IntPtr luaState, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushlightuserdata(IntPtr luaState, IntPtr udata);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_pushlstring(IntPtr luaState, byte[] s, ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushnil(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_pushthread(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawequal(IntPtr luaState, int index1, int index2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawget(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawgeti(IntPtr luaState, int index, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawgetp(IntPtr luaState, int index, IntPtr p);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong lua_rawlen(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawset(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawseti(IntPtr luaState, int index, long i);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawsetp(IntPtr luaState, int index, IntPtr p);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_resetthread(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_resume(IntPtr luaState, IntPtr from, int nargs, out int results);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rotate(IntPtr luaState, int index, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setallocf(IntPtr luaState, LuaAlloc f, IntPtr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_setfield(IntPtr luaState, int index, string key);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_sethook(IntPtr luaState, LuaHook f, int mask, int count);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_seti(IntPtr luaState, int index, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setiuservalue(IntPtr luaState, int index, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_setlocal(IntPtr luaState, LuaDebug ar, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setmetatable(IntPtr luaState, int objIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settable(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settop(IntPtr luaState, int newTop);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_setupvalue(IntPtr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setwarnf(IntPtr luaState, LuaWFunction warningFunctionPtr, IntPtr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_status(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern ulong lua_stringtonumber(IntPtr luaState, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_toboolean(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_tocfunction(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_toclose(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long lua_tointegerx(IntPtr luaState, int index, IntPtr isNum);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_tolstring(IntPtr luaState, int index, out ulong strLen);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double lua_tonumberx(IntPtr luaState, int index, IntPtr isNum);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_topointer(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_tothread(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_touserdata(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_type(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_typename(IntPtr luaState, int type);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_upvalueid(IntPtr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_upvaluejoin(IntPtr luaState, int funcIndex1, int n1, int funcIndex2, int n2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double lua_version(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_warning(IntPtr luaState, string msg, int tocont);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_xmove(IntPtr from, IntPtr to, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_yieldk(IntPtr luaState,
            int nresults,
            LuaKContext ctx,
            [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_argerror(IntPtr luaState, int arg, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_callmeta(IntPtr luaState, int obj, string e);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_checkany(IntPtr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_checkinteger(IntPtr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string luaL_checklstring(IntPtr luaState, int arg, out ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double luaL_checknumber(IntPtr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_checkoption(IntPtr luaState, int arg, string def, string[] list);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_checkstack(IntPtr luaState, int sz, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_checktype(IntPtr luaState, int arg, int type);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr luaL_checkudata(IntPtr luaState, int arg, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void luaL_checkversion_(IntPtr luaState, double ver, ulong sz);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_error(IntPtr luaState, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_execresult(IntPtr luaState, int stat);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_fileresult(IntPtr luaState, int stat, string fileName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_getmetafield(IntPtr luaState, int obj, string e);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_getsubtable(IntPtr luaState, int index, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_len(IntPtr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_loadbufferx
            (IntPtr luaState,
            byte[] buff,
            ulong sz,
            string name,
            string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_loadfilex(IntPtr luaState, string name, string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_newmetatable(IntPtr luaState, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr luaL_newstate();

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_openlibs(IntPtr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_optinteger(IntPtr luaState, int arg, long d);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double luaL_optnumber(IntPtr luaState, int arg, double d);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_ref(IntPtr luaState, int registryIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_requiref(IntPtr luaState, string moduleName, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction openFunction, int global);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_setmetatable(IntPtr luaState, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr luaL_testudata(IntPtr luaState, int arg, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string luaL_tolstring(IntPtr luaState, int index, out ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_traceback(IntPtr luaState, IntPtr luaState2, string message, int level);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_typeerror(IntPtr luaState, int arg, string typeName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_where(IntPtr luaState, int level);
    }
}