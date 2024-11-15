Shader "Unlit/AddFBySpring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _balancePosTex;
            float4 _balancePosTex_ST;
            sampler2D _r0Tex;
            float4 _r0Tex_ST;
            sampler2D _v0Tex;
            float4 _v0Tex_ST;

            float springFK;
            float dampFK;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 F = tex2D(_MainTex, i.uv).xyz;
                float3 balancePos = tex2D(_balancePosTex, i.uv).xyz;
                float3 r0 = tex2D(_r0Tex, i.uv).xyz;
                float3 v0 = tex2D(_v0Tex, i.uv).xyz;
                F += (balancePos - r0) * springFK;
                F += - v0 * dampFK;
                return float4(F,0);
            }
            ENDCG
        }
    }
}
