using UnityEngine;
using System.Collections;


public class ParallaxLayer : MonoBehaviour {
	
	[SerializeField]
	private float speedX;

	public bool isSetup = false;
	private Transform cameraTransform;
	private Vector3 previousCameraPosition;

	public void Setup(Camera gameCamera, float horizontalSpeed) {
		if (gameCamera != null)
		{
			cameraTransform = gameCamera.transform;
			previousCameraPosition = Vector2.zero;
			speedX = horizontalSpeed;
			isSetup = true;
			transform.position = Vector3.zero;
		}
	}

	void Update () {
		if (isSetup)
		{
			Vector3 distance = cameraTransform.position - previousCameraPosition;
			transform.position -= new Vector3(Vector2.Scale(distance, new Vector2(speedX - 1f, 0f)).x, 0f, 0f);

			previousCameraPosition = cameraTransform.position;
		}
	}
}
