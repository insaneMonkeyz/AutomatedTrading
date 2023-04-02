using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TradingConcepts
{
    public struct Decimal5 : IComparable
    {
        public static readonly Decimal5 MAX_VALUE = new() { mantissa = MANTISSA_MAX_VALUE };
        public static readonly Decimal5 MIN_VALUE = new() { mantissa = MANTISSA_MIN_VALUE };

        private const long POSITIVE = 1;
        private const long NEGATIVE = -1;
        private const long EXPONENT = 5;
        private const long DECIMAL_NUMERAL_SYSTEM_BASE = 10;
        private const long DECIMAL_EXPONENT = EXPONENT << 16;
        private const long MULTIPLIER = 100_000;
        private const long DIVIDER = MULTIPLIER * MULTIPLIER;
        private const long MANTISSA_MAX_VALUE =  9_999_999_999_999_999;
        private const long MANTISSA_MIN_VALUE = -9_999_999_999_999_999;

        private const long INTEGER_MAX_VALUE =  99_999_999_999;
        private const long INTEGER_MIN_VALUE = -99_999_999_999;

        private const ulong DECIMAL_EXPONENT_MASK = 0x000F0000;
        private const ulong DECIMAL_SIGN_MASK     = 0x80000000;

        private const long ZERO_CHAR_OFFSET = '0';
        private const  int MAX_STRING_LEN = 21;

        private static readonly ulong[] pow10
                          = new ulong[16]
                          {
                                  1, 10, 100, 1000, 10_000, 100_000, 1_000_000,
                                  10_000_000, 100_000_000, 1_000_000_000,
                                  10_000_000_000, 100_000_000_000,
                                  1_000_000_000_000, 10_000_000_000_000,
                                  100_000_000_000_000, 1_000_000_000_000_000
                          };
        private static readonly ulong[] roundToNearest
                          = new ulong[16]
                          {
                                  1, 5, 50, 500, 5_000, 50_000, 500_000,
                                  5_000_000, 50_000_000, 500_000_000,
                                  5_000_000_000, 50_000_000_000,
                                  500_000_000_000, 5_000_000_000_000,
                                  50_000_000_000_000, 500_000_000_000_000,
                              //5_000_000_000_000_000
                          };

        public  long InternalValue => mantissa;
        private long mantissa;

        public Decimal5(int value)
        {
            mantissa = value * MULTIPLIER;
        }
        public Decimal5(double value)
        {
            mantissa = (long)(value * MULTIPLIER);
        }
        public Decimal5(long value)
        {
            if (value > INTEGER_MAX_VALUE)
            {
                mantissa = MANTISSA_MAX_VALUE;
                return;
            }
            if (value < INTEGER_MIN_VALUE)
            {
                mantissa = MANTISSA_MIN_VALUE;
                return;
            }

            mantissa = value * MULTIPLIER;
        }
        public Decimal5(uint value)
        {
            mantissa = value * MULTIPLIER;
        }
        public Decimal5(ulong value)
        {
            if (value < INTEGER_MAX_VALUE)
                mantissa = (long)value * MULTIPLIER;
            else
                mantissa = MANTISSA_MAX_VALUE;
        }
        public Decimal5(decimal value)
        {
            unsafe
            {
                ulong* pVal = (ulong*)&value;

                if ((pVal[0] & 0xFFFFFFFF_00000000) > 0 ||
                     pVal[1] > MANTISSA_MAX_VALUE)
                {
                    mantissa = ((*pVal & DECIMAL_SIGN_MASK) == DECIMAL_SIGN_MASK)
                                ? MANTISSA_MIN_VALUE
                                : MANTISSA_MAX_VALUE;
                    return;
                }

                //   получаем разницу экспонент данного decimal числа 
                //   и константной экспоненты типа Decimal5

                long dExp = (long)((*pVal & DECIMAL_EXPONENT_MASK) >> 16) - EXPONENT;

                //  Здесь будет проводиться округление до ближайшего

                if (dExp > 0)
                {
                    bool doIncrement = pVal[1] % pow10[dExp] >= roundToNearest[dExp];

                    pVal[1] /= pow10[dExp];

                    //  если doIncrement = true, по ссылке указателя
                    //  будет записано значение 0x0000_0001

                    pVal[1] += *(uint*)&doIncrement;

                    //  На данном этапе значения приведены к одинаковым порядкам и 
                    //  разницы в экспонентах больше нет

                    dExp = 0;
                }

                mantissa = (long)(pVal[1] * pow10[-dExp]);

                if ((*pVal & DECIMAL_SIGN_MASK) == DECIMAL_SIGN_MASK) mantissa = -mantissa;
            }
        }


        // TYPE CONVERSIONS 

        public static implicit operator Decimal5(decimal obj)
        {
            return new Decimal5(obj);
        }
        public static implicit operator Decimal5(double obj)
        {
            return new Decimal5(obj);
        }
        public static implicit operator Decimal5(long obj)
        {
            return new Decimal5(obj);
        }
        public static implicit operator Decimal5(int obj)
        {
            return new Decimal5(obj);
        }
        public static implicit operator Decimal5(uint obj)
        {
            return new Decimal5(obj);
        }
        public static implicit operator Decimal5(ulong obj)
        {
            return new Decimal5(obj);
        }

        public static implicit operator decimal(Decimal5 obj)
        {
            decimal res = default;

            unsafe
            {
                var pRes = (ulong*)&res;

                *pRes = *pRes | DECIMAL_EXPONENT;

                if (obj.mantissa < 0L)
                {
                    *pRes = *pRes | DECIMAL_SIGN_MASK;
                    *(long*)(pRes + 1) = -obj.mantissa;
                }
                else
                    *(long*)(pRes + 1) = obj.mantissa;
            }

            return res;
        }
        public static implicit operator  double(Decimal5 obj)
        {
            return (double)obj.mantissa / MULTIPLIER;
        }
        public static implicit operator    long(Decimal5 obj)
        {
            return obj.mantissa / MULTIPLIER;
        }

        // COMPARATIONS 

        public static bool operator  <(Decimal5 val1, Decimal5 val2)
        {
            return val2.mantissa > val1.mantissa;
        }
        public static bool operator  >(Decimal5 val1, Decimal5 val2)
        {
            return val1.mantissa > val2.mantissa;
        }
        public static bool operator <=(Decimal5 val1, Decimal5 val2)
        {
            return val1.mantissa <= val2.mantissa;
        }
        public static bool operator >=(Decimal5 val1, Decimal5 val2)
        {
            return val1.mantissa >= val2.mantissa;
        }
        public static bool operator ==(Decimal5 val1, Decimal5 val2)
        {
            return val1.mantissa == val2.mantissa;
        }
        public static bool operator !=(Decimal5 val1, Decimal5 val2)
        {
            return val1.mantissa != val2.mantissa;
        }

        // MATHEMATICAL OPERATIONS 

        public static Decimal5 operator ++(Decimal5 val1)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa + MULTIPLIER
            };
        }
        public static Decimal5 operator +(Decimal5 val1, Decimal5 val2)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa + val2.mantissa
            };
        }
        public static  decimal operator +(Decimal5 val1, decimal val2)
        {
            return (decimal)val1 + val2;
        }
        public static  decimal operator +(decimal val1, Decimal5 val2)
        {
            return val1 + (decimal)val2;
        }
        public static     long operator +(Decimal5 val1, long val2)
        {
            return (long)val1 + val2;
        }
        public static     long operator +(long val1, Decimal5 val2)
        {
            return (long)val2 + val1;
        }

        public static Decimal5 operator  -(Decimal5 val1)
        {
            return new Decimal5
            {
                mantissa = -val1.mantissa
            };
        }
        public static Decimal5 operator --(Decimal5 val1)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa - MULTIPLIER
            };
        }
        public static Decimal5 operator  -(Decimal5 val1, Decimal5 val2)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa - val2.mantissa
            };
        }
        public static   decimal operator -(Decimal5 val1, decimal val2)
        {
            return (decimal)val1 - val2;
        }
        public static   decimal operator -(decimal val1, Decimal5 val2)
        {
            return val1 - (decimal)val2;
        }
        public static      long operator -(Decimal5 val1, long val2)
        {
            return (long)val1 - val2;
        }
        public static      long operator -(long val1, Decimal5 val2)
        {
            return val1 - (long)val2;
        }

        public static Decimal5 operator *(Decimal5 val1, Decimal5 val2)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa * val2.mantissa / MULTIPLIER
            };
        }
        public static  decimal operator *(Decimal5 val1, decimal val2)
        {
            return (decimal)val1 * val2;
        }
        public static  decimal operator *(decimal val1, Decimal5 val2)
        {
            return val1 * (decimal)val2;
        }
        public static     long operator *(Decimal5 val1, long val2)
        {
            return val2 * val1.mantissa / MULTIPLIER;
        }
        public static     long operator *(long val1, Decimal5 val2)
        {
            return val1 * val2.mantissa / MULTIPLIER;
        }

        public static Decimal5 operator /(Decimal5 val1, Decimal5 val2)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa / val2.mantissa * MULTIPLIER
            };
        }
        public static Decimal5 operator /(Decimal5 val1, long val2)
        {
            return new Decimal5
            {
                mantissa = val1.mantissa / val2
            };
        }
        public static Decimal5 operator /(long val1, Decimal5 val2)
        {
            return new Decimal5
            {
                mantissa = val1 * DIVIDER / val2.mantissa
            };
        }
        public static  decimal operator /(Decimal5 val1, decimal val2)
        {
            return (decimal)val1 / val2;
        }
        public static  decimal operator /(decimal val1, Decimal5 val2)
        {
            return val1 / (decimal)val2;
        }

        public string ToString(uint precision, bool separateThousands = false)
        {
            unsafe
            {
                const long FIRST_SPACE_POS = 3;
                const long COMMA_POS  = 5;

                precision = Math.Min(5, precision);

                var isNeg = mantissa < 0;
                var end = COMMA_POS - precision;
                var rest = Math.Abs(mantissa);
                var digit = 0L;
                var buffer = stackalloc char[MAX_STRING_LEN];
                var bufferPos = buffer + MAX_STRING_LEN;
                var commaPos = bufferPos - precision;
                var nextSpacePos = commaPos - FIRST_SPACE_POS - (precision > 0 ? 1 : 0);
                var pow = (long)pow10[end];
                var round = rest % pow;

                rest += round >= (long)roundToNearest[end]
                    ? pow - round
                    : 0;

                // decrease the number so that the last digit would match given precision
                rest /= (long)pow10[end];

                while (bufferPos > commaPos)
                {
                    digit = rest % 10;
                    rest /= 10;

                    bufferPos--;
                    *bufferPos = (char)(digit + ZERO_CHAR_OFFSET);
                }

                if (precision > 0)
                {
                    bufferPos--;
                   *bufferPos = CultureInfo
                        .CurrentCulture
                        .NumberFormat
                        .NumberDecimalSeparator[0];
                }

                //if the whole number after the decimal separator is 0
                do
                {
                    digit = rest % 10;
                    rest /= 10;

                    bufferPos--;

                   *bufferPos = digit > 0 
                        ? (char)(digit + ZERO_CHAR_OFFSET) 
                        : '0';

                    if (separateThousands && bufferPos == nextSpacePos && rest > 0)
                    {
                        bufferPos--;
                        nextSpacePos -= 4;
                        *bufferPos = ' ';
                    }
                }
                while (rest > 0);

                if (isNeg)
                {
                    bufferPos--;
                   *bufferPos = '-';
                }

                return new string(bufferPos, 0, MAX_STRING_LEN - (int)(bufferPos-buffer));
            }
        }
        public override string ToString()
        {
            unsafe
            {
                var rest = Math.Abs(mantissa);
                var digit = 0L;
                var buffer = stackalloc char[MAX_STRING_LEN];
                var bufferPos = buffer + MAX_STRING_LEN;
                var commaPos = bufferPos - 5;

                while (bufferPos > commaPos)
                {
                    digit = rest % 10;
                    rest /= 10;

                    bufferPos--;
                    *bufferPos = (char)(digit + ZERO_CHAR_OFFSET);
                }

                bufferPos--;
               *bufferPos = CultureInfo
                        .CurrentCulture
                        .NumberFormat
                        .NumberDecimalSeparator[0];                

                do
                {
                    digit = rest % 10;
                    rest /= 10;

                    bufferPos--;

                   *bufferPos = digit > 0 
                        ? (char)(digit + ZERO_CHAR_OFFSET) 
                        : '0';
                }
                while (rest > 0);

                if (mantissa < 0)
                {
                    bufferPos--;
                    *bufferPos = '-';
                }

                return new string(bufferPos, 0, MAX_STRING_LEN - (int)(bufferPos - buffer));
            }
        }

        public static bool TryParse(nint pointer, out Decimal5 result)
        {
            result = ParseInternal(pointer, out Exception? e);

            return e is null;
        }
        public static Decimal5 Parse(nint pointer)
        {
            var result = ParseInternal(pointer, out Exception? e);

            return e is null ? result : throw e;
        }
        public static bool TryParse(string? value, out Decimal5 result)
        {
            result = ParseInternal(value, out Exception? e);

            return e is null;
        }
        public static Decimal5 Parse(string? value)
        {
            var result = ParseInternal(value, out Exception? e);

            return e is null ? result : throw e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal5 ParseInternal(string? value, out Exception? exception)
        {
            if (value is null)
            {
                exception = new ArgumentNullException(nameof(value));
                return default;
            }

            static FormatException getException(string subj)
            {
                return new FormatException($"Cannot parse Decimal5 value from string '{subj}'");
            }

            exception = default;

            const long NOT_SEPARATED = 0;
            const long     SEPARATED = 1;

            long sign = POSITIVE;
            long decimalPartIncrement = NOT_SEPARATED;
            long decimalPartLenght = 0;
            long mantissa = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c is >= '0' and <= '9')
                {
                    mantissa = mantissa * DECIMAL_NUMERAL_SYSTEM_BASE + (c - ZERO_CHAR_OFFSET);

                    decimalPartLenght += decimalPartIncrement;

                    continue;
                }

                switch (c)
                {
                    case ' ': break;

                    case '\t':
                        {
                            if (mantissa != 0)
                            {
                                exception = getException(value);
                                return default;
                            }
                            break;
                        }

                    case '+':
                        {
                            if (mantissa > 0 || sign == NEGATIVE)
                            {
                                exception = getException(value);
                                return default;
                            }

                            break;
                        }

                    case '-':
                        {
                            if (mantissa > 0)
                            {
                                exception = getException(value);
                                return default;
                            }
                            else
                            {
                                sign = NEGATIVE;
                            }

                            break; 
                        }

                    case '.':
                    case ',':
                        {
                            if (decimalPartIncrement == SEPARATED)
                            {
                                exception = getException(value);
                                return default;
                            }

                            decimalPartIncrement = SEPARATED;

                            break; 
                        }

                    default:
                        exception = getException(value);
                        return default;
                }
            }

            if (decimalPartLenght > 0)
            {
                var roundexp = decimalPartLenght - EXPONENT;

                // when the number has more than 5 decimal digits
                if (roundexp > 0)
                {
                    var roundmultiplier = (long)pow10[roundexp];
                    var rest = mantissa % roundmultiplier;
                    var middlevalue = (long)roundToNearest[roundexp];
                    var round = roundmultiplier - rest;

                    // mantissa is 199
                    // rest is 99
                    // middlevalue is 50
                    // round is 1
                    if (middlevalue >= round)
                    {
                        // add missing bit to round to the nearest greater number
                        mantissa += round;
                    }
                    else
                    {
                        // discard rest to floor to the nearest
                        mantissa -= rest;
                    }

                    mantissa /= roundmultiplier;
                }
                else
                {
                    mantissa *= (long)pow10[roundexp * NEGATIVE];
                }
            }
            else
            {
                mantissa *= MULTIPLIER;
            }

            if (mantissa is > MANTISSA_MAX_VALUE or < MANTISSA_MIN_VALUE)
            {
                exception = new ArgumentOutOfRangeException($"Value {value} is too big to be represented as {nameof(Decimal5)}");
                return default;
            }

            return new Decimal5() { mantissa = mantissa * sign };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal5 ParseInternal(nint pValue, out Exception? exception)
        {
            if (pValue == default)
            {
                exception = new ArgumentException("Pointer is not set");
                return default;
            }

            static FormatException getException()
            {
                return new FormatException($"Cannot parse Decimal5 value from string at a given address. Format is incorrect");
            }

            exception = default;

            const long NOT_SEPARATED = 0;
            const long SEPARATED = 1;

            long sign = POSITIVE;
            long decimalPartIncrement = NOT_SEPARATED;
            long decimalPartLenght = 0;
            long mantissa = 0;

            unsafe
            {
                int i = 0;
                char* pChar = (char*)pValue;
                char c = pChar[i];

                while (c != '\0')
                {
                    if (c is >= '0' and <= '9')
                    {
                        mantissa = mantissa * DECIMAL_NUMERAL_SYSTEM_BASE + (c - ZERO_CHAR_OFFSET);

                        decimalPartLenght += decimalPartIncrement;
                    }
                    else
                    {
                        switch (c)
                        {
                            case ' ': break;

                            case '\t':
                                {
                                    if (mantissa != 0)
                                    {
                                        exception = getException();
                                        return default;
                                    }
                                    break;
                                }

                            case '+':
                                {
                                    if (mantissa > 0 || sign == NEGATIVE)
                                    {
                                        exception = getException();
                                        return default;
                                    }

                                    break;
                                }

                            case '-':
                                {
                                    if (mantissa > 0)
                                    {
                                        exception = getException();
                                        return default;
                                    }
                                    else
                                    {
                                        sign = NEGATIVE;
                                    }

                                    break;
                                }

                            case '.':
                            case ',':
                                {
                                    if (decimalPartIncrement == SEPARATED)
                                    {
                                        exception = getException();
                                        return default;
                                    }

                                    decimalPartIncrement = SEPARATED;

                                    break;
                                }

                            default:
                                exception = getException();
                                return default;
                        } 
                    }

                    ++i;
                    c = pChar[i];
                }
            }

            if (decimalPartLenght > 0)
            {
                var roundexp = decimalPartLenght - EXPONENT;

                // when the number has more than 5 decimal digits
                if (roundexp > 0)
                {
                    var roundmultiplier = (long)pow10[roundexp];
                    var rest = mantissa % roundmultiplier;
                    var middlevalue = (long)roundToNearest[roundexp];
                    var round = roundmultiplier - rest;

                    // mantissa is 199
                    // rest is 99
                    // middlevalue is 50
                    // round is 1
                    if (middlevalue >= round)
                    {
                        // add missing bit to round to the nearest greater number
                        mantissa += round;
                    }
                    else
                    {
                        // discard rest to floor to the nearest
                        mantissa -= rest;
                    }

                    mantissa /= roundmultiplier;
                }
                else
                {
                    mantissa *= (long)pow10[roundexp * NEGATIVE];
                }
            }
            else
            {
                mantissa *= MULTIPLIER;
            }

            if (mantissa is > MANTISSA_MAX_VALUE or < MANTISSA_MIN_VALUE)
            {
                var attemptValue = ((decimal)mantissa) / MULTIPLIER;
                exception = new ArgumentOutOfRangeException($"Attempted to parse a number {attemptValue:0.###############} that is too big to be represented as {nameof(Decimal5)}");
                return default;
            }

            return new Decimal5() { mantissa = mantissa * sign };
        }

        public override bool Equals(object? obj)
        {
            return obj is Decimal5 decimal5 && 
                   decimal5.mantissa == mantissa;
        }
        public override  int GetHashCode()
        {
            return 652132164 ^ mantissa.GetHashCode();
        }

        public int CompareTo(object? obj)
        {
            return obj is Decimal5 d
                ? (int)(mantissa - d.mantissa)
                : -1;
        }
    }
}
