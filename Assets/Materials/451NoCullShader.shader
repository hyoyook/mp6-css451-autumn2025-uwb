﻿//  Built with the help of AI
Shader "Custom/451NoCullShader"
{
    Properties
    {
        _MainTex        ("Main Texture", 2D) = "white" {}
        _Color          ("Base Color", Color) = (1,1,1,1)
        _UseTexture     ("Use Texture (0/1)", Float) = 1

        _Ambient        ("Ambient Intensity", Range(0,1)) = 0.15

        // Optional UI-tweakable point-light settings
        _PointLightColor     ("Point Light Color", Color) = (1,1,1,1)
        _PointLightIntensity ("Point Light Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off          // no culling, must handle two-sided in the frag

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;
            float4   _Color;
            float    _UseTexture;
            float    _Ambient;

            // Point light controls
            float4 _PointLightColor;
            float  _PointLightIntensity;

            // Global uniforms you control from LightControl.cs
            float  _EnableDirLight;    // 1 = use Unity main light, 0 = ignore
            float  _EnablePointLight;  // 1 = use LightPosition, 0 = ignore
            float4 _LightPosition;      // world-space position of ALightPosition

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                // Vertex position in object space to clip space
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos    = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                return o;
            }

            fixed4 frag (v2f i, float face : VFACE) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                // Two-sided: flip normal on back faces
                if (face < 0) n = -n;

                // --- Ambient ---
                float3 ambient = _Ambient * _Color.rgb;

                // --- Directional light from Unity main light ---
                float3 dirDiffuse = 0;

                if (_EnableDirLight > 0.5)
                {
                    // _WorldSpaceLightPos0.w == 0 → directional
                    float3 Ld;
                    if (_WorldSpaceLightPos0.w == 0)
                        Ld = normalize(_WorldSpaceLightPos0.xyz);
                    else
                        Ld = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);

                    float ndotl = saturate(dot(n, Ld));
                    dirDiffuse  = _LightColor0.rgb * ndotl;
                }

                // --- Point light using LightPosition (ALightPosition) ---
                float3 pointDiffuse = 0;

                if (_EnablePointLight > 0.5)
                {
                    float3 toPoint = _LightPosition.xyz - i.worldPos;
                    float  dist    = length(toPoint);

                    if (dist > 1e-4) // Avoid division by zero IF YOU USE AUTO FORMATTING (1e-4) CAN SPLIT AND NOT COUNT AS EXPONENT 
                    {
                        float3 Lp   = toPoint / dist;
                        float  ndot = saturate(dot(n, Lp));

                        // simple quadratic attenuation
                        float atten = 1.0 / (1.0 + 0.1 * dist + 0.02 * dist * dist);

                        pointDiffuse =
                            _PointLightColor.rgb * ndot * atten * _PointLightIntensity;
                    }
                }

                float3 lighting = ambient + dirDiffuse + pointDiffuse;

                // --- Base color: texture or plain color ---
                fixed4 baseCol;
                if (_UseTexture < 0.5)
                    baseCol = _Color;
                else
                    baseCol = tex2D(_MainTex, i.uv) * _Color;

                baseCol.rgb *= lighting;
                return baseCol;
            }
            ENDCG
        }
    }

    Fallback Off
}
