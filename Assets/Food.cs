using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private float lastingTime = 4.0f;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private float energyContain = 0.25f;

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

            // count to destroy
            if (lastingTime <= 0.0f)
            {
                CurrentScene.Instance().UnregisterFood(transform);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Return the energy given
    /// </summary>
    public float Consume()
    {
        CurrentScene.Instance().UnregisterFood(transform);
        Destroy(gameObject);

        return energyContain;
    }
}
