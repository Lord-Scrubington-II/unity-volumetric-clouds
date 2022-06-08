using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CloudRenderMaster))]
public class WindAnimEngine : MonoBehaviour
{
    CloudRenderMaster cloudRenderer;
    [SerializeField] [Tooltip("Controls the movement of the clouds")] Vector3 windVelocity = new Vector3(1.0f, 0.0f, 1.0f); // controls movement of cloud
    [SerializeField] [Tooltip("Controls the movement of the detail noise")] Vector3 windTurbulence = new Vector3(1.0f, -1.0f, -1.0f); // controls movement of detail noise

    // Awake is called before the first frame update
    void Awake()
    {
        cloudRenderer = gameObject.GetComponent<CloudRenderMaster>();
    }

    // Update is called once per frame
    void Update()
    {
        cloudRenderer.CloudsOffset = cloudRenderer.CloudsOffset + windVelocity * Time.deltaTime;
        cloudRenderer.CloudDetailOffset = cloudRenderer.CloudDetailOffset + windTurbulence * Time.deltaTime;
    }
}
