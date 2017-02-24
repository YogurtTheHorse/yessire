using System;
using System.Text;
using System.Threading;
using YesSir.Shared;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.CLI {
	class Program {
		public static void Print(MessageCallback msg) {	
			Console.Write("\r{0}> ", msg.Format());
		}

		static void Main(string[] args) {
			Thread.Sleep(2000);
			UserInfo ui = new UserInfo();
			ui.Name = "Test";
			ui.Language = "en";
			ui.Type = "cli";
			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;
			
			ui.ThirdPartyId = Console.ReadLine();
			Console.WriteLine($"Your id: {ui.ThirdPartyId}");

			bool running = true;
			ApiManager.OnMessage += (o) => Print(o.Message);
			ApiManager.StartPoll();

			while (running) {
				Console.Write("> ");
				string message = Console.ReadLine();
				switch (message) {
					case "start":
						ApiManager.Start(ui.ThirdPartyId);
						break;

					case "ru":
					case "en":
						ApiManager.SetLanguage(ui.ThirdPartyId, message);
						break;

					default:
						ApiManager.Message(ui.ThirdPartyId, message);
						break;
				}
			}
			Console.ReadLine();
		}
	}
}
