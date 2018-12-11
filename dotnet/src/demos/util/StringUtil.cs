using System;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.util
{
	public class StringUtil
	{
		public static string removeComma(string str)
		{
			if (!string.ReferenceEquals(str, null) && str.Length >= 2 && str.StartsWith("'", StringComparison.Ordinal) && str.EndsWith("'", StringComparison.Ordinal)) //$NON-NLS-1$ - $NON-NLS-1$
			{
				return str.Substring(1, (str.Length - 1) - 1);
			}
			return str;
		}

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }

    
}