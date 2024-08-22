Shader "ksp2-papi/add"
{
    Properties
    {
        _Tex1 ("Tex1", 2D) = "black" {}
        _Tex1_Offset ("Tex1 Offset", Range(-1.0, 1.0)) = 0.0
        _Tex2 ("Tex2", 2D) = "black" {}
        _Tex2_Offset ("Tex2 Offset", Range(-1.0, 1.0)) = 0.0
    }
    SubShader
    {
        Pass
        {
            ZWrite On ZTest Always Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "HLSLSupport.cginc"

            // #define OUTPUT_COLOR

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

            sampler2D_float _Tex1;
            float _Tex1_Offset;
            sampler2D_float _Tex2;
            float _Tex2_Offset;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i, out float outDepth : SV_Depth) : SV_Target
            {
                float d1 = SAMPLE_DEPTH_TEXTURE(_Tex1, i.uv) + _Tex1_Offset;
                float d2 = SAMPLE_DEPTH_TEXTURE(_Tex2, i.uv) + _Tex2_Offset;
                outDepth = max(d1, d2);
                #ifdef OUTPUT_COLOR
                return fixed4(d1, d2, outDepth, outDepth > 0);
                #else
                return 0;
                #endif
            }
            ENDCG
        }
    }
}