Shader "ksp2-papi/papi_head"
{
	Properties 
	{
		[MainTexture] _MainTex ("Texture", 2D) = "" {}
		_LowColor ("Low Color", Color) = (1,0,0,1)			  	// #ff0000
		_HighColor ("High Color", Color) = (1,1,1,1)			// #ffffff
		_OffColor ("Off Color", Color) = (0,0,0,1)				// #000000
		_LightMask ("Light Mask", Color) = (1,0,1,1) 		    // #ff00ff
		_Transition ("Transition", Float) = 0.1					// Angle in which the lights fade between colors
		_SelectionThreshold ("Color Selection Threshold", Float) = 0.0001
		_Angle ("Angle", Float) = 3
		_Off ("Turn off", Float) = 1
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Standard

		struct Input
		{
			float2 uv_MainTex;
		};

		// struct SurfaceOutputStandard
		// {
		// 	fixed3 Albedo;      // base (diffuse or specular) color
		// 	fixed3 Normal;      // tangent space normal, if written
		// 	half3 Emission;
		// 	half Metallic;      // 0=non-metal, 1=metal
		// 	half Smoothness;    // 0=rough, 1=smooth
		// 	half Occlusion;     // occlusion (default 1)
		// 	fixed Alpha;        // alpha for transparencies
		// };

		sampler2D _MainTex;
		float4 _LowColor;
		float4 _HighColor;
		float4 _OffColor;
		float4 _LightMask;
		float _Transition;
		float _Angle;
		float _Off;
		float _SelectionThreshold;

		void surf(Input i, inout SurfaceOutputStandard o)
		{
			float4 col = tex2D(_MainTex, i.uv_MainTex);
			if (length(col - _LightMask) < _SelectionThreshold)
			{
				col = lerp(lerp(_LowColor, _HighColor, saturate(_Angle / _Transition + 0.5)), _OffColor, saturate(_Off));
				o.Emission = col;
			}
			o.Albedo = col;
		}
		ENDCG
	}
	Fallback "Diffuse"
}