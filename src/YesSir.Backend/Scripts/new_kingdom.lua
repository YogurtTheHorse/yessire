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

	kingdom:create_human_with_skills ("building", rand.next_good_skill ())
	kingdom:add_building ("mill")
	kingdom:add_building ("field")
	kingdom:add_building ("field")

	kingdom:create_human_with_skills ("mining")
	kingdom:create_human_with_skills ("mining")
	kingdom:create_human_with_skills ("farming")
	kingdom:create_human_with_skills ("dyplomacy")

	return kingdom
end

return f