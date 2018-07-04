// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Vignette" {

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;

	half4 frag(v2f i) : SV_Target
	{
		// Calculate distance from center
		float2 normalizedCoords = i.vertex.xy / _ScreenParams.xy;
		float2 distFromCent = normalizedCoords - float2(0.5, 0.5);
		float len = length(distFromCent.xy);
		// Get the rgb value from the _MainTex
		half4 col = tex2D(_MainTex, i.uv);
		// Apply the vignette effect
		float percentage = 0.8;
		float softness = 0.4;
		// Make the vignette pulsate
		percentage -= abs(sin(_Time[1]))/10;
		// Interpolate the values using a polynomial
		float sm = smoothstep(percentage, percentage - softness, len);

		col.rgb = lerp(col.rgb, col.rgb * half3(sm, sm, sm), 0.4);
		return col;
	}
		ENDCG
	}
	}
}