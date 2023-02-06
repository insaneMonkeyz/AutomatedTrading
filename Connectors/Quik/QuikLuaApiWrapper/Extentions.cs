using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders;

namespace Quik
{
    internal static class Extentions
    {
        public static bool TryConvertToMoexExpiry(this string date, out DateTimeOffset result)
        {
            result = default;

            if (uint.TryParse(date, out uint value))
            {
                try
                {
                    result = new DateTimeOffset(
                               dateTime: MoexDate.GetDateTime(value) + MoexSpecifics.CommonExpiryTime,
                                 offset: MoexSpecifics.MoscowUtcOffset);

                    return true;
                }
                catch (Exception e)
                {
                    $"Exception while trying to convert a string representation of a Date '{date}'. {e}"
                        .DebugPrintWarning();
                }
            }

            return false;
        }
        public static Currencies CodeToCurrency(this string? code)
        {
            return code switch
            {
                MoexSpecifics.USD_CURRENCY => Currencies.USD,
                MoexSpecifics.SUR_CURRENCY => Currencies.RUB,
                MoexSpecifics.RUB_CURRENCY => Currencies.RUB,
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
        public static PropertyInfo? GetTaggedProperty<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type
                .GetProperties()
                .FirstOrDefault(p => p.HasAttribute<TAttribute>());
        }
        public static MethodInfo? GetTaggedMethod<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type
                .GetMethods()
                .FirstOrDefault(p => p.HasAttribute<TAttribute>());
        }
        public static bool HasAttribute<TAttribute>(this MemberInfo subj) where TAttribute : Attribute
        {
            return subj.CustomAttributes.Any(a => a.AttributeType == typeof(TAttribute));
        }
    }
}
