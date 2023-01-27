using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtention
    {
        public static int CalculateAge(this DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var _today = today.ToDateTime(TimeOnly.MinValue);
            var _dob = dob.ToDateTime(TimeOnly.MinValue);

            var days = (_today - _dob).TotalDays;

            var years = (int)(days / 365.242199);

            return years;
        }
    }
}