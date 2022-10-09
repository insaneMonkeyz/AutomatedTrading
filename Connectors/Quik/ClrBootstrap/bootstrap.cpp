#include "pch.h"
#include "bootstrap.h"

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        LuaGate::LuaGate^ gate = gcnew LuaGate::LuaGate();
        return gate->Initialize(luaStack);
    }
}