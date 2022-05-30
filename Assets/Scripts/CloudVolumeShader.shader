// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable

Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // float4x4 _CameraToWorld; // nvm, it is already accessible in this fragment shader
            float4x4 _CameraInverseProjection;

            // - BEGIN: Ray Generation  ----------------------------------------------------------
            struct Ray
            {
	            float3 origin;
	            float3 direction;
            };

            Ray NewRay(float3 origin, float3 direction)
            {
	            Ray ray;
	            ray.origin = origin;
	            ray.direction = direction;
	            return ray;
            }

            // Given a Vector2 in normalized device coordinates [-1, 1], generate a primary ray from the camera
            Ray CameraRayGen(float2 ndc_xy) 
            {
	            // As before, we want ray origin and direction in world space.
	
	            // Extract camera aperture transform
                float3 origin = mul(unity_CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

	            // invert the projection of camera-space direction --> ndc-uv coordinates...
                float3 direction = mul(_CameraInverseProjection, float4(ndc_xy, 0.0f, 1.0f)).xyz;

	            // ...then, transform to world space. NOTE: Not yet normalized, as we need the length of this vector!
                direction = normalize(
                    mul(unity_CameraToWorld, float4(direction, 0.0f)).xyz
                );
	
                return NewRay(origin, direction);
            }
            // - END: Ray Generation ----------------------------------------------------------

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                Ray cam_ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;

                float2 ndc_xy = float2(v.uv * 2.0f - 1.0f);
                
                // BEGIN ray generation
                float3 origin = _WorldSpaceCameraPos;

                // invert the projection of camera-space direction --> ndc-uv coordinates...
                float3 direction = mul(_CameraInverseProjection, float4(ndc_xy, 0.0f, -1.0f)).xyz;
	            // ...then, transform to world space and normalize.
                direction = mul(unity_CameraToWorld, float4(direction, 0.0f)).xyz;
                output.cam_ray = NewRay(origin, direction);
                // END ray generation

                return output;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float3 _BoxMax;
            float3 _BoxMin;

            float3 _SunDir;
            float3 _SunColour;

            // Ray-box intersection
            bool BoxIntersects(
                Ray ray, 
                float3 box_min, 
                float3 box_max, 
                inout float dist_to_box, 
                inout float dist_inside_box
            ) {
                // Adapted from Majercik et al. 2018 "A Ray-Box Intersection Algorithm and Efficient Dynamic Voxel Rendering"
                float3 v0 = (box_min - ray.origin) * (1.0f / ray.direction);
                float3 v1 = (box_max - ray.origin) * (1.0f / ray.direction);
                float3 v_max = max(v0, v1);
                float3 v_min = min(v0, v1);
                
                // now, we want to know if the max component of t_min
                // is greater than the min component of t_max. If so, then no intersection occurred.
                float dist_near = max(max(v_min.x, v_min.y), v_min.z);
                float dist_far = min(min(v_max.x, v_max.y), v_max.z);

                dist_to_box = max(0.0f, dist_near); // if inside box, will be 0
                dist_inside_box = max(0, dist_far - dist_to_box); // shouldn't really ever be negative unless facing away from the box

                if (dist_near > dist_far || dist_near < 0.0f) {
                    return false;
                } return true;
            }


            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, input.uv);

                float nonlinear_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv);
                //float cam_frustum_len = _ProjectionParams.z - _ProjectionParams.y; // far - near 
                float depth = LinearEyeDepth(nonlinear_depth) * length(input.cam_ray.direction);

                Ray cam_ray = input.cam_ray;
                cam_ray.direction = normalize(cam_ray.direction);

                // cloud container intersection t-values
                float dist_to_box; float dist_in_box; 
                bool hit_cloud = BoxIntersects(
                    cam_ray,
                    _BoxMin,
                    _BoxMax,
                    dist_to_box,
                    dist_in_box
                );

                if (hit_cloud && dist_to_box < depth) {
                    col = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}
