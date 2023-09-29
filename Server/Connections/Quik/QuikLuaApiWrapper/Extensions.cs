using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using TradingConcepts;
using Quik.Entities;
using Quik.EntityProviders;
using System.Xml.Linq;
using Quik.EntityProviders.QuikApiWrappers;
using System.Text;

namespace Quik
{
    internal static class Extensions
    {
        private static Log _log = LogManagement.GetLogger<Quik>();

        public static bool HasValue(this string? subj)
        {
            return !string.IsNullOrWhiteSpace(subj);
        }
        public static bool HasNoValue(this string? subj)
        {
            return string.IsNullOrWhiteSpace(subj);
        }
        public static bool TryConvertToMoexExpiry(this string date, out DateTime result)
        {
            result = default;

            if (uint.TryParse(date, out uint value))
            {
                try
                {
                    result = MoexDate.GetDateTime(value) 
                        + MoexSpecifics.CommonExpiryTime
                        - MoexSpecifics.MoscowTimeZone.BaseUtcOffset;

                    result = DateTime.SpecifyKind(result, DateTimeKind.Utc);

                    return true;
                }
                catch (Exception e)
                {
                    _log.Error($"Exception while trying to convert a string representation of a Date '{date}'", e);
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
        public static void DebugPrintException(this Exception e)
        {
            
            var text = "=======================================================================\n" +
                      $"                     {e.GetType()}\n" +
                       "=======================================================================\n" +
                       $"{e.StackTrace ?? "NO_STACKTRACE_PROVIDED"}\n" +
                       "=======================================================================";

            Debug.Print(text);
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
        public static void LogQuikFunctionCall(this string func, params string[] args)
        {
            if (!GlobalParameters.TraceQuikFunctionCalls)
            {
                return;
            }

            var arguments = args?.Length > 0 
                ? string.Join(',', args)
                : string.Empty;

            _log.Debug($"## CALLING QUIK FUNCTION {func}({arguments});");
        }
        public static QuikApiException ToSecurityParsingException(this string property)
        {
            return new QuikApiException($"Can't parse essential parameter '{property}' to build a security.");
        }
        public static ValueTuple<TValue,TAttribute>? GetValueByAttribute<TValue, TAttribute>(this Type type) where TAttribute : Attribute
        {
            var property = type
                .GetProperties()
                .FirstOrDefault(p => p.HasAttribute<TAttribute>());

            if (property == default)
            {
                return default;
            }

            return ((TValue)property.GetValue(null, null), property.GetCustomAttribute<TAttribute>());
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
        public static bool TryGetAttributeValue<TAttribute>(this MemberInfo subj, out TAttribute? attribute) where TAttribute : Attribute
        {
            attribute = subj.GetCustomAttribute<TAttribute>();

            return attribute != null;
        }
    }
}
