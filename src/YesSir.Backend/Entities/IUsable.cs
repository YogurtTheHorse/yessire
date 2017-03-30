using System;
using YesSir.Backend.Entities.Kingdoms;

namespace YesSir.Backend.Entities {
	public interface IUsable {
		EUsableType GetUsableType();
		bool IsBusy { get; set; }
		Guid Id { get; set; }
		Guid[] GetOwners();

		void OnUse();
		void OnUse(Guid h);
	}
	
	public enum EUsableType {
		Building,
		Item,
		Container
	}
}
