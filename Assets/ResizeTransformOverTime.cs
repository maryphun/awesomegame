using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeTransformOverTime : MonoBehaviour
{
    public float minimum;
    public float maximum;
    Transform targetTrans;

    // starting value for the Lerp
    static float t = 0.0f;

    
    public void Constructor(float time, float originalSize, float targetSize, Transform targetTranform)
    {
        t = 0.0f;
        minimum = originalSize;
        maximum = targetSize;
        targetTrans = targetTranform;
    }

    void Update()
    {
        // .. and increase the t interpolater
        t = Mathf.Clamp(t + 0.5f * Time.deltaTime, 0.0f, 1.0f);
        
        // rescale the transform
        float newScale = Mathf.Lerp(minimum, maximum, t);
        targetTrans.localScale = new Vector3(newScale, newScale, 1f);

        Debug.Log("t = [" + t + "], scale = [" + newScale + "]");

        if (t == 1.0f)
        {
            Destroy(this);
        }
    }
}
