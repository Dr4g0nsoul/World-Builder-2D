using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaWormBullet : MonoBehaviour
{
    [HideInInspector] public float velocity = 0f;
    [HideInInspector] public float duration = 0f;
    public LayerMask layersToCheck;
    public float colliderRadius;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = new Vector2((transform.localScale.x) * velocity, 0f);
        duration -= Time.fixedDeltaTime;

        if(duration < 0f || Physics2D.OverlapCircle(transform.position, colliderRadius, layersToCheck))
        {
            Destroy(gameObject);
        }
    }

}
