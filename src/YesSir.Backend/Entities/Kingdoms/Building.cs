using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Building {
		public Guid KingdomId;
		public Guid BuildingId;
		
		public string Name;
		public float Quality;

		public bool IsBusy = false;

		public string GetName(string langiage) {
			return Locale.Get("buildings." + Name + ".name", langiage);
		}
	}
}
