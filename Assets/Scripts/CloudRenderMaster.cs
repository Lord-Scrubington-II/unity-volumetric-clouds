using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] [ImageEffectAllowedInSceneView]
public class CloudRenderMaster : MonoBehaviour
{
    private Camera masterCam;
    public Light sun;

    [SerializeField] private Shader cloudShader;
    [SerializeField] private Transform cloudContainer; // a transform that defines the box surrounding the cloud.
    private Material cloudMaterial; // set in inspector, should be the scene's directional light. 

    private void Awake()
    {
        masterCam = gameObject.GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (cloudMaterial == null) {
            cloudMaterial = new Material(cloudShader);
        }

        PassShaderParams();

        // When Graphics.Blit() is called with a material argument,
        // it sets the _Maintex property on the material to the src texture.
        // Then, it sets the render target to the destination texture as usual.
        // This will let me use an image effect shader to modify the image after 
        // it has gone through the graphics pipeline.
        Graphics.Blit(source, destination, cloudMaterial);
    }

    private void PassShaderParams()
    {
        // - Lights -------------------------------
        Vector3 sunlight_dir = sun.transform.forward;
        cloudMaterial.SetVector("_SunDir", sunlight_dir);
        cloudMaterial.SetVector("_SunColour", sun.color);

        // - Camera ---------------------------------
        // pass the camera's model-view and projection matrix inverses to the compute shader
        // cloudMaterial.SetMatrix("_CameraToWorld", masterCam.cameraToWorldMatrix); // nvm, it is already accessible in this fragment shader
        cloudMaterial.SetMatrix("_CameraInverseProjection", masterCam.projectionMatrix.inverse);

        // - Action --------------------------------- // (lol)
        // Pass the cloud container's boundary points
        cloudMaterial.SetVector("_BoxMax", cloudContainer.position + cloudContainer.localScale / 2.0f);
        cloudMaterial.SetVector("_BoxMin", cloudContainer.position - cloudContainer.localScale / 2.0f);
    }

}
