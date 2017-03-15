using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;
using YesSir.Backend.Entities.Items;
using System.Collections;

namespace YesSir.Backend.Entities.Kingdoms {
	public partial class Kingdom {
		public Guid UserId;
		public string Language, Name;
		public List<Human> Humans;
		public List<Building> Buildings;
		public Dictionary<string, List<Item>> Resources;
		public bool Starving = false;
		public object Temp;
		public Point Coordinate;

		public Kingdom() {
			Humans = new List<Human>();
			Buildings = new List<Building>();
			Resources = new Dictionary<string, List<Item>>();
			Coordinate = RandomManager.GenerateKingdomPoint();
		}

		public Kingdom(UserInfo userinfo) : this() {
			this.UserId = userinfo.Id;
			this.Language = userinfo.Language;

			GenerateName();
		}

		public void GenerateName() {
			object _variants;

			if (!Locale.TryGet(Language + ".kingdom_names", "names", out _variants)) { return; }

			IEnumerable chosed_seq = (_variants as IEnumerable<object>).RandomChoice() as IEnumerable;

			List<string> names = new List<string>();
			foreach (object obj in chosed_seq) {
				names.Add((obj as IEnumerable<object>).RandomChoice() as string);
			}
			Name = string.Join(" ", names);
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
				Killed(h);
				
			}

			return res.ToArray();
		}

		private void Killed(Human h) {
			for (int i = 0; i < Humans.Count; i++) {
				if (Humans[i].HumanId == h.HumanId) continue;
				Humans[i].Mood = (float)Math.Pow(Humans[i].Mood, Math.Pow(4, Humans[i].GetFriendShip(h)));
			}

			Humans.Remove(h);
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
			Building b = ContentManager.NewBuilding(name);
			b.Id = (Guid)CombGuidGenerator.Instance.GenerateId(this, b);
			b.KingdomId = UserId;
			b.Quality = quality;
			
			Buildings.Add(b);
		}

		private MessageCallback[] WorkTasks(Human h, float delta) {
			List<MessageCallback> res = new List<MessageCallback>();
			HumanTask t = h.TasksToDo.FirstOrDefault();
			while (t != null) {
				if (t.TaskType != ETask.ListeningKing) {
					t.TimeLeft -= delta;
				}


				int k = h.Worked(delta, t.Difficulty) ? 1 : -1;

				var soworkers = new List<Human>();
				for (int i = 0; i < t.InUse.Count; ++i) {
					Guid[] owners_ids = t.InUse[i].GetOwners();
					for (int j = 0; j < owners_ids.Length; ++j) {
						h.UpdateFriendship(owners_ids[j], k*delta);
					}
				}

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

		private Human FindHuman(Guid s) {
			for (int i = 0; i < Humans.Count; i++) {
				if (s == Humans[i].HumanId) { return Humans[i]; }
			}
			
			return null;
		}

		public void AddResource(string r, int cnt, float quality = 0.5f) {
			if (Resources.ContainsKey(r)) {
				Resources[r].AddRange(Item.GenerateItems(cnt, r, quality));
			} else {
				Resources[r] = Item.GenerateItems(cnt, r, quality);
			}
		}

		public void CreateHumanWithSkills(string name) {
			CreateHumanWithSkills(name, RandomManager.NextGoodSkill());
		}

		public void CreateHumanWithSkills(string name, float skill_lev) {
			Human h = new Human(GenerateName(this.Language), RandomManager.Select<ESex>(), 32);
			h.HumanId = (Guid)CombGuidGenerator.Instance.GenerateId(this, h);
			h.KingdomId = UserId;

			foreach (string skill in ContentManager.GetSkills()) {
				h.SetSkill(skill, RandomManager.NextDefaultSkill());
			}
			h.SetSkill(name, skill_lev);
			
			Humans.Add(h);
		}

		public void CreateHumanWithSkills(Tuple<string, float>[] tuples) {
			Human h = new Human(GenerateName(this.Language), RandomManager.Select<ESex>(), 32);
			h.HumanId = (Guid)CombGuidGenerator.Instance.GenerateId(this, h);
			h.KingdomId = UserId;

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

		public Human FindBySkill(string skillname, bool maximal = true) {
			int mn_cnt = Humans.Min((h) => h.TasksToDo.Count);
			var selected = Humans.Where((h) => h.TasksToDo.Count == mn_cnt);

			Human res = null;
			float mx = maximal ? -1 : 2;

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
