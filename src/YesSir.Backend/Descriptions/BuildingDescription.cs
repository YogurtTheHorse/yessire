using System;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Descriptions {
	public struct BuildingDescription {
		public string Name;
		public IDependency[] Dependencies;

		public string[] GetAcceptableNames(string language="full") {
			return Locale.GetArray("buildings." + Name + ".names", language);
		}

		public Building CreateBuilding() {
			return new Building() {
				Name = Name
			};
		}
	}
}
