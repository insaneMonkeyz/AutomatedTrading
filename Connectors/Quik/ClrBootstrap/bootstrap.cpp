#include "pch.h"
#include "bootstrap.h"
using namespace System;

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        auto quik = gcnew Quik::Quik();
        return quik->Initialize((IntPtr)luaStack);
    }
}