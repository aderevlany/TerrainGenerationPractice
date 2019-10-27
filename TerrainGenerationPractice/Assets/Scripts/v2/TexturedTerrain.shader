Shader "Custom/TexturedTerrain"
{
	Properties
	{
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", FLoat) = 1
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

		const static int maxLayerCount = 8;
		const static float epsilon = 1E-4;	// to deal with the possibility of dividing by 0

		int layerCount;	// arrays will always be maxLayerCount size, but may not have that many items
		float3 baseColors[maxLayerCount];	// need to initialize the arrays
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColorStrength[maxLayerCount];
		float baseTextureScales[maxLayerCount];

		UNITY_DECLARE_TEX2DARRAY(baseTextures);

		float minHeight;	// need to access scrip that has this data
		float maxHeight;

		sampler2D testTexture;
		float testScale;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
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

		float3 triplanar(float3 worldPos, float scale, float3 blendAxis, int textureIndex) {
			float3 scaledWorldPos = worldPos / scale;
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxis.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxis.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxis.z;
			return xProjection + yProjection + zProjection;
		}

		// called for every pixel that is visible
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);	// lang -> CG
			float3 blendAxis = abs(IN.worldNormal);
			blendAxis /= blendAxis.x + blendAxis.y + blendAxis.z;

			for (int i = 0; i < layerCount; i++) {
				//float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
				float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

				float3 baseColor = baseColors[i] * baseColorStrength[i];
				float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxis, i) * (1 - baseColorStrength[i]);

				o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor+textureColor) * drawStrength;	// makes it so that 0 drawstength is not black
			}

				
		}
		ENDCG
	}
	FallBack "Diffuse"
}
