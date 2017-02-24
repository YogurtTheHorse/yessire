using System;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

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
			get { return AvaibalePlace <= UsedPlace; }
			set { UsedPlace += value ? 1 : -1; }
		}

		public virtual MessageCallback[] Update(Kingdom k, float delta) { return new MessageCallback[] { }; }

		public string GetName(string language) {
			return Locale.Get("buildings." + Name + ".name", language);
		}

		public virtual void OnUse() { }

		public EUsableType GetUsableType() {
			return EUsableType.Building;
		}
	}
}