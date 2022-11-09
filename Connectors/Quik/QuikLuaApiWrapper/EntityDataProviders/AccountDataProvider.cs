using BasicConcepts;
using Quik;
using Quik.Entities;
using Quik.EntityDataProviders;
using Quik.EntityDataProviders.EntityDummies;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.Method4ParamsNoReturn<Quik.Entities.DerivativesTradingAccount, Quik.LuaState>;

namespace Quik.EntityDataProviders
{
    internal sealed class AccountDataProvider : BaseDataProvider<DerivativesTradingAccount, DerivativesAccountDummy>
    {
        private static UpdateParams _updateParams = new()
        {
            Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
            Method = FuturesLimitsWrapper.GET_METOD,
            ReturnType = LuaApi.TYPE_TABLE,
        };

        public static AccountDataProvider Instance { get; } = new();

        protected override string QuikCallbackMethod => FuturesLimitsWrapper.CALLBACK_METHOD;

        public List<DerivativesTradingAccount> GetAllEntities()
        {
            return ReadWholeTable(FuturesLimitsWrapper.NAME, Create);
        }
        public override void Update(DerivativesTradingAccount entity)
        {
            lock (_userRequestLock)
            {
                if (entity.MoexCurrCode is not null)
                {
                    _updateParams.Arg0 = entity.FirmId;
                    _updateParams.Arg1 = entity.AccountCode;
                    _updateParams.Arg3 = entity.MoexCurrCode;
                    _updateParams.ActionParams.Arg0 = entity;
                    _updateParams.ActionParams.Arg1 = State;
                    _updateParams.Action = Update;

                    ReadSpecificEntry(ref _updateParams); 
                }
                else
                {
                    $"Can't update this {entity} account because its MoexCurrencyCode is null".DebugPrintWarning();
                }
            }
        }

        protected override void Update(DerivativesTradingAccount account, LuaState state)
        {
            FuturesLimitsWrapper.Set(state);

            account.TotalFunds = FuturesLimitsWrapper.TotalFunds;
            account.UnusedFunds = FuturesLimitsWrapper.UnusedFunds;
            account.CollateralMargin = FuturesLimitsWrapper.Collateral;
            account.FloatingIncome = FuturesLimitsWrapper.FloatingIncome
                                   + FuturesLimitsWrapper.RecorderIncome;
        }

        protected override void SetDummy(LuaState state)
        {
            FuturesLimitsWrapper.Set(state);

            _dummy.IsMoneyAccount = FuturesLimitsWrapper.IsMainAccount;
            _dummy.FirmId = FuturesLimitsWrapper.FirmId;
            _dummy.ClientCode = FuturesLimitsWrapper.ClientCode;
        }
        protected override DerivativesTradingAccount Create(LuaState state)
        {
            FuturesLimitsWrapper.Set(state);

            var currCode = FuturesLimitsWrapper.MoexCurrencyCode;

            var result = new DerivativesTradingAccount()
            {
                AccountCode = FuturesLimitsWrapper.ClientCode,
                FirmId = FuturesLimitsWrapper.FirmId,
                IsMoneyAccount = FuturesLimitsWrapper.IsMainAccount,
                AccountCurrency = currCode.CodeToCurrency(),
                MoexCurrCode = currCode
            };

            Update(result, state);

            return result;
        }
    }
}
