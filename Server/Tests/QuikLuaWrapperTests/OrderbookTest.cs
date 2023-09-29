using Quik;
using Quik.Entities;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace QuikLuaWrapperTests
{
    public class QuikApiOrderbookTest
    {
        [Test]
        public void PrintResult()
        {
            var container = new SecurityParamsContainer();
            var sec = new Futures(ref container);
            var book = new OrderBook(sec)
            {
                MarketDepth = 10
            };

            book.UseQuotes((b, a, d) =>
            {
                var r = new Random(567645);

                for (int i = 0; i < d; i++)
                {
                    a[i] = new Quote
                    {
                        Price = 100.00d + i * 0.01d,
                        Size = r.Next(1, 300),
                        Operation = TradingConcepts.Operations.Sell
                    };

                    b[i] = new Quote
                    {
                        Price = 99.97d - i * 0.01d,
                        Size = r.Next(1, 300),
                        Operation = TradingConcepts.Operations.Buy
                    };
                }
            });

            Assert.IsNotNull(null, book.Print());
        }

        [Test]
        public void CopyingCorrectness()
        {
            var secParams = new SecurityParamsContainer();
            var sec = new Futures(ref secParams);
            var orderbook = new OrderBook(sec);

            var q1 = new Quote()
            {
                Operation = Operations.Sell,
                Price = 99.5d,
                Size = 10
            };
            var q2 = new Quote()
            {
                Operation = Operations.Sell,
                Price = 99.51d,
                Size = 7
            };
            var q3 = new Quote()
            {
                Operation = Operations.Sell,
                Price = 99.52d,
                Size = 16
            };

            void reader(Quote[] quotes, Operations operation, long marketDepth)
            {
                quotes[0] = q1;
                quotes[1] = q2;
                quotes[2] = q3;
            }

            orderbook.UseBids(reader);

            var bidsCopy = orderbook.Bids;

            Assert.That(bidsCopy[0], Is.EqualTo(q1));
            Assert.That(bidsCopy[1], Is.EqualTo(q2));
            Assert.That(bidsCopy[2], Is.EqualTo(q3));
        }
    }
}
