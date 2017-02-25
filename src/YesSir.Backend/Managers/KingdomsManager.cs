using YesSir.Backend.Entities.Kingdoms;
using MongoDB.Driver;
using System;
using YesSir.Shared.Users;
using YesSir.Shared.Messages;
using System.Collections.Generic;
using YesSir.Backend.Entities;

namespace YesSir.Backend.Managers {
	public static class KingdomsManager {
		private static List<Kingdom> Kingdoms;
		private static float TimeToSave;
		private const float TIME_TO_SAVE = 30;
		private static Tuple<string, Guid> select;

		public static void Init() {
			Kingdoms = DatabaseManager.Kingdoms.Find(_ => true).ToList();
			TimeToSave = TIME_TO_SAVE;
		}

		public static Kingdom FindKingdom(UserInfo userinfo) {
			return FindKingdom(userinfo.Id) ?? new Kingdom(userinfo);
		}

		public static Kingdom FindKingdom(Guid userid) {
			return Kingdoms.Find(k => k.UserId == userid);
		}

		public static void SaveKingdom(Kingdom kingdom) {
			var cursor = DatabaseManager.Kingdoms.Find(k => k.UserId == kingdom.UserId);
			if (cursor.Count() == 0) {
				DatabaseManager.Kingdoms.InsertOneAsync(kingdom);
			} else {
				DatabaseManager.Kingdoms.ReplaceOneAsync(k => k.UserId == kingdom.UserId, kingdom);
			}
		}

		public static Tuple<string, Guid>[] GetKingdomsNames() {
			List<Tuple<string, Guid>> res = new List<Tuple<string, Guid>>(Kingdoms.Count);

			foreach (Kingdom k in Kingdoms) {
				if (k.Name != null) {
					res.Add(new Tuple<string, Guid>(k.Name, k.UserId));
				}
			}

			return res.ToArray();
		}

		public static List<Kingdom> GetKingdoms() {
			return new List<Kingdom>(Kingdoms);
		}

		public static float Distance(Guid kingdomId, Guid to) {
			var first = FindKingdom(kingdomId);
			var second = FindKingdom(to);

			return first.Coordinate.Distance(second.Coordinate);
		}

		public static string CreateKingdom(UserInfo ui) {
			Kingdom kingdom = ScriptManager.DoFile("Scripts/new_kingdom.lua").ToObject() as Kingdom;
			kingdom.UserId = ui.Id;
			kingdom.Language = ui.Language;
			kingdom.GenerateName();

			Kingdoms.RemoveAll(k => k.UserId == ui.Id);
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

		public static void SendMessage(Human ambassador, Guid destination, string msg) {
			Kingdom k = FindKingdom(destination);
			msg = string.Format(Locale.Get("commands.send.recieved", k.Language), ambassador.GetName(k.Language), k.Name, msg);
			UsersManager.Send(destination, new MessageCallback(msg));
		}
	}
}
