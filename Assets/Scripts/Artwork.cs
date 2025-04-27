using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artwork : MonoBehaviour
{
    public void Clear()
    {
        for (int i = 0; i < transform.childCount; i++) {
            // if (transform.GetChild(i).gameObject)
            // {
            if (transform.GetChild(i).gameObject.tag != "Save")
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
                
            // }
        }
    }
}
