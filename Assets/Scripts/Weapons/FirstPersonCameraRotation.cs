using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// A simple FPP (First Person Perspective) camera rotation script.
/// Like those found in most FPS (First Person Shooter) games.
/// </summary>
public class FirstPersonCameraRotation : MonoBehaviour
{
	// const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
	const string yAxis = "Mouse Y";


	[Range(0.1f, 9f)][SerializeField] float m_Sensitivity = 0.65f;
	[Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
	[Range(0f, 90f)][SerializeField] float m_YRotationLimit = 88f;


	private float m_YForce = 0;
	private Vector2 rotation = Vector2.zero;
	private Transform mainCameraTransform = null;



	public float Sensitivity
	{
		get { return m_Sensitivity; }
		set { m_Sensitivity = value; }
	}

	public void Initialize()
	{
		// Stack weapons camera to main base camera...
		Camera baseCamera = Camera.main;
		Camera[] cameras = FindObjectsOfType<Camera>();
		UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();

		for (int i = 0; i < cameras.Length; i++)
		{
			if (cameras[i] == baseCamera)
				continue;

			UniversalAdditionalCameraData cameraData = cameras[i].GetUniversalAdditionalCameraData();
			if (cameraData.renderType == CameraRenderType.Overlay)
				baseCameraData.cameraStack.Add(cameras[i]);
		}

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		mainCameraTransform = baseCamera.transform;
		WeaponsController.WeaponYForce += AddWeaponForceHandler;
	}

	private void OnDestroy()
	{
		WeaponsController.WeaponYForce -= AddWeaponForceHandler;
	}

	private void AddWeaponForceHandler(float yForce)
	{
		m_YForce += yForce;
		m_YForce = Mathf.Clamp(m_YForce, 0f, 30f);
	}

	void Update()
	{
		if (GameManager.IsGameOver || GameManager.IsGamePause)
			return;

		// for look rotation...
		// rotation.x += Input.GetAxis(xAxis) * m_Sensitivity;
		rotation.y += Input.GetAxis(yAxis) * m_Sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -m_YRotationLimit, m_YRotationLimit);
		// var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		var yQuat = Quaternion.AngleAxis(rotation.y + m_YForce, Vector3.left);

		// transform.localRotation = xQuat * yQuat; // Quaternions seem to rotate more consistently than EulerAngles. Sensitivity seemed to change slightly at certain degrees using Euler. transform.localEulerAngles = new Vector3(-rotation.y, rotation.x, 0);
		mainCameraTransform.localRotation = yQuat;

		if (m_YForce > 0)
			m_YForce = Mathf.Lerp(m_YForce, 0, Time.deltaTime * 15f);

		m_YForce = Mathf.Clamp(m_YForce, 0f, 30f);
	}
}