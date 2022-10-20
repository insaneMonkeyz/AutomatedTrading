using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using MoexCommonTypes;
using QuikLuaApi;
using QuikLuaApi.QuikApi;

namespace QuikLuaApiWrapper.Extensions
{
    internal static class Extentions
    {
        public static bool TryConvertToMoexExpiry(this string date, out DateTimeOffset result)
        {
            result = default;

            if (uint.TryParse(date, out uint value))
            {
                result = new DateTimeOffset(
                   dateTime: new MoexDate(value).Date + MoexSpecifics.CommonExpiryTime,
                     offset: MoexSpecifics.MoscowUtcOffset);

                return true;
            }

            return false;
        }
        public static Currencies CodeToCurrency(this string code)
        {
            return code switch
            {
                Account.USD_CURRENCY => Currencies.USD,
                Account.SUR_CURRENCY => Currencies.RUB,
                Account.RUB_CURRENCY => Currencies.RUB,
                _ => throw new NotImplementedException($"Currency '{code}' is not supported.")
            };
        }
    }
}
