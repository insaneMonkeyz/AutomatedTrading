namespace Quik.EntityProviders.Attributes
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
