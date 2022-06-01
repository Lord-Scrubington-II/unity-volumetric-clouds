using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	// Inspector vars: linear & angular velocity
	[SerializeField] [Range(1.0f, 40.0f)] float turnSpeed = 20.0f;
	[SerializeField] [Range(1.0f, 40.0f)] float panSpeed = 20.0f;
	[SerializeField] [Range(1.0f, 3.0f)] float movementBoostCoefficient = 2.0f; // when holding shift move faster

	private const float camRotateCoeff = 20.0f; // cam rotate needs to be much faster

	private Vector3 mouseOrigin;  // This is the position of the cursor when mouse dragging starts
	private bool useMouseRotation; // what it says on the tin

	private Camera mainCam;

    private void Start()
    {
		mainCam = Camera.main;
    }

    // - BEGIN: Control Flow
    // On mouse drag: rotate camera
    // On WASD: move forward, left, back, and right respectively
    // On Space Bar: move up
    // On CTRL: move down
    void Update()
	{
		Vector3 moveDirection;
		float movementMultiplier = 1.0f;

		if (Input.GetMouseButtonDown(0)) { // left mouse button
			// snapshot the mouse position at the beginning of the frame
			mouseOrigin = Input.mousePosition;
			useMouseRotation = true;
		}

		if (Input.GetKey(KeyCode.LeftShift)) { // holding shift
			movementMultiplier = movementBoostCoefficient;
		}

		if (!Input.GetMouseButton(0)) { useMouseRotation = false; }

		// adapted from a forum conversation: http://forum.unity3d.com/threads/39513-Click-drag-camera-movement
		if (useMouseRotation) {
 			moveDirection = mainCam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

			// rotate the camera first around the x-axis proportional to the movement in screen-space y,
			// then rotate the camera around the z-axis proportional to the movement in screen-space x.
			transform.RotateAround(transform.position, transform.right, -moveDirection.y * turnSpeed * Time.deltaTime * camRotateCoeff);
			transform.RotateAround(transform.position, Vector3.up, moveDirection.x * turnSpeed * Time.deltaTime * camRotateCoeff);
		}

		// Move the camera upwards
		if (Input.GetKey(KeyCode.Space)) {
			moveDirection = Vector3.up;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

		// Move the camera downwards
		if (Input.GetKey(KeyCode.LeftControl)) {
			moveDirection = Vector3.down;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

		// Move the camera forward (along the local positive z-axis)
		if (Input.GetKey(KeyCode.W)) {
			moveDirection = transform.forward;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

		// Move the camera left (along the local negative y-axis)
		if (Input.GetKey(KeyCode.A)) {
			moveDirection = -transform.right;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

		// Move the camera left (along the local negative z-axis)
		if (Input.GetKey(KeyCode.S)) {
			moveDirection = -transform.forward;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

		// Move the camera right (along the local positive y-axis)
		if (Input.GetKey(KeyCode.D)) {
			moveDirection = transform.right;
			transform.Translate(movementMultiplier * panSpeed * Time.deltaTime * moveDirection, Space.World);
		}

	}
}