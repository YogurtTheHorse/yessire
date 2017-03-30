local cm = contentmanager

print ('Loading objects')
print ('Registering jobs')

cm.register_job ("peasant", "farming", 10)
cm.register_job ("woodcutter", "chopping", 20)
cm.register_job ("baker", "baking", 20)
cm.register_job ("miner", "mining", 50)
cm.register_job ("builder", "building", 100)
cm.register_job ("smith", "smithing", 100)
cm.register_job ("ambassador", "diplomacy", 300)


print ('Registering buildings')
cm.register_building ("forge", {
	rock = 5;
	iron = 5;
})

cm.register_building ("well", {
	rock = 2;
	iron = 2;
})

cm.register_building ("field", {
	wood = 5;
}, nil, 5)

cm.register_building ("mill", {
	wood = 5;
	rock = 3;
}, nil, 5)

cm.register_building ("bakery", {
	wood = 5;
	rock = 5;
})

print ('Registering resources')
cm.register_resource ("water_bucket", 1, "farming", nil, {
	item_dep ("bucket", 1),
	building_dep ("well", true)
})

cm.register_resource ("bucket", 1, "smithing", {
	item_dep ("iron", 2),
	building_dep ("forge", true)
})

cm.register_resource ("millet", 1, nil, {
	item_dep ("grain", 2)
})
cm.RegisterGrain ("grain", "millet")

cm.register_resource ("flour", 2, "farming", nil, {
	building_dep ("mill", true),
	item_dep ("millet", 2)
})

cm.register_resource ("wood", 1, "chopping", { })

cm.register_resource ("rock", 1, "mining", { })
cm.register_resource ("iron", 1.5, "mining", { })
cm.register_resource ("gold", 2, "mining", { })

print ('Registering food')
cm.register_food ("bread", 1.5, "bakinkg", nil, {
	building_dep ("bakery", true),
	item_dep ("flour", 2)
})
cm.register_food ("fish", 1, "fishing", {
	building_dep ("fishery", true)
})
