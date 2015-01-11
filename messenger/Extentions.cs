using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utilities
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

	public static class DateTimeExtension
	{
		public static string TimeAgo(this DateTime date)
		{
			TimeSpan timeSince = DateTime.Now.Subtract(date);
			if (timeSince.TotalMinutes < 1) return "Now";
			if (timeSince.TotalMinutes < 2) return "1 min";
			if (timeSince.TotalMinutes < 60) return string.Format("{0} mins", timeSince.Minutes);
			if (timeSince.TotalMinutes < 120) return "1 hour";
			if (timeSince.TotalHours < 24) return string.Format("{0} hours", timeSince.Hours);
			if (timeSince.TotalDays < 2) return "Yesterday";
			if (timeSince.TotalDays < 7) return date.DayOfWeek + " " +  String.Format("{0:t}",date); 
			if (timeSince.TotalDays < 14) return String.Format("{ddd 0:d, 0:t}",date);
			return string.Format("{0} years ago", Math.Round(timeSince.TotalDays / 365));
		}
	}

	/**
	 * Credit to sound : 
	 * http://stackoverflow.com/questions/6594250/type-cast-from-java-lang-object-to-native-clr-type-in-monodroid
	 **/
	public static class ObjectTypeHelper
	{
		public static T Cast<T>(this Java.Lang.Object obj) where T : class
		{
			var propertyInfo = obj.GetType().GetProperty("Instance");
			return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
		}
	} 

}