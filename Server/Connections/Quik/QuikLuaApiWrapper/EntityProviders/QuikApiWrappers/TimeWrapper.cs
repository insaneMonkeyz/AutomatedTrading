using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.Lua;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal static class TimeWrapper
    {
        private const string MICROSECONDS = "mcs";
        private const string MILLISECONDS = "ms";
        private const string SECONDS = "sec";
        private const string MINUTES = "min";
        private const string HOURS = "hour";
        private const string DAYS = "day";
        private const string MONTHS = "month";
        private const string YEARS = "year";

        private static readonly TimeSpan MoscowUtcOffset = TimeSpan.FromHours(3);

        public static DateTimeOffset? GetTime(LuaWrap state, string column)
        {
            if (state.PushColumnValueTable(column))
            {
                var result = new DateTimeOffset(
                    (int)state.ReadRowValueInteger(YEARS),
                    (int)state.ReadRowValueInteger(MONTHS),
                    (int)state.ReadRowValueInteger(DAYS),
                    (int)state.ReadRowValueInteger(HOURS),
                    (int)state.ReadRowValueInteger(MINUTES),
                    (int)state.ReadRowValueInteger(SECONDS),
                    (int)state.ReadRowValueInteger(MILLISECONDS),
                     MoscowUtcOffset);

                state.PopFromStack();

                return result;
            }

            return default;
        }
    }
}
