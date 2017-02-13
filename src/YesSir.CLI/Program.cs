using System;
using System.Text;
using YesSir.Shared;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.CLI {
	class Program {
		public static void Print(MessageCallback msg) {
			Console.Write("\r{0}> ", msg.Format());
		}

		static void Main(string[] args) {
			UserInfo ui = new UserInfo();
			ui.Name = "Test";
			ui.Language = "en";
			ui.Type = "cli";
			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;

			Console.WriteLine("Your id: 1");
			ui.ThirdPartyId = "1";

			bool running = true;
			ApiManager.OnMessage += (o) => Print(o.Message);
			ApiManager.StartPoll();

			while (running) {
				Console.Write("> ");
				string message = Console.ReadLine();
				if (message == "start") {
					ApiManager.Start(ui.ThirdPartyId);
				} else {
					ApiManager.Message(ui.ThirdPartyId, message);
				}
			}
			Console.ReadLine();
		}
	}
}
