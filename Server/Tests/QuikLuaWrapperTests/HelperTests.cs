using System.Globalization;
using Quik;
using TradingConcepts.SecuritySpecifics.Options;

namespace QuikLuaWrapperTests
{
    public class HelperTests
    {
        private static DateTime ToDate(string date)
        {
            return DateTime.Parse(date, CultureInfo.GetCultureInfo("de_DE"));
        }

        [TestCase("BRM3", "01.06.2023")]
        [TestCase("BRN4", "01.07.2024")]
        [TestCase("BRF0", "01.01.2030")]
        [TestCase("BR-08.30", "01.08.2030")]
        [TestCase("BR-01.24", "01.01.2024")]
        [SetCulture("de-DE")]
        public void FuturesDescriptionInferringTest(string ticker, string expirystring)
        {
            var expiry = ToDate(expirystring);

            var descr = Helper.InferFuturesFromTicker(ticker);

            Assert.That(descr.ExpiryDate, Is.EqualTo(expiry));
        }

        [TestCase("BRF3BRG3","01.01.2023")]
        [TestCase("BRG3BRH3","01.02.2023")]
        [TestCase("BRH3BRJ3","01.03.2023")]
        [TestCase("BRJ3BRK3","01.04.2023")]
        [TestCase("BRK3BRM3","01.05.2023")]
        [TestCase("BRZ3BRF4","01.12.2023")]
        public void CalendarSpreadsDescriptionInferringTest(string ticker, string expirystring)
        {
            var expiry = ToDate(expirystring);

            var descr = Helper.InferSpreadFromTicker(ticker);

            Assert.That(descr.ExpiryDate, Is.EqualTo(expiry));
        }

        [TestCase("BR00096BF3A", true, "01.06.2023", 96d, OptionTypes.Call)]
        [TestCase("BR00099BM3D", true, "01.01.2023", 99d, OptionTypes.Put)]
        [TestCase("Si71500BX4", false, "01.12.2024", 71500d, OptionTypes.Put)]
        [TestCase("ED0.9999BX4", false, "01.12.2024", 0.9999d, OptionTypes.Put)]
        [SetCulture("de-DE")]
        public void OptionsDescriptionInferringTest(string ticker, bool shortterm, string expirystring, double strike, OptionTypes type)
        {
            var expiry = ToDate(expirystring);

            var descr = Helper.InferOptionFromTicker(ticker);

            Assert.Multiple(() =>
            {
                Assert.That(descr.ExpiryDate, Is.EqualTo(expiry));
                Assert.That((double)descr.Strike, Is.EqualTo(strike));
                Assert.That(descr.IsShortTermExpiry, Is.EqualTo(shortterm));
                Assert.That(descr.OptionType, Is.EqualTo(type));
            });
        }
    }
}
