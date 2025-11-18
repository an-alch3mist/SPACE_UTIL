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

namespace SPACE_UISystem.Rebinding
{
	// Depend on: InputActionAsset
	// no external dependencies other than that.
	public class UIRebindingSystem : MonoBehaviour
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

		[TextArea(minLines: 10, maxLines: 12)]
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

# Required Prior
## reference [serializeField]
- template -- button Prefab
- contentHolder (got contentSizeFitter, VerticalLayoutGroup components Attached)
- template -- row Prefab
- save, reset, close window button

## make sure: 
- There is PlayerInputActions.cs generated from inputAction
- GameStore.playerIA shall lead to one of its instance";

		[Header("Input Configuration")]
		[Tooltip("Drag your .inputactions asset here OR set via GameStore")]
		[SerializeField] private InputActionAsset _inputActionAsset;

		enum GameDataType
		{
			inputKeyBindings,
		}

		InputActionAsset IA;
		// PlayerInputActions IA;
		#region Unity LifeCycle
		private void OnEnable()
		{
			Debug.Log(C.method(this, color: "white"));
			this.IA = this._inputActionAsset;
			if (this.IA == null)
			{
				Debug.Log($"unassigned InputActionAsset IA {this._inputActionAsset}".colorTag("red"));
				return;
			}

			// playerIA from GameStore
			// this.IA = GameStore.playerIA;

			this.IA = _inputActionAsset;

			// not required( since universal rule: load at start of game, save at any time in game ), just done to keep from in-game script dependency.
			this.IA.LoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));

			// start of rebinding UI initialization and routine
			StopAllCoroutines();
			CancelActiveRebinding();
			UIIAMapIteration();

			// Hook up reset and save buttons
			if (this._resetBinding != null)
				_resetBinding.onClick.AddListener(ResetAllBindingsToDefault);

			if (this._saveBinding != null)
				_saveBinding.onClick.AddListener(SaveBindings);

			if (this._closeBtn != null)
				_closeBtn.onClick.AddListener(() => { this.gameObject.SetActive(false); });
		}
		private void OnDisable()
		{
			Debug.Log(C.method(this, "orange"));

			if (this.IA == null) return;
			CancelActiveRebinding();
		}
		private void OnDestroy()
		{
			Debug.Log(C.method(this, "orange"));

			if (this.IA == null) return;
			CancelActiveRebinding();
		}
		#endregion

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

		[Header("templateUI/UI elem reference")]
		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		[SerializeField] Button _resetBinding;
		[SerializeField] Button _saveBinding;
		[SerializeField] Button _closeBtn;
		[SerializeField] string pressAnyKey = "Press Any key....";
		[SerializeField] string emptyBinding = "@";

		Dictionary<string, Button> MAP_BindingpathBtn;
		#region <Binding(struct), Button> -> depend on hasCode of Binding
		/*
		Dictionary<InputBinding, Button> MAP_BindingBtn;

		MAP<> BindingBtn Count: 6                                         
		---------------------------------------------------------------------
		 [jump:<Keyboard>/space, pfButton(Clone) (UnityEngine.UI.Button)]    
		 [jump:, pfButton(Clone) (UnityEngine.UI.Button)]                    
		 [shoot:<Mouse>/leftButton, pfButton(Clone) (UnityEngine.UI.Button)] 
		 [shoot:, pfButton(Clone) (UnityEngine.UI.Button)]                   
		 [brake:<Keyboard>/space, pfButton(Clone) (UnityEngine.UI.Button)]   
		 [brake:<Keyboard>/numpad0, pfButton(Clone) (UnityEngine.UI.Button)] 
		*/
		#endregion
		void UIIAMapIteration()
		{
			// Clear existing UI
			this._contentScrollViewTr.destroyLeaves();

			// init MAP_binding_btn
			// this.MAP_BindingBtn = new Dictionary<InputBinding, Button>();
			this.MAP_BindingpathBtn = new Dictionary<string, Button>();

			// Iterate through ALL action maps
			// foreach (var actionMap in IA.asset.actionMaps)
			foreach (var actionMap in IA.actionMaps)
			{
				// Create a header/separator for each action map
				GameObject headerRow = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr); headerRow.transform.destroyLeaves();

				// Create a non-clickable label button for the action map name
				Button headerBtn = GameObject.Instantiate(this._buttonPrefab, headerRow.transform).gc<Button>();
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
					newRowTr = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr).transform; newRowTr.destroyLeaves();
					GameObject.Instantiate(this._buttonPrefab, newRowTr).gc<Button>().setBtnTxt(action.name);

					for (int i2 = 0; i2 < action.bindings.Count; i2 += 1)
					{
						InputBinding binding = action.bindings[i2];

						if (binding.isComposite)
						{
							LOG.AddLog($"  Binding[{i2}]: COMPOSITE '{binding.name}'");
							continue;
						}
						if (binding.isPartOfComposite)
						{
							continue;
						}

						// Create button with rebinding functionality
						Button btn = GameObject.Instantiate(this._buttonPrefab, newRowTr).gc<Button>();
						this.MAP_BindingpathBtn[GetUniqueBindingPath(action, bindingIndex: i2)] = btn;
						#region ad
						btn.gameObject.name += GetUniqueBindingPath(action, bindingIndex: i2); 
						#endregion
						
						//
						UpdateButtonText(btn, action, i2);

						InputAction currentAction = action;
						int currentBindingIndex = i2;

						btn.onClick.AddListener(() =>
						{
							CancelActiveRebinding();
							this.StopAllCoroutines();

							if (EventSystem.current != null)
								EventSystem.current.SetSelectedGameObject(null);

							this.StartCoroutine(PerformRebinding(currentAction, currentBindingIndex, btn));
						});

						LOG.AddLog($"  Binding[{i2}]: {binding.effectivePath ?? "EMPTY"}");
					}
				}
			}

			Debug.Log($"{typeof(UIRebindingSystem).Name}.UIMapIternation() done with UIIAMapIteration()".colorTag("cyan"));
			LOG.AddLog(MAP_BindingpathBtn.ToTable(toString: true, name: "MAP<> BindingpathBtn"));
		}

		#region Helper

		private static string GetUniqueBindingPath(InputAction action, int bindingIndex)
		{
			InputActionMap actionMap = action.actionMap;
			return $"{actionMap.name}->{action.name}->{bindingIndex}";
		}

		// MODIFY: Add new helper method to update button text based on binding
		private static void UpdateButtonText(Button button, InputAction action, int bindingIndex, string emptyText = "@")
		{
			string displayText = action.bindings[bindingIndex].ToDisplayString();

			// If the binding is empty or returns empty string, show "Not Bound"
			if (string.IsNullOrEmpty(displayText) ||
				string.IsNullOrEmpty(action.bindings[bindingIndex].effectivePath))
			{
				displayText = emptyText;
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

		#region private API Crucial


		IEnumerator PerformRebinding(InputAction action, int bindingIndex, Button button)
		{
			Debug.Log(C.method(this));
			activeRebindingButton = button;
			// MODIFY: Store action and binding index instead of text string
			// We'll use these to get the correct display text when needed
			activeRebindingOriginalText = null; // Not needed anymore, but keeping for cleanup
			//
			button.setBtnTxt(this.pressAnyKey);

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
				// .WithControlsExcluding("Mouse")
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
							Debug.Log($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnPotentialMatch Intercepted cancel key: {control.path}".colorTag("orange"));
							op.Cancel();
							return;
						}
					}
				})

				.OnMatchWaitForAnother(0.05f)
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnComplete() NewBinding: {action.bindings[bindingIndex].effectivePath}");
					Debug.Log($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnComplete() {action.bindings[bindingIndex].effectivePath}".colorTag("cyan"));

					// NEW: Clear duplicate bindings in the same action map
					ClearDuplicateBindingsInActionMapOf(currAction: action, bindingIndex, this.MAP_BindingpathBtn);

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
						Debug.Log($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnComplete() Error disposing rebinding operation in OnComplete: {e.Message}".colorTag("red"));
					}
				}))
				.OnCancel((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog(".OnCancel() with cancel key");
					Debug.Log(".OnCancel()".colorTag("orange"));

					// MODIFY: Use helper method to restore button text based on current binding state
					UpdateButtonText(button, action, bindingIndex);

					if (EventSystem.current != null) // deselect all button to avoid selection of same button which is not desired
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					wasCancelled = true;
					done = true;

					try
					{
						Debug.Log($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnCancel() Success disposing rebinding operation in OnCancel".colorTag("orange"));
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.Log($"[{typeof(UIRebindingSystem).Name}.PerformRebinding()] .OnCancel() Error disposing rebinding operation in OnCancel: {e.Message}".colorTag("red"));
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
			}
		}

		private static void ClearDuplicateBindingsInActionMapOf(InputAction currAction, int currBindingIndex, Dictionary<string, Button> MAP_BindingpathBtn)
		{
			InputActionMap actionMap = currAction.actionMap;
			string currDeviceKeyPath = currAction.bindings[currBindingIndex].effectivePath;

			// nothing to check
			if (currDeviceKeyPath == "")
				return;

			int clearedBindingCount = 0;
			Debug.Log($"began ClearDuplicateBindingsInActionMapOf()".colorTag("cyan"));
			foreach (var otherAction in actionMap.actions)
				for (int i2 = 0; i2 < otherAction.bindings.Count; i2 += 1)
				{
					var otherBinding = otherAction.bindings[i2];

					// do nothing
					if (otherAction == currAction && i2 == currBindingIndex)
						continue;
					#region ad
					// composite vec2 keyBindings
					if (otherBinding.isComposite || otherBinding.isPartOfComposite)
						continue;
					#endregion

					if (otherBinding.effectivePath == currDeviceKeyPath)
					{
						// clear binding >>
						otherAction.ApplyBindingOverride(i2, path: "");

						string otherBindingId = GetUniqueBindingPath(otherAction, i2);
						UpdateButtonText(MAP_BindingpathBtn[otherBindingId], otherAction, i2);

						clearedBindingCount += 1;
						// << clear binding
					}
				}

			Debug.Log($"clearedCount: {clearedBindingCount}".colorTag("cyan"));
		}
		/// <summary>
		/// Reset ALL bindings in the entire InputActionAsset to defaults
		/// </summary>
		public void ResetAllBindingsToDefault()
		{
			// Method 1: Reset the entire asset
			IA.RemoveAllBindingOverrides();
			Debug.Log("[UIRebindingSystem.ResetAllBindingsToDefault()] All input bindings reset to defaults! (note: not saved yet)".colorTag("lime"));

			// Do not Save override until save was pressed
			// Clear saved overrides
			// LOG.SaveGameData(GameDataType.inputKeyBindings, "");

			// Refresh the UI
			this.UIIAMapIteration();
		}

		/// <summary>
		/// Save current bindings
		/// </summary>
		private void SaveBindings()
		{
			string json = IA.SaveBindingOverridesAsJson();
			LOG.SaveGameData(GameDataType.inputKeyBindings, json); // GameDataType Situated Inside UIRebindingSystem Class

			Debug.Log("[UIRebindingSystem.SaveBindings()] Bindings saved! as json".colorTag("lime"));
			LOG.AddLog(json, "json");
			
		}

		// loading is done via IA.tryLoadBindingOverridesFromJson() to avoid corrupted Load
		#endregion
	}
}