using YesSir.Backend.Entities.Kingdoms;
using MongoDB.Driver;
using System;
using YesSir.Shared.Users;
using YesSir.Shared.Messages;
using System.Collections.Generic;
using YesSir.Backend.Entities;

namespace YesSir.Backend.Managers {
	public static class KingdomsManager {
		private static HashSet<Guid> Saved;
		private static List<Kingdom> Kingdoms;
		private static List<Human> HumansOnJourney;
		private static float TimeToSave;
		private const float TIME_TO_SAVE = 30;
		private static Tuple<string, Guid> select;

		public static void Init() {
			Saved = new HashSet<Guid>();
			Kingdoms = DatabaseManager.Kingdoms.Find(_ => true).ToList();
			HumansOnJourney = DatabaseManager.HumansOnJourney.Find(_ => true).ToList();
			TimeToSave = TIME_TO_SAVE;

			Kingdoms.ForEach(k => Saved.Add(k.UserId));
			HumansOnJourney.ForEach(h => Saved.Add(h.HumanId));
		}

		public static Kingdom FindKingdom(UserInfo userinfo) {
			return FindKingdom(userinfo.Id) ?? new Kingdom(userinfo);
		}

		public static Kingdom FindKingdom(Guid userid) {
			return Kingdoms.Find(k => k.UserId == userid);
		}

		public static void SaveKingdom(Kingdom kingdom) {
			if (Saved.Contains(kingdom.UserId)) {
				DatabaseManager.Kingdoms.ReplaceOneAsync(k => k.UserId == kingdom.UserId, kingdom);
			} else {
				Saved.Add(kingdom.UserId);
				DatabaseManager.Kingdoms.InsertOneAsync(kingdom);
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

			// TODO: Optimize
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
			Dictionary<Human, Guid> humansToAdd = new Dictionary<Human, Guid>();


			for (int i = HumansOnJourney.Count - 1; i >= 0; i--) {
				Human h = HumansOnJourney[i];
				HumanTask t = h.TasksToDo[0];

				t.TimeLeft -= deltatime;
				if (t.TimeLeft <= 0) {
					Guid k_id = Guid.Parse(t.Destination);
					h.TasksToDo.RemoveAt(0);

					humansToAdd.Add(h, k_id);
					HumansOnJourney.RemoveAt(i);

					Saved.Remove(h.HumanId);
					DatabaseManager.HumansOnJourney.DeleteOneAsync(hh => h.HumanId == hh.HumanId);

					MessageSent(h, k_id, t.Context as string);
				}
			}
			foreach (Kingdom k in Kingdoms) {
				if (humansToAdd.ContainsValue(k.UserId)) {
					foreach (var kv in humansToAdd) {
						if (kv.Value == k.UserId) {
							k.Humans.Add(kv.Key);
						}
					}
				}
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
				foreach (Human h in HumansOnJourney) {
					if (Saved.Contains(h.HumanId)) {
						DatabaseManager.HumansOnJourney.ReplaceOneAsync(hh => h.HumanId == hh.HumanId, h);
					} else {
						Saved.Add(h.HumanId);
						DatabaseManager.HumansOnJourney.InsertOneAsync(h);
					}
				}
			}
		}

		public static void MessageSent(Human ambassador, Guid destination, string msg) {
			Kingdom k = FindKingdom(destination);

			HumanTask gettingBackTask = new HumanTask() {
				TaskType = ETask.SendingMessage,
				Destination = k.UserId.ToString(),
				Context = Locale.Get("commands.send.empty_answer", k.Language)
			};
			gettingBackTask.CalculateTaskTime(ambassador);

			ambassador.TasksToDo.AddRange(new[] {
				new HumanTask() {
					TaskType = ETask.Waiting,
					TimeLeft = 3
				},
				gettingBackTask
			});

			msg = string.Format(Locale.Get("commands.send.recieved", k.Language), ambassador.GetName(k.Language), k.Name, msg);
			UsersManager.Send(destination, new MessageCallback(msg));
		}

		public static void OnJorney(Human human, Kingdom kingdom) {
			HumansOnJourney.Add(human);
			kingdom.Humans.Remove(human);
		}
	}
}
