using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Human {
		public Guid HumanId;
		public float DepressionLevel = 0;
		public float Mood = 0.7f;
		public float Satiety = 0.9f;
		public float KingAccpetance = 0.9f;

		public string Name;
		public ESex Sex;
		public float Age = 0;
		public List<HumanTask> TasksToDo;
		public Dictionary<string, float> Skills;

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
			Satiety -= delta * difficulty / 100f;
		}

		public string GetName(string language) {
			return GetJobName(language) + " " + Name;
		}

		private string GetJobName(string language) {
			string bestat = Skills.Aggregate((first, second) => first.Value > second.Value ? first : second).Key;
			return ContentManager.GetJobBySkill(bestat, language);
		}
	}

	public enum ESex {
		Female,
		Male
	}
}
