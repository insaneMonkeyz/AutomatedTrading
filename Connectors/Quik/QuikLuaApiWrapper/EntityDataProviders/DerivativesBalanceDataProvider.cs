using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Quik.Entities;
using Quik.EntityDataProviders.EntityDummies;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.Method4ParamsNoReturn<Quik.Entities.SecurityBalance, Quik.LuaState>;

namespace Quik.EntityDataProviders
{
    internal delegate ISecurity? GetSecurityHandler(SecurityDummy security);
    internal class DerivativesBalanceDataProvider : BaseDataProvider<SecurityBalance, SecurityDummy>
    {
        private static UpdateParams _updateParams;

        protected override string QuikCallbackMethod => DerivativesPositionsWrapper.CALLBACK_METHOD;

        public GetSecurityHandler GetSecurity = delegate { return default; };

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

        protected override void Update(SecurityBalance entity, LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            entity.Collateral = DerivativesPositionsWrapper.Collateral;
            entity.Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault();
        }
        protected override void SetDummy(LuaState state)
        {
            DerivativesPositionsWrapper.Set(state);

            _dummy.Ticker = DerivativesPositionsWrapper.Ticker;
        }
        protected override SecurityBalance? Create(LuaState state)
        {
            SetDummy(state);

            if (!string.IsNullOrEmpty(_dummy.Ticker) && 
                GetSecurity(_dummy) is ISecurity security)
            {
                DerivativesPositionsWrapper.Set(state);

                return new SecurityBalance(security)
                {
                    FirmId = DerivativesPositionsWrapper.FirmId,
                    AccountId = DerivativesPositionsWrapper.AccountId,
                    Collateral = DerivativesPositionsWrapper.Collateral,
                    Amount = DerivativesPositionsWrapper.CurrentPos.GetValueOrDefault()
                };
            }
            else
            {
                return default;
            }
        }

        #region Singleton
        public static DerivativesBalanceDataProvider Instance { get; } = new();
        private DerivativesBalanceDataProvider()
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
