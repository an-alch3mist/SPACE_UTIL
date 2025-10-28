
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

// must be applied at beginning of game to each characters, vehicle, etc anything that has keyBindings
public class DEBUG_IAEventsController : MonoBehaviour
{
	[SerializeField] InputActionAsset IA;

	#region Unity Life Cycle
	private void Awake()
	{
		Debug.Log(("Awake(): " + this).colorTag("orange"));
		// this.IA = new PlayerInputActions();
		this.InitIAEvents();
	}

	private void OnEnable()
	{
		Debug.Log(("OneEnable(): " + this).colorTag("orange"));
		IA.Enable();
	}

	private void OnDisable()
	{
		Debug.Log(("OnDisable(): " + this).colorTag("magenta"));
		IA.Disable();
	}  
	#endregion

	enum ActionType
	{
		character__jump,
		character__shoot,
	}

	[Header("just to log")]
	[SerializeField] Vector2 movementDir;
	void InitIAEvents()
	{
		// IA.character.jump.performed += (ctx) => this.Jump();
		IA.tryGet(ActionType.character__jump).started += (ctx) => this.Jump();
		IA.tryGet(ActionType.character__shoot).started += (ctx) => this.Shoot();
	}
	void Jump()
	{
		Debug.Log("Jump()");
	}
	void Shoot()
	{
		Debug.Log("Shoot()");
	}

}
