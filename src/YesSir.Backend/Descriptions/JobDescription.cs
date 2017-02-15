using System;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Descriptions {
	public struct JobDescription {
		public string Name, SkillName;
		public IDependency[] HireDepence, TrainDepence, WorkDependence;

		public string[] GetAcceptableNames(string language="full") {
			return Locale.GetArray("jobs." + Name + ".names", language);
		}

		public string GetName(string language) {
			return Locale.Get("jobs." + Name + ".name", language);
		}
	}
}
