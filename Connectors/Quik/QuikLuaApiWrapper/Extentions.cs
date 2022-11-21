using System.Diagnostics;
using System.Runtime.CompilerServices;
using BasicConcepts;
using Quik.Entities;
using Quik.QuikApi;

namespace Quik
{
    internal static class Extentions
    {
        public static bool TryConvertToMoexExpiry(this string date, out DateTimeOffset result)
        {
            result = default;

            if (uint.TryParse(date, out uint value))
            {
                result = new DateTimeOffset(
                   dateTime: MoexDate.GetDateTime(value) + MoexSpecifics.CommonExpiryTime,
                     offset: MoexSpecifics.MoscowUtcOffset);

                return true;
            }

            return false;
        }
        public static Currencies CodeToCurrency(this string? code)
        {
            return code switch
            {
                Account.USD_CURRENCY => Currencies.USD,
                Account.SUR_CURRENCY => Currencies.RUB,
                Account.RUB_CURRENCY => Currencies.RUB,
                null => Currencies.Unknown,
                "" => Currencies.Unknown,
                _ => throw new NotImplementedException($"Currency '{code}' is not supported.")
            };
        }
        public static void DebugPrintWarning(this string msg, [CallerMemberName] string? caller = null)
        {
            var prefix = string.IsNullOrEmpty(caller)
                ? string.Empty
                : caller + ": ";

            var text = "=======================================================================\n" +
                       "                               W A R N I N G                           \n" +
                       "=======================================================================\n" +
                       prefix + msg +
                       "\n=======================================================================";

            Debug.Print(text);
        }
        public static QuikApiException ToSecurityParsingException(this string property)
        {
            return new QuikApiException($"Can't parse essential parameter '{property}' to build a security.");
        }
    }
}
