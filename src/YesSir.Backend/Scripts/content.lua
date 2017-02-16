local cm = contentmanager

cm.RegisterJob ("peasant", "farming", 10)
cm.RegisterJob ("woodcutter", "chopping", 20)
cm.RegisterJob ("baker", "baking", 20)
cm.RegisterJob ("builder", "building", 100)
cm.RegisterJob ("miner", "mining", 100)


-- 
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


-- Resources
cm.RegisterResource ("corn", 1, "farming", {
	building_dep ("field", true)
})

cm.RegisterResource("flour", 2, "milling", nil, {
	building_dep("mill", true), 
	resource_dep("corn", 2) 
})
cm.RegisterFood ("bread", 1.5, "bakinkg", nil, {
	building_dep ("bakery", true),
	resource_dep ("flour", 2)
})
cm.RegisterFood ("fish", 1, "fishing", {
	building_dep ("fishery", true)
})

cm.RegisterResource("wood", 1, "chopping", { })

cm.RegisterResource("rock", 1, "mining", { })
cm.RegisterResource("iron", 1.5, "mining", { })
cm.RegisterResource ("gold", 2, "mining", { })
