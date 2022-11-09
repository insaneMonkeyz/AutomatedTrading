using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using BasicConcepts.SecuritySpecifics.Options;
using Quik.Entities;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;

using GetCsv1Param = Quik.QuikProxy.Method1Param<System.Collections.Generic.IEnumerable<System.String>>;
using GetCsvNoParams = Quik.QuikProxy.MethodNoParams<System.Collections.Generic.IEnumerable<System.String>>;
using GetSecurityParams = Quik.QuikProxy.Method2Params<BasicConcepts.ISecurity?>;

namespace Quik.EntityDataProviders
{
    internal delegate ISecurity? ResolveSecurityHandler(Type securityType, string ticker);

    internal sealed class SecurityDataProvider
    {
        private static Dictionary<Type, Func<ISecurity?>> _securityTypeToCreateMethod = new(4);
        private static readonly Dictionary<Type, string> _securityTypeToClassCode = new()
        {
            { typeof(IStock), SecurityWrapper.STOCK_CLASS_CODE },
            { typeof(IFutures), SecurityWrapper.FUTURES_CLASS_CODE },
            { typeof(IOption), SecurityWrapper.OPTIONS_CLASS_CODE },
            { typeof(ICalendarSpread), SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE },
        };
        private static readonly Dictionary<string, Type> _classCodeToSecurityType = new()
        {
            { SecurityWrapper.STOCK_CLASS_CODE, typeof(IStock) },
            { SecurityWrapper.FUTURES_CLASS_CODE, typeof(IFutures) },
            { SecurityWrapper.OPTIONS_CLASS_CODE, typeof(IOption) },
            { SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE, typeof(ICalendarSpread) },
        };

        private static GetCsv1Param _securitiesCsvRequest = new()
        {
            Method = SecurityWrapper.GET_SECURITIES_OF_A_CLASS_METHOD,
            ReturnType = LuaApi.TYPE_STRING,
            Arg0 = string.Empty,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetCsvNoParams _classesCsvRequest = new()
        {
            Method = SecurityWrapper.GET_CLASSES_LIST_METHOD,
            ReturnType = LuaApi.TYPE_STRING,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetSecurityParams _getSecurityRequest = new()
        {
            Method = SecurityWrapper.GET_METOD,
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

        private readonly object _userRequestLock = new();

        public static SecurityDataProvider Instance { get; } = new();

        /// <summary>
        /// Reference to a method that SecurityWrapper will invoke 
        /// when it needs to find specific security. <para/>
        /// If left unset, <see cref="SecurityDataProvider"/> will be unable 
        /// to resolve underlying security dependencies
        /// </summary>
        public static ResolveSecurityHandler ResolveSecurity = delegate { return null; };

        private SecurityDataProvider()
        {
            _securityTypeToCreateMethod.Add(typeof(IStock), CreateStock);
            _securityTypeToCreateMethod.Add(typeof(IFutures), CreateFutures);
            _securityTypeToCreateMethod.Add(typeof(IOption), CreateOption);
            _securityTypeToCreateMethod.Add(typeof(ICalendarSpread), CreateCalendarSpread);

            _securitiesCsvRequest.Action = GetCsvValues;
            _classesCsvRequest.Action = GetCsvValues;

            SecurityWrapper.Set(State);
        }

        public Decimal5? GetBuyMarginRequirements(SecurityBase security)
        {
            return GetDecimal5Param(security, SecurityWrapper.PARAM_BUY_MARGIN_REQUIREMENTS);
        }
        public Decimal5? GetSellMarginRequirements(SecurityBase security)
        {
            return GetDecimal5Param(security, SecurityWrapper.PARAM_SELL_MARGIN_REQUIREMENTS);
        }

        public IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

                return ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public IEnumerable<string> GetAvailableClasses()
        {
            lock (_userRequestLock)
            {
                return ReadSpecificEntry(ref _classesCsvRequest); 
            }
        }
        public ISecurity? GetSecurity(Type securityType, string ticker)
        {
            lock (_userRequestLock)
            {
                _getSecurityRequest.Action = _securityTypeToCreateMethod[securityType];
                _getSecurityRequest.Arg0 = _securityTypeToClassCode[securityType];
                _getSecurityRequest.Arg1 = ticker;

                return ReadSpecificEntry(ref _getSecurityRequest); 
            }
        }
        public void UpdateSecurity<T>(T security) where T : SecurityBase
        {
            security.PriceStepValue = GetDecimal5Param(security, SecurityWrapper.PARAM_PRICE_STEP_VALUE);

            if (security is MoexDerivativeBase baseSec)
            {
                baseSec.UpperPriceLimit = GetDecimal5Param(security, SecurityWrapper.PARAM_UPPER_PRICE_LIMIT);
                baseSec.LowerPriceLimit = GetDecimal5Param(security, SecurityWrapper.PARAM_LOWER_PRICE_LIMIT);
            }
        }
        
        private ISecurity? CreateCalendarSpread()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var nearTermLeg = ResolveUnderlying(SecurityWrapper.NearTermLegClassCode, SecurityWrapper.NearTermLegSecCode);
                var longTermLeg = ResolveUnderlying(SecurityWrapper.LongTermLegClassCode, SecurityWrapper.LongTermLegSecCode);
                var expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string");

                return nearTermLeg is IExpiring nearterm && longTermLeg is IExpiring longterm
                    ? new CalendarSpread(ref container, nearterm, longterm)
                    : new CalendarSpread(ref container, expiry);
            }

            return null;
        }
        private ISecurity? CreateOption()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                return new Option(ref container)
                {
                    Underlying = ResolveUnderlying(SecurityWrapper.UnderlyingClassCode, SecurityWrapper.UnderlyingSecCode),

                    Strike = SecurityWrapper.Strike
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Strike), "decimal5"),

                    Expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string")
                };
            }

            return null;
        }
        private ISecurity? CreateFutures()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                return new Futures(ref container)
                {
                    Underlying = ResolveUnderlying(SecurityWrapper.UnderlyingClassCode, SecurityWrapper.UnderlyingSecCode),

                    Expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string")
                };
            }

            return null;
        }
        private ISecurity? CreateStock()
        {
            return TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
                ? new Stock(ref container)
                : null;
        }

        private bool TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
        {
            container = new ()
            {
                Ticker = SecurityWrapper.Ticker,
                ClassCode = SecurityWrapper.ClassCode,
                Description = SecurityWrapper.Description,
                MinPriceStep = SecurityWrapper.MinPriceStep.GetValueOrDefault(),
                ContractSize = SecurityWrapper.ContractSize.GetValueOrDefault(),
                DenominationCurrency = SecurityWrapper.Currency,
                PricePrecisionScale = SecurityWrapper.PricePrecisionScale.GetValueOrDefault(),
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
        private IEnumerable<string> GetCsvValues()
        {
            var csv = State.ReadAsString();

            return string.IsNullOrWhiteSpace(csv)
                ? Enumerable.Empty<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        private ISecurity? ResolveUnderlying(string? classCode, string? secCode)
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
