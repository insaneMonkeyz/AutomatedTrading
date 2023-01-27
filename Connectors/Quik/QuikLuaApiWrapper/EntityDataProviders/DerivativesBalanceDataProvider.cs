using BasicConcepts;
using Quik.Entities;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.EntityDataProviders.RequestContainers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.Method4ParamsNoReturn<Quik.Entities.SecurityBalance, Quik.LuaState>;

namespace Quik.EntityDataProviders
{
    internal delegate ISecurity? GetSecurityHandler(SecurityRequestContainer security);
    
    internal class DerivativesBalanceDataProvider : DataProvider<SecurityBalance, SecurityBalanceRequestContainer>
    {
        private static UpdateParams _updateParams;

        private readonly DerivativeRequestContainer _securityRequest = new();
        private readonly EntityResolver<SecurityRequestContainer, Security> _securitiesResolver
            = EntityResolversFactory.GetSecurityResolver();

        protected override string QuikCallbackMethod => DerivativesPositionsWrapper.CALLBACK_METHOD;

        public List<SecurityBalance> GetAllPositions()
        {
            return ReadWholeTable(DerivativesPositionsWrapper.NAME, Create);
        }
        public override void Update(SecurityBalance entity)
        {
            lock (_userRequestLock)
            {
                _updateParams.Arg0 = entity.FirmId;
                _updateParams.Arg1 = entity.AccountId;
                _updateParams.Arg2 = entity.Security.Ticker;
                _updateParams.ActionParams.Arg0 = entity;

                ReadSpecificEntry(ref _updateParams); 
            }
        }

        private void BuildSecurityResolveRequest(LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            _securityRequest.Ticker = DerivativesPositionsWrapper.Ticker;
        }
        protected override void BuildEntityResolveRequest(LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            _resolveEntityRequest.Ticker = DerivativesPositionsWrapper.Ticker;
            _resolveEntityRequest.FirmId = DerivativesPositionsWrapper.FirmId;
            _resolveEntityRequest.ClientCode = DerivativesPositionsWrapper.AccountId;
        }

        protected override void Update(SecurityBalance entity, LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            entity.Collateral = DerivativesPositionsWrapper.Collateral;
            entity.Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault();
        }
        protected override SecurityBalance? Create(LuaState state)
        {
            BuildSecurityResolveRequest(state);

            if (_securitiesResolver.GetEntity(_securityRequest) is not ISecurity security)
            {
                $"Coudn't create SecurityBalance entity. Failed to resolve security {_securityRequest.Ticker} it belongs to".DebugPrintWarning();
                return default;
            }
            
            DerivativesPositionsWrapper.Set(state);

            return new SecurityBalance(security)
            {
                FirmId = DerivativesPositionsWrapper.FirmId,
                AccountId = DerivativesPositionsWrapper.AccountId,
                Collateral = DerivativesPositionsWrapper.Collateral,
                Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault()
            };
        }

        #region Singleton
        [SingletonInstance]
        public static DerivativesBalanceDataProvider Instance { get; } = new();
        private DerivativesBalanceDataProvider() : base(EntityResolversFactory.GetBalanceResolver())
        {
            _updateParams = new()
            {
                Arg3 = DerivativesPositionsWrapper.LIMIT_TYPE,
                Method = DerivativesPositionsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                ActionParams = new() { Arg1 = State },
                Action = Update,
            };
        }
        #endregion
    }
}
