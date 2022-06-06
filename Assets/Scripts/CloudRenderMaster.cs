using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] [ImageEffectAllowedInSceneView]
public class CloudRenderMaster : MonoBehaviour
{
    private Camera masterCam;
    [SerializeField] private Light sun;

    [SerializeField] private Shader cloudShader;
    [SerializeField] private Transform cloudContainer; // a transform that defines the box surrounding the cloud.
    private Material cloudMaterial; // set in inspector, should be the scene's directional light. 

    // 3D Perlin-Worley shape and detail noise textures passed from inspector
    [SerializeField] private Texture3D cloudShapeNoise;
    [SerializeField] private Texture3D cloudDetailNoise;

    // Cloud sampling control vars
    [SerializeField] private Vector3 cloudsOffset = new Vector3(0.0f, 0.0f, 0.0f); // should allow the cloud to "move" in the box
    [SerializeField] private float cloudsScale = 1.0f; // used to manipulate the mapping from texture to world space
    [SerializeField] [Range(0, 10)] private float emptySpaceOffset = 0.2f; // any density reading below this threshold is considered 0
    [SerializeField] [Range(-1, 1)] private float densityControlMultiplier = 1.0f; // just a multiplier for the density reading, should be used to manipulate cloud darkness
    [SerializeField] private float absorptionCoefficient = 1.0f; // controls # of steps taken when marching the light ray
    [SerializeField] [Range(0, 1)] private float darknessThreshold = 0.5f; // controls # of steps taken when marching the light ray
    [SerializeField] private float lightScatteringMultiplier = 0.5f; // controls # of steps taken when marching the light ray

    [SerializeField] [Range(1, 200)] private int sampleCount = 6; // controls # of steps taken when marching the light ray
    [SerializeField] [Range(1, 10)] private int lightSampleCount = 5; // controls # of steps taken when marching the light ray
    public Vector3 CloudsOffset { get => cloudsOffset; set => cloudsOffset = value; }
    public float CloudsScale { get => cloudsScale; set => cloudsScale = value; }
    public float EmptySpaceOffset { get => emptySpaceOffset; set => emptySpaceOffset = value; }
    public float DensityControlMultiplier { get => densityControlMultiplier; set => densityControlMultiplier = value; }
    public int SampleCount { get => sampleCount; set => sampleCount = value; }
    public int LightSampleCount { get => lightSampleCount; set => lightSampleCount = value; }
    public float AbsorptionCoefficient { get => absorptionCoefficient; set => absorptionCoefficient = value; }
    public float DarknessThreshold { get => darknessThreshold; set => darknessThreshold = value; }
    public float LightScatteringMultiplier { get => lightScatteringMultiplier; set => lightScatteringMultiplier = value; }

    private void Awake()
    {
        masterCam = gameObject.GetComponent<Camera>();
        // noiseGenerator = FindObjectOfType<NoiseGenerator>();
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
        // - Lights ---------------------------------
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

        // Pass the 3D noise textures
        cloudMaterial.SetTexture("_Worley", cloudShapeNoise);
        cloudMaterial.SetTexture("_Perlin", cloudDetailNoise);

        // Pass the cloud control vars
        cloudMaterial.SetVector("_CloudsOffset", CloudsOffset);
        cloudMaterial.SetFloat("_CloudsScale", CloudsScale);

        cloudMaterial.SetFloat("_DensityReadOffset", EmptySpaceOffset);
        cloudMaterial.SetFloat("_DensityMult", DensityControlMultiplier);
        cloudMaterial.SetFloat("_AbsorptionCoeff", AbsorptionCoefficient);
        cloudMaterial.SetFloat("_DarknessThreshold", DarknessThreshold);

        cloudMaterial.SetInt("_NumSamples", SampleCount);
        cloudMaterial.SetInt("_StepsToLight", LightSampleCount);

        cloudMaterial.SetFloat("_LightScatteringMult", LightScatteringMultiplier);
    }
}
