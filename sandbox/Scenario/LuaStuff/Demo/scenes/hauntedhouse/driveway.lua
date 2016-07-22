--
-- The driveway in front of the house.
--

driveway = {}
driveway.id = "urn:location:hauntedhouse:driveway"
driveway.name = "LUA: A gravel driveway in front of a mansion."
driveway.description = [[
A large circular driveway with a odd-looking fountain on a small lawn.
The weed is slowly taking over the place. The sky is <lua>if GetGlobal('g:timeofday') == 'day' then return 'a bright blue' else return 'pitch black' end</lua>.
]]

driveway.exits = {}
driveway.exits.entrance = CreateExit("The mansion entrance", "urn:location:hauntedhouse:entrance", "The mansion entrance looms.")