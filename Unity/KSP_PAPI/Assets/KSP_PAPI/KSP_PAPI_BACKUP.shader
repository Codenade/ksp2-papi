Shader "KSP_PAPI/PAPI"
{
	Properties 
	{
		[MainTexture] _MainTex ("Texture", 2D) = "" {}
		_LowColor ("Low Color", Color) = (1,0,0,1)
		_HighColor ("High Color", Color) = (1,1,1,1)
		_LightMask ("Light Mask", Color) = (1,0,1,1)
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		
		#include "../LightingKSP.cginc"
        #pragma surface surf BlinnPhong keepalpha
		#pragma target 3.0
		
		uniform sampler2D _MainTex;
		uniform half4 _LowColor;
		uniform half4 _HighColor;
		uniform half4 _LightMask;
		
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
			bool cmp = tex2D(_MainTex, IN.uv_MainTex).rgb == _LightMask.rgb;
			if (cmp)
			{
				// is light
				//float4 part1 = (0.0, _HeightAboveGround, 0.0, 1.0);
				//float4 part3 = mul(unity_ObjectToWorld, part1);
				float3 part4 = o.Normal.y <= normalize(ObjSpaceViewDir((IN.worldPos, 0.0)/* - part3*/)).y ? _HighColor.rgb : _LowColor.rgb;
				o.Albedo = fixed3(part4);
				o.Emission = 5*half3(part4);
			}
			else
			{
				// is case
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			}

			//float4 fog = UnderwaterFog(IN.worldPos, color);

			o.Alpha = 1;
		}
		ENDCG
	}
	Fallback "Standard"
}