using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.VoidMethod4Params<Quik.Entities.SecurityBalance, Quik.LuaState>;
using CreateParams = Quik.QuikProxy.Method4Params<Quik.LuaState, Quik.Entities.SecurityBalance?>;

namespace Quik.EntityProviders
{
    internal delegate ISecurity? GetSecurityHandler(SecurityRequestContainer security);
    
    internal class DerivativesBalanceProvider : UpdatableEntitiesProvider<SecurityBalance, SecurityBalanceRequestContainer>
    {
        private static UpdateParams _updateParams;
        private static CreateParams _createParams;

        private readonly object _securityRequestLock = new();
        private readonly DerivativeRequestContainer _securityRequest = new();
        private readonly EntityResolver<SecurityRequestContainer, Security> _securitiesResolver
            = EntityResolvers.GetSecurityResolver();

        protected override string QuikCallbackMethod => DerivativesPositionsWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => DerivativesPositionsWrapper.NAME;

        public override SecurityBalance? Create(SecurityBalanceRequestContainer request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException($"{nameof(SecurityBalanceRequestContainer)} request is missing essential parameters");
            }

            lock (_userRequestLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;
                _createParams.Arg2 = request.Ticker;

                return ReadSpecificEntry(ref _createParams);
            }
        }
        protected override SecurityBalance? Create(LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            _securityRequest.Ticker = DerivativesPositionsWrapper.Ticker;

            if (_securitiesResolver.GetEntity(_securityRequest) is not ISecurity security)
            {
                $"Coudn't create SecurityBalance entity. Failed to resolve security {_securityRequest.Ticker} it belongs to".DebugPrintWarning();
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
        public override void Update(SecurityBalance entity)
        {
            lock (_userRequestLock)
            {
                _updateParams.Arg0 = entity.FirmId;
                _updateParams.Arg1 = entity.Account;
                _updateParams.Arg2 = entity.Security.Ticker;
                _updateParams.Callback.Arg0 = entity;

                ReadSpecificEntry(ref _updateParams); 
            }
        }
        protected override void Update(SecurityBalance entity, LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            entity.Collateral = DerivativesPositionsWrapper.Collateral;
            entity.Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault();
        }

        protected override void BuildEntityResolveRequest(LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            _resolveEntityRequest.Ticker = DerivativesPositionsWrapper.Ticker;
            _resolveEntityRequest.FirmId = DerivativesPositionsWrapper.FirmId;
            _resolveEntityRequest.Account = DerivativesPositionsWrapper.AccountId;
        }


        #region Singleton
        [SingletonInstance]
        public static DerivativesBalanceProvider Instance { get; } = new();
        private DerivativesBalanceProvider()
        {
            _updateParams = new()
            {
                Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
                Method = DerivativesPositionsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                Callback = new() 
                { 
                    Arg1 = State,
                    Invoke = Update
                },
            };
            _createParams = new()
            {
                Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
                Method = DerivativesPositionsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                Callback = new() 
                { 
                    Arg = State,
                    Invoke = Create,
                    DefaultValue = null
                },
            };
        }
        #endregion
    }
}
