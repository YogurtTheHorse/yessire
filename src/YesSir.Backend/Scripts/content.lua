local cm = contentmanager

cm.RegisterJob ("peasant", "farming", 10)
cm.RegisterJob ("woodcutter", "chopping", 20)
cm.RegisterJob ("baker", "baking", 20)
cm.RegisterJob ("miner", "mining", 50)
cm.RegisterJob ("builder", "building", 100)
cm.RegisterJob ("smith", "smithing", 100)


-- 
cm.RegisterBuilding ("forge", {
	rock = 5;
	iron = 5;
})

cm.RegisterBuilding ("well", {
	rock = 2;
	iron = 2;
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
cm.RegisterResource ("water_bucket", 1, "farming", nil, {
	item_dep ("bucket", 1),
	building_dep ("well", true)
})

cm.RegisterResource ("bucket", 1, "smithing", {
	item_dep ("iron", 2),
	building_dep ("forge", true)
})

--cm.RegisterResource ("grain", 1, "farming")
cm.RegisterResource ("millet", 1, nil, {
	item_dep ("grain", 2)
})
cm.RegisterGrain ("grain", "millet")

cm.RegisterResource ("flour", 2, "farming", nil, {
	building_dep ("mill", true),
	item_dep ("millet", 2)
})
cm.RegisterFood ("bread", 1.5, "bakinkg", nil, {
	building_dep ("bakery", true),
	item_dep ("flour", 2)
})
cm.RegisterFood ("fish", 1, "fishing", {
	building_dep ("fishery", true)
})

cm.RegisterResource ("wood", 1, "chopping", { })

cm.RegisterResource ("rock", 1, "mining", { })
cm.RegisterResource ("iron", 1.5, "mining", { })
cm.RegisterResource ("gold", 2, "mining", { })
