using System.Text;

namespace Quik;

internal static class FuturesExpiryHelper
{
    private static readonly int[] _futuresExpiryMonthCodes = new int['Z' + 1];

    public static int GetMonth(ReadOnlySpan<char> buffer)
    {
        var first = buffer[^2] -'0';
        var second = buffer[^1] - '0';

        return (first == 0 || first == 1) && (second > 0 && second < 13)
            ? first * 10 + second
            : throw new ArgumentException($"Cannot parse month value from {buffer}");
    }
    public static int GetMonth(char code)
    {
        if (code < 'F' || code > 'Z')
        {
            throw new ArgumentException($"Invalid month code '{code}'");
        }

        int value = _futuresExpiryMonthCodes[(int)code];

        return value != default
            ? value
            : throw new ArgumentException($"Invalid month code '{code}'");
    }
    public static int GetMonth(FuturesExpiryCodes code) => GetMonth((char)code);
    public static int GetYear(char code)
    {
        const int STARTING_EXPIRY_YEAR_VALUE = 2020 - '0';
        const int DECADE = 10;

        int year;

        if (code < '0' || code > '9')
        {
            throw new ArgumentException($"Char '{code}' does not represent a year");
        }

        year = code + STARTING_EXPIRY_YEAR_VALUE;

        if (DateTime.Now.Year > year)
        {
            // moex option tickers provide only last digit of their expiry year
            // if it is less than the last digit of the current year
            // then we're dealing with an option expiring in the next decade
            // example:
            //           current year: 202[3]
            //     provided year code: ...[1]
            // since we don't support archived options, we will assume that
            // expiry code [1] designates year 2031

            year += DECADE;
        }

        return year;
    }

    static FuturesExpiryHelper()
    {
        int month = 1;

        foreach (var code in Enum.GetValues<FuturesExpiryCodes>())
        {
            _futuresExpiryMonthCodes[(int)code] = month++;
        }
    }
}
