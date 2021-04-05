using UnityEngine;
using System.Collections;


public class ParallaxLayer : MonoBehaviour {
	
	[SerializeField]
	private float speedX;
	private float speedY;

	private bool isSetup = false;
	private Transform cameraTransform;
	private Vector3 previousCameraPosition;

	public void Setup(Camera gameCamera, float horizontalSpeed) {
		if (gameCamera != null)
		{
			cameraTransform = gameCamera.transform;
			previousCameraPosition = cameraTransform.position;
			speedX = horizontalSpeed;
			speedY = 1f;
			isSetup = true;
		}
	}

	void Update () {
		if (isSetup)
		{
			previousCameraPosition = cameraTransform.position;

			Vector3 distance = cameraTransform.position - previousCameraPosition;
			transform.position += Vector3.Scale(distance, new Vector3(speedX, speedY));

			previousCameraPosition = cameraTransform.position;
		}
	}
}
