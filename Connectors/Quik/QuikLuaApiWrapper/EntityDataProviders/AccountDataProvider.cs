﻿using BasicConcepts;
using Quik;
using Quik.Entities;
using Quik.EntityDataProviders;
using Quik.EntityDataProviders.Attributes;
using Quik.EntityDataProviders.RequestContainers;
using Quik.EntityDataProviders.QuikApiWrappers;

using static Quik.QuikProxy;
using UpdateParams = Quik.QuikProxy.VoidMethod4Params<Quik.Entities.DerivativesTradingAccount, Quik.LuaState>;
using CreateParams = Quik.QuikProxy.Method4Params<Quik.LuaState, Quik.Entities.DerivativesTradingAccount?>;

namespace Quik.EntityDataProviders
{
    internal sealed class AccountDataProvider : UpdatesSupportingDataProvider<DerivativesTradingAccount, AccountRequestContainer>
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

            lock (_userRequestLock)
            {
                _createParams.Arg0 = request.FirmId;
                _createParams.Arg1 = request.Account;

                return ReadSpecificEntry(ref _createParams);
            }
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
        public    override void Update(DerivativesTradingAccount entity)
        {
            lock (_userRequestLock)
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
            _resolveEntityRequest.Account = FuturesLimitsWrapper.ClientCode;
        }

        #region Singleton
        [SingletonInstance]
        public static AccountDataProvider Instance { get; } = new();
        private AccountDataProvider()
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
