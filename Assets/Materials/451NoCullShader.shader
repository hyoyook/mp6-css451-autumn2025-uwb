﻿Shader "Unlit/451NoCullShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Base Color", Color) = (1,1,1,1)
        _UseTexture ("Use Texture (0/1)", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"     // for _WorldSpaceLightPos0, _LightColor0, ambient

            sampler2D _MainTex;
            float4 _Color;
            float  _UseTexture;

			float enableDirLight;
			float EnablePointLight;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex     : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
                float3 worldNormal: TEXCOORD2;
            };

			float4 LightPosition;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex      = UnityObjectToClipPos(v.vertex);
                o.worldPos    = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv          = v.uv;
                return o;
            }

            fixed4 frag(v2f i, float face : VFACE) : SV_Target
            {
                // Two-sided normal
                float3 n = normalize(i.worldNormal);
                if (face < 0) n = -n;

                // Support both directional (w==0) and point (w==1) light
                float3 lightDir = (_WorldSpaceLightPos0.w == 0)
                    ? normalize(_WorldSpaceLightPos0.xyz)
                    : normalize(_WorldSpaceLightPos0.xyz - i.worldPos);

                float ndotl = saturate(dot(n, lightDir));

                // Ambient + diffuse
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                float3 diffuse = _LightColor0.rgb * ndotl;
                float3 lighting = ambient + diffuse;

                fixed4 baseCol = (_UseTexture < 0.5)
                    ? _Color
                    : tex2D(_MainTex, i.uv);

                baseCol.rgb *= lighting;
                return baseCol;
            }
            ENDCG
        }
    }
    Fallback Off
}