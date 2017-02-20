using System;

namespace YesSir.Backend.Entities {
	public interface IUsable {
		EUsableType GetUsableType();
		bool IsBusy { get; set; }
		Guid Id { get; set; }
	}
	
	public enum EUsableType {
		Building,
		Item,
		Container
	}
}
