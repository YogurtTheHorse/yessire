using System;
using MongoDB.Driver;
using YesSir.Backend.Commands;
using System.Collections.Generic;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Commands.Parts;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;
using YesSir.Shared.Queues;
using System.Linq;

namespace YesSir.Backend.Managers {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public static class UsersManager {
		private static List<Command> Commands;

		static UsersManager() {
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
			foreach (ItemDescription r in ContentManager.GetResources()) {
				foreach (string s in r.GetAcceptableNames()) {
					resourceTuples.Add(new Tuple<string, object>(s, r));
				}
			}
			CommandPart extractable = new CommandPart("resource", resourceTuples.FindAll(t => {
				return (t.Item2 as ItemDescription).Extractable;
			}).ToArray());
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(
					Locale.GetArray("commands.extract.list"),
					new CommandPart[] { extractable, new CommandPart() }
				),
				(k, dict) => k.Extract(dict)
			));
			CommandPart creatable = new CommandPart("resource", resourceTuples.FindAll(t => {
				return (t.Item2 as ItemDescription).Creatable;
			}).ToArray());
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(
					Locale.GetArray("commands.create.list"),
					new CommandPart[] { creatable, new CommandPart() }
				),
				(k, dict) => k.Create(dict)
			));
			CommandPart growable = new CommandPart("resource", resourceTuples.ToArray());
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency(), new BuildingDependency("field", true) },
				new CommandPart(
					Locale.GetArray("commands.grow.list"),
					new CommandPart[] { growable, new CommandPart() }
				),
				(k, dict) => k.Grow(dict)
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
			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "deb" }), (k, d) => {
				string msg = ObjectDumper.Dump(k);
				return new MessageCallback(msg, ECharacter.Admin);
			}));

			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "res" }), (k, d) => {
				string msg = string.Join("\n", k.Resources.Select(r => r.Key + ": " + r.Value.Count));

				return new MessageCallback(msg, ECharacter.Admin);
			}));

			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "tas" }), (k, d) => {
				string msg = string.Join("\n", k.Humans.Select(h => h.GetName(k.Language) + ": " + h.GetStatus(k.Language)));
				return new MessageCallback(msg, ECharacter.Admin);
			}));
		}
#endif

		public static MessageCallback[] OnMessage(MessageInfo message) {
			UpdateUserInfo(message.UserInfo);
			Kingdom kingdom = KingdomsManager.FindKingdom(message.UserInfo);
			List<MessageCallback> msgs = new List<MessageCallback>();
			bool cont = true;
			string text = message.Text;

			while (cont) {
				cont = false;
				foreach (Command c in Commands) {
					Tuple<int, MessageCallback> res = c.CheckAndExecute(text, kingdom);
					if (res.Item1 >= 0) {
						KingdomsManager.SaveKingdom(kingdom);
						msgs.Add(res.Item2);
						text = text.Substring(res.Item1);
						cont = true;
						break;
					}
				}
			}
			if (msgs.Count == 0) {
				return new MessageCallback[] { new MessageCallback("nyet.", ECharacter.Knight) };
			} else {
				return msgs.ToArray();
			}
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
