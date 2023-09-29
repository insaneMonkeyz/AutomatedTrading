using System;

namespace Quik.Entities
{
    public readonly struct MoexDate
    {
        public readonly int Day;
        public readonly int Month;
        public readonly int Year;
        public readonly DateTime Date;

        private MoexDate(uint value) : this()
        {
            Day = GetDay(value);
            Month = GetMonth(value);
            Year = GetYear(value);

            Date = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Local);
        }

        public static int GetYear(uint value)
        {
            return (int)value / 10_000;
        }
        public static int GetMonth(uint value)
        {
            return (int)value / 100 % 100;
        }
        public static int GetDay(uint value)
        {
            return (int)value % 100;
        }
        public static DateTime GetDateTime(uint value)
        {
            return (new MoexDate(value)).Date;
        }

        public static implicit operator MoexDate(uint value)
        {
            return new MoexDate(value);
        }
    }
}
