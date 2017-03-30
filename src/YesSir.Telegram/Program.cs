using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using YesSir.Shared;
using YesSir.Shared.Queues;

namespace YesSir.Telegram {
	class Program {
		private static readonly TelegramBotClient Bot = new TelegramBotClient("317796681:AAFyKFi8Rs6rjZ9C5itvjs4tLLwrXrfX0eY");

		public static void Main(string[] args) {
			Bot.OnMessage += BotOnMessageReceived;

			var me = Bot.GetMeAsync().Result;

			Console.WriteLine(me.Username);
			//Console.Title = me.Username;
			ApiManager.UserType = "telegram";
			ApiManager.OnMessage += ApiMessage;
			ApiManager.StartPoll();

			Bot.StartReceiving();
			Console.ReadLine();
			Bot.StopReceiving();
			ApiManager.StopPoll();
		}

		private static void ApiMessage(Outgoing outgoing) {
			Bot.SendTextMessageAsync(long.Parse(outgoing.UserInfo.ThirdPartyId), outgoing.Message.Format());
		}

		private static void BotOnMessageReceived(object sender, MessageEventArgs e) {
			string txt = e.Message.Text.ToLower().Trim();
			string id = e.Message.Chat.Id.ToString();

			switch (txt) {
				case "/start":
					ApiManager.Start(id);
					break;

				case "/ru":
				case "/en":
					ApiManager.SetLanguage(id, txt.Substring(1));
					break;

				default:
					ApiManager.Message(id, txt);
					break;
			}
		}
	}
}
