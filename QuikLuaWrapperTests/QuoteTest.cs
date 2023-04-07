//using System.Runtime.InteropServices;
//using TradingConcepts;

//namespace QuikLuaWrapperTests
//{
//    public class QuoteTest
//    {
//        private const int OPERATION_OFFSET = 0;
//        private const int PRICE_OFFSET = 8;
//        private const int SIZE_OFFSET = 16;

//        [SetUp]
//        public void Setup()
//        {
//        }

//        [Test]
//        public void QuoteStructLaoutCorrectness()
//        {
//            unsafe
//            {
//                var operation = Operations.Sell;
//                var price = (Decimal5)61.86d;
//                var size = 228L;

//                var operationBuffer = (long)operation;
//                var priceBuffer = (long)(price.InternalValue);

//                var quote = new Quote()
//                {
//                    Price = price,
//                    Size = size,
//                    Operation = operation
//                };

//                var p = (long*)&quote;

//                Assert.That(p[0] == operationBuffer);
//                Assert.That(p[1] == priceBuffer);
//                Assert.That(p[2] == size);
//            }
//        }

//        [Test]
//        public void QuoteArrayCopyingCorrectnessTest()
//        {
//            const int SIZE_OF_QUOTE = 24;
//            const int SAMPLE_SIZE = 3;
//            const int ARRAY_TOTAL_SIZE = SIZE_OF_QUOTE * SAMPLE_SIZE;

//            unsafe
//            {
//                var copy = new Quote[SAMPLE_SIZE];
//                var quotes = new Quote[SAMPLE_SIZE]
//                {
//                    new Quote()
//                    {
//                        Operation = Operations.Sell,
//                        Price = 99.5d,
//                        Size = 10
//                    },
//                    new Quote()
//                    {
//                        Operation = Operations.Sell,
//                        Price = 99.51d,
//                        Size = 7
//                    },
//                    new Quote()
//                    {
//                        Operation = Operations.Sell,
//                        Price = 99.52d,
//                        Size = 16
//                    },
//                };


//                var srcHandle  = GCHandle.Alloc(quotes, GCHandleType.Pinned);
//                var copyHandle = GCHandle.Alloc(copy, GCHandleType.Pinned);

//                var pQuotes = (long*)srcHandle.AddrOfPinnedObject().ToPointer();
//                var pCopy   = (long*)copyHandle.AddrOfPinnedObject().ToPointer();

//                try
//                {
//                    Buffer.MemoryCopy(pQuotes, pCopy, ARRAY_TOTAL_SIZE, ARRAY_TOTAL_SIZE);
//                }
//                finally
//                {
//                    srcHandle.Free();
//                    copyHandle.Free();
//                }

//                for (int i = 0; i < SAMPLE_SIZE; i++)
//                {
//                    Assert.That(quotes[i], Is.EqualTo(copy[i]));
//                }
//            }
//        }
//    }
//}