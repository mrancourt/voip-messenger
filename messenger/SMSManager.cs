using System;
using Couchbase.Lite;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Utilities;
using System.Threading;
using Android.App;
using Newtonsoft.Json;
using System.Json;
using System.Collections.Generic;

namespace messenger
{
	public class SMSManager
	{
		// TODO : A verifer/Changer
		readonly static string DocType = typeof(SMS).Name.ToLower();
		const string ViewName = "messages";
		Database database;

		public SMSManager(Database db) {
			database = db;
		}

		public Query GetQuery(string normalizedPhone)
		{
			var view = database.GetView(ViewName + "_" + normalizedPhone);

			if (view.Map == null)
			{
				view.SetMap ((doc, emit) => {

					if (!doc.ContainsKey("type")) 
					{
						return;
					}

					// If document is a conversation, we emit it
					if (doc["type"].ToString() == DocType && 
						doc["conversationId"].ToString() == normalizedPhone) {
						emit (
							// Order by conversation, then timestamp
							doc["time"],
							doc
							);
					}

				}, "7");
			}
			var query = view.CreateQuery();
			query.Descending = true;
			return query;
		}
			
		public static VoipMsResponse SendSMS(SMS sms)
		{
		
			Tuple<CookieContainer, VoipMsResponse> login = Login ();
			VoipMsResponse res; 

			// Login error
			if (login.Item2.page != "sms.html") {
				// Error response from login
				res = login.Item2;
			} 
			// We are logued in, now try to send the message !
			else {
				var request = (HttpWebRequest)WebRequest.Create("https://sms.voip.ms/sms_manage.php");
				request.CookieContainer = login.Item1;

				var postData = "contact=" + sms.Target.ToDigitsOnly();
				//var postData = "contact=" + "4182221617";
				postData += "&did=5143123182";
				postData += "&msg=" + sms.Message;
				postData += "&method=send_sms";
				postData += "&type=";
				postData += "&type=0";
				postData += "&sync=1";

				var data = Encoding.ASCII.GetBytes(postData);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				request.ContentLength = data.Length;

				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				var response = (HttpWebResponse)request.GetResponse();

				var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

				Console.WriteLine("-------------- SMS RESPONSE STRING --------------" + "\n" + responseString);

				// Convert JSON response to VoipMsResponse
				res = (VoipMsResponse) JsonConvert.DeserializeObject(responseString, typeof(VoipMsResponse));
			}

			return res;
		}

		protected static Tuple<CookieContainer, VoipMsResponse> Login() {

			var cookieContainer = new CookieContainer();
			var request = (HttpWebRequest)WebRequest.Create("https://sms.voip.ms/sms_manage.php");
			request.CookieContainer = cookieContainer;

			// TODO : externalize user/pass!
			var postData = "col_email=***EMAIL***";
			postData += "&col_password=***PASSWORD***";
			postData += "&method=login_json";
			postData += "&action=login";
			postData += "&sync=1";

			var data = Encoding.ASCII.GetBytes(postData);

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			request.ContentLength = data.Length;

			using (var stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			var response = (HttpWebResponse)request.GetResponse();

			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

			Console.WriteLine("-------------- LOGIN RESPONSE STRING --------------" + "\n" + responseString);

			// Print the properties of each cookie. 
			foreach (Cookie cook in response.Cookies)
			{
				Console.WriteLine("Cookie:");
				Console.WriteLine("{0} = {1}", cook.Name, cook.Value);
				Console.WriteLine("Domain: {0}", cook.Domain);
				Console.WriteLine("Path: {0}", cook.Path);
				Console.WriteLine("Port: {0}", cook.Port);
				Console.WriteLine("Secure: {0}", cook.Secure);

				Console.WriteLine("When issued: {0}", cook.TimeStamp);
				Console.WriteLine("Expires: {0} (expired? {1})",
					cook.Expires, cook.Expired);
				Console.WriteLine("Don't save: {0}", cook.Discard);
				Console.WriteLine("Comment: {0}", cook.Comment);
				Console.WriteLine("Uri for comments: {0}", cook.CommentUri);
				Console.WriteLine("Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");

				// Show the string representation of the cookie.
				Console.WriteLine ("String: {0}", cook);
			}

			// Convert JSON response to VoipMsResponse
			var res = (VoipMsResponse) JsonConvert.DeserializeObject(responseString, typeof(VoipMsResponse));

			return Tuple.Create(cookieContainer, res);
		}

		public void AddMessage(SMS sms)
		{

			const string Tag = "conversation";

			sms.Time = DateTime.Now.ToString ("yyyyMMddHHmmssffff");

			// Create message's document
			var docMessage = database.CreateDocument();
			var props = new Dictionary<string, object>
			{
				{ "conversationId", sms.Target },
				{ "type", typeof(SMS).Name.ToLower() },
				{ "time", sms.Time },
				{ "text", sms.Message },
				{ "source", sms.Source }
			};
			docMessage.PutProperties(props);

			// Update conversation 
			// TODO : check if message update is OK
			var docConversation = database.GetDocument(sms.Target);
			docConversation.Update((UnsavedRevision newRevision) => 
				{
					var properties = newRevision.Properties;
					properties["conversationId"] = sms.Target;
					properties["type"] = Tag;
					properties["lastMessage"] =  sms.Message;
					properties["lastMessageTime"] = DateTime.Now.ToString("yyyyMMddHHmmssffff");
					properties["source"] = sms.Source;
					return true;
				});
		}

	}
}

