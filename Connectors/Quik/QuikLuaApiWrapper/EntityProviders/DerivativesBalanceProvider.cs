using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Quik.Lua;

using TradingConcepts;

using CreateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method4Params<Quik.Lua.LuaWrap, Quik.Entities.SecurityBalance?>;
using UpdateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.VoidMethod4Params<Quik.Entities.SecurityBalance, Quik.Lua.LuaWrap>;

namespace Quik.EntityProviders
{
    internal delegate ISecurity? GetSecurityHandler(SecurityRequestContainer security);
    
    internal sealed class DerivativesBalanceProvider : UpdatableEntitiesProvider<SecurityBalance, SecurityBalanceRequestContainer>
    {
        private static UpdateParams _updateParams = new()
        {
            Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
            Method = DerivativesPositionsWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
            Callback = new()
            {
                Arg1 = default,
                Invoke = default
            },
        };
        private static CreateParams _createParams = new()
        {
            Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
            Method = DerivativesPositionsWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
            Callback = new()
            {
                Arg = default,
                Invoke = default,
                DefaultValue = null
            },
        };

        private readonly object _securityRequestLock = new();
        private SecurityResolver _securitiesResolver;

        protected override string QuikCallbackMethod => DerivativesPositionsWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => DerivativesPositionsWrapper.NAME;
        protected override Action<LuaWrap> SetWrapper => DerivativesPositionsWrapper.Set;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _updateParams.Callback.Invoke = Update;
            _createParams.Callback.Invoke = Create;
            _updateParams.Callback.Arg1 = Quik.Lua;
            _createParams.Callback.Arg = Quik.Lua;

            _securitiesResolver = EntityResolvers.GetSecurityResolver();

            base.Initialize(entityNotificationLoop);
        }
        public override SecurityBalance? Create(ref SecurityBalanceRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
            base.Create(ref request);

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;
                _createParams.Arg2 = request.Ticker;

                return FunctionsWrappers.ReadSpecificEntry(ref _createParams);
            }
        }
        protected override SecurityBalance? Create(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            lock (DerivativesPositionsWrapper.Lock)
            {
                DerivativesPositionsWrapper.Set(state);

                var resolveSecurityRequest = new SecurityRequestContainer
                {
                    Ticker = DerivativesPositionsWrapper.Ticker
                };

                if (_securitiesResolver.Resolve(ref resolveSecurityRequest) is not ISecurity security)
                {
                    _log.Error($"Coudn't create SecurityBalance entity. Failed to resolve account that security {resolveSecurityRequest.Ticker} belongs to");
                    return default;
                }

                return new SecurityBalance(security)
                {
                    FirmId = DerivativesPositionsWrapper.FirmId,
                    Account = DerivativesPositionsWrapper.AccountId,
                    Collateral = DerivativesPositionsWrapper.Collateral,
                    Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault()
                }; 
            }
        }
        public override void Update(SecurityBalance entity)
        {
#if TRACE
            this.Trace();
#endif
            base.Update(entity);

            lock (_requestInProgressLock)
            {
                _updateParams.Arg0 = entity.FirmId;
                _updateParams.Arg1 = entity.Account;
                _updateParams.Arg2 = entity.Security.Ticker;
                _updateParams.Callback.Arg0 = entity;

                FunctionsWrappers.ReadSpecificEntry(ref _updateParams); 
            }
        }
        protected override void Update(SecurityBalance entity, LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            lock (DerivativesPositionsWrapper.Lock)
            {
                DerivativesPositionsWrapper.Set(state);

                entity.Collateral = DerivativesPositionsWrapper.Collateral;
                entity.Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault(); 
            }
        }

        protected override SecurityBalanceRequestContainer CreateRequestFrom(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
            lock (DerivativesPositionsWrapper.Lock)
            {
                DerivativesPositionsWrapper.Set(state);

                return new()
                {
                    Ticker = DerivativesPositionsWrapper.Ticker,
                    FirmId = DerivativesPositionsWrapper.FirmId,
                    Account = DerivativesPositionsWrapper.AccountId,
                }; 
            }
        }
        protected override void LogEntityCreated(SecurityBalance entity)
        {
            _log.Debug($@"Received new {nameof(SecurityBalance)} from quik
    {nameof(entity.Security)}={entity.Security.Ticker}
    {nameof(entity.Collateral)}={entity.Collateral}
    {nameof(entity.Account)}={entity.Account}
    {nameof(entity.Amount)}={entity.Amount}
    {nameof(entity.FirmId)}={entity.FirmId}");
        }
        protected override void LogEntityUpdated(SecurityBalance entity)
        {
            _log.Debug($@"Received updates for {nameof(SecurityBalance)} {entity.Security.Ticker}
    {nameof(entity.Collateral)}={entity.Collateral}
    {nameof(entity.Amount)}={entity.Amount}");
        }

        #region Singleton
        [SingletonInstance(rank: 10)]
        public static DerivativesBalanceProvider Instance { get; } = new();
        private DerivativesBalanceProvider() { }
        #endregion
    }
}
