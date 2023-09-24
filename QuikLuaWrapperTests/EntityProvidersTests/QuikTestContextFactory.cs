namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal class QuikTestContextFactory
    {
        public static QuikTestContextFactory Instance { get; } = new ();
        private QuikTestContextFactory() { }


    }
}