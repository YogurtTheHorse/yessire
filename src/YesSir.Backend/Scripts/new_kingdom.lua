local f = function (kingdom)
	kingdom:add_resource ("money", 1000, 0.5, 0.05)
	kingdom:add_resource ("wood", 200, 0.5, 0.05)
	kingdom:add_resource ("corn", 1000, 0.5, 0.05)
	kingdom:add_resource ("rock", 50, 0.5, 0.05)
	kingdom:add_resource ("bread", 50, 0.5, 0.05)
	kingdom:add_resource ("fish", 50, 0.5, 0.05)
	kingdom:add_resource ("bucket", 50, 0.5, 0.05)
	kingdom:add_resource ("water_bucket", 20, 0.5, 0.05)
	kingdom:add_resource ("grain", 1000, 0.5, 0.05)
	kingdom:add_resource ("grain", 1000, 0.5, 0.05)

	kingdom:add_building ("mill")
	kingdom:add_building ("field")
	kingdom:add_building ("field")

	profs = {
		{ "building", 1 },
		{ "mining", 2 },
		{ "farming", 2 },
		{ "dyplomacy", 1 }
	}

	for _, des in pairs (profs) do
		name = des[0]
		count = des[1]

		for i=1, count do
			kingdom:create_human_with_skills (name)
		end
	end

	return kingdom
end

return f