#pragma once

#include "pch.h"

#ifndef BOOTSTRAP_LOADED
#define EXPORT_SPEC __declspec( dllexport )
#else
#define EXPORT_SPEC __declspec( dllimport )
#endif

namespace NativeToManagedProxy
{
	EXPORT_SPEC int Initialize(void* luaState);
}