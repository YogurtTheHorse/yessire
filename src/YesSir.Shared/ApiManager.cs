using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using YesSir.Shared.Queues;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System;
using System.Net.Http;
using Nancy.Helpers;

namespace YesSir.Shared {
	public static class ApiManager {
		public delegate void OnMessageDelegate(Outgoing outgoing);

		public static event OnMessageDelegate OnMessage;
		public static string UserType = "cli";

		private static volatile int _isPolling = 0;
		private static bool IsPolling
		{
			get
			{
				return _isPolling != 0;
			}
			set
			{
				Interlocked.Exchange(ref _isPolling, value ? 1 : 0);
			}
		}

		private static async Task<string> Method(string path, Dictionary<string, string> d = null) {
			string uri = "http://localhost:9797" + path.Replace('\t', ' ');

			if (d != null) {
				using (var client = new HttpClient()) {
					var content = new FormUrlEncodedContent(d);

					var response = await client.PostAsync(uri, content);

					return await response.Content.ReadAsStringAsync();
				}
			} else {
				using (var client = new HttpClient()) {
					var response = await client.GetAsync(uri);

					return await response.Content.ReadAsStringAsync();
				}
			}
		}

		public static void StopPoll() {
			IsPolling = false;
		}

		public static void StartPoll(string userId = "all", long pollInterval = 1000 / 60) {
			IsPolling = true;
			Task.Run(async () => {
				Stopwatch sw = Stopwatch.StartNew();
				while (IsPolling) {
					foreach (Outgoing o in GetMessages(userId)) {
						OnMessage(o);
					}
					if (sw.ElapsedMilliseconds < pollInterval) {
						long delta = pollInterval - sw.ElapsedMilliseconds;
						await Task.Delay((int)delta);
					}
					sw = Stopwatch.StartNew();
				}
			});
		}

		public static string Code(string s) {
			string res = "";
			foreach (byte b in Encoding.Unicode.GetBytes(s)) {
				res += (int)b;
				res += ",";
			}
			Console.WriteLine(s);
			return res;
		}

		public static async void Start(string userId) {
			await Method(string.Format("/start/{0}/{1}/", UserType, userId));
		}

		public static async void Message(string userId, string msg) {
			await Method(string.Format("/message/{0}/{1}/{2}", UserType, userId, msg));
		}

		public static async void SetLanguage(string userId, string lang) {
			await Method(string.Format("/setlang/{0}/{1}/{2}", UserType, userId, lang));
		}

		public static Outgoing[] GetMessages(string userId = "all") {
			var m = Method(string.Format("/get/{0}/{1}", UserType, userId));
			string resp = m.GetAwaiter().GetResult();
			List<Outgoing> res = JsonConvert.DeserializeObject<List<Outgoing>>(resp);
			return res.ToArray();
		}
	}
}
