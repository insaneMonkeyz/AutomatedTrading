using System.Runtime.InteropServices;
using KeraLua;

namespace LuaGate
{
    internal struct LuaState
    {
        private static readonly LuaKFunction emptyKFunction = delegate { return 0; };
        private readonly IntPtr state;

        public unsafe LuaState(void* state)
        {
            this.state = new IntPtr(state);
        }
        public LuaState(IntPtr state)
        {
            this.state = state;
        }

        public void TieProxyLibrary(string dllname)
        {
            var reg = new LuaRegister[]
            {
                new()
            };

            LuaApi.lua_createtable(state, 0, 0);
            LuaApi.luaL_setfuncs(state, reg, 0);
            LuaApi.lua_pushvalue(state, -1);
            LuaApi.lua_setglobal(state, dllname);
        }
        public void RegisterCallback(LuaFunction function, string alias)
        {
            LuaApi.lua_pushcclosure(state, function, 0);
            LuaApi.lua_setglobal(state, alias);
        }

        public int Exec(string function)
        {
            LuaApi.lua_getglobal(state, function);
            return LuaApi.lua_pcallk(state, 0, 1, 0, IntPtr.Zero, emptyKFunction);
        }
        public int Exec(string function, string arg)
        {
            LuaApi.lua_getglobal(state, function);
            LuaApi.lua_pushstring(state, arg);
            return LuaApi.lua_pcallk(state, 1, 1, 0, IntPtr.Zero, emptyKFunction);
        }
        public long ToLong(int i)
        {
            return LuaApi.lua_tointegerx(state, i, IntPtr.Zero);
        }
        public string ToString(int i)
        {
            return Marshal.PtrToStringAnsi(LuaApi.lua_tolstring(state, i, out ulong len));
        }

        public static implicit operator LuaState(IntPtr pointer) => new(pointer);
    }
}