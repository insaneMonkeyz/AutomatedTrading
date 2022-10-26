using QuikLuaApi;
using QuikLuaApiWrapper.ApiWrapper;
using QuikLuaApiWrapper.ApiWrapper.QuikApi;
using QuikLuaApiWrapper.Entities;

namespace Quik.ApiWrapper
{
    internal delegate DerivativesTradingAccount? DerivativesAccountChangingHandler(bool isMoneyAccount, string clientCode, string firmId);
    internal delegate void DerivativesTradingAccountChangedHandler(DerivativesTradingAccount account);

    internal class AccountWrapper : BaseWrapper<DerivativesTradingAccount>
    {
        public static AccountWrapper Instance { get; } = new();

        internal event DerivativesAccountChangingHandler DerivativesAccountChanging;
        internal event DerivativesTradingAccountChangedHandler DerivativesAccountChanged;

        internal override List<DerivativesTradingAccount> GetAllEntities()
        {
            return QuikLuaApi.QuikLuaApiWrapper.ReadWholeTable(FuturesLimits.NAME, Create);
        }
        internal override void Update(DerivativesTradingAccount account)
        {
            account.TotalFunds = FuturesLimits.TotalFunds;
            account.UnusedFunds = FuturesLimits.UnusedFunds;
            account.CollateralMargin = FuturesLimits.Collateral;
            account.FloatingIncome = FuturesLimits.FloatingIncome
                                   + FuturesLimits.RecorderIncome;
        }
        internal override void Subscribe(CallbackSubscriber subscribe)
        {
            subscribe(OnEntityChanged, FuturesLimits.CALLBACK_METHOD);
        }

        protected override int OnEntityChanged(IntPtr state)
        {
            _state = state;

            FuturesLimits.Set(state);

            if (DerivativesAccountChanging(
                    FuturesLimits.IsMainAccount, 
                    FuturesLimits.ClientCode, 
                    FuturesLimits.FirmId) 
                        is DerivativesTradingAccount account)
            {
                Update(account);

                DerivativesAccountChanged(account);
            }

            _state = default;

            return 1;
        }
        protected override DerivativesTradingAccount Create(LuaState state)
        {
            FuturesLimits.Set(state);

            var result = new DerivativesTradingAccount()
            {
                AccountCode = FuturesLimits.ClientCode,
                FirmId = FuturesLimits.FirmId,
                IsMoneyAccount = FuturesLimits.IsMainAccount,
                AccountCurrency = FuturesLimits.AccountCurrency ?? BasicConcepts.Currencies.RUB
            };

            Update(result);

            return result;
        }
    }
}
