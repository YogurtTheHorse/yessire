using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Descriptions {
	public struct ResourceDescription {
		public string Name;
		public float Difficulty;
		public bool IsOre;

		public string[] GetAcceptableNames(string language = "full") {
			return Locale.GetArray("resources." + Name + ".names", language);
		}

		public string GetName(string language) {
			return Locale.Get("resources." + Name + ".name", language);
		}
	}
}
