Shader "Papimod/unlit"
{
    CGINCLUDE
	#include "UnityCG.cginc"
	#include "HLSLSupport.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
	    float4 vertex : SV_POSITION;
	    float2 uv : TEXCOORD0;
	};

	v2f vert(appdata v)
	{
	    v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
	    return fixed4(0, 0, 0, 1);
	}

	fixed4 frag_flare(v2f i) : SV_Target
	{
		return fixed4(1, 1, 1, 1);
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
		UsePass "Standard/ShadowCaster"
	}

	// SubShader
	// {
	// 	Tags { "RenderType" = "FlareOcclusion" "IgnoreProjector" = "True" }
	// 	Conservative True
	// 	Pass
	// 	{
	// 		Name "Mark"
	// 		CGPROGRAM
	// 		#pragma vertex vert
	// 		#pragma fragment frag_flare
	// 		ENDCG
	// 	}
	// 	UsePass "Standard/ShadowCaster"
	// }
}
