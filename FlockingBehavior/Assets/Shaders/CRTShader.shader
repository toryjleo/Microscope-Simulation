
Shader "Custom/CRTShader" {

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_VertsColor("VertsColor", Range(0, 1)) = .7
		_Br("Brightness", Range(-50, 50)) = 33
		_Contrast("Contrast", Range(-5, 15)) = -1
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
		float4 screenPos : TEXCOORD1;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.screenPos = ComputeScreenPos(o.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;
	float _VertsColor;
	float _Br;
	float _Contrast;

	half4 frag(v2f i) : SV_Target
	{

		half4 col = tex2D(_MainTex, i.uv);

		col += (_Br / 255);
		col = col - _Contrast * (col - 1.0) * col * (col - 0.5);

		// Find the pixel number
		float2 pixLoc = i.screenPos.xy * _ScreenParams.xy;
		// Create a mask to apply to the color
		float modulo = fmod(pixLoc.x, 3);
		float4 muls = float4(0, 0, 0, 1);
		if ((int)modulo == 1) 
		{
			muls.r = 1;
			muls.g = _VertsColor;
		}
		else if ((int)modulo == 2) 
		{
			muls.g = 1;
			muls.b = _VertsColor;
		}
		else if ((int)modulo == 0)
		{
			muls.b = 1;
			muls.r = _VertsColor;
		}
		
		col = col * muls;

		return col;
	}
		ENDCG
	}
	}
}