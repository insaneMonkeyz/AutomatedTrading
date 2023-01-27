using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quik.EntityDataProviders.QuikApiWrappers
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

        public static DateTimeOffset? GetTime(LuaState state, string column)
        {
            if (state.PushColumnValueTable(column))
            {
                var result = new DateTimeOffset(
                    (int)state.ReadRowValueLong(YEARS),
                    (int)state.ReadRowValueLong(MONTHS),
                    (int)state.ReadRowValueLong(DAYS),
                    (int)state.ReadRowValueLong(HOURS),
                    (int)state.ReadRowValueLong(MINUTES),
                    (int)state.ReadRowValueLong(SECONDS),
                    (int)state.ReadRowValueLong(MILLISECONDS),
                     MoscowUtcOffset);

                state.PopFromStack();

                return result;
            }

            return default;
        }
    }
}
