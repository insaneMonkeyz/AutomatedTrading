using Quik;
using Tools.Logging;

namespace QuikLuaWrapperTests
{
    public class QuikApiOrderbookTest
    {
        [Test]
        public void CopyingCorrectness()
        {
            this.Trace();
            this.Trace();
            var common = LogManagement.GetLogger(typeof(QuikApiOrderbookTest));
            var dedicated = LogManagement.GetDebugDedicatedFileLogger<QuikApiOrderbookTest>();

            dedicated.Info("Happy birthday Nick Gurr");
            try
            {
                throw new ArgumentException("hat");
            }
            catch (Exception e)
            {
                dedicated.Fatal("Deez nuts", e);
                common.Error("This is the message I was trying to convey",e);
                this.Error("cmon");
                this.Fatal("this is gonna kill you someday");
            }
            common.Debug("Deez nuts");

            LogManagement.Dispose();

            //throw new Exception(Directory.GetCurrentDirectory());


            //IOptimizedOrderBook orderbook = Quik.GetOrderbook(null);

            //var q1 = new Quote()
            //{
            //    Operation = Operations.Sell,
            //    Price = 99.5d,
            //    Size = 10
            //};
            //var q2 = new Quote()
            //{
            //    Operation = Operations.Sell,
            //    Price = 99.51d,
            //    Size = 7
            //};
            //var q3 = new Quote()
            //{
            //    Operation = Operations.Sell,
            //    Price = 99.52d,
            //    Size = 16
            //};

            //void reader(Quote[] quotes, Operations operation, long marketDepth)
            //{
            //    quotes[0] = q1;
            //    quotes[1] = q2;
            //    quotes[2] = q3;
            //}

            //orderbook.UseBids(reader);

            //var bidsCopy = orderbook.Bids;

            //Assert.That(bidsCopy[0], Is.EqualTo(q1));
            //Assert.That(bidsCopy[1], Is.EqualTo(q2));
            //Assert.That(bidsCopy[2], Is.EqualTo(q3));
        }
    }
}
