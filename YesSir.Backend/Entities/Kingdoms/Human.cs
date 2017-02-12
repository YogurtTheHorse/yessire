using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Human {
		public Guid HumanId;
		public float Mood = 0.7f;
		public float KingAccpetance = 0.7f;

		public string Name;
		public ESex Sex;
		public float Age = 0;
		public List<HumanTask> TasksToDo;
		public Dictionary<string, float> Skills;

		public Human() {
			Skills = new Dictionary<string, float>();
		}

		public Human(string name, ESex sex, float age) : this() {
			TasksToDo = new List<HumanTask>();
			Name = name;
			Sex = sex;
			Age = age;
		}

		public float GetSkill(string skill) {
			return Skills.ContainsKey(skill) ? Skills[skill] : 0;
		}

		public void UpgradeSkill(string skill) {
			float c = GetSkill(skill);
			Skills[skill] = (float)Math.Sqrt(c);
		}

		public void SetSkill(string name, float skill) {
			Skills[name] = skill;
		}
	}

	public enum ESex {
		Female,
		Male
	}
}
