using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApi.Entities;
using QuikLuaApi.QuikApi;
using QuikLuaApiWrapper.ApiWrapper;
using QuikLuaApiWrapper.ApiWrapper.Account;
using QuikLuaApiWrapper.Entities;
using QuikLuaApiWrapper.Extensions;

namespace QuikLuaApi
{
    public partial class QuikLuaApiWrapper
    {
        private static List<DerivativesTradingAccount> GetDerivativesExchangeAccounts()
        {
            static DerivativesTradingAccount? create()
            {
                if (FuturesLimits.IsMainAccount)
                {
                    return new()
                    {
                        Code = FuturesLimits.AccountId,
                        FirmId = FuturesLimits.FirmId,
                        TotalFunds = FuturesLimits.TotalFunds,
                        UnusedFunds = FuturesLimits.UnusedFunds,
                        FloatingIncome = FuturesLimits.FloatingIncome
                                       + FuturesLimits.RecorderIncome,
                        CollateralMargin = FuturesLimits.Collateral,
                    };

                }

                return null;
            }

            return GenericQuikTable.ReadTable(_localState, FuturesLimits.NAME, create, FuturesLimits.ReadAllocated);
        }

        private static void GetStockMarketFunds(string clientCode, string firmId, string tag, string currencyCode)
        {
            if(_localState.ExecFunction(Methods.GET_SPOT_FUNDS, LuaApi.TYPE_TABLE, clientCode, firmId, tag, currencyCode))
            {
                var b = _localState.ReadRowValueDecimal5(Account.SPOT_AVAILABLE_FUNDS);
                var a = _localState.ReadRowValueDecimal5(Account.SPOT_CURRENT_FUNDS);
                var c = _localState.ReadRowValueDecimal5(Account.SPOT_CURRENT_FUNDS_LIMIT);
                var d = _localState.ReadRowValueDecimal5(Account.SPOT_NON_MARGIN_ODRERS_COLLATERAL);
                var e = _localState.ReadRowValueDecimal5(Account.SPOT_ODRERS_COLLATERAL);
                var f = _localState.ReadRowValueDecimal5(Account.SPOT_INCOMING_FUNDS);
                var g = _localState.ReadRowValueDecimal5(Account.SPOT_INCOMING_FUNDS_LIMIT);
            }

            _localState.PopFromStack();
        }
    }
}
