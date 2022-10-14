using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;
using MoexCommonTypes;
using QuikLuaApi.Entities;

namespace QuikLuaApi
{
    internal static class SecuritiesFactory
    {
        public static ISecurity CreateStock(string ticker, string description)
        {
            return new Stock
            {
                Ticker = ticker,
                Description = description,
            };
        }

        public static ISecurity CreateFutures(string ticker, string description, 
            MoexDate expiry, ISecurity underlying)
        {
            return new Futures
            {
                Underlying = underlying,
                Ticker = ticker,
                Expiry = expiry.Date + MoexSpecifics.CommonExpiryTime,
                Description = description
            };
        }

        public static ISecurity CreateCalendarSpread(string ticker, string description, 
            IExpiring nearTermLeg, IExpiring longTermLeg)
        {
            return new CalendarSpread
            {
                Ticker = ticker,
                Description = description,
                NearTermLeg = nearTermLeg,
                LongTermLeg = longTermLeg,                
            };
        }

        public static ISecurity CreateOption(string ticker, string description,
            DateTimeOffset expiry, ISecurity underlying, Decimal5 strike, OptionTypes type)
        {
            return new Option
            {
                Underlying = underlying,
                Ticker = ticker,
                Expiry = expiry,
                Description = description,
                Strike = strike,
                OptionType = type,
            };
        }
    }
}
