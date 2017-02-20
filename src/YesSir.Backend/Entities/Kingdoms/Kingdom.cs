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
using YesSir.Backend.Entities.Items;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Kingdom {
		public Guid UserId;
		public string Language;
		public List<Human> Humans;
		public List<Building> Buildings;
		public Dictionary<string, List<Item>> Resources;
		public bool Starving = false;

		public Kingdom() {
			Humans = new List<Human>();
			Buildings = new List<Building>();
			Resources = new Dictionary<string, List<Item>>();
		}

		public Kingdom(UserInfo userinfo) : this() {
			this.UserId = userinfo.Id;
			this.Language = userinfo.Language;
		}

		public int GetResourcesCount(string resorce) {
			if (Resources.ContainsKey(resorce)) {
				return Resources[resorce].Count;
			} else {
				return 0;
			}
		}

		public bool TakeResource(string resource, int count) {
			if (GetResourcesCount(resource) >= count) {
				Resources[resource].RemoveRange(0, count);
				return true;
			} else {
				return false;
			}
		}

		public float GetDayTime() {
			return 15f;
		}

		public MessageCallback[] Update(float delta) {
			List<MessageCallback> res = new List<MessageCallback>();
			List<Human> died = new List<Human>();
			foreach (Human h in Humans) {
				if (h.TasksToDo.Count > 0) {
					res.AddRange(WorkTasks(h, delta));
				}
				res.AddRange(UpdateLife(h, delta));

				if (h.Died) {
					died.Add(h);
				}
			}
			foreach (Building b in Buildings) {
				res.AddRange(b.Update(this, delta));
			}

			foreach (Human h in died) {
				Humans.Remove(h);
			}

			return res.ToArray();
		}

		private MessageCallback[] UpdateLife(Human h, float delta) {
			List<MessageCallback> res = new List<MessageCallback>();
			if (h.Satiety < 0.5f) {
				while (h.Satiety < 0.9f && h.Eat(this)) ;
				if (h.Satiety < 0.9f && !Starving) {
					Starving = true;

					res.Add(new MessageCallback(Locale.Get("problems.starving", Language), ECharacter.Knight));
				}
				if (h.Satiety <= 0) {
					h.Died = true;

					res.Add(new MessageCallback(string.Format(Locale.Get("problems.died", Language), h.Name), ECharacter.Knight));
					return res.ToArray();
				}
			}

			if (h.DepressionLevel >= GetDayTime() * 14) {
				h.IsInDepression = true;
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


			return res.ToArray();
		}

		public void AddBuilding(string name, float quality = 0.5f) {
			Building b = new Building();
			b.Id = (Guid)CombGuidGenerator.Instance.GenerateId(this, b);
			b.KingdomId = this.UserId;
			b.Name = name;
			b.Quality = quality;
			Buildings.Add(name == "field" ? new Field(b) : b);
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
							AddBuilding(t.Destination, h.GetSkill("building"));
							string bname = ContentManager.GetBuildingName(t.Destination, Language);
							string msg = string.Format(Locale.Get("notifications.builded", Language), bname);
							res.Add(new MessageCallback(msg, ECharacter.King));
							break;

						case ETask.Training:
							h.UpgradeSkill(t.Destination);
							break;

						case ETask.Extracting:
						case ETask.Creating:
							AddResource(t.Destination, 1, h.GetSkill(t.Skill));
							break;
					}
					t = h.TasksToDo.FirstOrDefault();
				} else {
					break;
				}
			}

			return res.ToArray();
		}

		public MessageCallback Grow(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight);
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			if (r.Culture == null) {
				return new MessageCallback(Locale.Get("resources.no_culture", this.Language), ECharacter.Farmer);
			}

			IDependency dep = new ItemDependency("water_bucket", 2);
			Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
			if (!res.Item1) {
				return res.Item2;
			}

			dep.Use(this);
			foreach (Building b in Buildings) {
				if (b is Field) {
					(b as Field).Count = 50;
					(b as Field).Culture = r.Culture;
					(b as Field).IsWorking = true;
					(b as Field).TimeLeft = 3f;
					break;
				}
			}

			return new MessageCallback(Locale.Get("answers.yes", this.Language));
		}

		public MessageCallback Create(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight);
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			foreach (IDependency dep in r.CreationDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}

			Human h = FindBySkill(r.Skill);
			HumanTask t = new HumanTask() {
				Destination = r.Name,
				TaskType = ETask.Creating
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

		public void AddResource(string r, int cnt, float quality = 0.5f) {
			if (Resources.ContainsKey(r)) {
				Resources[r].AddRange(Item.GenerateItems(cnt, r, quality));
			} else {
				Resources[r] = Item.GenerateItems(cnt, r, quality);
			}
		}

		public MessageCallback Extract(Dictionary<string, object> dict) {
			if (!dict.ContainsKey("resource")) {
				return new MessageCallback(Locale.Get("resources.no_resource", this.Language), ECharacter.Knight);
			}

			ItemDescription r = (ItemDescription)dict["resource"];

			foreach (IDependency dep in r.ExtractionDependencies) {
				Tuple<bool, MessageCallback> res = dep.CheckKingdom(this);
				if (!res.Item1) {
					return res.Item2;
				}
			}

			Human h = FindBySkill(r.Skill);
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
			Human h = new Human(GenerateName(this.Language), RandomManager.Select<ESex>(), 32);
			h.HumanId = (Guid)CombGuidGenerator.Instance.GenerateId(this, h);
			foreach (string skill in ContentManager.GetSkills()) {
				h.SetSkill(skill, RandomManager.NextDefaultSkill());
			}
			foreach (Tuple<string, float> t in tuples) {
				h.SetSkill(t.Item1, t.Item2);
			}
			Humans.Add(h);
		}

		private string GenerateName(string lang) {
			return Locale.GetArray(lang + ".firstnames", "names").RandomChoice() + " " + Locale.GetArray(lang + ".lastnames", "names").RandomChoice();
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
			int mn_cnt = Humans.Min((h) => h.TasksToDo.Count);
			var selected = Humans.Where((h) => h.TasksToDo.Count == mn_cnt);

			Human res = null;
			float mx = -1;

			foreach (var h in selected) {
				float p = h.GetSkill(skillname);

				if (maximal ? p > mx : p < mx) {
					mx = p;
					res = h;
				}
			}

			return res;
		}
	}
}
