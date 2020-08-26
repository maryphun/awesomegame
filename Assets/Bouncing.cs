using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncing : MonoBehaviour
{
    SpriteRenderer renderer;
    Vector2 originalScale;
    float bounce;
    float bounceCnt;
    

    private void Start()
    {
        if (renderer == null)
        {
            renderer = GetComponent<SpriteRenderer>();
        }
        originalScale = transform.localScale;
    }

    private void Update()
    {
        bounceCnt += 0.005f;
        bounce = Mathf.Sin(bounceCnt);
        transform.localScale = new Vector3(originalScale.x, originalScale.y + bounce/30, 1f);
    }
}
