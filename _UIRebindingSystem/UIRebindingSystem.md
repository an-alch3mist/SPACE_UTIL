# current behaviour

+ UI Populate OnEnable(GameObject is the container of entire InputRebindingSystem)
	+ iterate thorugh actionMap
		+ action
			+ binding

+ the effective path with "" shall appear as "Not Bound" in text
+ when clicked on a button "press any key...."
	+ if user press keyboard esc/enter/backspace/delete -> keep binding back to what it was -> deselect
	+ if user press any other button -> keep binding back to what it was -> deselect -> "press any key...." on this new button
	+ if user keyboard key -> assign the button and effective path for that linked input binding with this new input

+ when save button pressed -> save override binding as json into a file using LOG.SaveGameData(GameDataType.inputKeyBindings)
+ when reset button pressed -> reset all bindings to normal



# required behaviour -0

+ save to override JSON when any if key binding is altered, that also includes when reset is pressed
+ in a certain action map if same binding was entered, all other keyBindings with same keyBinding shall be override to empty
+ a pop-up appear when entered to modify a kebinding, which lasts for 5sec, if esc pressed any point in time the keyBinding shall be mapped to empty
+ keybinding supoorts mouse/keyboard for now