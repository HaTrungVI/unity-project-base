using System;

namespace ProjectBase.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] SizeSuffixes = { "", "K", "M", "B", "T" };

        public static string FormatNumber(this long number)
        {
            if (number < 1000) return number.ToString();

            int tier = 0;
            double value = number;
            while (value >= 1000 && tier < SizeSuffixes.Length - 1)
            {
                value /= 1000;
                tier++;
            }

            return value >= 100 ? $"{value:F0}{SizeSuffixes[tier]}"
                 : value >= 10 ? $"{value:F1}{SizeSuffixes[tier]}"
                 : $"{value:F2}{SizeSuffixes[tier]}";
        }

        public static string FormatNumber(this int number) => ((long)number).FormatNumber();

        public static string FormatTimeSpan(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours:D2}h";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes:D2}m";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds:D2}s";
            return $"{timeSpan.Seconds}s";
        }

        public static string FormatTimeSpan(this float totalSeconds)
        {
            return TimeSpan.FromSeconds(totalSeconds).FormatTimeSpan();
        }

        public static string FormatTimeSpan(this double totalSeconds)
        {
            return TimeSpan.FromSeconds(totalSeconds).FormatTimeSpan();
        }
    }
}
