﻿using System;
using MongoDB.Driver;
using YesSir.Backend.Commands;
using System.Collections.Generic;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Commands.Parts;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Commands.Dependencies;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;
using YesSir.Shared.Queues;

namespace YesSir.Backend.Managers {
	public static class UsersManager {
		private static List<Command> Commands;

		public static void Init() {
			Commands = new List<Command>();

			List<Tuple<string, object>> jobTuples = new List<Tuple<string, object>>();
			foreach (JobDescription d in ContentManager.GetJobs()) {
				foreach (string s in d.GetAcceptableNames()) {
					jobTuples.Add(new Tuple<string, object>(s, d));
				}
			}

			CommandPart jobPart = new CommandPart("job", jobTuples.ToArray());
			Commands.Add(new Command(
				new IDependency[] { },
				new CommandPart(Locale.GetArray("commands.hire.list"),
					new CommandPart[] {
						new CommandPart("count",
							CommandsStandartFunctions.CheckInt,
							CommandsStandartFunctions.ParseInt,
							new CommandPart[] { jobPart }),
						jobPart,
						new CommandPart()
					}
				), (k, dict) => k.Hire(dict)
			));
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(Locale.GetArray("commands.train.list"),
					new CommandPart[] {
						new CommandPart("count",
							CommandsStandartFunctions.CheckInt,
							CommandsStandartFunctions.ParseInt,
							new CommandPart[] { jobPart }),
						jobPart,
						new CommandPart()
					}
				), (k, dict) => k.Train(dict)
			));

			List<Tuple<string, object>> resourceTuples = new List<Tuple<string, object>>();
			foreach (ResourceDescription r in ContentManager.GetResources()) {
				foreach (string s in r.GetAcceptableNames()) {
					resourceTuples.Add(new Tuple<string, object>(s, r));
				}
			}
			CommandPart orePart = new CommandPart("ore", resourceTuples.ToArray());
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(
					Locale.GetArray("commands.mine.list"),
					new CommandPart[] { orePart, new CommandPart() }
				), 
				(k, dict) => k.Mine(dict)
			));

			List<Tuple<string, object>> buildingsTuples = new List<Tuple<string, object>>();
			foreach (BuildingDescription b in ContentManager.GetBuildings()) {
				foreach (string s in b.GetAcceptableNames()) {
					buildingsTuples.Add(new Tuple<string, object>(s, b));
				}
			}
			CommandPart buildingPart = new CommandPart("building", buildingsTuples.ToArray());
			Command b_cmd = new Command(
				new IDependency[] { },
				new CommandPart(Locale.GetArray("commands.build.list"),
					new CommandPart[] { buildingPart, new CommandPart() }
				), (k, dict) => k.Build(dict));
			Commands.Add(b_cmd);

#if DEBUG
			LoadDebugCommands();
#endif
		}

		public static void Send(Guid userId, MessageCallback msg) {
			UserInfo ui = GetUser(userId);
			QueueManager.Push(new Outgoing() {
				Message = msg,
				UserInfo = ui
			});
		}

#if DEBUG
		private static void LoadDebugCommands() {
			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "res" }), (k, d) => {
				string msg = "";
				foreach (KeyValuePair<string, int> r in k.Resources) {
					msg += string.Format("{0}: {1}\n", r.Key, r.Value);
				}
				return new MessageCallback(msg, ECharacter.Admin);
			}));
			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "deb" }), (k, d) => {
				string msg = ObjectDumper.Dump(k);
				return new MessageCallback(msg, ECharacter.Admin);
			}));
		}
#endif

		public static MessageCallback OnMessage(MessageInfo message) {
			UpdateUserInfo(message.UserInfo);
			Kingdom kingdom = KingdomsManager.FindKingdom(message.UserInfo);
			foreach (Command c in Commands) {
				Tuple<bool, MessageCallback> res = c.CheckAndExecute(message.Text, kingdom);
				if (res.Item1) {
					KingdomsManager.SaveKingdom(kingdom);
					return res.Item2;
				}
			}
			return new MessageCallback("nyet.", ECharacter.Knight);
		}

		public static MessageCallback Start(UserInfo ui) {
			UpdateUserInfo(ui);
			string plot = KingdomsManager.CreateKingdom(ui);

			return new MessageCallback(Locale.Get("start_messages." + plot, ui.Language), ECharacter.Knight);

		}

		public static UserInfo GetUser(string userType, string id) {
			UserInfo ui = new UserInfo() {
				ThirdPartyId = id,
				Type = userType
			};

			UpdateUserInfo(ui);

			return ui;
		}

		public static UserInfo GetUser(Guid userId) {
			return DatabaseManager.Users.Find(u => u.Id == userId).First();
		}

		private static bool UpdateUserInfo(UserInfo ui) {
			var cursor = DatabaseManager.Users.Find(u => u.ThirdPartyId == ui.ThirdPartyId && u.Type == ui.Type);

			if (cursor.Count() == 0) {
				ui.Id = new Guid();
				DatabaseManager.Users.InsertOne(ui);

				return true;
			} else {
				var update = Builders<UserInfo>.Update.Set("Name", ui.Name);
				DatabaseManager.Users.UpdateMany(u => u.ThirdPartyId == ui.ThirdPartyId && u.Type == ui.Type, update);

				ui.Id = cursor.First().Id;
				//ui.Language = cursor.First().Language;

				return false;
			}
		}
	}
}