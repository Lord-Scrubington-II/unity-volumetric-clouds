using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunsetSimulator : MonoBehaviour
{
    Light sun;
    [SerializeField] float sunsetSpeed;
    // Start is called before the first frame update
    void Awake()
    {
        sun = FindObjectOfType<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(-sunsetSpeed * Time.deltaTime, 0.0f, 0.0f));
    }
}
