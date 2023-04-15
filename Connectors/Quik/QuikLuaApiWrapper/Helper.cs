using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik
{
    internal static class Helper
    {
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Security CastToMoexSecurity(ISecurity security)
        {
            return security switch
            {
                Security moexSecurity => moexSecurity,
                null => throw new ArgumentNullException(nameof(security)),
                   _ => throw new ArgumentException($"Security {security} does not belong to MOEX and therefore cannot be used in orders for it.")
            };
        }

        public static OptionDescription InferOptionFromTicker(string ticker)
        {
            if (string.IsNullOrEmpty(ticker))
            {
                throw new ArgumentNullException(nameof(ticker), $"{nameof(ticker)} is null or empty");
            }
            else if (ticker.Length < 7)
            {
                throw new ArgumentException($"{ticker} is invalid option ticker", nameof(ticker));
            }
            // Si65000BL2D

            //  01   23456   7  8   9  10
            // [Si] [65000] [B][L] [2] [A/D]

            // [7]B - American margin type
            // [8]A-L inclusive = calls
            // [8]M-X inclusive = puts

            const int INDEXOF_STRIKE = 2;
            const int CALL_EXPIRY_MONTH_UPPERCASE_OFFSET = 1 - 'A';
            const int CALL_EXPIRY_MONTH_LOWERCASE_OFFSET = 1 - 'a';
            const int PUT_EXPIRY_MONTH_UPPERCASE_OFFSET = 1 - 'M';
            const int PUT_EXPIRY_MONTH_LOWERCASE_OFFSET = 1 - 'm';

            var baseOffset = 1;
            var isShortTermExpiry = false;
            var lastCode = ticker[^baseOffset];
            int specialCodesLength = 3;

            if (lastCode >= 'A' && lastCode <= 'D')
            {
                ++baseOffset;
                ++specialCodesLength;
                isShortTermExpiry = true;
            }

            var expiryYearIndexOffset = baseOffset;
            var expiryMonthIndexOffset = expiryYearIndexOffset + 1;
            var marginTypeIndexOffset = expiryMonthIndexOffset + 1;
            OptionTypes optionType;
            int month;

            var yearChar = ticker[^expiryYearIndexOffset];
            var year = FuturesExpiryHelper.GetYear(yearChar);

            #region Month and OptionType
            var monthChar = ticker[^expiryMonthIndexOffset];

            if (monthChar >= 'A' && monthChar <= 'X')
            {
                if (monthChar <= 'L')
                {
                    optionType = OptionTypes.Call;
                    month = monthChar + CALL_EXPIRY_MONTH_UPPERCASE_OFFSET;
                }
                else
                {
                    optionType = OptionTypes.Put;
                    month = monthChar + PUT_EXPIRY_MONTH_UPPERCASE_OFFSET;
                }
            }
            else if (monthChar >= 'a' && monthChar <= 'x')
            {
                if (monthChar <= 'l')
                {
                    optionType = OptionTypes.Call;
                    month = monthChar + CALL_EXPIRY_MONTH_LOWERCASE_OFFSET;
                }
                else
                {
                    optionType = OptionTypes.Put;
                    month = monthChar + PUT_EXPIRY_MONTH_LOWERCASE_OFFSET;
                }
            }
            else
            {
                throw new Exception($"Could not resolve option type from ticker {ticker}");
            }
            #endregion

            var strikeDataBuffer = ticker.AsSpan(INDEXOF_STRIKE, ticker.Length - (INDEXOF_STRIKE + specialCodesLength));

            return new OptionDescription
            {
                Ticker = ticker,
                ExpiryDate = new DateTime(year, month, 1),
                IsShortTermExpiry = isShortTermExpiry,
                OptionType = optionType,
                Strike = Decimal5.Parse(strikeDataBuffer)
            };
        }
        public static FuturesDescription InferFuturesFromTicker(string ticker)
        {
            static Exception getException(string subj)
            {
                return new Exception($"Cannot infer expiry date from ticker {subj}. Format is invalid");
            }

            var buffer = ticker.AsSpan();
            var formatDefiningChar = buffer[^2];

            var isShortFormat = formatDefiningChar switch
            {
                >  '0' and <  '9' => false,
                >= 'F' and <= 'Z' => true,
                                _ => throw getException(ticker)
            };

            var yearCode = buffer[^1];
            var year = FuturesExpiryHelper.GetYear(yearCode);

            if (isShortFormat)
            {
                var month = FuturesExpiryHelper.GetMonth(formatDefiningChar);

                return new FuturesDescription
                {
                    Ticker = ticker,
                    ExpiryDate = new DateTime(year, month, 1),
                };
            }
            else
            {
                // 0|1|2|3|4|5|6|7
                // B|R|-|0|6|.|2|3
                var monthValueSpan = ticker.AsSpan(3, 2);
                var month = FuturesExpiryHelper.GetMonth(monthValueSpan);

                return new FuturesDescription
                {
                    Ticker = ticker,
                    ExpiryDate = new DateTime(year, month, 1),
                };
            }
        }
        public static CalendarSpreadDescription InferSpreadFromTicker(string ticker)
        {
            // 0|1|2|3|4|5|6|7|
            // B|R|K|3|B|R|M|3| 8 chars

            if (ticker.Length != 8)
            {
                throw new ArgumentException($"Cannot infer calendar spread data from {ticker}. Invalid format");
            }

            const int MONTH_CODE_INDEX = 2;
            const int  YEAR_CODE_INDEX = 3;

            var month = FuturesExpiryHelper.GetMonth(ticker[MONTH_CODE_INDEX]);
            var  year = FuturesExpiryHelper.GetYear (ticker[ YEAR_CODE_INDEX]);

            return new CalendarSpreadDescription
            {
                Ticker = ticker,
                ExpiryDate = new DateTime(year, month, 1)
            };
        }

        public static string PrintQuikParameters(Type t)
        {
#if !DEBUG
            throw new InvalidOperationException("Do not call this method in production!");
#endif
            var sb = new StringBuilder(2048);

            sb.Append(t.Name);
            sb.AppendLine(" Data:");

            foreach (var p in t.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (p.TryGetAttributeValue(out QuikCallbackFieldAttribute? attribute))
                {
                    sb.Append(p.Name);
                    sb.Append(" (");
                    sb.Append(attribute.Parameter);
                    sb.Append("): ");
                    sb.AppendLine(p.GetValue(null)?.ToString() ?? "null");
                }
            }

            return sb.ToString();
        }

        public static unsafe long AsciiToLong(byte* asciiBuffer, int symbolNum)
        {
            if (symbolNum == 0)
            {
                return 0;
            }

            var unicodeBuffer = stackalloc byte[symbolNum * 2];
            var unicodeBufferSpan = new Span<char>(unicodeBuffer, symbolNum);

            // translating ascii to unicode
            for (int i = 0; i < symbolNum; i++)
            {
                unicodeBuffer[i * 2] = asciiBuffer[i];
            }

            return long.Parse(unicodeBufferSpan, NumberStyles.Integer | NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }
    }
}
