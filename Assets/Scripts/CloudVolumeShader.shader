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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float3 _BoxMax;
            float3 _BoxMin;
            // float4x4 _CameraToWorld; // nvm, it is already accessible in this fragment shader
            float4x4 _CameraInverseProjection;
            float3 _SunDir;
            float3 _SunColour;

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

	            // ...then, transform to world space and normalize.
                direction = normalize(
		            mul(unity_CameraToWorld, float4(direction, 0.0f)).xyz
	            );
	
                return NewRay(origin, direction);
            }
            // - END: Ray Generation ----------------------------------------------------------

            // Ray-box intersection



            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
