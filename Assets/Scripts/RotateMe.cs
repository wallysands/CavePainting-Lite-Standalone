using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMe : MonoBehaviour
{
    public float degreesPerSecond = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float degrees = degreesPerSecond * Time.deltaTime;
        transform.Rotate(new Vector3(0, degrees, 0));
    }
}
