Shader "KSP2_PAPI/flare"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "OcclusionLensFlareSun" }

        Pass
        {
            Lighting Off
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }

    //Properties
    //{
    //    _MainTex ("Texture", 2D) = "white" {}
    //}
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" }
    //    LOD 100

    //    Pass
    //    {
    //        CGPROGRAM
    //        #pragma vertex vert
    //        #pragma fragment frag
    //        // make fog work
    //        #pragma multi_compile_fog

    //        #include "UnityCG.cginc"

    //        struct appdata
    //        {
    //            float4 vertex : POSITION;
    //            float2 uv : TEXCOORD0;
    //        };

    //        struct v2f
    //        {
    //            float2 uv : TEXCOORD0;
    //            float4 vertex : SV_POSITION;
    //            float4 color : COLOR0;
    //            UNITY_FOG_COORDS(1)
    //        };

    //        sampler2D _MainTex;
    //        float4 _MainTex_ST;

    //        v2f vert (appdata v)
    //        {
    //            v2f o;
    //            o.vertex = UnityObjectToClipPos(v.vertex);
    //            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    //            UNITY_TRANSFER_FOG(o,o.vertex);
    //            return o;
    //        }

    //        fixed4 frag (v2f i) : SV_Target
    //        {
    //            // sample the texture
    //            fixed4 col = i.uv.xxyy;
    //            // apply fog
    //            UNITY_APPLY_FOG(i.fogCoord, col);
    //            return i.color;
    //        }
    //        ENDCG
    //    }
    //}
}
