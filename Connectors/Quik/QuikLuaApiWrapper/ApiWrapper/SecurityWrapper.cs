using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using BasicConcepts.SecuritySpecifics.Options;
using QuikLuaApi;
using QuikLuaApiWrapper.ApiWrapper.QuikApi;
using QuikLuaApiWrapper.Entities;

using GetSecurityParams = QuikLuaApi.QuikLuaApiWrapper.Method2Params<BasicConcepts.ISecurity?>;
using GetCsvNoParams = QuikLuaApi.QuikLuaApiWrapper.MethodNoParams<System.Collections.Generic.IEnumerable<System.String>>;
using GetCsv1Param = QuikLuaApi.QuikLuaApiWrapper.Method1Param<System.Collections.Generic.IEnumerable<System.String>>;

using static QuikLuaApi.QuikLuaApiWrapper;

using QuikLuaApi.Entities;
using QuikLuaApiWrapper.Extensions;

namespace Quik.ApiWrapper
{
    internal delegate ISecurity? ResolveSecurityHandler(Type securityType, string ticker);

    internal class SecurityWrapper
    {
        private static readonly Dictionary<Type, Func<ISecurity?>> _securityTypeToCreateMethod = new()
        {
            { typeof(IStock), CreateStock },
            { typeof(IFutures), CreateFutures },
            { typeof(IOption), CreateOption },
            { typeof(ICalendarSpread), CreateCalendarSpread },
        };
        private static readonly Dictionary<Type, string> _securityTypeToClassCode = new()
        {
            { typeof(IStock), QuikSecurity.STOCK_CLASS_CODE },
            { typeof(IFutures), QuikSecurity.FUTURES_CLASS_CODE },
            { typeof(IOption), QuikSecurity.OPTIONS_CLASS_CODE },
            { typeof(ICalendarSpread), QuikSecurity.CALENDAR_SPREADS_CLASS_CODE },
        };
        private static readonly Dictionary<string, Type> _classCodeToSecurityType = new()
        {
            { QuikSecurity.STOCK_CLASS_CODE, typeof(IStock) },
            { QuikSecurity.FUTURES_CLASS_CODE, typeof(IFutures) },
            { QuikSecurity.OPTIONS_CLASS_CODE, typeof(IOption) },
            { QuikSecurity.CALENDAR_SPREADS_CLASS_CODE, typeof(ICalendarSpread) },
        };

        private static GetCsv1Param _securitiesCsvRequest = new()
        {
            Method = QuikSecurity.GET_SECURITIES_OF_A_CLASS_METHOD,
            ReturnType = LuaApi.TYPE_STRING,
            Arg0 = string.Empty,
            Action = GetCsvValues,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetCsvNoParams _classesCsvRequest = new()
        {
            Method = QuikSecurity.GET_CLASSES_LIST_METHOD,
            ReturnType = LuaApi.TYPE_STRING,
            Action = GetCsvValues,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetSecurityParams _getSecurityRequest = new()
        {
            Method = QuikSecurity.GET_METOD,
            ReturnType = LuaApi.TYPE_TABLE,
            Arg0 = string.Empty,
            Arg1 = string.Empty
        };
        private static GetItemParams _getSecurityParamRequest = new()
        {
            Ticker = string.Empty,
            ClassCode = string.Empty,
            Parameter = string.Empty
        };

        public static SecurityWrapper Instance { get; } = new();

        /// <summary>
        /// Reference to a method that SecurityWrapper will invoke 
        /// when it needs to find specific security. <para/>
        /// If left unset, <see cref="SecurityWrapper"/> will be unable 
        /// to resolve underlying security dependencies
        /// </summary>
        public static ResolveSecurityHandler ResolveSecurity = delegate { return null; };

        public static IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
            _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

            return ReadSpecificEntry(ref _securitiesCsvRequest);
        }
        public static IEnumerable<string> GetAvailableClasses()
        {
            return ReadSpecificEntry(ref _classesCsvRequest);
        }
        public static ISecurity? GetSecurity(Type securityType, string ticker)
        {
            _getSecurityRequest.Action = _securityTypeToCreateMethod[securityType];
            _getSecurityRequest.Arg0 = _securityTypeToClassCode[securityType];
            _getSecurityRequest.Arg1 = ticker;

            return ReadSpecificEntry(ref _getSecurityRequest);
        }
        public static void UpdateSecurity<T>(T security) where T : SecurityBase
        {
            security.PriceStepValue = GetDecimal5Param(security, QuikSecurity.PARAM_PRICE_STEP_VALUE);

            if (security is MoexDerivativeBase baseSec)
            {
                baseSec.UpperPriceLimit = GetDecimal5Param(security, QuikSecurity.PARAM_UPPER_PRICE_LIMIT);
                baseSec.LowerPriceLimit = GetDecimal5Param(security, QuikSecurity.PARAM_LOWER_PRICE_LIMIT);
            }
        }
        
        private static ISecurity? CreateCalendarSpread()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var nearTermLeg = ResolveUnderlying(QuikSecurity.NearTermLegClassCode, QuikSecurity.NearTermLegSecCode);
                var longTermLeg = ResolveUnderlying(QuikSecurity.LongTermLegClassCode, QuikSecurity.LongTermLegSecCode);
                var expiry = QuikSecurity.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(QuikSecurity.Expiry), "string");

                return nearTermLeg is IExpiring nearterm && longTermLeg is IExpiring longterm
                    ? new CalendarSpread(ref container, nearterm, longterm)
                    : new CalendarSpread(ref container, expiry);
            }

            return null;
        }
        private static ISecurity? CreateOption()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                return new Option(ref container)
                {
                    Underlying = ResolveUnderlying(QuikSecurity.UnderlyingClassCode, QuikSecurity.UnderlyingSecCode),

                    Strike = QuikSecurity.Strike
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(QuikSecurity.Strike), "decimal5"),

                    Expiry = QuikSecurity.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(QuikSecurity.Expiry), "string")
                };
            }

            return null;
        }
        private static ISecurity? CreateFutures()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                return new Futures(ref container)
                {
                    Underlying = ResolveUnderlying(QuikSecurity.UnderlyingClassCode, QuikSecurity.UnderlyingSecCode),

                    Expiry = QuikSecurity.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(QuikSecurity.Expiry), "string")
                };
            }

            return null;
        }
        private static ISecurity? CreateStock()
        {
            return TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
                ? new Stock(ref container)
                : null;
        }

        private static bool TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
        {
            container = new ()
            {
                Ticker = QuikSecurity.Ticker,
                ClassCode = QuikSecurity.ClassCode,
                Description = QuikSecurity.Description,
                MinPriceStep = QuikSecurity.MinPriceStep.GetValueOrDefault(),
                ContractSize = QuikSecurity.ContractSize.GetValueOrDefault(),
                DenominationCurrency = QuikSecurity.Currency.GetValueOrDefault(),
                PricePrecisionScale = QuikSecurity.PricePrecisionScale.GetValueOrDefault(),
            };

            if (!container.StateIsCorrect)
            {
                ("Failed to create security. " +
                    "Essentials parameters missing or incorrect")
                        .DebugPrintWarning();
                return false;
            }

            return true;
        }
        private static IEnumerable<string> GetCsvValues()
        {
            var csv = State.ReadAsString();

            return string.IsNullOrWhiteSpace(csv)
                ? Enumerable.Empty<string>()
                : csv.Split(',');
        }
        private static ISecurity? ResolveUnderlying(string? classCode, string? secCode)
        {
            return

                  secCode is string underlyingCode  &&
                classCode is string underlyingClass &&
                _classCodeToSecurityType.TryGetValue(underlyingClass, out Type type)

                    ? ResolveSecurity(type, underlyingCode)
                    : null;
        }
    }
}
