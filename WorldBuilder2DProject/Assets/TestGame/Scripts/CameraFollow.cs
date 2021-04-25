using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform playerTransform;
    public float smoothSpeed = 0.125f;
    public Vector2 offset;
    private Camera cam;
    private BoxCollider2D camCollider;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = GetComponent<Camera>();
        camCollider = GetComponent<BoxCollider2D>();
        camCollider.size = new Vector2(cam.orthographicSize * 2f * (16f / 9f), cam.orthographicSize * 2f);
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 desiredPosition = (Vector2)playerTransform.position + offset;
        Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, smoothSpeed);
        rb.MovePosition(smoothedPosition);
        //transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
