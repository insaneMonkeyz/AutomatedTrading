namespace QuikLuaApi
{
    public class QuikApiException : Exception
    {
        public QuikApiException(string message) : base(message)
        {

        }

        public static QuikApiException ParseExceptionMsg(string property, string expectedType)
        {
            return new QuikApiException($"Failed to parse '{property}' value. Provided value is not a {expectedType}");
        }
    }
}