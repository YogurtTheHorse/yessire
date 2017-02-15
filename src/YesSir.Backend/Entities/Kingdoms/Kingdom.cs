using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Descriptions;
using YesSir.Backend.Helpers;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Kingdom {
		public Guid UserId;
		public string Language;
		public List<Human> Humans;
		public List<Building> Buildings;
		public Dictionary<string, int> Resources;

		public Kingdom() {
			Humans = new List<Human>();
			Buildings = new List<Building>();
			Resources = new Dictionary<string, int>() {
				{ "money", 1000 }//,
				//{ "rock", 25 },
				//{ "wood", 25 }
			};
		}

		public Kingdom(UserInfo userinfo) : this() {
			this.UserId = userinfo.Id;
			this.Language = userinfo.Language;
		}

		public int GetResource(string resorce) {
			int res = 0;
			Resources.TryGetValue(resorce, out res);
			return res;
		}

		public bool TakeResource(string resource, int count) {
			if (GetResource(resource) >= count) {
				Resources[resource] -= count;
				return true;
			} else {
				return false;
			}
		}

		public float GetDayTime() {
			return 1f;
		}

		public MessageCallback[] Update(float delta) {
			List<MessageCallback> res = new List<MessageCallback>();
			foreach (Human h in Humans) {
				if (h.TasksToDo.Count > 0) {
					MessageCallback[] msg = WorkTasks(h, delta);
					res.AddRange(msg);
					msg = UpdateLife(h, delta);
					res.AddRange(msg);
				}
			}

			return res.ToArray();
		}

		private MessageCallback[] UpdateLife(Human h, float delta) {
			if (h.DepressionLevel >= GetDayTime() * 14) {
				h.IsInDepression = true;
				// Depression
			} else {
				h.IsInDepression = false;

				if (h.DepressionLevel < 0) {
					h.KingAccpetance = (float)Math.Pow(h.KingAccpetance, 0.99f);
				} else if (h.Mood <= 0.3) {
					h.DepressionLevel += delta;
					h.KingAccpetance = (float)Math.Pow(h.KingAccpetance, 1.01f);
				} else if (h.Mood >= 0.8) {
					h.DepressionLevel -= delta;
				}
			}


			return new MessageCallback[] { };
		}

		private MessageCallback[] WorkTasks(Human h, float delta) {
			List<MessageCallback> res = new List<MessageCallback>();
			HumanTask t = h.TasksToDo.FirstOrDefault();
			while (t != null) {
				t.TimeLeft -= delta;
				h.Worked(delta, t.Difficulty);
				if (t.TimeLeft <= 0) {
					if (t.Skill != null) {
						h.UpgradeSkill(t.Skill, 0.999f);
					}
					h.TasksToDo.RemoveAt(0);
					delta = delta + t.TimeLeft;
					if (t.Repeating) {
						t.CalculateTaskTime(h, t.Difficulty, t.Skill);
						if (!h.AddTask(t)) {
							res.Add(new MessageCallback(
								Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
								ECharacter.Knight
							));
						}
					}
					switch (t.TaskType) {
						case ETask.Building:
							Building b = new Building();
							b.Id = (Guid)CombGuidGenerator.Instance.GenerateId(this, b);
							b.KingdomId = this.UserId;
							b.Name = t.Destination;
							b.Quality = h.GetSkill("building");
							Buildings.Add(b);
							res.Add(new MessageCallback(string.Format(Locale.Get("notifications.builded", this.Language), b.GetName(this.Language)), ECharacter.King));
							break;

						case ETask.Training:
							h.UpgradeSkill(t.Destination);
							break;

						case ETask.Extraction:
						case ETask.Creation:
							AddResource(t.Destination, 1);
							break;
					}
					t = h.TasksToDo.FirstOrDefault();
				} else {
					break;
				}
			}
			return res.ToArray();
		}

		public MessageCallback Create(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight);
			}

			ResourceDescription r = (ResourceDescription)dict["resource"];

			foreach (IDependency dep in r.CreationDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}

			Human h = FindBySkill(r.Skill);
			HumanTask t = new HumanTask() {
				Destination = r.Name,
				TaskType = ETask.Creation
			};
			t.CalculateTaskTime(h, r.Difficulty, r.Skill);
			foreach (IDependency dep in r.ExtractionDependencies) {
				t.Use(dep.Use(this));
			}

			if (!h.AddTask(t)) {
				return new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				);
			} else {

				return new MessageCallback(Locale.Get("answers.yes", this.Language));
			}
		}

		private void AddResource(string r, int cnt) {
			Resources[r] = cnt + Resources.Get(r);
		}

		public MessageCallback Extract(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight);
			}

			ResourceDescription r = (ResourceDescription)dict["resource"];

			foreach (IDependency dep in r.ExtractionDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}

			Human h = FindBySkill("mining");
			HumanTask t = new HumanTask() {
				Destination = r.Name,
				TaskType = ETask.Extraction,
				Repeating = true
			};
			t.CalculateTaskTime(h, r.Difficulty, r.Skill);
			foreach (IDependency dep in r.ExtractionDependencies) {
				t.Use(dep.Use(this));
			}

			if (!h.AddTask(t)) {
				return new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				);
			} else {

				return new MessageCallback(Locale.Get("answers.yes", this.Language));
			}
		}

		public MessageCallback Hire(Dictionary<string, object> dict) {
			int count = (int)(dict.Get("count") ?? 1);
			if (!dict.ContainsKey("job")) {
				return new MessageCallback(Locale.Get("jobs.no_job", this.Language), ECharacter.Knight);
			}

			JobDescription j = (JobDescription)dict["job"];
			List<IDependency> deps = new List<IDependency>(j.HireDepence);

			for (int i = 0; i < count; ++i) {
				foreach (IDependency dep in deps) {
					Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
					if (!res.Item1) {
						return res.Item2;
					}
				}
				foreach (IDependency dep in j.HireDepence) {
					dep.Use(this);
				}
				CreateHumanWithSkills(new Tuple<string, float>[] {
					new Tuple<string, float>(j.SkillName, RandomManager.NextGoodSkill())
				});
			}

			return new MessageCallback(Locale.Get("answers.yes", this.Language), ECharacter.Knight);
		}

		public MessageCallback Train(Dictionary<string, object> dict) {
			int count = 1;
			if (dict.ContainsKey("count")) { count = (int)dict["count"]; }
			if (!dict.ContainsKey("job")) {
				return new MessageCallback(Locale.Get("jobs.no_job", this.Language), ECharacter.Knight);
			}
			JobDescription j = (JobDescription)dict["job"];
			List<IDependency> deps = new List<IDependency>(j.TrainDepence);
			deps.Add(new HumanDependency());

			foreach (IDependency dep in deps) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}
			Human h = FindBySkill(j.SkillName, false);
			HumanTask task = new HumanTask() {
				Destination = j.SkillName,
				TaskType = ETask.Training
			};
			foreach (IDependency dep in j.HireDepence) {
				task.Use(dep.Use(this));
			}

			task.CalculateTaskTime(h);
			if (!h.AddTask(task)) {
				return new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				);
			} else {
				return new MessageCallback(Locale.Get("answers.yes", this.Language));
			}
		}

		private void CreateHumanWithSkills(Tuple<string, float>[] tuples) {
			Human h = new Human("John", RandomManager.Select<ESex>(), 32);
			h.HumanId = (Guid)CombGuidGenerator.Instance.GenerateId(this, h);
			foreach (string skill in ContentManager.GetSkills()) {
				h.SetSkill(skill, RandomManager.NextDefaultSkill());
			}
			foreach (Tuple<string, float> t in tuples) {
				h.SetSkill(t.Item1, t.Item2);
			}
			Humans.Add(h);
		}

		public MessageCallback Build(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("building")) {
				return new MessageCallback(Locale.Get("buildings.no_building", this.Language), ECharacter.Knight);
			}
			BuildingDescription b = (BuildingDescription)dict["building"];

			foreach (IDependency dep in b.Dependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}

			Human h = FindBySkill("building");
			HumanTask t = new HumanTask();
			t.Destination = b.Name;
			t.TaskType = ETask.Building;

			foreach (IDependency dep in b.Dependencies) {
				t.Use(dep.Use(this));
			}

			t.CalculateTaskTime(h);

			if (!h.AddTask(t)) {
				return new MessageCallback(
					Locale.Get(string.Format("problems.dont_work", h.GetName(Language)), Language),
					ECharacter.Knight
				);
			} else {
				return new MessageCallback(Locale.Get("answers.yes", this.Language));
			}
		}

		private Human FindBySkill(string skillname, bool maximal = true) {
			return Humans.Aggregate((curMin, x) =>
				(curMin == null || (maximal ? x.GetSkill(skillname) < curMin.GetSkill(skillname) : x.GetSkill(skillname) > curMin.GetSkill(skillname)) ? x : curMin)
			);
		}
	}
}
