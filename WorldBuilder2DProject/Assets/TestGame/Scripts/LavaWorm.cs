using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaWorm : MonoBehaviour
{

    public float viewRange = 3f;
    public GameObject bullet;
    public Transform bulletSpawnPos;
    public float bulletSpeed;
    public float bulletDuration;

    private bool shooting;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        shooting = false;
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, viewRange, LayerMask.GetMask("Player"));
        if (col && Mathf.Sign(col.transform.position.x - transform.position.x) == Mathf.Sign(transform.localScale.x) ) {
            if(!shooting)
            {
                shooting = true;
                anim.SetBool("InRange", shooting);
            }
        }
        else if (shooting)
        {
            shooting = false;
            anim.SetBool("InRange", shooting);
        }
    }


    public void TriggerShot()
    {
        GameObject newBullet = Instantiate(bullet, bulletSpawnPos.position, Quaternion.identity);
        newBullet.transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x), 1f, 1f);
        newBullet.GetComponent<LavaWormBullet>().velocity = bulletSpeed;
        newBullet.GetComponent<LavaWormBullet>().duration = bulletDuration;
    }

    public void TriggerDeath()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRange);
        Gizmos.color = Color.white;

    }
}
