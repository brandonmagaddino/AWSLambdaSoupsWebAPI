using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSLambdaSoupsWebAPI.Utilities
{
    public static class TimeFormatter
    {
        public static string FormatSlackTime(this TimeSpan timeSpan)
        {
            var str = string.Format("{0}{1}{2}",
                timeSpan.Days > 0       ? timeSpan.Days + " Days "      : string.Empty,
                timeSpan.Hours > 0      ? timeSpan.Hours + " Hours "     : string.Empty,
                timeSpan.Minutes > 0    ? timeSpan.Minutes + " Minutes " : string.Empty);

            if (str == string.Empty)
                return "just now";

            return str + "ago";
        }
    }
}
