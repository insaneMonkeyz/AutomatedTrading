using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts.SecuritySpecifics.Options;
using BasicConcepts;
using QuikLuaApi.Entities;
using QuikLuaApiWrapper;
using System.ComponentModel;
using System.Diagnostics;

namespace QuikLuaApi
{
    public delegate ISecurity GetSecurity(string classcode, string seccode);

    public partial class QuikLuaApiWrapper
    {
        internal static GetSecurity RequestSavedSecurity = delegate { return null; };

        internal static IEnumerable<string> GetClasses()
        {
            IEnumerable<string> parser()
            {
                var chain = _localState.PopString();

                return string.IsNullOrWhiteSpace(chain) 
                    ? Enumerable.Empty<string>() 
                    : chain.Split(',');
            }

            return _localState.ExecFunction(
                      name: QuikApi.GET_CLASSES_LIST_METHOD,
                returnType: LuaApi.TYPE_STRING,
                  callback: parser);
        }
        internal static IEnumerable<string> GetSecuritiesOfAClass(string classcode)
        {
            IEnumerable<string> parser()
            {
                var chain = _localState.PopString();

                return string.IsNullOrWhiteSpace(chain)
                    ? Enumerable.Empty<string>()
                    : chain.Split(',');
            }

            return _localState.ExecFunction(
                      name: QuikApi.GET_SECURITIES_OF_A_CLASS_METHOD,
                returnType: LuaApi.TYPE_STRING,
                  callback: parser,
                      arg0: classcode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="ticker"></param>
        /// <returns></returns>
        /// <exception cref="QuikApiException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        internal static ISecurity GetSecurity(string classCode, string ticker)
        {
            static ISecurity parseSecurityParameters()
            {
                var container = GetSecurityParamsContainer();

                if (!container.StateIsCorrect)
                {
                    throw new QuikApiException($"Security {container.Ticker} parsing exception. Essential parameters missing in the feed");
                }

                return container.ClassCode switch
                {
                    MoexSpecifics.OPTIONS_CLASS_CODE => GetOption(container),
                    MoexSpecifics.FUTURES_CLASS_CODE => GetFutures(container),
                    MoexSpecifics.CALENDAR_SPREADS_CLASS_CODE => GetCalendarSpread(container),
                    MoexSpecifics.STOCK_CLASS_CODE => new Stock(container),
                    _ => throw new NotImplementedException($"{container.ClassCode} is not a supported class code.")
                };
            }

            return _localState.ExecFunction(
                      name: QuikApi.GET_SECURITY_METHOD,
                returnType: LuaApi.TYPE_TABLE,
                  callback: parseSecurityParameters,
                      arg0: classCode,
                      arg1: ticker);
        }

        private static OptionTypes GetOptionType(string ticker)
        {
            var span = ticker.AsSpan();

            // Si65000BL2D

            //  01   23456   7  8   9  10
            // [Si] [65000] [B][L] [2] [D]

            // [7]B - American margin type
            // [8]A-L inclusive = calls
            // [8]M-X inclusive = puts

            var bIndex = ticker.LastIndexOf('B');
            var codeIndex = bIndex + 1;

            if(bIndex > -1 && codeIndex < ticker.Length)
            {
                var code = ticker[codeIndex];

                if (code >= 'A' && code <= 'X')
                {
                    return code <= 'L'
                        ? OptionTypes.Call
                        : OptionTypes.Put;
                }
                else if(code >= 'a' && code <= 'x')
                {
                    return code <= 'l'
                        ? OptionTypes.Call
                        : OptionTypes.Put;
                }
            }

            return OptionTypes.Undefined;
        }
        private static SecurityParamsContainer GetSecurityParamsContainer()
        {
            var ticker = _localState.ReadRowValueString(QuikApi.SECURITY_TICKER_PROPERTY);
            var classCode = _localState.ReadRowValueString(QuikApi.SECURITY_CLASS_CODE_PROPERTY);
            var currency = _localState.ReadRowValueString(QuikApi.SECURITY_CURRENCY_PROPERTY);

            return new()
            {
                Ticker = ticker,
                ClassCode = classCode,
                PricePrecisionScale = _localState.ReadRowValueLong(QuikApi.SECURITY_PRICE_SCALE_PROPERTY),
                MinPriceStep = _localState.ReadRowValueDecimal5(QuikApi.SECURITY_MIN_PRICE_STEP_PROPERTY),
                ContractSize = _localState.ReadRowValueLong(QuikApi.SECURITY_CONTRACT_SIZE_PROPERTY),
                Description = _localState.ReadRowValueString(QuikApi.SECURITY_DESCRIPTION_PROPERTY),
                DenominationCurrency = currency switch
                {
                    QuikApi.USD_CURRENCY => Currencies.USD,
                    QuikApi.RUB_CURRENCY => Currencies.RUB,
                    _ => throw new NotImplementedException(
                        $"Security {ticker} is denominated in currency '{currency}' that is not supported.")
                },
            };
        }

        private static ISecurity GetOption(SecurityParamsContainer container)
        {
            var optype = GetOptionType(container.Ticker);

            _localState.PrintStack("Parsing the option");

            if (optype != OptionTypes.Undefined &&
                _localState.TryFetchDecimalFromTable(QuikApi.SECURITY_STRIKE_PROPERTY, out Decimal5 strike) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_EXPIRY_DATE_PROPERTY, out string mat_date) &&
                mat_date.TryConvertToMoexExpiry(out DateTimeOffset expiry))
            {
                _localState.PrintStack("Options main parameters parsed");

                var underlying =
                    _localState.TryFetchStringFromTable(QuikApi.SECURITY_UNDERLYING_CLASS_CODE_PROPERTY, out string undClass) &&
                    _localState.TryFetchStringFromTable(QuikApi.SECURITY_UNDERLYING_SEC_CODE_PROPERTY, out string undTicker)
                        ? FindCachedOrLoadFromQuik(undClass, undTicker)
                        : default;

                _localState.PrintStack("Completed resolving the underlying of the option");

                return new Option(container)
                {
                    Strike = strike,
                    OptionType = optype,
                    Expiry = expiry,
                    Underlying = underlying,
                };
            }

            _localState.PrintStack("Option parsing failed");
            throw new QuikApiException($"Option {container.Ticker} parsing exception. Essential parameters missing in the feed");
        }
        private static ISecurity GetFutures(SecurityParamsContainer container)
        {
            _localState.PrintStack("Building futures");
            if (_localState.TryFetchStringFromTable(QuikApi.SECURITY_UNDERLYING_CLASS_CODE_PROPERTY, out string undClass) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_UNDERLYING_SEC_CODE_PROPERTY, out string undTicker) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_EXPIRY_DATE_PROPERTY, out string mat_date) &&
                mat_date.TryConvertToMoexExpiry(out DateTimeOffset expiry))
            {
                _localState.PrintStack("Futures successfully built");
                return new Futures(container)
                {
                    Expiry = expiry,
                    Underlying = FindCachedOrLoadFromQuik(undClass, undTicker),
                };
            }

            _localState.PrintStack("Failed parsing futures parameters");
            throw new QuikApiException($"Futures {container.Ticker} parsing exception. Essential parameters missing in the feed");
        }
        private static ISecurity GetCalendarSpread(SecurityParamsContainer container)
        {
            _localState.PrintStack("Building calendar spread");
            if (_localState.TryFetchStringFromTable(QuikApi.SECURITY_NEAR_TERM_LEG_CLASS_CODE_PROPERTY, out string neartermClass) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_NEAR_TERM_LEG_SEC_CODE_PROPERTY, out string neartermCode) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_LONG_TERM_LEG_CLASS_CODE_PROPERTY, out string longtermClass) &&
                _localState.TryFetchStringFromTable(QuikApi.SECURITY_LONG_TERM_LEG_SEC_CODE_PROPERTY, out string longtermCode) &&
                FindCachedOrLoadFromQuik(neartermClass, neartermCode) is IExpiring leg1 &&
                FindCachedOrLoadFromQuik(longtermClass, longtermCode) is IExpiring leg2)
            {
                _localState.PrintStack("Calendar spread fully parsed");
                return new CalendarSpread(container, leg1, leg2);
            }
            else 
            if(_localState.TryFetchStringFromTable(QuikApi.SECURITY_EXPIRY_DATE_PROPERTY, out string mat_date) &&
               mat_date.TryConvertToMoexExpiry(out DateTimeOffset expiry))
            {
                _localState.PrintStack("Couldnt resolve legs of the calendar spread");
                return new CalendarSpread(container, expiry);
            }

            _localState.PrintStack("Calendar spread parsing failed");
            throw new QuikApiException($"Calendar spread {container.Ticker} parsing exception. Essential parameters missing in the feed");
        }

        private static ISecurity FindCachedOrLoadFromQuik(string classcode, string ticker)
        {
            return RequestSavedSecurity(classcode, ticker) ?? GetSecurity(classcode, ticker);
        }
    }
}
