using BasicConcepts;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.VoidMethod4Params<Quik.Entities.SecurityBalance, Quik.LuaState>;
using CreateParams = Quik.QuikProxy.Method4Params<Quik.LuaState, Quik.Entities.SecurityBalance?>;
using System.Diagnostics;

namespace Quik.EntityProviders
{
    internal delegate ISecurity? GetSecurityHandler(SecurityRequestContainer security);
    
    internal class DerivativesBalanceProvider : UpdatableEntitiesProvider<SecurityBalance, SecurityBalanceRequestContainer>
    {
        private static UpdateParams _updateParams;
        private static CreateParams _createParams;

        private readonly object _securityRequestLock = new();
        private SecurityResolver _securitiesResolver;

        protected override string QuikCallbackMethod => DerivativesPositionsWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => DerivativesPositionsWrapper.NAME;

        public override void Initialize()
        {
            Debugger.Launch();

            _securitiesResolver = EntityResolvers.GetSecurityResolver();

            base.Initialize();
        }
        public override SecurityBalance? Create(SecurityBalanceRequestContainer request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException($"{nameof(SecurityBalanceRequestContainer)} request is missing essential parameters");
            }

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;
                _createParams.Arg2 = request.Ticker;

                return ReadSpecificEntry(ref _createParams);
            }
        }
        protected override SecurityBalance? Create(LuaState state)
        {
            lock (DerivativesPositionsWrapper.Lock)
            {
                DerivativesPositionsWrapper.Set(state);

                var resolveSecurityRequest = new SecurityRequestContainer
                {
                    Ticker = DerivativesPositionsWrapper.Ticker
                };

                if (_securitiesResolver.Resolve(resolveSecurityRequest) is not ISecurity security)
                {
                    $"Coudn't create SecurityBalance entity. Failed to resolve security {resolveSecurityRequest.Ticker} belongs to".DebugPrintWarning();
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
            lock (_requestInProgressLock)
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
            lock (DerivativesPositionsWrapper.Lock)
            {
                DerivativesPositionsWrapper.Set(state);

                entity.Collateral = DerivativesPositionsWrapper.Collateral;
                entity.Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault(); 
            }
        }

        protected override SecurityBalanceRequestContainer CreateRequestFrom(LuaState state)
        {
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
