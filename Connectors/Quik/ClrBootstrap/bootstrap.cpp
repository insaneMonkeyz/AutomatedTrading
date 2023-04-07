#include "pch.h"
#include "bootstrap.h"
using namespace System;

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        return Quik::Loader::Initialize((IntPtr)luaStack);
    }
}