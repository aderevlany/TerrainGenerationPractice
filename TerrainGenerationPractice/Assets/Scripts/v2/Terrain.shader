Shader "Custom/Terrain"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		float minHeight;	// need to access scrip that has this data
		float maxHeight;

        struct Input
        {
			float3 worldPos;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		// called for every pixel that is visible
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			o.Alpha = float3(0, 1, 0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
