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
using YesSir.Backend.Helpers;
using MongoDB.Bson;
using YesSir.Backend.Entities;

namespace YesSir.Backend.Managers {
	[MoonSharp.Interpreter.MoonSharpUserData]
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

			List<Tuple<string, object>> skills = new List<Tuple<string, object>>();
			foreach (string sk in ContentManager.GetSkills()) {
				foreach (string lsk in Locale.GetArray("skills." + sk + ".names")) {
					skills.Add(new Tuple<string, object>(lsk, sk));
				}
			}
			CommandPart skillPart = new CommandPart("skill", skills.ToArray());
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(Locale.GetArray("commands.train.list"),
					new CommandPart[] {
						new CommandPart("count",
							CommandsStandartFunctions.CheckInt,
							CommandsStandartFunctions.ParseInt,
							new CommandPart[] { skillPart }),
						skillPart,
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
			Commands.Add(new Command(
				new IDependency[] { },
				new CommandPart(Locale.GetArray("commands.build.list"),
					new CommandPart[] { buildingPart, new CommandPart() }
				), (k, dict) => k.Build(dict)
			));

			CommandPart kingdomPart = new CommandPart("kingdom", CommandsStandartFunctions.CheckKingdom, CommandsStandartFunctions.ParseKingomd);
			Commands.Add(new Command(
				new IDependency[] { new HumanDependency() },
				new CommandPart(Locale.GetArray("commands.send.list"),
					new CommandPart[] { kingdomPart, new CommandPart() }
				), (k, dict) => SendMessage(k, dict)
			));
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
				return new ExecutionResult(true, new MessageCallback(msg, ECharacter.Admin));
			}));

			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "res" }), (k, d) => {
				string msg = string.Join("\n", k.Resources.Select(r => r.Key + ": " + r.Value.Count));

				return new ExecutionResult(new MessageCallback(msg, ECharacter.Admin));
			}));

			Commands.Add(new Command(new IDependency[] { }, new CommandPart(new string[] { "tas" }), (k, d) => {
				string msg = string.Join("\n", k.Humans.Select(h => h.GetName(k.Language) + ": " + h.GetStatus(k.Language)));
				return new ExecutionResult(new MessageCallback(msg, ECharacter.Admin));
			}));
		}
#endif

		public static void SetLanguage(MessageInfo message) {
			UpdateUserInfo(message.UserInfo);
			message.UserInfo.Language = message.Text;

			DatabaseManager.Users.ReplaceOneAsync(ui => message.UserInfo.Equals(ui), message.UserInfo);
			KingdomsManager.FindKingdom(message.UserInfo).Language = message.Text;
		}

		public static MessageCallback OnMessage(MessageInfo message) {
			UpdateUserInfo(message.UserInfo);
			switch (message.UserInfo.State) {
				case EState.Main:
					return OnCommand(message);
				case EState.Dictates:
					return OnDictates(message);
			}

			return null;
		}

		private static MessageCallback OnCommand(MessageInfo message) {
			Kingdom kingdom = KingdomsManager.FindKingdom(message.UserInfo);
			List<ExecutionResult> exec = new List<ExecutionResult>();
			bool cont = true, succ = false;
			string text = message.Text;

			while (cont && (exec.Count == 0 || exec.Last().Successful)) {
				cont = false;
				foreach (Command c in Commands) {
					ExecutionResult res = c.Check(text, kingdom);
					if (res.Applied) {
						KingdomsManager.SaveKingdom(kingdom);
						res.Text = text.Substring(0, res.CommandLength).ToLower();
						text = text.Substring(res.CommandLength);
						succ |= (cont = res.Successful);
						exec.Add(res);
						break;
					}
				}
			}
			if (succ) {
				Human selected = null;
				for (int i = 0; i < exec.Count; ++i) {
					foreach (Human h in kingdom.Humans) {
						// TODO: Do something with that
						if (exec[i].Text.Contains(h.Name.Split(' ')[0].ToLower()) || exec[i].Text.Contains(h.Name.Split(' ')[1].ToLower())) {
							selected = h;
							break;
						}
					}
					if (selected != null) {
						exec[i].Parsed["human"] = selected;
					}
					exec[i] = exec[i].Execute(kingdom);

					if (exec[i].NewState.HasValue) {
						UpdateState(message.UserInfo, exec[i].NewState.Value);
					}

					if (!exec[i].Successful || exec[i].LastCommand) {
						return exec[i].Message;
					} else if (selected != null && exec[i].HumanBusy) {
						selected = null;
					}
				}
			}

			if (exec.Count == 0) {
				return new MessageCallback("nyet.", ECharacter.Knight);
			} else {
				return exec.Last().Message;
			}
		}

		private static ExecutionResult SendMessage(Kingdom k, Dictionary<string, object> dict) {
			if (!dict.ContainsKey("kingdom")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("commands.build.no_kingdom", k.Language), ECharacter.Knight));
			}

			Guid hid = (dict.Get("human") as Human ?? k.FindBySkill("dyplomacy")).HumanId;
			string dest = dict["kingdom"] as string;

			int index;
			for (index = 0; index < k.Humans.Count && k.Humans[index].HumanId != hid; ++index) ;

			if (k.Humans[index].AddTask(new HumanTask() { Destination = dest, TaskType = ETask.ListeningKing })) {
				k.Humans[index].TasksToDo.Last().CalculateTaskTime(k.Humans[index], 1, "dyplomacy");
				k.Temp = k.Humans[index].HumanId.ToString();

				return new ExecutionResult(new MessageCallback(Locale.Get("answers.write_message", k.Language))) {
					NewState = EState.Dictates
				};
			} else {
				return new ExecutionResult(false, new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", k.Humans[index].GetName(k.Language)), k.Language),
					ECharacter.Knight
				));
			}
		}

		private static MessageCallback OnDictates(MessageInfo message) {
			Kingdom kingdom = KingdomsManager.FindKingdom(message.UserInfo);
			Guid hid = Guid.Parse(kingdom.Temp as string);

			Human h = null;
			foreach (Human hh in kingdom.Humans) {
				if (hh.HumanId == hid) {
					h = hh;
					break;
				}
			}

			int i;
			for (i = 0; i < h.TasksToDo.Count; ++i) {
				if (h.TasksToDo[i].TaskType == ETask.ListeningKing) {
					break;
				}
			}

			h.TasksToDo[i].TaskType = ETask.SendingMessage;
			h.TasksToDo[i].Context = message.Text;
			h.TasksToDo[i].CalculateTaskTime(h);

			UpdateState(message.UserInfo, EState.Main);
			KingdomsManager.OnJorney(h, kingdom);

			return new MessageCallback(Locale.Get("answers.yes", kingdom.Language));
		}

		public static void UpdateState(UserInfo ui, EState state) {
			ui.State = state;

			var filter = Builders<UserInfo>.Filter.Eq("Id", ui.Id);
			var update = Builders<UserInfo>.Update.Set("State", state);
			DatabaseManager.Users.UpdateOne(filter, update);
		}

		public static MessageCallback Start(UserInfo ui) {
			UpdateUserInfo(ui);
			UpdateState(ui, EState.Main);

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
				var new_ui = cursor.First();

				ui.Id = new_ui.Id;
				ui.Language = new_ui.Language;
				ui.State = new_ui.State;

				return false;
			}
		}
	}
}
