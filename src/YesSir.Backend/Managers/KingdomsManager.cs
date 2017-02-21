using YesSir.Backend.Entities.Kingdoms;
using MongoDB.Driver;
using System;
using YesSir.Shared.Users;
using YesSir.Shared.Messages;
using System.Collections.Generic;

namespace YesSir.Backend.Managers {
	public static class KingdomsManager {
		private static List<Kingdom> Kingdoms;
		private static float TimeToSave;
		private const float TIME_TO_SAVE = 30;

		static KingdomsManager() {
			Kingdoms = DatabaseManager.Kingdoms.Find(_ => true).ToList();
			TimeToSave = TIME_TO_SAVE;
		}

		public static Kingdom FindKingdom(UserInfo userinfo) {
			return Kingdoms.Find(k => k.UserId == userinfo.Id) ?? new Kingdom(userinfo);
		}

		public static void SaveKingdom(Kingdom kingdom) {
			var cursor = DatabaseManager.Kingdoms.Find(k => k.UserId == kingdom.UserId);
			if (cursor.Count() == 0) {
				DatabaseManager.Kingdoms.InsertOneAsync(kingdom);
			} else {
				DatabaseManager.Kingdoms.ReplaceOneAsync(k => k.UserId == kingdom.UserId, kingdom);
			}
		}

		public static string CreateKingdom(UserInfo ui) {
			Kingdom kingdom = ScriptManager.DoFile("Scripts/new_kingdom.lua").ToObject() as Kingdom;
			kingdom.UserId = ui.Id;
			kingdom.Language = ui.Language;
			
			Kingdoms.Add(kingdom);
			SaveKingdom(kingdom);

			int res = RandomManager.Next(0, 3);

			switch (res) {
				case 0:
					return "revolution";

				case 1:
					return "legacy";

				default:
					return "self_proclaimed";
			}
		}

		public static void UpdateKingdoms(int deltatime) {
			foreach (Kingdom k in Kingdoms) {
				MessageCallback[] msgs = k.Update((deltatime / 1000f) / k.GetDayTime());
				foreach (MessageCallback msg in msgs) {
					UsersManager.Send(k.UserId, msg);
				}
			}
			TimeToSave -= deltatime / 1000f;
			if (TimeToSave < 0) {
				TimeToSave = TIME_TO_SAVE;
				foreach (Kingdom k in Kingdoms) {
					SaveKingdom(k);
				}
			}
		}
	}
}
