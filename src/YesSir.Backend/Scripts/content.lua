cm = contentmanager

cm.RegisterJob ("peasant", "farming", 10)
cm.RegisterJob ("woodcutter", "chopping", 20);
cm.RegisterJob ("baker", "baking", 20);
cm.RegisterJob ("builder", "building", 100);
cm.RegisterJob ("miner", "mining", 100);

cm.RegisterBuilding ("forge", {
	rock = 5;
	iron = 5;
})

cm.RegisterBuilding ("field", {
	wood = 5;
})

cm.RegisterBuilding ("mill", {
	wood = 5;
	rock = 3;
})

cm.RegisterBuilding ("bakery", {
	wood = 5;
	rock = 5;
})

cm.RegisterResource ("corn", 1, "farming", {
	building_dep ("field", true)
})