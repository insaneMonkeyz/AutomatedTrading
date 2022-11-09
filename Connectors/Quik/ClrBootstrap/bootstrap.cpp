#include "pch.h"
#include "bootstrap.h"

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        Quik::QuikProxy^ apiWrapper = gcnew Quik::QuikProxy();
        return apiWrapper->Initialize(luaStack);
    }
}