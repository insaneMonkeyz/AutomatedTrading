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

        // https://stackoverflow.com/questions/9957544/callbackoncollecteddelegate-in-globalkeyboardhook-was-detected
        private static readonly LuaFunction _onStop = OnStop;
        private static readonly LuaFunction _main = Main;

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
                Lua.RegisterCallback(_main, "main");
                Lua.RegisterCallback(_onStop, "OnStop");

                LiveSmokeTest.Instance.Initialize();
            }
            catch (Exception ex)
            {
                ex.Message.DebugPrintWarning();
                return -1;
            }

            return 0;
        }

        private static int OnStop(IntPtr state)
        {
            try
            {
                lock (SyncRoot)
                {
#if TRACE
                    Extentions.Trace(nameof(Quik));
#endif
                    LiveSmokeTest.Instance.Complete(); 
                }
            }
            catch (Exception e)
            {
                $"{e.Message}\n{e.StackTrace ?? "NO_STACKTRACE_PROVIDED"}".DebugPrintWarning();
                return -1;
            }

            return 1;
        }

        private static int Main(IntPtr state)
        {
            Lua = new(state, "Main thread");

            for (int i = 0; i < 5; i++)
            {
                Api.lua_pushstring(Quik.Lua, "AUTOMATED TRADING");
            }

            try
            {
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

                Debugger.Launch();
                LiveSmokeTest.Instance.Begin();
            }
            catch (Exception ex)
            {
                ex.DebugPrintException();
                return -1;
            }
            return 1;
        }
    }
}