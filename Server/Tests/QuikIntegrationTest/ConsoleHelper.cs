using System.Runtime.CompilerServices;

namespace QuikIntegrationTest
{
    static class ConsoleHelper
    {
        public static void NotifyTestStarted([CallerMemberName] string? callerMethod = null)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine($"          {callerMethod ?? "UNKNOWN METHOD"} Started");
            Console.WriteLine("=======================================================");
        }
        public static string? AskUser(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }
        public static void PrintException(string msg, Exception e)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine($"                           {msg}");
            Console.WriteLine(e);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        public static void KeepAskingUntilInputIsValid(
            string initialMessage, string messageOnFailure, Action<string?> processInput)
        {
            Console.WriteLine(initialMessage);
            var input = Console.ReadLine();

            retryOnException:
            try
            {
                processInput(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(messageOnFailure);
                goto retryOnException;
            }

        }
    }
}