using System;

namespace messenger
{
	public class SMS
	{
		public SMS ()
		{
		}

		public string Message { get; set; }
		public string Target { get; set; }
		public string Time = DateTime.Now.ToString("yyyyMMddHHmmssffff");
		public string Type = "list";
		public string Text = "text";
	}
}

