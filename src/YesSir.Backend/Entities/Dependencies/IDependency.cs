using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Entities.Dependencies {
	public interface IDependency {
		Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom);
		IUsable Use(Kingdom kingdom);
	}
}
