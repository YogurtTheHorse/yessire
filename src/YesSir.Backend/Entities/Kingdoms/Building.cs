using System;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Entities.Kingdoms {
	[MoonSharp.Interpreter.MoonSharpUserData]
	public class Building : IUsable {
		public Guid Id { get; set; }
		public int AvaibalePlace = 1; // Count of human that can be at once in building
		public int UsedPlace = 0;
		public Guid KingdomId;

		public string Name;
		public float Quality;

		public bool IsBusy
		{
			get
			{
				return AvaibalePlace <= UsedPlace;
			}
			set
			{
				if (value) {
					UsedPlace++;
				} else {
					UsedPlace--;
				}
			}
		}

		public string GetName(string language) {
			return Locale.Get("buildings." + Name + ".name", language);
		}

		public EUsableType GetUsableType() {
			return EUsableType.Building;
		}
	}
}