using System.Collections.Generic;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Field : Building {
		public string Culture;
		public int Count;
		public float TimeLeft;
		public bool IsWorking;

		public new bool IsBusy
		{
			get { return base.IsBusy && IsWorking; }
			set { base.IsBusy = value; }
		}

		public Field() {
			Count = 0;
			IsWorking = false;
			Name = "field";
		}

		public Field(Building b) : this() {
			this.Id = b.Id;
			this.KingdomId = b.KingdomId;
			this.Quality = b.Quality;
		}

		public override void OnUse() {
			base.OnUse();
			IsWorking = true;
		}

		public override MessageCallback[] Update(Kingdom k, float delta) {
			List<MessageCallback> res = new List<MessageCallback>(base.Update(k, delta));

			if (IsWorking) {
				TimeLeft -= delta;

				if (TimeLeft < 0) {
					IsWorking = false;

					k.AddResource(Culture, (int)(Count * Quality), Quality);
				}
			}

			return res.ToArray();
		}
	}
}
