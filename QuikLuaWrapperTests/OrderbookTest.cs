using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApiWrapper;

namespace QuikLuaWrapperTests
{
    public class QuikApiOrderbookTest
    {
        [Test]
        public void CopyingCorrectness()
        {
            IOptimizedOrderBook orderbook = Quik.GetOrderbook(null);

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
