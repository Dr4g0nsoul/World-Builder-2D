using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAnimation : MonoBehaviour
{

    public float transitionDuration;
    public Transform playerTransform;

    public bool IsAnimating { get => isAnimating; }

    private Camera cam;
    private SpriteRenderer transitionSprite;
    private Material transitionMaterial;
    private bool isAnimating;

    // Start is called before the first frame update
    void Start()
    {
        isAnimating = false;
        cam = transform.parent.GetComponent<Camera>();
        transitionSprite = GetComponent<SpriteRenderer>();
        transitionSprite.transform.localScale = new Vector3(cam.orthographicSize * 2f * (16f / 9f), cam.orthographicSize * 2f, 1f);
        transitionSprite.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        transitionMaterial = transitionSprite.sharedMaterial;
        transitionMaterial.SetFloat("_Animation", 0f);
        
    }

    

    public void Animate(bool invert = false)
    {
        if(!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimationRoutine(invert));
        }
    }

    IEnumerator AnimationRoutine(bool invert)
    {
        transitionMaterial.SetVector("_CirclePos", GetNormalizedCameraPos());
        transitionMaterial.SetFloat("_Animation", 0f);

        float currTime = 0f;
        float t;

        while(currTime < transitionDuration)
        {
            t = currTime / transitionDuration;
            transitionMaterial.SetFloat("_Animation", invert ? 1f - t : t);
            yield return new WaitForEndOfFrame();
            currTime += Time.deltaTime;
        }

        transitionMaterial.SetFloat("_Animation", invert ? 0f : 1f);

        isAnimating = false;
    }

    private Vector2 GetNormalizedCameraPos()
    {
        Vector2 screenPos = cam.WorldToScreenPoint(playerTransform.position);
        return new Vector2(screenPos.x / cam.pixelWidth, screenPos.y / cam.pixelHeight);
    }

}
