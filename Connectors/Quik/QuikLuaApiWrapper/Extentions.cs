using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoexCommonTypes;
using QuikLuaApi;

namespace QuikLuaApiWrapper
{
    internal static class Extentions
    {
        public static bool TryConvertToMoexExpiry(this string date, out DateTimeOffset result)
        {
            result = default;

            if (uint.TryParse(date, out uint value))
            {
                result = new DateTimeOffset(
                   dateTime: new MoexDate(value).Date + MoexSpecifics.CommonExpiryTime, 
                     offset: MoexSpecifics.MoscowUtcOffset);

                return true;
            }

            return false;
        }
    }
}
