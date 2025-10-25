# current behaviour

+ UI Populate OnEnable(GameObject is the container of entire InputRebindingSystem)
	+ iterate thorugh actionMap
		+ action
			+ binding

+ the effective path with "" (nothing) shall appear as "Not Bound" in text
+ when clicked on a button "press any key...." shall be the button text
	+ if user press keyboard esc/enter/backspace/delete -> keep binding back to what it was -> deselect
	+ if user press any other button -> keep binding back to what it was -> deselect -> "press any key...." on this new button
	+ if user keyboard key -> assign the button and effective path for that linked input binding with this new input

+ when save button pressed -> save override binding as json into a file using LOG.SaveGameData(GameDataType.inputKeyBindings)
+ when reset button pressed -> reset all bindings to normal



# required behaviour -0

+ in a certain action map if same binding was entered, all other keyBindings with same keyBinding shall be override to empty
+ a pop-up appear when entered to modify a kebinding, which lasts for 5sec, if esc pressed any point in time the keyBinding shall be mapped to empty
+ keybinding supports mouse/keyboard for now



# Response -00:
 + in a certain action map if same binding was entered, all other keyBindings with same keyBinding shall be override to empty, without button live update
  
  ### Key Addition: `ClearDuplicateBindingsInActionMap()` Method
  
  This new method (lines 286-336) is called after a successful rebinding and performs the following:
  
  1. **Gets the newly assigned binding path** from the action that was just rebound
  2. **Iterates through all actions** in the same action map
  3. **Checks each binding** to see if it matches the newly assigned key
  4. **Clears duplicates** by calling `ApplyBindingOverride(i, "")` on any matching bindings (except the one just set)
  5. **Logs the results** showing how many duplicates were cleared
  
  ### Integration Points
  
  1. **Called in `OnComplete` callback** (line 235): When a rebinding successfully completes, it immediately clears any duplicates
  2. **UI refresh** (line 285): After successful rebinding, the UI is refreshed to show the "Not Bound" status on cleared bindings
  
  ### Behavior
  
  - **Example**: If you rebind "jump" to `Space` in the "character" action map, and "brake" in the "car" action map also uses `Space`, nothing happens (different action maps)
  - **Example**: If you rebind "jump" to `Space` in the "character" action map, and "shoot" in the same "character" action map also uses `Space`, then "shoot" will be automatically cleared to "Not Bound"
  
  The system respects action map boundaries and only clears duplicates within the same map, as per your requirements.


# Response -01:
with: 
  + in a certain action map if same binding was entered, all other keyBindings with same keyBinding shall be override to empty

  ### 1. **Button Tracking Dictionary** (line 198)
  ```csharp
  private Dictionary<string, Button> bindingButtonMap = new Dictionary<string, Button>();
  ```
  This dictionary stores references to all binding buttons using a unique key.

  ### 2. **Button Key Generator** (line 299)
  ```csharp
  private string GetBindingKey(InputAction action, int bindingIndex)
  ```
  Creates unique keys in the format: `"actionMapName:actionName:bindingIndex"`

  ### 3. **Store Button References** (line 171)
  When creating buttons in `UIIAMapIteration()`, each button is now stored in the dictionary with its unique key.

  ### 4. **Update Buttons on Duplicate Clear** (lines 346-353)
  When `ClearDuplicateBindingsInActionMap()` clears a duplicate binding:
  - It looks up the button using the binding key
  - Immediately updates the button text to "Not Bound"
  - No need to rebuild the entire UI

  ### 5. **Removed Full UI Refresh** (line 284)
  Since buttons are updated individually, we no longer need to call `StartCoroutine(RebindingSystemAnalysis())` after successful rebinding, which improves performance.

  ## Result:
  Now when you rebind a key that's already assigned elsewhere in the same action map, you'll instantly see the duplicate binding's button change to "Not Bound" without any UI flickering or full refresh!

```cs
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using SPACE_UTIL;
using SPACE_SYNTAX;

namespace SPACE_UISystem
{
	// depend on external: GameStore.playerIA, GameDataType.inputKeyBindings
	public class UIRebindingSystem_NEW : MonoBehaviour
	{
		#region SEEK
		// Track the active rebinding operation and button
		private InputActionRebindingExtensions.RebindingOperation activeRebindingOperation;
		private Button activeRebindingButton;
		private string activeRebindingOriginalText;

		// Cancellation keys
		private List<string> cancelKeys = new List<string>
		{
			"<Keyboard>/escape",
			"<Keyboard>/backspace",
			"<Keyboard>/delete",
		};
		#endregion

		private void Awake()
		{
			// Debug.Log(C.method("Awake", this, "white"));
		}

		[TextArea(minLines: 22, maxLines: 24)]
		[SerializeField] string README = $@"# file structure: 
UIRebindingSystem( -> Attach {typeof(UIRebindingSystem).Name}.cs to UIRebindingSystem )
	template--scroll_view/viewport/content
		template--row
			action name button(0)
			binding button(1)
			binding button(2)
			.
			.
	save / reset [panel]
		start button
		save button
	close button [button]

# reference [serializeField]
- template -- button Prefab
- contentHolder (got contentSizeFitter, VerticalLayoutGroup components Attached)
- template -- row Prefab
- save, reset, close window button

# make sure: 
- There is PlayerInputActions.cs generated from inputAction
- GameStore.playerIA shall lead to one of its instance";

		PlayerInputActions IA;
		private void OnEnable()
		{
			Debug.Log(C.method("OnEnable", this, "white"));

			// playerIA from GameStore
			this.IA = GameStore.playerIA;

			// start of rebinding UI initialization and routine
			StopAllCoroutines();
			CancelActiveRebinding();
			StartCoroutine(STIMULATE());

			// Hook up reset and save buttons
			if (this._resetBinding != null)
				_resetBinding.onClick.AddListener(ResetAllBindingsToDefault);

			if (this._saveBinding != null)
				_saveBinding.onClick.AddListener(SaveBindings);

			if (this._closeBtn != null)
				_closeBtn.onClick.AddListener(() => { this.gameObject.SetActive(false); });
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(1))
			{
				// the start of routine
				// started from Start() [UnityLifeCycle]
			}
		}

		// Cancel any active rebinding operation
		private void CancelActiveRebinding()
		{
			if (activeRebindingOperation != null)
			{
				try
				{
					activeRebindingOperation.Cancel();
				}
				catch (Exception e)
				{
					Debug.Log($"Error canceling rebinding operation: {e.Message}".colorTag("yellow"));
				}
				finally
				{
					activeRebindingOperation = null;
				}

				// Restore the original button text
				if (activeRebindingButton != null && !string.IsNullOrEmpty(activeRebindingOriginalText))
				{
					activeRebindingButton.setBtnTxt(activeRebindingOriginalText);
					activeRebindingButton = null;
					activeRebindingOriginalText = null;
				}
			}
		}

		IEnumerator STIMULATE()
		{
			yield return null;
			yield return RebindingSystemAnalysis();
			yield break;
		}

		[Header("templateUI/UI elem reference")]
		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		[SerializeField] Button _resetBinding;
		[SerializeField] Button _saveBinding;
		[SerializeField] Button _closeBtn;

		IEnumerator RebindingSystemAnalysis()
		{
			yield return null;
			this.UIIAMapIteration(this.IA);
		}

		void UIIAMapIteration(PlayerInputActions IA)
		{
			// Clear existing UI
			this._contentScrollViewTr.clearLeaves();

			// Clear the button map when rebuilding UI
			bindingButtonMap.Clear();

			// Iterate through ALL action maps
			foreach (var actionMap in IA.asset.actionMaps)
			{
				// Create a header/separator for each action map
				GameObject headerRow = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr); headerRow.transform.clearLeaves();

				// Create a non-clickable label button for the action map name
				Button headerBtn = GameObject.Instantiate(this._buttonPrefab, headerRow.transform).GC<Button>();
				headerBtn.setBtnTxt($"== {actionMap.name.ToUpper()} ==");
				headerBtn.interactable = false; // Make it non-interactive

				// rebinding buttons
				foreach (InputAction action in actionMap.actions)
				{
					Transform newRowTr = null;

					// do nothing if its composite action
					if (action.bindings[0].isComposite == true)
						continue;

					// button to show case action
					newRowTr = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr).transform; newRowTr.clearLeaves();
					GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(action.name);

					for (int i = 0; i < action.bindings.Count; i += 1)
					{
						InputBinding binding = action.bindings[i];

						if (binding.isComposite)
						{
							LOG.AddLog($"  Binding[{i}]: COMPOSITE '{binding.name}'");
							continue;
						}
						if (binding.isPartOfComposite)
						{
							continue;
						}

						// Create button with rebinding functionality
						Button btn = GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>();

						UpdateButtonText(btn, action, i);

						// NEW: Store button reference in dictionary
						string buttonKey = GetBindingKey(action, i);
						bindingButtonMap[buttonKey] = btn;

						InputAction currentAction = action;
						int currentBindingIndex = i;

						btn.onClick.AddListener(() =>
						{
							CancelActiveRebinding();
							this.StopAllCoroutines();

							if (EventSystem.current != null)
								EventSystem.current.SetSelectedGameObject(null);

							this.StartCoroutine(PerformRebinding(currentAction, currentBindingIndex, btn));
						});

						LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
					}
				}
			}
		}

		// NEW: Dictionary to track button references for each action/binding pair
		private Dictionary<string, Button> bindingButtonMap = new Dictionary<string, Button>();

		IEnumerator PerformRebinding(InputAction action, int bindingIndex, Button button)
		{
			activeRebindingButton = button;
			// MODIFY: Store action and binding index instead of text string
			// We'll use these to get the correct display text when needed
			activeRebindingOriginalText = null; // Not needed anymore, but keeping for cleanup

			button.setBtnTxt("Press A Key....");

			LOG.H($"Rebinding {action.name}");
			LOG.AddLog($"Original binding: {action.bindings[bindingIndex].effectivePath}", "");

			// Get the action map that this action belongs to
			InputActionMap actionMap = action.actionMap;
			// disable character actionMap for rebinding
			// actionMap.Disable();
			IA.Disable();

			bool done = false;
			bool wasCancelled = false;
			Debug.Log($"Press any key to rebind {action.name} (ESC/Backspace/Delete to cancel)...".colorTag("lime"));

			activeRebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				// REMOVE: These don't reliably prevent anyKey binding
				// .WithControlsExcluding("<Keyboard>/escape")
				// .WithControlsExcluding("<Keyboard>/backspace")
				// .WithControlsExcluding("<Keyboard>/delete")
				// .WithControlsExcluding("<Keyboard>/enter")

				// ADD: Use OnPotentialMatch to intercept BEFORE binding completes
				.OnPotentialMatch(op =>
				{
					// Check if the potential match is one of our cancel keys
					var control = op.candidates.Count > 0 ? op.candidates[0] : null;
					if (control != null)
					{
						string path = control.path.ToLower();

						// If it's a cancel key, manually cancel the operation
						if (path.Contains("/escape") ||
							path.Contains("/backspace") ||
							path.Contains("/delete") ||
							path.Contains("/enter") ||
							path.Contains("/numpadEnter"))
						{
							Debug.Log($"Intercepted cancel key: {control.path}".colorTag("orange"));
							op.Cancel();
							return;
						}
					}
				})

				.OnMatchWaitForAnother(0.05f)
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog($".OnComplete() NewBinding: {action.bindings[bindingIndex].effectivePath}");
					Debug.Log($".OnComplete() {action.bindings[bindingIndex].effectivePath}".colorTag("lime"));

					// NEW: Clear duplicate bindings in the same action map
					ClearDuplicateBindingsInActionMap(action, bindingIndex);

					// MODIFY: Use helper method to update button text
					UpdateButtonText(button, action, bindingIndex);

					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					done = true;

					try
					{
						// completed -> op.Dispose()
						// canceled -> op.Cancel()
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.Log($"Error disposing rebinding operation in OnComplete: {e.Message}".colorTag("yellow"));
					}
				}))
				.OnCancel((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog(".OnCancel() with cancel key");
					Debug.Log(".OnCancel()".colorTag("lime"));

					// MODIFY: Use helper method to restore button text based on current binding state
					UpdateButtonText(button, action, bindingIndex);

					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					wasCancelled = true;
					done = true;

					try
					{
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.Log($"Error disposing rebinding operation in OnCancel: {e.Message}".colorTag("yellow"));
					}
				}))
				.Start();

			// Manual cancellation check for multiple keys
			while (!done)
			{
				foreach (string cancelPath in cancelKeys)
				{
					if (IsCancelKeyPressed(cancelPath)) // not required since the input leading to cancel are intercepted in Potential Match which fires before OnComplete, or OnCancel
					{
						Debug.Log($"Cancel key pressed: {cancelPath}".colorTag("yellow"));
						activeRebindingOperation?.Cancel();
						break;
					}
				}
				yield return null;
			}

			// reEnable character actionMap, done with a certain Rebinding/cancelRebinding
			// actionMap.Enable();
			IA.Enable();

			if (wasCancelled == false)
			{
				// successfully assigned
				// No need to refresh entire UI anymore since buttons are updated individually
			}
		}

		#region Helper

		/// <summary>
		/// NEW: Generate a unique key for each action/binding pair
		/// </summary>
		private string GetBindingKey(InputAction action, int bindingIndex)
		{
			return $"{action.actionMap.name}:{action.name}:{bindingIndex}";
		}

		/// <summary>
		/// NEW: Clear duplicate bindings within the same action map
		/// If the newly assigned binding already exists elsewhere in the action map, clear those duplicates
		/// </summary>
		private void ClearDuplicateBindingsInActionMap(InputAction justBoundAction, int justBoundBindingIndex)
		{
			// Get the newly assigned binding path
			string newBindingPath = justBoundAction.bindings[justBoundBindingIndex].effectivePath;

			// If the binding is empty, nothing to check
			if (string.IsNullOrEmpty(newBindingPath))
			{
				return;
			}

			// Get the action map this action belongs to
			InputActionMap actionMap = justBoundAction.actionMap;

			Debug.Log($"Checking for duplicate bindings of '{newBindingPath}' in action map '{actionMap.name}'".colorTag("cyan"));

			int clearedCount = 0;

			// Iterate through all actions in the same action map
			foreach (InputAction otherAction in actionMap.actions)
			{
				// Iterate through all bindings in this action
				for (int i = 0; i < otherAction.bindings.Count; i++)
				{
					InputBinding otherBinding = otherAction.bindings[i];

					// Skip composite bindings and parts of composite
					if (otherBinding.isComposite || otherBinding.isPartOfComposite)
					{
						continue;
					}

					// Skip the binding we just set (same action and same index)
					if (otherAction == justBoundAction && i == justBoundBindingIndex)
					{
						continue;
					}

					// Check if this binding has the same path
					if (otherBinding.effectivePath == newBindingPath)
					{
						// Clear this duplicate binding
						otherAction.ApplyBindingOverride(i, "");
						clearedCount++;

						Debug.Log($"Cleared duplicate binding: {otherAction.name}[{i}] had '{newBindingPath}'".colorTag("yellow"));

						// NEW: Update the button text for the cleared binding
						string buttonKey = GetBindingKey(otherAction, i);
						if (bindingButtonMap.ContainsKey(buttonKey))
						{
							Button buttonToUpdate = bindingButtonMap[buttonKey];
							UpdateButtonText(buttonToUpdate, otherAction, i);
							Debug.Log($"Updated button text for {otherAction.name}[{i}] to 'Not Bound'".colorTag("cyan"));
						}
					}
				}
			}

			if (clearedCount > 0)
			{
				Debug.Log($"Cleared {clearedCount} duplicate binding(s) in action map '{actionMap.name}'".colorTag("green"));
			}
			else
			{
				Debug.Log($"No duplicates found for '{newBindingPath}'".colorTag("cyan"));
			}
		}

		// MODIFY: Add new helper method to update button text based on binding
		private void UpdateButtonText(Button button, InputAction action, int bindingIndex)
		{
			string displayText = action.bindings[bindingIndex].ToDisplayString();

			// If the binding is empty or returns empty string, show "Not Bound"
			if (string.IsNullOrEmpty(displayText) ||
				string.IsNullOrEmpty(action.bindings[bindingIndex].effectivePath))
			{
				displayText = "@";
			}

			button.setBtnTxt(displayText);
		}

		// Helper method to check if a cancel key is pressed
		private bool IsCancelKeyPressed(string controlPath)
		{
			var control = InputSystem.FindControl(controlPath);
			if (control is UnityEngine.InputSystem.Controls.ButtonControl button)
			{
				return button.wasPressedThisFrame;
			}
			return false;
		}

		// RESET FUNCTIONALITY
		/// <summary>
		/// Reset a specific binding to its default value
		/// </summary>
		private void ResetBindingToDefault(InputAction action, int bindingIndex)
		{
			action.RemoveBindingOverride(bindingIndex);
			Debug.Log($"Reset binding {bindingIndex} of action '{action.name}' to default".colorTag("cyan"));
		}

		/// <summary>
		/// Reset all bindings for a specific action to defaults
		/// </summary>
		private void ResetActionToDefault(InputAction action)
		{
			action.RemoveAllBindingOverrides();
			Debug.Log($"Reset all bindings for action '{action.name}' to defaults".colorTag("cyan"));
		}

		/// <summary>
		/// Reset all bindings in an action map to defaults
		/// </summary>
		private void ResetActionMapToDefault(InputActionMap actionMap)
		{
			actionMap.RemoveAllBindingOverrides();
			Debug.Log($"Reset all bindings in action map '{actionMap.name}' to defaults".colorTag("cyan"));
		}
		#endregion

		/// <summary>
		/// Reset ALL bindings in the entire InputActionAsset to defaults
		/// </summary>
		public void ResetAllBindingsToDefault()
		{
			// Method 1: Reset the entire asset
			IA.RemoveAllBindingOverrides();

			Debug.Log("All input bindings reset to defaults! (note: not saved yet)".colorTag("green"));

			// Do not Save override until save was pressed
			// Clear saved overrides
			// LOG.SaveGameData(GameDataType.inputKeyBindings, "");

			// Refresh the UI
			StartCoroutine(RebindingSystemAnalysis());
		}

		/// <summary>
		/// Save current bindings
		/// </summary>
		private void SaveBindings()
		{
			string json = IA.SaveBindingOverridesAsJson();
			LOG.SaveGameData(GameDataType.inputKeyBindings, json);

			Debug.Log("Bindings saved!".colorTag("green"));
			LOG.AddLog("Saved JSON:", "json");
			LOG.AddLog(json, "");
		}

		/// <summary>
		/// Load saved bindings (call this on game start)
		/// </summary>
		public void LoadSavedBindings()
		{
			// Assuming you have a method to load the saved JSON
			string savedJson = LOG.LoadGameData(GameDataType.inputKeyBindings);

			if (!string.IsNullOrEmpty(savedJson))
			{
				IA.LoadBindingOverridesFromJson(savedJson);
				Debug.Log("Loaded saved bindings".colorTag("green"));
			}
		}

		private void OnDisable()
		{
			Debug.Log(C.method("OnDisable", this, "orange"));
			CancelActiveRebinding();
		}
		private void OnDestroy()
		{
			Debug.Log(C.method("OnDestroy", this, "orange"));
			CancelActiveRebinding();
		}
	}
}
 ```