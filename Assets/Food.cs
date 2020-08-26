using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private float lastingTime = 4.0f;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private SpriteRenderer shadowRenderer;


    private void Start()
    {
        renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        shadowRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        CurrentScene.Instance().RegisterNewFood(transform);
    }
    
    // Update is called once per frame
    void Update()
    {
        lastingTime -= Time.deltaTime;

        if (lastingTime <= 1.0f)
        {
            // update transparent
            Color tmp = renderer.color;
            tmp.a = lastingTime;
            renderer.color = tmp;
            
            tmp = shadowRenderer.color;
            tmp.a = lastingTime;
            if (shadowRenderer.color.a > tmp.a)
            {
                shadowRenderer.color = tmp;
            }


            if (lastingTime <= 0.0f)
            {
                CurrentScene.Instance().UnregisterFood(transform);
                Destroy(gameObject);
            }
        }
    }

    public void Consume()
    {
        CurrentScene.Instance().UnregisterFood(transform);
        Destroy(gameObject);
    }
}
