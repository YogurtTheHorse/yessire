using System;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands.Dependencies {
	public interface IDependency {
		Tuple<bool, MessageCallback> CheckKingdom(Kingdom kingdom);
		void Use(Kingdom kingdom);
	}
}
