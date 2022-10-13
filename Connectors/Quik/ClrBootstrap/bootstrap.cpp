#include "pch.h"
#include "bootstrap.h"

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        QuikLuaApi::QuikLuaApiWrapper^ apiWrapper = gcnew QuikLuaApi::QuikLuaApiWrapper();
        return apiWrapper->Initialize(luaStack);
    }
}