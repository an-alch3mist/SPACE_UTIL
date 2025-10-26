using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

public class DEBUG_IAEventsAssetController : MonoBehaviour
{
	[SerializeField] private InputActionAsset _inputActionAsset;

	// Cache actions in Awake
	private InputAction jump, shoot, walk;

	[Header("just to log")]
	[SerializeField] Vector2 movementDir;
	private void Awake()
	{
		Debug.Log("Awake(): " + this);
		InputActionAsset IA = this._inputActionAsset;
		// Cache once for performance
		InputActionMap charMap = _inputActionAsset.FindActionMap("character");
		jump = charMap.FindAction("jump");
		shoot = charMap.FindAction("shoot");
		walk = charMap.FindAction("walk");

		// Bind events
		jump.performed += ctx => Jump();
		shoot.performed += ctx => Shoot();
		walk.performed += ctx => movementDir = ctx.ReadValue<Vector2>();
		walk.canceled += ctx => movementDir = Vector2.zero;

		/*
		IA.character.jump.performed += (ctx) => this.Jump();
		IA.character.shoot.performed += (ctx) => this.Shoot();

		IA.character.walk.performed += (ctx) => this.movementDir = ctx.ReadValue<Vector2>();
		IA.character.walk.canceled += (ctx) => this.movementDir = Vector2.zero;
		*/
	}

	private void OnEnable() => _inputActionAsset.Enable();
	private void OnDisable() => _inputActionAsset.Disable();


	void Jump()
	{
		Debug.Log("Jump()");
	}
	void Shoot()
	{
		Debug.Log("Shoot()");
	}
}
