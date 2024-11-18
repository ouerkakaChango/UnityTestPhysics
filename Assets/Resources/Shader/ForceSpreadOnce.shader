Shader "Unlit/ForceSpreadOnce"
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
            float4 _MainTex_TexelSize;

            sampler2D _impIDTex;
            float4 _impIDTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float spreadK;

            float4 frag (v2f i) : SV_Target
            {
                float3 spreadFa = tex2D(_MainTex, i.uv).xyz;
                //float impID = tex2D(_impIDTex, i.uv).r;
                ////Ö»Ð´0µÄspreadF
                //if(impID>0.5)
                //{
                //    return float4(spreadFa,0);
                //}

                float2 spreadUVDirs[4]={float2(1,0),float2(-1,0),float2(0,1),float2(0,-1)};

                for(int iter=0;iter<4;iter++)
                {
                    float2 uv_neighbour = i.uv+spreadUVDirs[iter]*_MainTex_TexelSize.xy;
                    uv_neighbour = saturate(uv_neighbour);

                    //float imp_neighbour = tex2D(_impIDTex, uv_neighbour).r;
                    //if(imp_neighbour>0.5)
                    {
                        //???
                        float3 springF_neighbour = tex2D(_MainTex, uv_neighbour).xyz;
                        spreadFa+=-spreadK*1.0/4*springF_neighbour;  
                    }
                }
                
                return float4(spreadFa,0);
            }
            ENDCG
        }
    }
}
