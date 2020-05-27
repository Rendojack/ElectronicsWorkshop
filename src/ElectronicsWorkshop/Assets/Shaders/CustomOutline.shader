Shader "Custom/Outline"
{
    Properties
    {
        _Color("Main Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor("Outline color", color) = (0,0,0,1)
        _OutlineWidth("Outline width", Range(1.0,5.0)) = 1.05
    }

    SubShader
    {
        // Always render on top of everything Z axis
        Tags {  "Queue" = "Overlay"}
        ZTest Always
        ZWrite off

        Pass // Outline
        {       
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : POSITION;
                    float3 normal : NORMAL;
                };

                float _OutlineWidth;
                float4 _OutlineColor;

                v2f vert(appdata v)
                {
                    v.vertex.xyz *= _OutlineWidth;

                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    return o;
                }       

                half4 frag(v2f i) : COLOR
                {
                    return _OutlineColor;
                }

            ENDCG
        }

        Pass // Object itself
        {    
            Lighting On    
            Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM

                #include "UnityCG.cginc"

                #pragma vertex vert
                #pragma fragment frag
   
                sampler2D _MainTex;
                float4 _MainTex_ST;

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
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    return o;
                }
   
                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv); // Sample the texture
                    return col;
                }

            ENDCG
        }
    }
}

