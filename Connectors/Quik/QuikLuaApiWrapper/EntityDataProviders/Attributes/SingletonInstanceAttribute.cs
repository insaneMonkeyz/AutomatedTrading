namespace Quik.EntityDataProviders.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class SingletonInstanceAttribute : Attribute
    {
        public SingletonInstanceAttribute() { }
    }
}
