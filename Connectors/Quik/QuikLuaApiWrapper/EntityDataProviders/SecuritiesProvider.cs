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
using CallbackParameters = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.ReadCallbackArgs<string?, string?, Quik.EntityProviders.RequestContainers.SecurityRequestContainer>;
using System.Diagnostics;

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
        private static CallbackParameters _updatedArgs;

        private static readonly object _callbackLock = new();
        private static readonly object _userRequestLock = new();
        private static SecurityResolver? _entityResolver;
        
        private static IEntityEventSignalizer<Security> _eventSignalizer = new DirectEntitySignalizer<Security>();

        public static AllowEntityCreationFilter<SecurityRequestContainer> CreationIsApproved = delegate { return true; };
        public static EntityEventHandler<Security> EntityChanged = delegate { };
        public static EntityEventHandler<Security> NewEntity = delegate { };

        public static void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            SecurityWrapper.Set(Quik.Lua);
            _updatedArgs.LuaProvider = Quik.Lua;
            _updatedArgs.Callback = SecurityRequestContainer.Create;
            _entityResolver = EntityResolvers.GetSecurityResolver();
            _eventSignalizer = new EventSignalizer<Security>(entityNotificationLoop)
            {
                IsEnabled = true
            };
        }
        public static void SubscribeCallback()
        {
            Quik.Lua.RegisterCallback(OnNewData, SecurityWrapper.CALLBACK_METHOD);
        }

        public static Decimal5? GetBuyMarginRequirements(Security security)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_BUY_MARGIN_REQUIREMENTS);
        }
        public static Decimal5? GetSellMarginRequirements(Security security)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_SELL_MARGIN_REQUIREMENTS);
        }

        public static IEnumerable<string> GetAvailableSecuritiesOfType(string classcode)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = classcode;

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public static IEnumerable<string> GetAvailableClasses()
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            lock (_userRequestLock)
            {
                return FunctionsWrappers.ReadSpecificEntry(ref _classesCsvRequest); 
            }
        }
        public static Security? Create(ref SecurityRequestContainer request)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
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
        public static void Update(Security security)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
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
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
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

                Update(result);

                return result;
            }

            return null;
        }
        private static Security? CreateOption()
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
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

                Update(result);

                return result;
            }

            return null;
        }
        private static Security? CreateFutures()
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            if (TryCreateSecurityParamsContainer(out SecurityParamsContainer container))
            {
                var result = new Futures(ref container)
                {
                    Underlying = ResolveUnderlying(SecurityWrapper.UnderlyingClassCode, SecurityWrapper.UnderlyingSecCode),

                    Expiry = SecurityWrapper.Expiry
                        ?? throw QuikApiException.ParseExceptionMsg(nameof(SecurityWrapper.Expiry), "string")
                };

                Update(result);

                return result;
            }

            return null;
        }
        private static Security? CreateStock()
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            return TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
                ? new Stock(ref container)
                : null;
        }
        
        private static bool TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
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
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            var csv = Quik.Lua.ReadAsString();

            return string.IsNullOrWhiteSpace(csv)
                ? Enumerable.Empty<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        private static Security? ResolveUnderlying(string? classCode, string? secCode)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            var request = SecurityRequestContainer.Create(secCode, classCode);
            return _entityResolver.Resolve(ref request);
        }

        private static int OnNewData(IntPtr state)
        {
#if TRACE
            Extentions.Trace(nameof(SecuritiesProvider));
#endif
            lock (_callbackLock)
            {
                _updatedArgs.LuaProvider = state;

                var request = FunctionsWrappers.ReadCallbackArguments(ref _updatedArgs);
                var entity = _entityResolver.GetFromCache(ref request);

                if (entity != null)
                {
                    Update(entity);

                    _eventSignalizer.QueueEntity(EntityChanged, entity);
                    return 1;
                }

                if (CreationIsApproved(ref request) && (entity = Create(ref request)) != null)
                {
                    _entityResolver.CacheEntity(ref request, entity);
                    _eventSignalizer.QueueEntity(NewEntity, entity);
                }

                return 1;
            }
        }
    }
}
