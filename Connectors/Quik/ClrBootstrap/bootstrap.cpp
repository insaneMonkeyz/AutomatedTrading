#include "pch.h"
#include "bootstrap.h"
using namespace System;

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        auto loader = gcnew Quik::Loader();
        return loader->Initialize((IntPtr)luaStack);
    }
}