using System;
using System.Text.RegularExpressions;

namespace BotScaffold
{
    public static class RegexHelper
    {
        public static readonly string VALUE_GROUP_NAME = "value";

        /// <summary>
        /// Extracts an integer from the string when it matches the given search pattern.
        /// This method expects that the group containing the value is named the same as the
        /// VALUE_GROUP_NAME constant.
        /// </summary>
        /// <param name="source">The source string to extract the integer from.</param>
        /// <param name="pattern">The regex pattern to match the value.</param>
        /// <returns>An integer if the extraction was successful, null if not.</returns>
        public static int? ExtractInt(this string source, string pattern)
        {
            Match match = Regex.Match(source, pattern);
            if (match.Success)
            {
                if (int.TryParse(match.Groups["value"].Value, out int value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Extracts an enumerated type from the string when it matches the given search pattern.
        /// This method expects that the group containing the value is named the same as the
        /// VALUE_GROUP_NAME constant.
        /// </summary>
        /// <param name="source">The source string to extract the enum from.</param>
        /// <param name="pattern">The regex pattern to match the value.</param>
        /// <returns>An enum if the extraction was successful, null if not.</returns>
        public static TEnum? ExtractEnum<TEnum>(this string source, string pattern) where TEnum : struct, Enum
        {
            Match match = Regex.Match(source, pattern);
            if (match.Success)
            {
                if (Enum.TryParse<TEnum>(match.Groups["value"].Value, true, out TEnum value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}