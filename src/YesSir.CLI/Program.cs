using System;
using System.IO;
using System.Text;
using System.Threading;
using YesSir.Shared;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.CLI {
	public class Program {
		private static UserInfo ui;
		private static bool ReadingScenario;

		public static void Print(MessageCallback msg) {
			Print(msg.Format());
		}

		public static void Print(string msg) {
			Console.Write("\r{0}\n> ", msg);
		}

		static void Main(string[] args) {
#if DEBUG
			Thread.Sleep(3000);
#endif

			ui = new UserInfo();
			ui.Name = "Test";
			ui.Language = "ru";
			ui.Type = "cli";
			ui.ThirdPartyId = "test";

			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;

			bool running = true;
			ApiManager.OnMessage += (o) => Print(o.Message);
			ApiManager.StartPoll();
			ApiManager.WaitForConnect();

#if DEBUG
			if (File.Exists("scen.txt")) {
				Console.WriteLine("Reading scen.txt...");
				using (var stream = new FileStream("scen.txt", FileMode.Open)) {
					using (var reader = new StreamReader(stream)) {
						foreach (string s in reader.ReadToEnd().Split('\n')) {
							if (s.Length < 3) {
								continue;
							} else if (s.StartsWith("!")) {
								ui.ThirdPartyId = s.Substring(1).Trim();
								Print($"Your id: {ui.ThirdPartyId}");
							} else if (s.StartsWith("%")) {
								int tm = int.Parse(s.Substring(1));
								Print($"Waiting {tm}ms...");
								Thread.Sleep(tm);
								Print("Done");
							} else if (s.StartsWith("#")) {
								Print(s);
							} else { 
								Print("> " + s);
								OnMessage(s);
							}
							Thread.Sleep(1000);
						}
					}
				}
			}
#endif
			if (ui.ThirdPartyId == "") {
				ui.ThirdPartyId = Console.ReadLine();
				Console.WriteLine($"Your id: {ui.ThirdPartyId}");
			}

			while (running) {
				Console.Write("> ");
				string message = Console.ReadLine();
				OnMessage(message);

			}
			Console.ReadLine();
		}

		public static void OnMessage(string msg) {
			msg = msg.Trim(' ', '\n', '\r');
			switch (msg) {
				case "start":
					ApiManager.Start(ui.ThirdPartyId);
					break;

				case "ru":
				case "en":
					ApiManager.SetLanguage(ui.ThirdPartyId, msg);
					break;

				default:
					ApiManager.Message(ui.ThirdPartyId, msg);
					break;
			}
		}
	}
}
