#include "pch.h"
#include "bootstrap.h"
using namespace System;

namespace NativeToManagedProxy
{
    #pragma managed
    int Initialize(void* luaStack)
    {
        auto loader = gcnew Quik::Loader();
        //auto loader = gcnew QuikIntegrationTest::IntegrationTest();

        int quikLaunched = loader->Initialize((IntPtr)luaStack) >= 0;

        if (quikLaunched) {
            Core::Core::Initialize();
        }

        return TRUE;
    }
}