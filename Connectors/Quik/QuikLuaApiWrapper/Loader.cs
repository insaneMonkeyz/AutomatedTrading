using Core;

namespace Quik
{
    public static class Loader
    {
        /// <summary>
        /// Entry Point. This method gets called by lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to lua state struct</param>
        /// <returns></returns>
        public static int Initialize(IntPtr luaStack)
        {
            Quik.Instance.Initialize(luaStack);
            DI.RegisterInstance<IQuik>(Quik.Instance);
            return 1;
        }
    }
}
