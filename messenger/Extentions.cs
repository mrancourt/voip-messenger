using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomExtensions
{
	//Extension methods must be defined in a static class
	public static class StringExtension
	{
		public static string ToDigitsOnly(this string input)
		{
			Regex digitsOnly = new Regex(@"[^\d]");
			return digitsOnly.Replace(input, "");
		}
	}
}