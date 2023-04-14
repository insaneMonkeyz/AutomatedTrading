namespace Quik.EntityProviders.QuikApiWrappers
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class QuikCallbackFieldAttribute : Attribute
    {
        public string Parameter { get; }

        public QuikCallbackFieldAttribute(string parameter)
        {
            Parameter = parameter;
        }
    }
}
