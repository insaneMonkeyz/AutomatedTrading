using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using BasicConcepts.SecuritySpecifics.Options;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using static Quik.QuikProxy;

using GetCsv1Param = Quik.QuikProxy.Method1Param<System.Collections.Generic.IEnumerable<System.String>>;
using GetCsvNoParams = Quik.QuikProxy.MethodNoParams<System.Collections.Generic.IEnumerable<System.String>>;
using GetSecurityParams = Quik.QuikProxy.Method2Params<Quik.Entities.Security?>;

namespace Quik.EntityProviders
{
    internal static class SecuritiesProvider
    {
        private static readonly Dictionary<Type, Func<Security?>> _securityTypeToCreateMethod = new(4)
        {
            { typeof(IStock), CreateStock },
            { typeof(IFutures), CreateFutures },
            { typeof(IOption), CreateOption },
            { typeof(ICalendarSpread), CreateCalendarSpread }
        };
        private static readonly Dictionary<string, Func<Security?>> _classcodeToCreateMethod = new(4)
        {
            { SecurityWrapper.STOCK_CLASS_CODE, CreateStock },
            { SecurityWrapper.FUTURES_CLASS_CODE, CreateFutures },
            { SecurityWrapper.OPTIONS_CLASS_CODE, CreateOption },
            { SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE, CreateCalendarSpread }
        };
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

        private static readonly object _userRequestLock = new();
        private static readonly SecurityResolver _entityResolver = EntityResolvers.GetSecurityResolver();

        public static Decimal5? GetBuyMarginRequirements(Security security)
        {
            return GetDecimal5From_getParamEx(security, SecurityWrapper.PARAM_BUY_MARGIN_REQUIREMENTS);
        }
        public static Decimal5? GetSellMarginRequirements(Security security)
        {
            return GetDecimal5From_getParamEx(security, SecurityWrapper.PARAM_SELL_MARGIN_REQUIREMENTS);
        }

        public static IEnumerable<string> GetAvailableSecuritiesOfType(string classcode)
        {
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = classcode;

                return ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

                return ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableClasses()
        {
            lock (_userRequestLock)
            {
                return ReadSpecificEntry(ref _classesCsvRequest); 
            }
        }
        public static Security? GetSecurity(Type securityType, string ticker)
        {
            lock (_userRequestLock)
            {
                _getSecurityRequest.Action = _securityTypeToCreateMethod[securityType];
                _getSecurityRequest.Arg0 = _securityTypeToClassCode[securityType];
                _getSecurityRequest.Arg1 = ticker;

                return ReadSpecificEntry(ref _getSecurityRequest); 
            }
        }
        public static Security? GetSecurity(SecurityRequestContainer request)
        {
            if (!request.HasData)
            {
                return null;
            }

            lock (_userRequestLock)
            {
                _getSecurityRequest.Action = _classcodeToCreateMethod[request.ClassCode];
                _getSecurityRequest.Arg0 = request.ClassCode;
                _getSecurityRequest.Arg1 = request.Ticker;

                return ReadSpecificEntry(ref _getSecurityRequest);
            }
        }
        public static void UpdateSecurity(Security security)
        {
            lock (_userRequestLock)
            {
                security.PriceStepValue = GetDecimal5From_getParamEx(security, SecurityWrapper.PARAM_PRICE_STEP_VALUE);

                if (security is MoexDerivativeBase baseSec)
                {
                    baseSec.UpperPriceLimit = GetDecimal5From_getParamEx(security, SecurityWrapper.PARAM_UPPER_PRICE_LIMIT);
                    baseSec.LowerPriceLimit = GetDecimal5From_getParamEx(security, SecurityWrapper.PARAM_LOWER_PRICE_LIMIT);
                } 
            }
        }

        private static Security? CreateCalendarSpread()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var nearTermLeg = ResolveUnderlying(SecurityWrapper.NearTermLegClassCode, SecurityWrapper.NearTermLegSecCode);
                var longTermLeg = ResolveUnderlying(SecurityWrapper.LongTermLegClassCode, SecurityWrapper.LongTermLegSecCode);
                var expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string");

                var result = nearTermLeg is IExpiring nearterm 
                          && longTermLeg is IExpiring longterm
                    ? new CalendarSpread(ref container, nearterm, longterm)
                    : new CalendarSpread(ref container, expiry);

                UpdateSecurity(result);

                return result;
            }

            return null;
        }
        private static Security? CreateOption()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var result = new Option(ref container)
                {
                    Underlying = ResolveUnderlying(SecurityWrapper.UnderlyingClassCode, SecurityWrapper.UnderlyingSecCode),

                    Strike = SecurityWrapper.Strike
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Strike), "decimal5"),

                    Expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string")
                };

                UpdateSecurity(result);

                return result;
            }

            return null;
        }
        private static Security? CreateFutures()
        {
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var result = new Futures(ref container)
                {
                    Underlying = ResolveUnderlying(SecurityWrapper.UnderlyingClassCode, SecurityWrapper.UnderlyingSecCode),

                    Expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string")
                };

                UpdateSecurity(result);

                return result;
            }

            return null;
        }
        private static Security? CreateStock()
        {
            return TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
                ? new Stock(ref container)
                : null;
        }
        
        private static bool TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
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
                "Failed to create security. Essential parameters missing or incorrect"
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
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        private static Security? ResolveUnderlying(string? classCode, string? secCode)
        {
            return _entityResolver.GetEntity(new SecurityRequestContainer
            {
                ClassCode = classCode,
                Ticker = secCode
            });
        }

        static SecuritiesProvider()
        {
            _securitiesCsvRequest.Action = GetCsvValues;
            _classesCsvRequest.Action = GetCsvValues;

            SecurityWrapper.Set(State);
        }
    }
}
