// dllmain.cpp : Defines the entry point for the DLL application.

#define BOOTSTRAP_LOADED

#include "pch.h"
#include "..//ClrBootstrap/bootstrap.h"
#include <exception>

extern "C" 
{
    #define LUA_LIB
    #define LUA_BUILD_AS_DLL

    #include "Lua/include/lauxlib.h"
    #include "Lua/include/lua.h"

    LUALIB_API int luaopen_NativeToManagedProxy(lua_State * L)
    {
        return NativeToManagedProxy::Initialize(L);
    } 
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    return TRUE;
}



