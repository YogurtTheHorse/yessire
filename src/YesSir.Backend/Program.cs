using Nancy;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;
using YesSir.Shared.Queues;
using YesSir.Shared.Users;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Nancy.Owin;
using System.Text;
using System.Collections.Generic;
using Nancy.Helpers;
using MoonSharp.Interpreter;
using System.Reflection;

namespace YesSir.Backend {
	public class Startup {
		public void Configure(IApplicationBuilder app) {
			app.UseOwin(x => x.UseNancy());
		}
	}

	public class ApiModule : NancyModule {
		public ApiModule() {
			Get("/start/{userType}/{userId}", args => {
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
			});
			Post("/message/{userType}/{userId}/", args => {
				Console.WriteLine(DateTime.Now.ToString() + " " + Request.Method + " " + Request.Path);
				var body = this.Request.Body;
				int length = (int)body.Length;
				byte[] data = new byte[length];
				body.Read(data, 0, length);
				string encoded = Encoding.UTF8.GetString(data);
				Dictionary<string, string> vars = new Dictionary<string, string>();
				foreach (string line in encoded.Split('\n')) {
					string[] arr = line.Split('=');
					List<byte> res = new List<byte>();
					foreach (string s in arr[1].Replace("%2C", ",").Split(',')) {
						if (s.Length > 0) {
							res.Add((byte)int.Parse(s));
						}
					}
					vars[arr[0]] = Encoding.Unicode.GetString(res.ToArray());
					Console.WriteLine(vars[arr[0]]);
				}

				MessageInfo msg = new MessageInfo() {
					UserInfo = new UserInfo() {
						Type = args.userType,
						ThirdPartyId = args.userId
					},
					Text = vars["message"]
				};
				QueueManager.Push(new Incoming() {
					UserInfo = msg.UserInfo,
					Message = msg,
					Method = "message",
					IsWaiting = true
				});
				return HttpStatusCode.OK;
			});
			Get("/message/{userType}/{userId}/{message? }", args => {
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
			});
			Get("/get/{userType}/{userId?all}", args => {
				return JsonConvert.SerializeObject(QueueManager.GetOutgoing(args.userType, args.userId));
			});
		}
	}

	public class Program {
		public static void Main(string[] args) {
			var host = new WebHostBuilder()
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseKestrel()
						.UseStartup<Startup>()
						.UseUrls("http://localhost:9797")
						.Build();
			
			Console.OutputEncoding = Encoding.UTF8;
			Console.InputEncoding = Encoding.UTF8;

			DatabaseManager.Init();
			ScriptManager.Init();
			ContentManager.Init();
				
			Console.WriteLine("Starting Nancy on http://localhost:9797");
			host.Start();

			int updatetime = 100 / 3, deltatime = 0;
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
