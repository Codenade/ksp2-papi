Shader "Papimod/conservativefill"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "Mark"
            Conservative True
            Offset -1, -1
            // Cull Off
            // ZTest Always
            CGPROGRAM
            #include "UnityStandardCore.cginc"
            #pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile_instancing

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                return fixed4(1, 1, 1, 1);
            }

            ENDCG
        }
        // UsePass "Standard/ShadowCaster"
    }
}
