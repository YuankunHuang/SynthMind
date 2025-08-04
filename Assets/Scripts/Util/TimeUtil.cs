using System.Collections;
using System.Collections.Generic;
using System;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// @ingroup Utility
    /// @class TimeUtil
    /// @brief A utility class for performing general time-related operations.
    /// </summary>
    public static class TimeUtil
    {
        /// <summary>
        /// Retrieves the name of the month for a given month number.
        /// </summary>
        /// <param name="month">The month number (1 for January, 2 for February, etc.).</param>
        /// <returns>The name of the month as a string. If the month number is invalid, an empty string is returned.</returns>
        public static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    LogHelper.LogError($"Invalid month: {month}");
                    return "";
            }
        }
    }
}