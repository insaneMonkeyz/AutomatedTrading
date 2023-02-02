using BasicConcepts;
using Quik;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.VoidMethod4Params<Quik.Entities.DerivativesTradingAccount, Quik.LuaState>;
using CreateParams = Quik.QuikProxy.Method4Params<Quik.LuaState, Quik.Entities.DerivativesTradingAccount?>;

namespace Quik.EntityProviders
{
    internal sealed class AccountsProvider : UpdatableEntitiesProvider<DerivativesTradingAccount, AccountRequestContainer>
    {
        private static UpdateParams _updateParams;
        private static CreateParams _createParams;

        protected override string QuikCallbackMethod => FuturesLimitsWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => FuturesLimitsWrapper.NAME;

        public override DerivativesTradingAccount? Create(AccountRequestContainer request)
        {
            if (!request.HasData)
            {
                throw new ArgumentException($"{nameof(AccountRequestContainer)} request is missing essential parameters");
            }

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;

                return ReadSpecificEntry(ref _createParams);
            }
        }
        protected override DerivativesTradingAccount Create(LuaState state)
        {
            lock (FuturesLimitsWrapper.Lock)
            {
                FuturesLimitsWrapper.Set(state);

                var currCode = FuturesLimitsWrapper.MoexCurrencyCode;

                var result = new DerivativesTradingAccount()
                {
                    AccountCode = FuturesLimitsWrapper.ClientCode,
                    FirmId = FuturesLimitsWrapper.FirmId,
                    LimitType = FuturesLimitsWrapper.LimitType,
                    AccountCurrency = currCode.CodeToCurrency(),
                    MoexCurrCode = currCode
                };

                Update(result, state);

                return result; 
            }
        }
        public    override void Update(DerivativesTradingAccount entity)
        {
            lock (_requestInProgressLock)
            {
                _updateParams.Arg0 = entity.FirmId;
                _updateParams.Arg1 = entity.AccountCode;
                _updateParams.Arg3 = entity.MoexCurrCode ?? string.Empty;
                _updateParams.Callback.Arg0 = entity;

                ReadSpecificEntry(ref _updateParams);
            }
        }
        protected override void Update(DerivativesTradingAccount account, LuaState state)
        {
            lock (FuturesLimitsWrapper.Lock)
            {
                FuturesLimitsWrapper.Set(state);

                account.TotalFunds = FuturesLimitsWrapper.TotalFunds;
                account.UnusedFunds = FuturesLimitsWrapper.UnusedFunds;
                account.CollateralMargin = FuturesLimitsWrapper.Collateral;
                account.FloatingIncome = FuturesLimitsWrapper.FloatingIncome
                                       + FuturesLimitsWrapper.RecordedIncome; 
            }
        }

        protected override AccountRequestContainer CreateRequestFrom(LuaState state)
        {
            lock (FuturesLimitsWrapper.Lock)
            {
                FuturesLimitsWrapper.Set(state);

                return new()
                {
                    LimitType = FuturesLimitsWrapper.LimitType,
                    Account = FuturesLimitsWrapper.ClientCode,
                    FirmId = FuturesLimitsWrapper.FirmId,
                }; 
            }
        }

        #region Singleton
        [SingletonInstance]
        public static AccountsProvider Instance { get; } = new();
        private AccountsProvider()
        {
            _updateParams = new()
            {
                Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
                Method = FuturesLimitsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                Callback = new()
                {
                    Arg1 = State,
                    Invoke = Update
                },
            };
            _createParams = new()
            {
                Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
                Arg3 = string.Empty,
                Method = FuturesLimitsWrapper.GET_METOD,
                ReturnType = LuaApi.TYPE_TABLE,
                Callback = new()
                {
                    Arg = State,
                    Invoke = Create,
                    DefaultValue = null
                }
            };
        } 
        #endregion
    }
}
