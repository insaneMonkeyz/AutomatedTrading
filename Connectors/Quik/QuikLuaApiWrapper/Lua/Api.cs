using System.Runtime.InteropServices;
using System.Security;
using LPStr = System.IntPtr;
using LuaAlloc = System.IntPtr;
using LuaDebug = System.IntPtr;
using LuaHook = System.IntPtr;
using LuaKContext = System.IntPtr;
using LuaReader = System.IntPtr;
using LuaWFunction = System.IntPtr;
using LuaWriter = System.IntPtr;

namespace Quik.Lua
{
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
    public delegate int LuaFunction(LPStr luaState);

    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaKFunction(LPStr luaState, int status, long ctx);

    internal struct Api
    {
        internal static readonly LuaKFunction EmptyKFunction = delegate { return 0; };

        internal const string LUA_DLLNAME = "lua54";
        internal const int OK_RESULT = 0;
        internal const int TYPE_NULL = 0;
        internal const int TYPE_BOOL = 1;
        internal const int TYPE_NUMBER = 3;
        internal const int TYPE_STRING = 4;
        internal const int TYPE_TABLE = 5;

        internal const int TRUE = 1;


        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_createtable(LPStr luaState, int narr, int nrec);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_setfuncs(LPStr luaState, [In] LuaRegister[] luaReg, int numUp);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushvalue(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_setglobal(LPStr luaState, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern LPStr lua_pushstring(LPStr state, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getglobal(LPStr state, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_pushnumber(LPStr state, double n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_pcallk(LPStr state, int nargs, int nresults, int msgh, LuaKContext ctx, [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushcclosure(LPStr luaState, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction f, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long lua_tointeger(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_absindex(LPStr luaState, int idx);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_arith(LPStr luaState, int op);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_atpanic(LPStr luaState, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction panicf);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_callk(LPStr luaState, int nargs, int nresults, LuaKContext ctx, [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_checkstack(LPStr luaState, int extra);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_close(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_compare(LPStr luaState, int index1, int index2, int op);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_concat(LPStr luaState, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_copy(LPStr luaState, int fromIndex, int toIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_dump(LPStr luaState, LuaWriter writer, LPStr data, int strip);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_error(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gc(LPStr luaState, int what, int data);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gc(LPStr luaState, int what, int data, int data2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaAlloc lua_getallocf(LPStr luaState, ref LPStr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getfield(LPStr luaState, int index, string k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaHook lua_gethook(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gethookcount(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gethookmask(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_geti(LPStr luaState, int index, long i);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_getinfo(LPStr luaState, string what, LuaDebug ar);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getiuservalue(LPStr luaState, int idx, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_getlocal(LPStr luaState, LuaDebug ar, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getmetatable(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getstack(LPStr luaState, int level, LuaDebug n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gettable(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gettop(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_getupvalue(LPStr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_iscfunction(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isinteger(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isnumber(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isstring(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isuserdata(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_isyieldable(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_len(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int lua_load
           (LPStr luaState,
            LuaReader reader,
            LPStr data,
            string chunkName,
            string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_newstate(LuaAlloc allocFunction, LPStr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_newthread(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_newuserdatauv(LPStr luaState, ulong size, int nuvalue);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_next(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushboolean(LPStr luaState, int value);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushinteger(LPStr luaState, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushlightuserdata(LPStr luaState, LPStr udata);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_pushlstring(LPStr luaState, byte[] s, ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushnil(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_pushthread(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawequal(LPStr luaState, int index1, int index2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawget(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawgeti(LPStr luaState, int index, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_rawgetp(LPStr luaState, int index, LPStr p);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong lua_rawlen(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawset(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawseti(LPStr luaState, int index, long i);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawsetp(LPStr luaState, int index, LPStr p);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_resetthread(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_resume(LPStr luaState, LPStr from, int nargs, out int results);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rotate(LPStr luaState, int index, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setallocf(LPStr luaState, LuaAlloc f, LPStr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_setfield(LPStr luaState, int index, string key);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_sethook(LPStr luaState, LuaHook f, int mask, int count);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_seti(LPStr luaState, int index, long n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setiuservalue(LPStr luaState, int index, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_setlocal(LPStr luaState, LuaDebug ar, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setmetatable(LPStr luaState, int objIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settable(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settop(LPStr luaState, int newTop);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_setupvalue(LPStr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setwarnf(LPStr luaState, LuaWFunction warningFunctionPtr, LPStr ud);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_status(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern ulong lua_stringtonumber(LPStr luaState, string s);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_toboolean(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_tocfunction(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        internal static extern LuaFunction lua_toclose(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long lua_tointegerx(LPStr luaState, int index, LPStr isNum);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_tolstring(LPStr luaState, int index, out ulong strLen);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double lua_tonumberx(LPStr luaState, int index, LPStr isNum);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_topointer(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_tothread(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_touserdata(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_type(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string lua_typename(LPStr luaState, int type);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr lua_upvalueid(LPStr luaState, int funcIndex, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_upvaluejoin(LPStr luaState, int funcIndex1, int n1, int funcIndex2, int n2);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double lua_version(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void lua_warning(LPStr luaState, string msg, int tocont);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_xmove(LPStr from, LPStr to, int n);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_yieldk(LPStr luaState,
            int nresults,
            LuaKContext ctx,
            [MarshalAs(UnmanagedType.FunctionPtr)] LuaKFunction k);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_argerror(LPStr luaState, int arg, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_callmeta(LPStr luaState, int obj, string e);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_checkany(LPStr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_checkinteger(LPStr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string luaL_checklstring(LPStr luaState, int arg, out ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double luaL_checknumber(LPStr luaState, int arg);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_checkoption(LPStr luaState, int arg, string def, string[] list);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_checkstack(LPStr luaState, int sz, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_checktype(LPStr luaState, int arg, int type);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern LPStr luaL_checkudata(LPStr luaState, int arg, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void luaL_checkversion_(LPStr luaState, double ver, ulong sz);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_error(LPStr luaState, string message);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_execresult(LPStr luaState, int stat);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_fileresult(LPStr luaState, int stat, string fileName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_getmetafield(LPStr luaState, int obj, string e);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_getsubtable(LPStr luaState, int index, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_len(LPStr luaState, int index);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_loadbufferx
            (LPStr luaState,
            byte[] buff,
            ulong sz,
            string name,
            string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_loadfilex(LPStr luaState, string name, string mode);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_newmetatable(LPStr luaState, string name);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern LPStr luaL_newstate();

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_openlibs(LPStr luaState);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long luaL_optinteger(LPStr luaState, int arg, long d);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double luaL_optnumber(LPStr luaState, int arg, double d);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_ref(LPStr luaState, int registryIndex);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_requiref(LPStr luaState, string moduleName, [MarshalAs(UnmanagedType.FunctionPtr)] LuaFunction openFunction, int global);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_setmetatable(LPStr luaState, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern LPStr luaL_testudata(LPStr luaState, int arg, string tName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string luaL_tolstring(LPStr luaState, int index, out ulong len);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void luaL_traceback(LPStr luaState, LPStr luaState2, string message, int level);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int luaL_typeerror(LPStr luaState, int arg, string typeName);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_unref(LPStr luaState, int registryIndex, int reference);

        [DllImport(LUA_DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_where(LPStr luaState, int level);
    }
}