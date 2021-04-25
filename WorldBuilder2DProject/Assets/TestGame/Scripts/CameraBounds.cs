using dr4g0nsoul.WorldBuilder2D.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{

    public LevelManager levelManager;
    private EdgeCollider2D cameraBounds;

    // Start is called before the first frame update
    void Start()
    {
        cameraBounds = GetComponent<EdgeCollider2D>();
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return null;
        SetupCameraBounds();
    }

    public void SetupCameraBounds()
    {
        Rect levelBounds = levelManager.LevelLoader.CurrentLevel.Level.levelBoundaries;
        levelBounds.position -= Vector2.one * (levelBounds.size / 2f);
        Vector2[] corners = new Vector2[5];
        corners[0] = levelBounds.position;
        corners[1] = levelBounds.position + Vector2.right * levelBounds.size.x;
        corners[2] = levelBounds.position + Vector2.one * levelBounds.size;
        corners[3] = levelBounds.position + Vector2.up * levelBounds.size.y;
        corners[4] = levelBounds.position;
        cameraBounds.points = corners;
    }
}
