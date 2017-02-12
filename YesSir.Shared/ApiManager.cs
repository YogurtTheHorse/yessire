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

namespace YesSir.Shared {
	public static class ApiManager {
		public delegate void OnMessageDelegate(Outgoing outgoing);
		
		public static event OnMessageDelegate OnMessage;
		public static string UserType = "cli";

		private static volatile int _isPolling = 0;
		private static bool IsPolling {
			get {
				return _isPolling != 0;
			}
			set {
				Interlocked.Exchange(ref _isPolling, value ? 1 : 0);
			}
		}

		private static string Method(string path, string httpMethod="GET") {
			string uri = "http://localhost:9797" + path.Replace('\t', ' ');

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream resStream = response.GetResponseStream();
			StreamReader red = new StreamReader(resStream);

			return red.ReadToEnd();
		}

		public static void StopPoll() {
			IsPolling = false;
		}

		public static void StartPoll(string userId="all", long pollInterval=1000/60) {
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

		public static void Start(string userId) {
			Method(string.Format("/start/{0}/{1}/", UserType, userId));
		}

		public static void Message(string userId, string msg) {
			Method(string.Format("/message/{0}/{1}/{2}/", UserType, userId, msg));
		}

		public static Outgoing[] GetMessages(string userId="all") {
			string resp = Method(string.Format("/get/{0}/{1}", UserType, userId));
			List<Outgoing> res = JsonConvert.DeserializeObject<List<Outgoing>>(resp);
			return res.ToArray();
		}
	}
}
