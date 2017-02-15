using System;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Building : IUsable {
		public Guid Id { get; set; }
		public Guid KingdomId;

		public string Name;
		public float Quality;

		public bool IsBusy { get; set; } = false;

		public string GetName(string language) {
			return Locale.Get("buildings." + Name + ".name", language);
		}

		public EUsableType GetUsableType() {
			return EUsableType.Building;
		}
	}
}