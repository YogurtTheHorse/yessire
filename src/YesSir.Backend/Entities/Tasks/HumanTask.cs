using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YesSir.Backend.Entities {
	public class HumanTask {
		public Guid? BuildingId;
		public bool NeedBuilding {
			get {
				return BuildingId.HasValue;
			}
		}
		public bool Repeating = false;
		public string Destination;
		public ETask TaskType;
		public float TimeLeft; // In days
	}
}
