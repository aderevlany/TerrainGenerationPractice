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

		const static int maxColorCount = 8;
		const static float epsilon = 1E-4;	// to deal with the possibility of dividing by 0

		int baseColorCount;	// arrays will always be maxColorCount size, but may not have that many items
		float3 baseColors[maxColorCount];	// need to initialize the arrays
		float baseStartHeights[maxColorCount];
		float baseBlends[maxColorCount];

		float minHeight;	// need to access scrip that has this data
		float maxHeight;

        struct Input
        {
			float3 worldPos;
        };

		float inverseLerp(float min, float max, float value) {
			return saturate((value - min) / (max - min));	// use saturdate because we cannot assume that value is between min and max, clamps in range 0-1
		}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		// called for every pixel that is visible
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);	// lang -> CG
			//o.Albedo = heightPercent;
			for (int i = 0; i < baseColorCount; i++) {
				//float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseStartHeights[i]);
				o.Albedo = o.Albedo * (1 - drawStrength) + baseColors[i] * drawStrength;	// makes it so that 0 drawstength is not black
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
