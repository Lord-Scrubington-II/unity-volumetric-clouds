using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudContainer : MonoBehaviour
{
    Transform myTransform;
    public Color colour = Color.white;
    public Color selectedColor = Color.green;
    public bool drawBox = true;

    void Awake()
    {
        myTransform = gameObject.GetComponent<Transform>();
    }

    private void OnDrawGizmos()
    {
        if (drawBox) {
            Gizmos.color = colour;
            Gizmos.DrawWireCube(myTransform.position, myTransform.localScale);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (drawBox) {
            Gizmos.color = selectedColor;
            Gizmos.DrawWireCube(myTransform.position, myTransform.localScale);
        }
    }

}
