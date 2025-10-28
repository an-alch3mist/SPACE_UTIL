using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

public class DEBUG_IAEventsAssetController : MonoBehaviour
{
	[SerializeField] private InputActionAsset _inputActionAsset;

	enum InputActionType
	{
		character__jump,
		character__shoot,
	}

	private void Awake()
	{
		Debug.Log(C.method("Awake", this));

		var IA = this._inputActionAsset;
		IA.tryGet(InputActionType.character__jump).started += (ctx) => { this.jump(); };
		IA.tryGet(InputActionType.character__shoot).started += (ctx) => { this.shoot(); };
	}

	private void OnEnable() => _inputActionAsset.Enable();
	private void OnDisable() => _inputActionAsset.Disable();

	void jump()
	{
		Debug.Log("Jump()");
	}
	void shoot()
	{
		Debug.Log("Shoot()");
	}
}
