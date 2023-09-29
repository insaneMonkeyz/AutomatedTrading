using System.Runtime.InteropServices;
using Quik;
using Quik.Entities;
using Tools;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

using static QuikIntegrationTest.ConsoleHelper;

namespace QuikIntegrationTest
{
    public class IntegrationTest
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("kernel32")]
        static extern bool FreeConsole();

        static IQuik? _quik;

        static Log _logger = LogManagement.GetLogger("QuikIntegrationTest");

        public static int Initialize(IntPtr luaStack)
        {
            var quikInitializationResult = Loader.Initialize(luaStack);
            _quik = DI.Resolve<IQuik>();
            Task.Run(BeginTesting);
            return quikInitializationResult;
        }

        static void BeginTesting()
        {
            if (!AllocConsole())
            {
                _logger.Fatal("Failed to allocate the console");
                return;
            }

            AskUser("Press Enter to begin testing");

            try
            {
                PropertyTest_IsConnected();
                PropertyTest_Account();
                MethodTest_GetAvailableSecurities();
                MethodTest_GetSecurity();
                MethodTest_GetOrders();
                MethodTest_PlaceNewOrder(out IOrder order);
                MethodTest_MoveOrder(order, out IOrder neworder);
                MethodTest_CancelOrder(order);
            }
            catch (Exception e)
            {
                PrintException("Test failed", e);
                _logger.Error("Test failed", e);
            }

            AskUser("Press Enter to finish");
            FreeConsole();
        }

        static void PropertyTest_Account()
        {
            NotifyTestStarted();

            try
            {
                var account = _quik.DerivativesAccount;
            }
            catch (Exception e)
            {
                throw new Exception($"Quik failed to evaluate property {nameof(IQuik.DerivativesAccount)}", e);
            }
        }
        static void PropertyTest_IsConnected()
        {
            NotifyTestStarted();

            try
            {
                var connected = _quik.IsConnected;
            }
            catch (Exception e)
            {
                throw new Exception($"Quik failed to evaluate property {nameof(IQuik.IsConnected)}", e);
            }
        }
        static void MethodTest_GetAvailableSecurities()
        {
            NotifyTestStarted();

            var futures = _quik.GetAvailableSecurities<IFutures>();

            if (!futures.Any())
            {
                throw new Exception($"{nameof(IQuik.GetAvailableSecurities)}<{nameof(IFutures)}> did not yield any result");
            }

            var options = _quik.GetAvailableSecurities<IOption>();

            if (!options.Any())
            {
                throw new Exception($"{nameof(IQuik.GetAvailableSecurities)}<{nameof(IOption)}> did not yield any result");
            }

            var calendars = _quik.GetAvailableSecurities<ICalendarSpread>();

            if (!calendars.Any())
            {
                throw new Exception($"{nameof(IQuik.GetAvailableSecurities)}<{nameof(ICalendarSpread)}> did not yield any result");
            }
        }
        static void MethodTest_GetSecurity()
        {
            var definition = _quik.GetAvailableSecurities<IFutures>().First();
            var sec = _quik.GetSecurity<IFutures>(definition.Ticker);

            if (sec is null)
            {
                throw new Exception($"{nameof(IQuik.GetSecurity)}<{nameof(IFutures)}> failed to provide listed security {definition}");
            }
        }
        static void MethodTest_GetOrders()
        {
            NotifyTestStarted();
            AskUser($"This test requires existing orders history present in the terminal.\nPress Enter to continue");

            var orders = _quik.GetOrders();

            if (!orders.Any())
            {
                throw new Exception("Quik did not provide any order");
            }
        }
        static void MethodTest_PlaceNewOrder(out IOrder? order)
        {
            NotifyTestStarted();

            order = CreateNewOrderFromUserInput();

            IOrder placeOrder(IOrder o)
            {
                _quik.PlaceNewOrder(o);
                return o;
            }

            SubmitOrderAndWaitForReply(order, placeOrder);
        }
        static void MethodTest_MoveOrder(IOrder order, out IOrder neworder)
        {
            NotifyTestStarted();

            IOrder moveOrder(IOrder order)
            {
                var newPrice = order.Quote.Price + order.Security.MinPriceStep;
                return _quik.ChangeOrder(order, newPrice, order.Quote.Size);
            }

            neworder = SubmitOrderAndWaitForReply(order, moveOrder);
        }
        static void MethodTest_CancelOrder(IOrder order)
        {
            NotifyTestStarted();

            IOrder cancelOrder(IOrder order)
            {
                _quik.CancelOrder(order);
                return order;
            }

            SubmitOrderAndWaitForReply(order, cancelOrder);
        }

        static IOrder CreateNewOrderFromUserInput()
        {
            var getSecurity = default(Func<string, IDerivative?>);
            var quote = default(Quote);
            var ticker = default(string);

            void parseInput(string? input)
            {
                var splitInput = input.Split(' ');

                int orderCount = 0;

                getSecurity = splitInput[orderCount++] switch
                {
                    "futures" => _quik.GetSecurity<IFutures>,
                    "option" => _quik.GetSecurity<IOption>,
                    "spread" => _quik.GetSecurity<ICalendarSpread>,
                    _ => throw new Exception("Unrecognized security type")
                };

                ticker = splitInput[orderCount++];
                quote = new Quote
                {
                    Operation = (Operations)Enum.Parse(typeof(Operations), splitInput[orderCount++], ignoreCase: true),
                    Price = Decimal5.Parse(splitInput[orderCount++]),
                    Size = int.Parse(splitInput[orderCount++])
                };
            }

            KeepAskingUntilInputIsValid(
                "In order to continue with the test you need provide the following parameters separated with a space:\n" +
                    "Security type, Ticker, Operation, Price, Size",
                "Provided data is invalid. Try again",
                parseInput);

            var sec = getSecurity(ticker);

            return _quik.CreateDerivativeOrder(sec, ref quote);
        }
        static IOrder? SubmitOrderAndWaitForReply(IOrder order, Func<IOrder, IOrder?> submit)
        {
            var cancellationSrc = new CancellationTokenSource();

            void onOrderChanged(IOrder o)
            {
                if (o.TransactionId == order.TransactionId &&
                    // предыдущий метод отправив заявку сразу установит состояние в OrderStates.Changing/Registering/Cancelling
                    // и будет получен этот коллбек.
                    // так что будем реагировать только на коллбек с конечным состоянием OrderStates.Active
                    o.State.HasFlag(OrderStates.Active))
                {
                    _quik.OrderChanged -= onOrderChanged;
                    cancellationSrc.Cancel();
                }
            }

            _quik.OrderChanged += onOrderChanged;
            var result = submit(order);

            if (order.State.HasFlag(OrderStates.Rejected))
            {
                throw new Exception("Order was rejected");
            }

            try
            {
                Task.Delay(5000, cancellationSrc.Token).Wait();
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException
                                            || e.InnerException is TaskCanceledException)
            {
                return result;
            }

            throw new Exception("Quik did not reply on the submitted order");
        }
    }
}