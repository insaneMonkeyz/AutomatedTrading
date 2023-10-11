using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class ConsoleHelper
    {
        public static void TracePrintMethod([CallerMemberName] string? callerMethod = null)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine($"          {callerMethod ?? "UNKNOWN METHOD"}");
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
