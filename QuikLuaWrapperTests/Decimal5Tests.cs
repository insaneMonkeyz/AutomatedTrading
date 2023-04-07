using System.Text;
using TradingConcepts;

namespace QuikLuaWrapperTests
{
    public class Decimal5Test
    {
        [Test]
        public void ParsingTest_DecimalSeparator()
        {
            var values = new ValueTuple<string, Decimal5>[]
            {
                ( "4", 4 ),
                ( "4,", 4 ),
                ( "4.", 4 ),
                ( ",4", 0.4m ),
                ( ".4", 0.4m ),
                ( "0.4", 0.4m ),
                ( "0,4", 0.4m ),
                ( "0,0", 0 ),
                ( "0.0", 0 ),
                ( ".", 0 ),
            };

            foreach (var (stringvalue, decimalvalue) in values)
            {
                Assert.That(Decimal5.Parse(stringvalue), Is.EqualTo(decimalvalue));
            }
        }

        [Test]
        public void ParsingTest_Spaces()
        {
            var values = new ValueTuple<string, Decimal5>[]
            {
                ( " 4", 4 ),
                ( " 4 ", 4 ),
                ( " 4 4", 44 ),
                ( " 4 4 123.00001", 44123.00001m ),
                ( "  4  4  1   2 3 .     0 0 0 0 1", 44123.00001m ),
            };

            foreach (var (stringvalue, decimalvalue) in values)
            {
                Assert.That(Decimal5.Parse(stringvalue), Is.EqualTo(decimalvalue));
            }
        }

        [Test]
        public void ParsingTest_DecimalPartCorrectness()
        {
            var values = new ValueTuple<string, Decimal5>[]
            {
                ( "8", 8 ),
                ( "9.", 9 ),
                ( "7.0", 7 ),
                ( "6.00", 6 ),
                ( "5.000", 5 ),
                ( "4.0000", 4 ),
                ( "3.00000", 3 ),
                ( "8.12345", 8.12345m ),
                ( "8.00001", 8.00001m ),
                ( "0.00001", 0.00001m ),
                ( "777999999.00001", 777_999_999.00001m ),
                ( "666999999.0000099", 666_999_999.00001m ),
                ( "888999999.000005", 888_999_999.00001m ),
                ( "222999999.0000049", 222_999_999 ),
                ( "2.999995", 3 ),
                ( "77.99999409", 77.99999m ),
                ( "99.999994445", 99.99999m ),
            };

            foreach (var (stringvalue, decimalvalue) in values)
            {
                Assert.That(Decimal5.Parse(stringvalue), Is.EqualTo(decimalvalue));
            }
        }

        [Test]
        public void ParsingTest_Sign()
        {
            var values = new ValueTuple<string, Decimal5>[]
            {
                ( "- 4", -4 ),
                ( " -5.0 ",-5 ),
                ( "-\t6.0", -6 ),
                ( "-     7", -7 ),
                ( "1", 1 ),
                ( "2.0", 2),
                ( "+3.0", 3 ),
            };

            foreach (var (stringvalue, decimalvalue) in values)
            {
                Assert.That(Decimal5.Parse(stringvalue), Is.EqualTo(decimalvalue));
            }
        }

        [Test]
        public void ParsingTest_BoundariesCheck()
        {
            var throws = new string[]
            {
                "-100 000 000 000",
                " 100 000 000 000",
            };
            var nothrows = new string[]
            {
                "  99 999 999 999.99999",
                " -99 999 999 999.99999",
            };

            foreach (var str in throws)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Decimal5.Parse(str));
            }

            foreach (var str in nothrows)
            {
                Assert.DoesNotThrow(() => Decimal5.Parse(str));
            }
        }

        [Test]
        public void ParsingTest_PointerCheck()
        {
            var cases = new ValueTuple<string, Decimal5>[]
            {
                ("-1.0\0", -1),
                ("2.1337\0", 2.1337m),
                ("+ 100 500.001\0", 100500.001m),
                ("-\t0.00001\0", -0.00001m),
            };

            var bytesarr = new byte[cases.Length][];

            using var stream = new MemoryStream() { Position = 0 };

            for (int i = 0; i < cases.Length; i++)
            {
                var bytesvalue = Encoding.Unicode.GetBytes(cases[i].Item1);
                bytesarr[i] = bytesvalue;
                stream.Write(bytesvalue, 0, bytesvalue.Length);
            }

            unsafe
            {
                var buffer = stream.GetBuffer();

                fixed (byte* p = buffer)
                {
                    int offset = 0;
                    for (int i = 0; i < cases.Length; i++)
                    {
                        Assert.That(Decimal5.Parse((nint)(p + offset)), Is.EqualTo(cases[i].Item2));
                        offset += bytesarr[i].Length;
                    }
                }
            }


        }
    }
}
