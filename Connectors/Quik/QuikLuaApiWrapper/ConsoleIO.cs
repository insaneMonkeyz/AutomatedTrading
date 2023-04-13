using System.Runtime.InteropServices;
using Core;
using Core.Tools;
using Quik.Entities;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik
{
    static partial class ConsoleIO
    {
        [DllImport("kernel32")]
        private static extern bool AllocConsole();

        private static bool _isRunning = true;
        private static IQuik _quik;
        private static Security? _currentSecurity;
        private static Order? _currentOrder;
        private static ITradingAccount? _currentAccount;

        public static void Main()
        {
            Task.Delay(5000);

            if (!AllocConsole())
            {
                LogManagement.GetLogger("ConsoleIO").Fatal("Failed to allocate the console");
            }

            _quik = DI.Resolve<IQuik>();
            _quik.NewOrder += (order) =>
            {
                Console.WriteLine($"New Order received: {order}");
            };
            _quik.OrderChanged += (order) =>
            {
                Console.WriteLine($"Order changed: {order}");
            };

            var commandsList =
                "find [security type] [ticker]\n" +
                "sell [price] [size] [gtd date]\n" +
                " buy [price] [size] [gtd date]\n" +
                "move [price] [size]\n" +
                "cancel\n" +
                "reset\n" +
                "refresh\n" +
                "terminate";

            var handlers = new Dictionary<string, Action<string[]>>
            {
                { "find", FindSecurityHandler },
                { "sell", TradeSecurity },
                { "byu", TradeSecurity },
                { "move", MoveOrder },
                { "cancel", CancelOrder },
                { "reset", Reset },
                { "refresh", Refresh },
                { "terminate", Terminate },
            };

            while (_isRunning)
            {
                Console.Clear();
                Console.WriteLine(commandsList);
                if (_currentSecurity != null)
                {
                    Console.WriteLine($"Security: {_currentSecurity}");
                }
                if (_currentOrder != null)
                {
                    Console.WriteLine($"Order: {_currentOrder}");
                }
                if (_currentAccount == null)
                {
                    _currentAccount = _quik.Account;
                    Console.WriteLine($"Account: {_currentAccount?.ToString() ?? "null"}");
                }
                else
                {
                    Console.WriteLine($"Account: {_currentAccount?.ToString() ?? "null"}");
                }

                try
                {
                    var input = Console.ReadLine();

                    var cmdparams = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    handlers[cmdparams[0]].Invoke(cmdparams);
                }
                catch (Exception e)
                {
                    LogManagement.GetLogger("ConsoleIO").Error("Error in console interface", e);
                    Console.WriteLine($"\n{e}\n");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
        }

        private static void Refresh(string[] args) { }
        private static void Terminate(string[] args) { _isRunning = false; }
        private static void Reset(string[] args)
        {
            _currentOrder = null;
            _currentSecurity = null;
        }

        private const int TRADECMD_PRICE_INDEX = 1;
        private const int TRADECMD_SIZE_INDEX = 2;
        private const int TRADECMD_CANCEL_DATE_INDEX = 3;
        private const int TRADE_ORDER_WITH_GTD_PARAM_ARGS_NUM = 4;
        private static void TradeSecurity(string[] args)
        {
            var submission = new MoexOrderSubmission(_currentSecurity)
            {
                AccountCode = _currentAccount.AccountCode,
                ClientCode = _currentSecurity.ClassCode,
                Quote = new Quote()
                {
                    Price = Decimal5.Parse(args[TRADECMD_PRICE_INDEX]),
                    Size = long.Parse(args[TRADECMD_SIZE_INDEX]),
                    Operation = args[0] switch
                    {
                        "sell" => Operations.Sell,
                        "buy" => Operations.Buy,
                    }
                },
                TransactionId = TransactionIdGenerator.CreateId(),
                ExecutionCondition = args.Length == TRADE_ORDER_WITH_GTD_PARAM_ARGS_NUM
                    ? OrderExecutionConditions.GoodTillDate
                    : OrderExecutionConditions.Session,
                Expiry = DateTimeOffset.Now + TimeSpan.FromDays(2),
            };

            _currentOrder = _quik.PlaceNewOrder(submission) as Order;
        }

        private const int FINDCMD_SEC_TYPE_INDEX = 1;
        private const int FINDCMD_TICKER_INDEX = 2;
        private static void FindSecurityHandler(string[] args)
        {
            var ticker = args[FINDCMD_TICKER_INDEX];
            _currentSecurity = args[FINDCMD_SEC_TYPE_INDEX] switch
            {
                "option" => _quik.GetSecurity<IOption>(ticker) as Security,
                "futures" => _quik.GetSecurity<IFutures>(ticker) as Security,
                "calendar" => _quik.GetSecurity<ICalendarSpread>(ticker) as Security,
                _ => throw new ArgumentException($"Invalid parameter '{args[FINDCMD_SEC_TYPE_INDEX]}'")
            };
        }

        private const int MOVECMD_PRICE_INDEX = 1;
        private const int MOVECMD_SIZE_INDEX = 2;
        private static void MoveOrder(string[] args)
        {
            _quik.ChangeOrder(_currentOrder, Decimal5.Parse(args[MOVECMD_PRICE_INDEX]), long.Parse(args[MOVECMD_SIZE_INDEX]));
        }
        private static void CancelOrder(string[] args)
        {
            _quik.CancelOrder(_currentOrder);
        }
    }
}
