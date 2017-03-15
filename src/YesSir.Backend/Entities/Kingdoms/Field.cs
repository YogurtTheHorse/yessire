using System.Collections.Generic;
using YesSir.Backend.Managers;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Kingdoms {
	public class Field : Building {
		public string Goal;
		public int Count;
		public float TimeLeft;
		public bool IsWorking;

		public new bool IsBusy
		{
			get { return base.IsBusy && IsWorking; }
			set { base.IsBusy = value; }
		}

		public Field() : base() {
			Count = 0;
			IsWorking = false;
			Name = "field";
			AvaibalePlace = 1;
		}

		public Field(Building b) : this() {
			this.Id = b.Id;
			this.KingdomId = b.KingdomId;
			this.Quality = b.Quality;
			this.UsedPlace = b.UsedPlace;
			this.AvaibalePlace = b.AvaibalePlace;
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

					res.Add(new MessageCallback(Locale.Get($"items.{Goal}.growed", k.Language), ECharacter.Farmer));
					k.AddResource(Goal, (int)(Count * Quality), Quality);
				}
			}

			return res.ToArray();
		}
	}
}
