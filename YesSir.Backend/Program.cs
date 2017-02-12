using Nancy;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;
using YesSir.Shared.Queues;
using YesSir.Shared.Users;

namespace YesSir.Backend {
	public class ApiModule : NancyModule {
			public ApiModule() {
			Get["/start/{userType}/{userId}"] = args => {
				Console.WriteLine(DateTime.Now.ToString() + " " + Request.Method + " " + Request.Path);

				UserInfo ui = new UserInfo() {
					Type = args.userType,
					ThirdPartyId = args.userId
				};
				QueueManager.Push(new Incoming() {
					UserInfo = ui,
					Method = "start",
					IsWaiting = true
				});
				
				return HttpStatusCode.OK;
			};
			Get["/message/{userType}/{userId}/{message? }"] = args => {
				Console.WriteLine(DateTime.Now.ToString() + " " + Request.Method + " " + Request.Path);

				MessageInfo msg = new MessageInfo() {
					UserInfo = new UserInfo() {
						Type = args.userType,
						ThirdPartyId = args.userId
					},
					Text = args.message
				};
				QueueManager.Push(new Incoming() {
					UserInfo = msg.UserInfo,
					Message = msg,
					Method = "message",
					IsWaiting = true
				});
				return HttpStatusCode.OK;
			};
			Get["/get/{userType}/{userId?all}"] = args => {
				return JsonConvert.SerializeObject(QueueManager.GetOutgoing(args.userType, args.userId));
			};
		}
	}

	public class Program {
		public static void Main(string[] args) {
			string uri = "http://localhost:9797";
			HostConfiguration hostConfigs = new HostConfiguration() {
				UrlReservations = new UrlReservations() { CreateAutomatically = true }
			};
			NancyHost host = new NancyHost(hostConfigs, new Uri(uri));
			Console.WriteLine("Starting Nancy on " + uri);

			UsersManager.Init();

			host.Start();

			int updatetime = 10, deltatime = 0;
			Stopwatch sw = Stopwatch.StartNew();
			while (true) {
				UpdateQueue();
				UpdateWorld(deltatime);
				deltatime = (int)sw.ElapsedMilliseconds;
				sw = Stopwatch.StartNew();
				if (deltatime < updatetime) {
					Thread.Sleep(updatetime - deltatime);
				}
			}
			
			host.Stop();
		}

		private static void UpdateWorld(int deltatime) {
			KingdomsManager.UpdateKingdoms(deltatime);
		}

		private static void UpdateQueue() {
			Incoming inc = QueueManager.GetNextIncoming();

			while (inc != null) {
				Outgoing o = new Outgoing();
				switch (inc.Method) {
					case "message":
						o.Message = UsersManager.OnMessage(inc.Message);
						break;

					case "start":
						o.Message = UsersManager.Start(inc.UserInfo);
						break;

					default:
						continue;
				}
				o.UserInfo = inc.UserInfo;
				QueueManager.Push(o);
				inc = QueueManager.GetNextIncoming();
			}
		}
	}
}
