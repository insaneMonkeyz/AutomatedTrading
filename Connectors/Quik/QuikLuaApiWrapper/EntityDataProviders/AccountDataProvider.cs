using BasicConcepts;
using Quik;
using Quik.Entities;
using Quik.EntityDataProviders;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.RequestContainers;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.Method4ParamsNoReturn<Quik.Entities.DerivativesTradingAccount, Quik.LuaState>;

namespace Quik.EntityDataProviders
{
    internal sealed class AccountDataProvider : DataProvider<DerivativesTradingAccount, AccountRequestContainer>
    {
        private static UpdateParams _updateParams;

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

        protected override void BuildEntityResolveRequest(LuaState state)
        {
            FuturesLimitsWrapper.Set(state);

            _resolveEntityRequest.IsMoneyAccount = FuturesLimitsWrapper.IsMainAccount;
            _resolveEntityRequest.FirmId = FuturesLimitsWrapper.FirmId;
            _resolveEntityRequest.ClientCode = FuturesLimitsWrapper.ClientCode;
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

        #region Singleton
        [SingletonInstance]
        public static AccountDataProvider Instance { get; } = new();
        private AccountDataProvider() : base(EntityResolversFactory.GetAccountsResolver())
        {
            _updateParams = new()
            {
                Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
                Method = FuturesLimitsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                Action = Update,
                ActionParams = new()
                {
                    Arg1 = State
                },
            };
        } 
        #endregion
    }
}
