using System;
using System.Collections.Generic;
using YesSir.Backend.Commands;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Entities;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Helpers;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Kingdoms {
	public partial class Kingdom {
		public ExecutionResult Kill(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("human")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("commands.execute.no_human", this.Language), ECharacter.Knight));
			}

			Human h = dict["human"] as Human;

			Killed(h);

			return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
		}

		public ExecutionResult Grow(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight));
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			if (r.Culture == null) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("resources.no_culture", this.Language), ECharacter.Farmer));
			}

			IDependency dep = new ItemDependency("water_bucket", 2);
			Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
			if (!res.Item1) {
				return new ExecutionResult(res.Item2);
			}

			dep.Use(this);
			foreach (Building b in Buildings) {
				Field f = b as Field;
				if (f != null) {
					f.Count = 50;
					f.Culture = r.Culture;
					f.IsWorking = true;
					f.TimeLeft = 3f;
					break;
				}
			}

			return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
		}

		public ExecutionResult Create(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight));
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			foreach (IDependency dep in r.CreationDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return new ExecutionResult(false, res.Item2);
				}
			}

			Human h = dict.Get("human") as Human ?? FindBySkill(r.Skill);
			HumanTask t = new HumanTask() {
				Destination = r.Name,
				TaskType = ETask.Creating
			};
			t.CalculateTaskTime(h, r.Difficulty, r.Skill);
			foreach (IDependency dep in r.ExtractionDependencies) {
				t.Use(dep.Use(this));
			}

			if (!h.AddTask(t)) {
				return new ExecutionResult(false, new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				));
			} else {
				return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
			}
		}

		public ExecutionResult Extract(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight));
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			foreach (IDependency dep in r.ExtractionDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return new ExecutionResult(false, res.Item2);
				}
			}

			Human h = dict.Get("human") as Human ?? FindBySkill(r.Skill);
			HumanTask t = new HumanTask() {
				Destination = r.Name,
				TaskType = ETask.Extracting,
				Repeating = true
			};
			t.CalculateTaskTime(h, r.Difficulty, r.Skill);
			foreach (IDependency dep in r.ExtractionDependencies) {
				t.Use(dep.Use(this));
			}

			if (!h.AddTask(t)) {
				return new ExecutionResult(false, new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				));
			} else {
				return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
			}
		}

		public ExecutionResult Hire(Dictionary<string, object> dict) {
			int count = (int)(dict.Get("count") ?? 1);
			if (!dict.ContainsKey("job")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("jobs.no_job", this.Language), ECharacter.Knight));
			}

			JobDescription j = (JobDescription)dict["job"];
			List<IDependency> deps = new List<IDependency>(j.HireDepence);

			for (int i = 0; i < count; ++i) {
				foreach (IDependency dep in deps) {
					Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
					if (!res.Item1) {
						return new ExecutionResult(false, res.Item2);
					}
				}
				foreach (IDependency dep in j.HireDepence) {
					dep.Use(this);
				}
				CreateHumanWithSkills(new Tuple<string, float>[] {
					new Tuple<string, float>(j.SkillName, RandomManager.NextGoodSkill())
				});
			}

			return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language), ECharacter.Knight));
		}

		public ExecutionResult Train(Dictionary<string, object> dict) {
			int count = 1;
			if (dict.ContainsKey("count")) { count = (int)dict["count"]; }
			if (!dict.ContainsKey("skill")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("jobs.no_job", this.Language), ECharacter.Knight));
			}
			string sk = (string)dict["skill"];
			JobDescription jb = ContentManager.GetJobDescriptionBySkill(sk);
			List<IDependency> deps = new List<IDependency>(jb.TrainDepence);
			deps.Add(new HumanDependency());

			foreach (IDependency dep in deps) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return new ExecutionResult(false, res.Item2);
				}
			}
			Human h = dict.Get("human") as Human ?? FindBySkill(sk, false);
			HumanTask task = new HumanTask() {
				Destination = sk,
				TaskType = ETask.Training
			};

			task.CalculateTaskTime(h);
			if (!h.AddTask(task)) {
				return new ExecutionResult(false, new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				));
			} else {
				return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
			}
		}

		public ExecutionResult Build(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("building")) {
				return new ExecutionResult(false, new MessageCallback(Locale.Get("buildings.no_building", this.Language), ECharacter.Knight));
			}
			BuildingDescription b = (BuildingDescription)dict["building"];

			foreach (IDependency dep in b.Dependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return new ExecutionResult(false, res.Item2);
				}
			}

			Human h = dict.Get("human") as Human ?? FindBySkill("building");

			HumanTask t = new HumanTask();
			t.Destination = b.Name;
			t.TaskType = ETask.Building;

			foreach (IDependency dep in b.Dependencies) {
				t.Use(dep.Use(this));
			}

			t.CalculateTaskTime(h);

			if (!h.AddTask(t)) {
				return new ExecutionResult(false, new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				));
			} else {
				return new ExecutionResult(new MessageCallback(Locale.Get("answers.yes", this.Language)));
			}
		}
	}
}