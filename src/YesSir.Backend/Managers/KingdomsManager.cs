﻿using YesSir.Backend.Entities.Kingdoms;
using MongoDB.Driver;
using System;
using YesSir.Shared.Users;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Managers {
	public static class KingdomsManager {
		public static Kingdom FindKingdom(UserInfo userinfo) {
			var cursor = DatabaseManager.Kingdoms.Find(k => k.UserId == userinfo.Id);

			if (cursor.Count() > 0) {
				return cursor.First<Kingdom>();
			} else {
				return new Kingdom(userinfo);
			}
		}

		public static void SaveKingdom(Kingdom kingdom) {
			var cursor = DatabaseManager.Kingdoms.Find(k => k.UserId == kingdom.UserId);
			if (cursor.Count() == 0) {
				DatabaseManager.Kingdoms.InsertOneAsync(kingdom);
			} else {
				DatabaseManager.Kingdoms.ReplaceOne(k => k.UserId == kingdom.UserId, kingdom);
			}
		}

		public static string CreateKingdom(UserInfo ui) {
			Kingdom kingdom = ScriptManager.DoFile("Scripts/new_kingdom.lua").ToObject() as Kingdom;
			kingdom.UserId = ui.Id;
			kingdom.Language = ui.Language;
			SaveKingdom(kingdom);
			kingdom = FindKingdom(ui);

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
			foreach (Kingdom k in DatabaseManager.Kingdoms.Find(_ => true).ToList()) {
				MessageCallback[] msgs = k.Update((deltatime / 1000f) / k.GetDayTime());
				foreach (MessageCallback msg in msgs) {
					UsersManager.Send(k.UserId, msg);
				}
				SaveKingdom(k);
			}
		}
	}
}
