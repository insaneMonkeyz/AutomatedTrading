using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders;
using Quik.Lua;
using Quik.EntityDataProviders.QuikApiWrappers;
using System.Diagnostics;

namespace Quik
{
    internal class BugLab
    {
        private static readonly LuaFunction _defaultCallback = Callback;

        public static void Initialize()
        {
            Debugger.Launch();
            Quik.Lua.RegisterCallback(_defaultCallback, TransactionWrapper.CALLBACK_METHOD);
            //Quik.Lua.RegisterCallback(_defaultCallback, DerivativesPositionsWrapper.CALLBACK_METHOD);
            //Quik.Lua.RegisterCallback(_defaultCallback, ExecutionWrapper.CALLBACK_METHOD);
            //Quik.Lua.RegisterCallback(_defaultCallback, OrdersWrapper.CALLBACK_METHOD);

            //SecuritiesProvider.SubscribeCallback();
            //OrderbooksProvider.Instance.SubscribeCallback();
        }

        public static void Begin()
        {
            var saloop = new ExecutionLoop();
            saloop.Execute += delegate { };
            saloop.Enter();

            return;

            var bookreq = new OrderbookRequestContainer
            {
                SecurityRequest = new()
                {
                    Ticker = "BRK3",
                    ClassCode = MoexSpecifics.FUTURES_CLASS_CODE
                }
            };
            var container = new SecurityParamsContainer
            {
                Ticker = bookreq.SecurityRequest.Ticker,
                Description = string.Empty,
                MinPriceStep = 0.01d,
                ContractSize = 10,
                PricePrecisionScale = 2,
                DenominationCurrency = Currencies.USD
            };

            var sec = new Futures(ref container);
            var book = new OrderBook(sec);

            EntityResolvers.GetSecurityResolver().CacheEntity(ref bookreq.SecurityRequest, sec);
            EntityResolvers.GetOrderbooksResolver().CacheEntity(ref bookreq, book);

            var loop = new ExecutionLoop();

            SecuritiesProvider.Initialize(loop);
            OrderbooksProvider.Instance.Initialize(loop);

            while (DateTime.Now < DateTime.Now.AddDays(1))
            {
                OrderbookWrapper.UpdateOrderBook(book);

                // just to delay things a little
                Callback(default);
            }

            loop.Enter();

            GC.KeepAlive(OrderbooksProvider.Instance);
        }

        public static int Callback(nint state)
        {
            TransactionWrapper.SetContext(state);

            Debug.Print($"Transaction reply OrderId={TransactionWrapper.ExchangeAssignedOrderId.ToString() ?? "N/A"} " +
                        $"Status={TransactionWrapper.Status} " +
                        $"ErrorSite={TransactionWrapper.ErrorSource}\n" +
                        $"ResultDescription={TransactionWrapper.ResultDescription}");
            return 1;
        }
    }
}
