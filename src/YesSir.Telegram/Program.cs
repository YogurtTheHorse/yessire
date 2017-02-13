using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using YesSir.Shared;
using YesSir.Shared.Queues;

namespace YesSir.Telegram {
	class Program {
		private static readonly TelegramBotClient Bot = new TelegramBotClient("275248602:AAElsfmV4NBoorQzHovyuLOiC2_GCNdcAio");

		public static void Main(string[] args) {
			Bot.OnMessage += BotOnMessageReceived;

			var me = Bot.GetMeAsync().Result;

			Console.Title = me.Username;
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

				default:
					ApiManager.Message(id, txt);
					break;
			}
		}
	}
}
