using System.Diagnostics;
using System.Text;
using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik
{
    public delegate void CallbackSubscriber(LuaFunction handler, string quikTableName);

    public class Quik
    {
        internal static readonly object SyncRoot = new();
#if DEBUG
        internal static LuaWrap Lua { get; private set; }
#else
        internal static LuaWrap Lua;
#endif

        /// <summary>
        /// Entry Point. This method gets called from the lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to the Lua state object</param>
        /// <returns></returns>
        public int Initialize(IntPtr luaState)
        {
            Lua = new(luaState, "Initializing thread");

            try
            {
                Lua.TieProxyLibrary("NativeToManagedProxy");
                Lua.RegisterCallback(Main, "main");

                LiveSmokeTest.Instance.Initialize();
            }
            catch (Exception ex)
            {
                ex.Message.DebugPrintWarning();
                return -1;
            }

            return 0;
        }

        private static int Main(IntPtr state)
        {
            Lua = new(state, "Main thread");

            try
            {
                Debugger.Launch();

                LiveSmokeTest.Instance.Begin();

                Thread.Sleep(60 * 1000);

                LiveSmokeTest.Instance.Complete();

                //==========================================================================
                //
                //  WARNING! It turns out that quik rounds long numbers of type 'number'
                //           when reading them as Int64.
                //           The only way to get precise values is to read them as strings.
                //
                //  TODO: Check all API calls that return type 'number' and make sure
                //        they are being treated as strings!
                //
                //==========================================================================

                //while (true)
                //{
                //    Thread.Sleep(1000);
                //    //var i = 5;
                //    //var s = 76;
                //    //var x = 5 * i * s / 128;
                //    //var g = x ^ x;
                //    //var n = g % 7;
                //    //var ff = Math.Max(4534,Math.Pow(n,2));
                //}
            }
            catch (Exception ex)
            {
                $"{ex.Message}\n{ex.StackTrace ?? "NO_STACKTRACE_PROVIDED"}".DebugPrintWarning();
                return -1;
            }
            return 1;
        }
    }
}