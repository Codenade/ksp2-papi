Shader "KSP_PAPI/PAPI"
{
	Properties 
	{
		[MainTexture] _MainTex ("Texture", 2D) = "" {}
		_LowColor ("Low Color", Color) = (1,0,0,1)			  	// #ff0000
		_HighColor ("High Color", Color) = (1,1,1,1)			// #ffffff
		_OffColor ("Off Color", Color) = (0,0,0,1)				// #000000
		_LightMaskLL ("Light Mask LL", Color) = (0.862,0,1,1) 	// #dc00ff
		_LightMaskLM ("Light Mask LM", Color) = (1,0,0.964,1) 	// #ff00f6
		_LightMaskRM ("Light Mask RM", Color) = (1,0,0.862,1) 	// #ff00dc
		_LightMaskRR ("Light Mask RR", Color) = (1,0,0.780,1) 	// #ff00c7
		_Slopes ("Slopes", Vector) = (3.5, 3.2, 2.8, 2.5)
		_Transition ("Transition", Float) = 0.1					// Angle in which the lights fade between colors
		[HideInInspector] _Angle ("Angle", Float) = 3
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back

		CGPROGRAM

		#include "./KSP_PAPI.cginc"
		#include "../LightingKSP.cginc"
        #pragma surface surf BlinnPhong keepalpha
		#pragma target 3.0
		
		uniform sampler2D _MainTex;
		uniform half4 _LowColor;
		uniform half4 _HighColor;
		uniform half4 _OffColor;
		uniform half4 _LightMaskLL;
		uniform half4 _LightMaskLM;
		uniform half4 _LightMaskRM;
		uniform half4 _LightMaskRR;
		uniform float _Angle;
		uniform float4 _Slopes;
		uniform float _Transition;
		
		struct Input
        {
            float2 uv_MainTex;
			float2 uv_Emissive;
			float3 viewDir;
			float3 worldPos;
			float4 color : COLOR;
        };

		//struct SurfaceOutputStandard
		// {
		// 	fixed3 Albedo;      // base (diffuse or specular) color
		// 	fixed3 Normal;      // tangent space normal, if written
		// 	half3 Emission;
		// 	half Metallic;      // 0=non-metal, 1=metal
		// 	half Smoothness;    // 0=rough, 1=smooth
		// 	half Occlusion;     // occlusion (default 1)
		// 	fixed Alpha;        // alpha for transparencies
		// };

		void surf (Input IN, inout SurfaceOutput o)
		{	
			float4 pxl = tex2D(_MainTex, IN.uv_MainTex);
			if (all(abs(pxl.rgb - _LightMaskLL.rgb) <= 0.03))
			{
				float4 mColor;
				if (IsNan(_Angle) || _Angle < 0)
					mColor = _OffColor;
				else
					mColor = lerp(_LowColor, _HighColor, clamp((_Angle - _Slopes[0] + (_Transition / 2.0)) / _Transition , 0, 1));
				o.Albedo = mColor.rgb;
				o.Emission = mColor;
			}
			else if (all(abs(pxl.rgb - _LightMaskLM.rgb) <= 0.03))
			{
				float4 mColor;
				if (IsNan(_Angle) || _Angle < 0)
					mColor = _OffColor;
				else
					mColor = lerp(_LowColor, _HighColor, clamp((_Angle - _Slopes[1] + (_Transition / 2.0)) / _Transition , 0, 1));
				o.Albedo = mColor.rgb;
				o.Emission = mColor;
			}
			else if (all(abs(pxl.rgb - _LightMaskRM.rgb) <= 0.03))
			{
				float4 mColor;
				if (IsNan(_Angle) || _Angle < 0)
					mColor = _OffColor;
				else
					mColor = lerp(_LowColor, _HighColor, clamp((_Angle - _Slopes[2] + (_Transition / 2.0)) / _Transition , 0, 1));
				o.Albedo = mColor.rgb;
				o.Emission = mColor;
			}
			else if (all(abs(pxl.rgb - _LightMaskRR.rgb) <= 0.03))
			{
				float4 mColor;
				if (IsNan(_Angle) || _Angle < 0)
					mColor = _OffColor;
				else
					mColor = lerp(_LowColor, _HighColor, clamp((_Angle - _Slopes[3] + (_Transition / 2.0)) / _Transition , 0, 1));
				o.Albedo = mColor.rgb;
				o.Emission = mColor; 
			}
			else
			{
				o.Albedo = pxl.rgb;
			}
			o.Alpha = 1;
			//float4 fog = UnderwaterFog(IN.worldPos, color);
		}
		ENDCG
	}
	Fallback "Standard"
}