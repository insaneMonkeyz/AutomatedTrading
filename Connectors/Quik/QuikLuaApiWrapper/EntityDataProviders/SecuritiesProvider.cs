using System.Runtime.CompilerServices;

using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using TradingConcepts;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

using CallbackParameters = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.ReadCallbackArgs<string?, string?, Quik.EntityProviders.RequestContainers.SecurityRequestContainer>;
using GetCsv1Param = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method1Param<System.Collections.Generic.IEnumerable<System.String>>;
using GetCsvNoParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.MethodNoParams<System.Collections.Generic.IEnumerable<System.String>>;
using GetItemParams = Quik.EntityProviders.QuikApiWrappers.TableWrapper.GetParamExParams;
using GetSecurityParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method2Params<Quik.Entities.Security?>;

namespace Quik.EntityProviders
{
    internal sealed class SecuritiesProvider : QuikDataConsumer<Security>
    {
        private readonly Dictionary<Type, Func<Security?>> _securityTypeToCreateMethod;
        private readonly Dictionary<string, Func<Security?>> _classcodeToCreateMethod;
        private readonly Dictionary<Type, string> _securityTypeToClassCode;
        private readonly Dictionary<string, Type> _classCodeToSecurityType;

        private GetCsv1Param _securitiesCsvRequest;
        private GetCsvNoParams _classesCsvRequest;
        private GetSecurityParams _getSecurityRequest = new()
        {
            Method = SecurityWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
            Arg0 = string.Empty,
            Arg1 = string.Empty
        };
        private GetItemParams _getSecurityParamRequest = new()
        {
            Ticker = string.Empty,
            ClassCode = string.Empty,
            Parameter = string.Empty
        };
        private CallbackParameters _requestContainerCreationArgs = new()
        {
            Callback = SecurityRequestContainer.Create
        };

        private bool _initialized;
        private readonly object _userRequestLock = new();
        private EntityResolver<SecurityRequestContainer, Security> _entityResolver = NoResolver<SecurityRequestContainer, Security>.Instance;

        public AllowEntityCreationFilter<SecurityRequestContainer> CreationIsApproved = delegate { return true; };
        public EntityEventHandler<Security> EntityChanged = delegate { };
        public EntityEventHandler<Security> NewEntity = delegate { };

        public event Action OnInitialized = delegate { };

        protected override string QuikCallbackMethod => SecurityWrapper.CALLBACK_METHOD;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            lock (_callbackLock)
            {
                SecurityWrapper.Set(Quik.Lua);
                _entityResolver = EntityResolvers.GetSecurityResolver();
                base.Initialize(entityNotificationLoop);
                _initialized = true;

                OnInitialized(); 
            }
        }

        public Decimal5? GetBuyMarginRequirements(Security security)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_BUY_MARGIN_REQUIREMENTS);
        }
        public Decimal5? GetSellMarginRequirements(Security security)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif
            return TableWrapper.FetchDecimal5ParamEx(security, SecurityWrapper.PARAM_SELL_MARGIN_REQUIREMENTS);
        }

        public IEnumerable<string> GetAvailableSecuritiesOfType(string classcode)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized(); 
#endif
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = classcode;

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public IEnumerable<string> GetAvailableSecuritiesOfType(Type type)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif
            lock (_userRequestLock)
            {
                _securitiesCsvRequest.Arg0 = _securityTypeToClassCode[type];

                return FunctionsWrappers.ReadSpecificEntry(ref _securitiesCsvRequest); 
            }
        }
        public IEnumerable<string> GetAvailableClasses()
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
#endif
            lock (_userRequestLock)
            {
                return FunctionsWrappers.ReadSpecificEntry(ref _classesCsvRequest); 
            }
        }
        public Security? Create(ref SecurityRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
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
        public void Update(Security security)
        {
#if TRACE
            this.Trace();
#endif
#if DEBUG
            EnsureInitialized();
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

        private Security? CreateCalendarSpread()
        {
#if TRACE
            this.Trace();
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
        private Security? CreateOption()
        {
#if TRACE
            this.Trace();
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
        private Security? CreateFutures()
        {
#if TRACE
            this.Trace();
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
        private Security? CreateStock()
        {
#if TRACE
            this.Trace();
#endif
            return TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
                ? new Stock(ref container)
                : null;
        }
        
        private bool TryCreateSecurityParamsContainer(out SecurityParamsContainer container)
        {
#if TRACE
            this.Trace();
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
        private IEnumerable<string> GetCsvValues()
        {
#if TRACE
            this.Trace();
#endif
            var csv = Quik.Lua.ReadAsString();

            return string.IsNullOrWhiteSpace(csv)
                ? Enumerable.Empty<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        private Security? ResolveUnderlying(string? classCode, string? secCode)
        {
#if TRACE
            this.Trace();
#endif
            var request = SecurityRequestContainer.Create(classCode, secCode);
            return _entityResolver.Resolve(ref request);
        }

        protected override int OnNewData(IntPtr state)
        {
#if TRACE
            this.Trace();
#endif
            lock (_callbackLock)
            {
                try
                {
                    _requestContainerCreationArgs.LuaProvider = state;

                    var request = FunctionsWrappers.ReadCallbackArguments(ref _requestContainerCreationArgs);
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
                catch (Exception e)
                {
                    e.DebugPrintException();

                    return 0;
                }
            }
        }

        private void EnsureInitialized([CallerMemberName] string? method = null)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"Calling {method ?? "METHOD_NOT_PROVIDED"} from {nameof(SecuritiesProvider)} before it was initialized.");
            }
        }

        #region Singleton
        [SingletonInstance(rank: 500)]
        public static SecuritiesProvider Instance { get; } = new();
        private SecuritiesProvider()
        {
            _securityTypeToCreateMethod = new(4)
            {
                { typeof(IStock), CreateStock },
                { typeof(IFutures), CreateFutures },
                { typeof(IOption), CreateOption },
                { typeof(ICalendarSpread), CreateCalendarSpread }
            };
            _classcodeToCreateMethod = new(4)
            {
                { SecurityWrapper.STOCK_CLASS_CODE, CreateStock },
                { SecurityWrapper.FUTURES_CLASS_CODE, CreateFutures },
                { SecurityWrapper.OPTIONS_CLASS_CODE, CreateOption },
                { SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE, CreateCalendarSpread }
            };
            _securityTypeToClassCode = new()
            {
                { typeof(IStock), SecurityWrapper.STOCK_CLASS_CODE },
                { typeof(IFutures), SecurityWrapper.FUTURES_CLASS_CODE },
                { typeof(IOption), SecurityWrapper.OPTIONS_CLASS_CODE },
                { typeof(ICalendarSpread), SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE },
            };
            _classCodeToSecurityType = new()
            {
                { SecurityWrapper.STOCK_CLASS_CODE, typeof(IStock) },
                { SecurityWrapper.FUTURES_CLASS_CODE, typeof(IFutures) },
                { SecurityWrapper.OPTIONS_CLASS_CODE, typeof(IOption) },
                { SecurityWrapper.CALENDAR_SPREADS_CLASS_CODE, typeof(ICalendarSpread) },
            };

            _classesCsvRequest = new()
            {
                Action = GetCsvValues,
                Method = SecurityWrapper.GET_CLASSES_LIST_METHOD,
                ReturnType = Api.TYPE_STRING,
                DefaultValue = Enumerable.Empty<string>()
            };
            _securitiesCsvRequest = new()
            {
                Action = GetCsvValues,
                Method = SecurityWrapper.GET_SECURITIES_OF_A_CLASS_METHOD,
                ReturnType = Api.TYPE_STRING,
                Arg0 = string.Empty,
                DefaultValue = Enumerable.Empty<string>()
            };
        }
        #endregion
    }
}
