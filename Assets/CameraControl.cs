using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	public float rotationSpeed = 100.0f;
	public float zoomSpeed = 10.0f;
	public float minZoom = 10.0f;
	public float maxZoom = 100.0f;
	public float moveSpeed = 10.0f;

	private Camera cam;

	void Start()
	{
		cam = Camera.main;
	}

	void Update()
	{
		RotateCamera();
		ZoomCamera();
		MoveCamera();
	}

	void RotateCamera()
	{
		if (Input.GetMouseButton(1)) // Right mouse button
		{
			float horizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
			float vertical = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

			transform.Rotate(vertical, horizontal, 0, Space.World);
		}
	}

	void ZoomCamera()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		cam.fieldOfView -= scroll;
		cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
	}

	void MoveCamera()
	{
		float moveHorizontal = 0;
		float moveVertical = 0;

		if (Input.GetKey(KeyCode.Z)) // Forward
		{
			moveVertical += moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S)) // Backward
		{
			moveVertical -= moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Q)) // Left
		{
			moveHorizontal -= moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D)) // Right
		{
			moveHorizontal += moveSpeed * Time.deltaTime;
		}

		transform.Translate(new Vector3(moveHorizontal, 0, moveVertical), Space.Self);
	}
}
