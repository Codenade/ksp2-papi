Shader "ksp2-papi/DepthCopy"
{
	Properties
	{
		_MyDepthTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite On ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "HLSLSupport.cginc"

			#define OUTPUT_COLOR

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
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D_float _MyDepthTex;

			fixed4 frag(v2f i, out float outDepth : SV_Depth) : SV_Target
			{
				float depth = SAMPLE_DEPTH_TEXTURE(_MyDepthTex, i.uv);
				outDepth = depth;
				#ifdef OUTPUT_COLOR
				return fixed4(depth, depth, depth, depth > 0);
				#else
				return 0;
				#endif
			}

			ENDCG
		}
	}
}
