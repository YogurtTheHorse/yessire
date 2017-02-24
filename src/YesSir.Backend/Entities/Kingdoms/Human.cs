using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Kingdoms {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public class Human {
		public Guid HumanId;
		public Guid KingdomId;
		public float DepressionLevel = 0;
		public float Mood = 0.7f;
		public float Satiety = 0.9f;
		public float KingAccpetance = 0.9f;

		public string Name;
		public ESex Sex;
		public float Age = 0;
		public List<HumanTask> TasksToDo;
		public Dictionary<string, float> Skills;
		public bool Died = false;

		public bool IsInDepression = false;


		public Human() {
			Skills = new Dictionary<string, float>();
		}

		public Human(string name, ESex sex, float age) : this() {
			TasksToDo = new List<HumanTask>();
			Name = name;
			Sex = sex;
			Age = age;
		}

		public bool AddTask(HumanTask t) {
			if (IsInDepression || RandomManager.QuanticFloat(1 - KingAccpetance) > 0.5f) {
				return false;
			}

			TasksToDo.Add(t);

			return true;
		}

		public float GetSkill(string skill) {
			return Skills.ContainsKey(skill) ? Skills[skill] : 0;
		}

		public void UpgradeSkill(string skill, float power = 0.6f) {
			float c = GetSkill(skill);
			Skills[skill] = (float)Math.Pow(c, power);
		}

		public void SetSkill(string name, float skill) {
			Skills[name] = skill;
		}

		public void Worked(float delta, float difficulty) {
			Satiety -= delta * difficulty / 10f;
		}

		public string GetName(string language) {
			return GetJobName(language) + " " + Name;
		}

		public string GetStatus(string language) {
			if (TasksToDo.Count == 0) {
				return Locale.Get("status.idle", language);
			} else {
				switch (TasksToDo[0].TaskType) {
					case ETask.Building:
						return string.Format(Locale.Get("status.building", language), Locale.Get("buildings." + TasksToDo[0].Destination + ".name", language));

					case ETask.Creating:
					case ETask.Extracting:
						var tsk = TasksToDo[0].TaskType.ToString().ToLower();
						return string.Format(Locale.Get("status." + tsk, language), Locale.Get("resources." + TasksToDo[0].Destination + ".name", language));

					default:
						return "-";
				}
			}
		}

		private string GetJobName(string language) {
			string bestat = Skills.Aggregate((first, second) => first.Value > second.Value ? first : second).Key;
			return ContentManager.GetJobBySkill(bestat, language);
		}

		public bool Eat(Kingdom kingdom) {
			foreach (string food in ContentManager.GetFood()) {
				if (kingdom.TakeResource(food, 1)) {
					Satiety += 0.05f;

					return true;
				}
			}

			return false;
		}
	}

	public enum ESex {
		Female,
		Male
	}
}
