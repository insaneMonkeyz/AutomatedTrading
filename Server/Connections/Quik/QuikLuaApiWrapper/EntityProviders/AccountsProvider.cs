﻿using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

using CreateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.Method4Params<Quik.Lua.LuaWrap, Quik.Entities.DerivativesTradingAccount?>;
using UpdateParams = Quik.EntityProviders.QuikApiWrappers.FunctionsWrappers.VoidMethod4Params<Quik.Entities.DerivativesTradingAccount, Quik.Lua.LuaWrap>;

namespace Quik.EntityProviders
{
    internal sealed class AccountsProvider : UpdatableEntitiesProvider<DerivativesTradingAccount, AccountRequestContainer>
    {
        private static UpdateParams _updateParams = new()
        {
            Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
            Method = FuturesLimitsWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
            Callback = new()
            {
                Arg1 = default,
                Invoke = default
            },
        };
        private static CreateParams _createParams = new()
        {
            Arg2 = FuturesLimitsWrapper.MONEY_LIMIT_TYPE.ToString(),
            Arg3 = string.Empty,
            Method = FuturesLimitsWrapper.GET_METOD,
            ReturnType = Api.TYPE_TABLE,
            Callback = new()
            {
                Arg = default,
                Invoke = default,
                DefaultValue = null
            }
        };

        protected override Type WrapperType => typeof(FuturesLimitsWrapper);
        protected override string QuikCallbackMethod => FuturesLimitsWrapper.CALLBACK_METHOD;
        protected override string AllEntitiesTable => FuturesLimitsWrapper.NAME;
        protected override Action<LuaWrap> SetWrapper => FuturesLimitsWrapper.Set;

        public override void Initialize(ExecutionLoop entityNotificationLoop)
        {
#if TRACE
            this.Trace();
#endif
            _updateParams.Callback.Arg1 = Quik.Lua;
            _createParams.Callback.Arg = Quik.Lua;
            _updateParams.Callback.Invoke = Update;
            _createParams.Callback.Invoke = Create;

            base.Initialize(entityNotificationLoop);
        }

        public override DerivativesTradingAccount? Create(ref AccountRequestContainer request)
        {
#if TRACE
            this.Trace();
#endif
            base.Create(ref request);

            lock (_requestInProgressLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;

                return FunctionsWrappers.ReadSpecificEntry(ref _createParams);
            }
        }
        protected override DerivativesTradingAccount Create(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
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
#if TRACE
            this.Trace();
#endif
            base.Update(entity);

            lock (_requestInProgressLock)
            {
                _updateParams.Arg0 = entity.FirmId;
                _updateParams.Arg1 = entity.AccountCode;
                _updateParams.Arg3 = entity.MoexCurrCode ?? string.Empty;
                _updateParams.Callback.Arg0 = entity;

                FunctionsWrappers.ReadSpecificEntry(ref _updateParams);
            }
        }
        protected override void Update(DerivativesTradingAccount account, LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
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

        protected override AccountRequestContainer CreateRequestFrom(LuaWrap state)
        {
#if TRACE
            this.Trace();
#endif
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
        [SingletonInstance(rank: 50)]
        public static AccountsProvider Instance { get; } = new();
        private AccountsProvider() { }
        #endregion
    }
}
