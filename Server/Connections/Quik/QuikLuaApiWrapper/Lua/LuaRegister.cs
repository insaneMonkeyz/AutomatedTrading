using System.Runtime.InteropServices;

namespace Quik.Lua
{
    /// <summary>
    /// LuaRegister stores the name and the delegate to register a native function
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LuaRegister
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name;
        /// <summary>
        /// Function delegate
        /// </summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LuaFunction Function;
    }
}