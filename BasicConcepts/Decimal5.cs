using System;
using System.Globalization;

namespace BasicConcepts
{
    public struct Decimal5 : IComparable
    {
        public static readonly Decimal5 MAX_VALUE = new (MANTISSA_MAX_VALUE);
        public static readonly Decimal5 MIN_VALUE = new (MANTISSA_MIN_VALUE);

        private const long EXPONENT = 5;
        private const long DECIMAL_EXPONENT = EXPONENT << 16;
        private const long MULTIPLIER = 100_000;
        private const long DIVIDER = MULTIPLIER * MULTIPLIER;
        private const long MANTISSA_MAX_VALUE = 9_999_999_999_999_999;
        private const long MANTISSA_MIN_VALUE = -9_999_999_999_999_999;

        private const long INTEGER_MAX_VALUE = 100_000_000_000;
        private const long INTEGER_MIN_VALUE = -100_000_000_000;

        private const ulong DECIMAL_EXPONENT_MASK = 0x000F0000;
        private const ulong DECIMAL_SIGN_MASK = 0x80000000;

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
            if (value >= INTEGER_MAX_VALUE)
            {
                mantissa = MANTISSA_MAX_VALUE;
                return;
            }
            if (value <= INTEGER_MIN_VALUE)
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

        public string ToString(uint precision)
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

                    if (bufferPos == nextSpacePos && rest > 0)
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

        public static bool TryParse(string? value, out Decimal5 result)
        {
            result = default;

            if (value == null)
            {
                return false;
            }

            int sign = 1;
            int decimalLengthDeviation = default;
            bool isSeparated = default;
            long mantissa = default;

            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case ' ':
                        break;

                    case '\t':
                        if (mantissa != 0)
                            return false;
                        break;

                    case '-':
                        if (mantissa > 0)
                            return false;
                        else 
                            sign = -1;
                        break;

                    case '0':
                        mantissa *= 10;
                        break;

                    case '1':
                        mantissa = mantissa * 10 + 1;
                        break;

                    case '2':
                        mantissa = mantissa * 10 + 2;
                        break;

                    case '3':
                        mantissa = mantissa * 10 + 3;
                        break;

                    case '4':
                        mantissa = mantissa * 10 + 4;
                        break;

                    case '5':
                        mantissa = mantissa * 10 + 5;
                        break;

                    case '6':
                        mantissa = mantissa * 10 + 6;
                        break;

                    case '7':
                        mantissa = mantissa * 10 + 7;
                        break;

                    case '8':
                        mantissa = mantissa * 10 + 8;
                        break;

                    case '9':
                        mantissa = mantissa * 10 + 9;
                        break;

                    case '.':
                        if (isSeparated) return false;
                        decimalLengthDeviation = value.Length - i - 6;
                        isSeparated = true;
                        break;

                    case ',':
                        if (isSeparated) return false;
                        decimalLengthDeviation = value.Length - i - 6;
                        isSeparated = true;
                        break;

                    default:
                        return false;
                }
            }

            if (decimalLengthDeviation > 0)
            {
                var pow = (long)pow10[decimalLengthDeviation];
                var round = mantissa % pow;

                mantissa += round >= (long)roundToNearest[decimalLengthDeviation]
                         ? pow - round
                         : 0;
                mantissa /= pow;
            }
            else if(decimalLengthDeviation < 0)
            {
                mantissa *= (long)pow10[decimalLengthDeviation * -1];
            }
            else if(!isSeparated)
            {
                mantissa *= MULTIPLIER;
            }

            result = new Decimal5() { mantissa = mantissa * sign };

            return !(mantissa > MANTISSA_MAX_VALUE || mantissa < MANTISSA_MIN_VALUE);
        }
        public static Decimal5 Parse(string value)
        {
            const string EXCEPTION_MSG = "Input string was not in a correct format.";

            int sign = 1;
            int decimalLengthDeviation = default;
            bool isSeparated = default;
            long mantissa = default;

            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case ' ':
                        break;

                    case '\t':
                        if (mantissa != 0)
                            throw new FormatException(EXCEPTION_MSG);
                        break;

                    case '-':
                        if (mantissa > 0)
                            throw new FormatException(EXCEPTION_MSG);
                        else
                            sign = -1;
                        break;

                    case '0':
                        mantissa *= 10;
                        break;

                    case '1':
                        mantissa = mantissa * 10 + 1;
                        break;

                    case '2':
                        mantissa = mantissa * 10 + 2;
                        break;

                    case '3':
                        mantissa = mantissa * 10 + 3;
                        break;

                    case '4':
                        mantissa = mantissa * 10 + 4;
                        break;

                    case '5':
                        mantissa = mantissa * 10 + 5;
                        break;

                    case '6':
                        mantissa = mantissa * 10 + 6;
                        break;

                    case '7':
                        mantissa = mantissa * 10 + 7;
                        break;

                    case '8':
                        mantissa = mantissa * 10 + 8;
                        break;

                    case '9':
                        mantissa = mantissa * 10 + 9;
                        break;

                    case '.':
                        if (isSeparated) throw new FormatException(EXCEPTION_MSG);;
                        decimalLengthDeviation = value.Length - i - 6;
                        isSeparated = true;
                        break;

                    case ',':
                        if (isSeparated) throw new FormatException(EXCEPTION_MSG);;
                        decimalLengthDeviation = value.Length - i - 6;
                        isSeparated = true;
                        break;

                    default:
                        throw new FormatException(EXCEPTION_MSG);;
                }
            }

            if (decimalLengthDeviation > 0)
            {
                var pow = (long)pow10[decimalLengthDeviation];
                var round = mantissa % pow;

                mantissa += round >= (long)roundToNearest[decimalLengthDeviation]
                         ? pow - round
                         : 0;
                mantissa /= pow;
            }
            else if (decimalLengthDeviation < 0)
            {
                mantissa *= (long)pow10[decimalLengthDeviation * -1];
            }
            else if (!isSeparated)
            {
                mantissa *= MULTIPLIER;
            }

            if(mantissa > MANTISSA_MAX_VALUE || mantissa < MANTISSA_MIN_VALUE)
            {
                throw new FormatException(EXCEPTION_MSG);;
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
            return 1337 + mantissa.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return mantissa.CompareTo(((Decimal5)obj).mantissa);
        }
    }
}
