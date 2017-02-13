using YesSir.Backend.Commands.Dependencies;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Descriptions {
	public struct JobDescription {
		public string Name, SkillName;
		public IDependency[] HireDepence, TrainDepence, WorkDependence;

		public string[] GetAcceptableNames(string language="full") {
			return Locale.GetArray("jobs." + Name + ".names", language);
		}
	}
}
