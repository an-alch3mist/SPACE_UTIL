using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using Cinemachine;

public class BirdViewCamManager : MonoBehaviour
{
	Vector3 StartingOffset;
	private void Start()
	{
		if (this.VCam == null)
			Debug.LogError("VCam reference is null in " + this);

		this.StartingOffset = VCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.normalized;
	}

	private void Update()
	{
		float dt = Time.unscaledDeltaTime;

		this.Translating = this.Rotating = this.Zooming = false; // initialize at start of frame, modified later while handling
																 //
		this.HandleTranslate(dt);
		this.HandleRotate(dt);
		this.HandleZoom();

		// ad
		if(this.EnableEdgeScroll)
		if(!this.Translating && !this.Rotating && !this.Zooming) // do not interrupt
			this.HandleEdgeScroll(dt);
	}

	#region README
	[TextArea(3, 10)]
	[SerializeField] string README = @"0. Attach to Target of Cinemachine
1. Assign VCam Reference to Main Cinemachine[VirtualCamera]
(optional) Set VCam YawDampening to 0.5f";
	#endregion

	[Header("Speed")]
	[SerializeField] float MoveSpeed = 5f;		// 5m/sec
	[SerializeField] float RotateSpeed = 95f;	// 95deg/sec
	[SerializeField] float ZoomSpeed = 0.5f;    // 0.6m/snap-click
	[Header("Clamp")]
	[SerializeField] float MinOffsetY = 4;
	[SerializeField] float MaxOffsetY = 24;
	[SerializeField] float MinFov = 35;
	[SerializeField] float MaxFov = 80;
	[Header("Smooth")]
	[Range(0.1f, 1f)] [SerializeField] float SmoothFov = 0.5f;
	[Header("EdgeScroll")]
	[SerializeField] bool EnableEdgeScroll = false;
	[SerializeField] int EdgeScrollPad = 40; // with respect to 1280 x 720

	[SerializeField] bool Translating, Rotating, Zooming; // Indicators During Runtime

	void HandleTranslate(float dt)
	{
		Vector3 move_vel =
		(
			Input.GetAxisRaw("Horizontal") * this.transform.right +
			Input.GetAxisRaw("Vertical") * this.transform.forward
		).normalized * this.MoveSpeed * (INPUT.K.HeldDown(KeyCode.LeftShift) ? 2f : 1f);

		this.transform.position += move_vel * dt;
		this.Translating = !C.zero(move_vel);
	}
	void HandleRotate(float dt)
	{
		if (INPUT.M.HeldDown(2)) // middle mouse button held down
		{
			Vector3 rotate_vel = this.transform.up * Input.GetAxisRaw("Mouse X") * this.RotateSpeed;
			this.Rotating = true; // until middle mouse button is released.
			this.transform.eulerAngles += rotate_vel * dt;
		}
	}
	[Header("vcam ref")] [SerializeField] CinemachineVirtualCamera VCam;
	void HandleZoom()
	{
		float dt = 1f;
		var ct = VCam.GetCinemachineComponent<CinemachineTransposer>();

		// zoom offset modify
		Vector3 zoom_vel = (this.transform.up + StartingOffset) * -Input.mouseScrollDelta.y * this.ZoomSpeed;
		Vector3 new_offset = ct.m_FollowOffset + zoom_vel * dt;
		if (new_offset.y >= this.MinOffsetY && new_offset.y <= this.MaxOffsetY)
			ct.m_FollowOffset = new_offset;

		this.Zooming = !C.zero(zoom_vel);

		// fov
		float t = Z.t(ct.m_FollowOffset.y, this.MinOffsetY, this.MaxOffsetY);
		float new_fov = Z.lerp(this.MinFov, this.MaxFov, t);
		VCam.m_Lens.FieldOfView = Z.lerp(VCam.m_Lens.FieldOfView, new_fov, this.SmoothFov); // smooth lerp
	}
	void HandleEdgeScroll(float dt)
	{
		float EdgeScrollSpeed = this.MoveSpeed * 0.5f * (INPUT.K.HeldDown(KeyCode.LeftShift) ? 2f : 1f); // half the normal MoveSpeed

		Vector3 move_vel = Vector2.zero;
		if (INPUT.UI.pos.x < this.EdgeScrollPad)					move_vel = -1 * this.transform.right   * EdgeScrollSpeed; 
		if (INPUT.UI.pos.y < this.EdgeScrollPad)					move_vel = -1 * this.transform.forward * EdgeScrollSpeed;
		if (INPUT.UI.pos.x > INPUT.UI.size.x - this.EdgeScrollPad)	move_vel = +1 * this.transform.right   * EdgeScrollSpeed;
		if (INPUT.UI.pos.y > INPUT.UI.size.y - this.EdgeScrollPad)	move_vel = +1 * this.transform.forward * EdgeScrollSpeed;

		this.transform.position += move_vel * dt;
	}
}
