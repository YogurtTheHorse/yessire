using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Entities.Items;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Descriptions {
	public class ItemDescription {
		public string Name;
		public float Difficulty;
		public string Skill;
		public string Culture;
		public bool Extractable = false,
					Creatable = false,
					IsFood = false;

		public IDependency[] ExtractionDependencies, CreationDependencies;

		public string[] GetAcceptableNames(string language = "full") {
			return Locale.GetArray("resources." + Name + ".names", language);
		}

		public string GetName(string language) {
			return Locale.Get("resources." + Name + ".name", language);
		}
	}
}
