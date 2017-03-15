using System;

namespace YesSir.Shared.Users {
	public class UserInfo {
		public Guid Id;
		public string Name;
		public string Type;
		public EState State = EState.Main;
		public string ThirdPartyId = "";
		public string Language = "ru";

		public bool Equals(UserInfo ui) {
			return ui.Id == Id || (ui.Type == Type && ui.ThirdPartyId == ThirdPartyId);
		}
	}

	public enum EState {
		Main = 0,
		Dictates = 1
	}
}
