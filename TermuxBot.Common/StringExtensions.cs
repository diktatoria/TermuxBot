using System;
using System.Collections.Generic;
using System.Text;

namespace TermuxBot.Common
{
    public static class StringExtensions
    {
        public static string ToDelimiterSeparatedString(this string[] stringArray, string delimiter = ", ")
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < stringArray.Length; i++)
            {
                result.Append(stringArray[i]);
                if (!String.IsNullOrEmpty(delimiter) &&
                   i < stringArray.Length - 1)
                {
                    result.Append(delimiter);
                }
            }

            return result.ToString();
        }
    }
}