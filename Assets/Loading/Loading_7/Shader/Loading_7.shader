
Shader "Hidden/Loading/Loading_7"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // === Custom Property Begin ===
        _Color1("Color1",Color) = (1,1,1,1)
        _Color2("Color3",Color) = (1,1,1,1)
        _Color3("Color3",Color) = (1,1,1,1)
        _InnerRadius("Inner Radius",float) = 0.6
        _OuterRadrid("Outer Radrid",float) = 1
        _NoiseScale("Noise Scale",float) = 0.65
        _HightLightSepeed("HightLight Sepeed",float) = -1
        _ColorLightSpeed("ColorLight Speed",float) = 2
        _RingScale("_ringScale",float) = 1.2
        // === Custom Property End ===

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //sampler2D _MainTex;
            fixed4 _Color;
            //fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            //float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            // === Custom Property Begin ===
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _InnerRadius;
            float _OuterRadrid;
            float _NoiseScale;
            float _HightLightSepeed;
            float _ColorLightSpeed;
            float _RingScale;
            // === Custom Property End ===

            // https://www.shadertoy.com/view/3tBGRm
            float light1(float intensity, float attenuation, float dist)
            {
                return intensity / (1.0 + dist * attenuation);
            }

            float light2(float intensity, float attenuation, float dist)
            {
                return intensity / (1.0 + dist * dist * attenuation);
            }

            float3 hash33(float3 p3)
            {
                p3 = frac(p3 * float3(0.1031, 0.11369, 0.13787));
                p3 += dot(p3, p3.yxz + 19.19);
                return -1 + 2 * frac(float3(p3.x + p3.y, p3.x + p3.z, p3.y + p3.z) * p3.zyx);
            }

            float snoise3(float3 p)
            {
                const float K1 = 0.333333333;
                const float K2 = 0.166666667;

                float3 i = floor(p + (p.x + p.y + p.z) * K1);
                float3 d0 = p - (i - (i.x + i.y + i.z) * K2);

                float3 e = step(0, d0 - d0.yzx);
                float3 i1 = e * (1.0 - e.zxy);
                float3 i2 = 1.0 - e.zxy * (1.0 - e);

                float3 d1 = d0 - (i1 - K2);
                float3 d2 = d0 - (i2 - K1);
                float3 d3 = d0 - 0.5;

                float4 h = max(0.6 - float4(dot(d0, d0), dot(d1, d1), dot(d2, d2), dot(d3, d3)), 0.0);
                float4 n = h * h * h * h * float4(dot(d0, hash33(i)), dot(d1, hash33(i + i1)), dot(d2, hash33(i + i2)), dot(d3, hash33(i + 1.0)));

                return dot(float4(31.316, 31.316, 31.316, 31.316), n);
            }

            float4 Draw(float2 vUv)
            {
                float2 uv = vUv;
                float angle = atan2(uv.y, uv.x);
                float len = length(uv);
                float v0, v1, v2, v3, c1;
                float r0, d0, n0;
                float r, d;
                float time = _Time.y;

                //ring
                n0 = snoise3(float3(uv * _NoiseScale, time * 0.5)) * 0.5 + 0.5;
                r0 = lerp(lerp(_InnerRadius, 1.0, 0.4), lerp(_InnerRadius, 1.0, 0.6), n0);
                d0 = distance(uv, r0 / len * uv);
                v0 = light1(1.0, 10.0, d0);
                v0 *= smoothstep(r0 * 1.05, r0, len);
                c1 = cos(angle + time * _ColorLightSpeed) * 0.5 + 0.5;

                //high light
                float a = time * _HightLightSepeed;
                float2 pos = float2(cos(a), sin(a)) * r0;
                d = distance(uv, pos);
                v1 = light2(1.5, 5.0, d);
                v1 *= light1(1.0, 50.0, d0);

                //back ring
                v2 = smoothstep(1.0, lerp(_InnerRadius, 1.0, n0 * 0.5), len);
                //v2 *= smoothstep(_InnerRadius, lerp(r0, _InnerRadius, n0 * 0.5), len);

                //hole
                v3 = smoothstep(_InnerRadius, _InnerRadius * 1.1, len);

                //color
                float3 c = lerp(_Color1, _Color2, c1);
                float4 col;
                col.rgb = lerp(_Color3, c, v0);
                col.rgb += float3(v1, v1, v1);
                col.a = max(v0, max(v1, v2) * v3);
                col = clamp(col, 0, 1);

                return col;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                //OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.texcoord = v.texcoord.xy;
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = IN.color;

                color = Draw((IN.texcoord * 2 - float2(1, 1)) * _RingScale);
               

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
}
