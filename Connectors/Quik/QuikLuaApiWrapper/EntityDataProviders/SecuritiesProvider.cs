using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using BasicConcepts.SecuritySpecifics.Options;
using Quik.Entities;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using GetItemParams = Quik.EntityProviders.QuikApiWrappers.TableWrapper.GetParamExParams;
using GetCsv1Param = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method1Param<System.Collections.Generic.IEnumerable<System.String>>;
using GetCsvNoParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.MethodNoParams<System.Collections.Generic.IEnumerable<System.String>>;
using GetSecurityParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method2Params<Quik.Entities.Security?>;

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
            Action = GetCsvValues,
            Method = SecurityWrapper.GET_SECURITIES_OF_A_CLASS_METHOD,
            ReturnType = Api.TYPE_STRING,
            Arg0 = string.Empty,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetCsvNoParams _classesCsvRequest = new()
        {
            Action = GetCsvValues,
            Method = SecurityWrapper.GET_CLASSES_LIST_METHOD,
            ReturnType = Api.TYPE_STRING,
            DefaultValue = Enumerable.Empty<string>()
        };
        private static GetSecurityParams _getSecurityRequest = new()
        {
            Method = SecurityWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
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
        private static SecurityResolver _entityResolver;

        public static void Initialize()
        {
            SecurityWrapper.Set(Quik.Lua);
            _entityResolver = EntityResolvers.GetSecurityResolver();
        }

        public static Decimal5? GetBuyMarginRequirements(Security security)
        {
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_BUY_MARGIN_REQUIREMENTS);
        }
        public static Decimal5? GetSellMarginRequirements(Security security)
        {
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_SELL_MARGIN_REQUIREMENTS);
        }

        public static IEnumerable<string> GetAvailableSecuritiesOfType(string classcode)
        {
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = classcode;

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableClasses()
        {
            lock (_userRequestLock)
            {
                return FunctionsWrappers.ReadSpecificEntry(ref _classesCsvRequest); 
            }
        }
        public static Security? GetSecurity(Type securityType, string ticker)
        {
            lock (_userRequestLock)
            {
                _getSecurityRequest.Action = _securityTypeToCreateMethod[securityType];
                _getSecurityRequest.Arg0 = _securityTypeToClassCode[securityType];
                _getSecurityRequest.Arg1 = ticker;

                return FunctionsWrappers.ReadSpecificEntry(ref _getSecurityRequest); 
            }
        }
        public static Security? GetSecurity(ref SecurityRequestContainer request)
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

                return FunctionsWrappers.ReadSpecificEntry(ref _getSecurityRequest);
            }
        }
        public static void UpdateSecurity(Security security)
        {
            lock (_userRequestLock)
            {
                security.PriceStepValue = TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_PRICE_STEP_VALUE);

                if (security is MoexDerivativeBase baseSec)
                {
                    baseSec.UpperPriceLimit = TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_UPPER_PRICE_LIMIT);
                    baseSec.LowerPriceLimit = TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_LOWER_PRICE_LIMIT);
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
            var csv = Quik.Lua.ReadAsString();

            return string.IsNullOrWhiteSpace(csv)
                ? Enumerable.Empty<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        private static Security? ResolveUnderlying(string? classCode, string? secCode)
        {
            var request = new SecurityRequestContainer
            {
                ClassCode = classCode,
                Ticker = secCode
            };
            return _entityResolver.Resolve(ref request);
        }

        static SecuritiesProvider()
        {
            SecurityWrapper.Set(Quik.Lua);
        }
    }
}
