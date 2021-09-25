
Shader "Hidden/Loading/Loading_2"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // === Custom Property Begin ===
        _Move("Move", Range(0, 1)) = 0.1
        _Breath("Breath", float) = 0.1
        _Wait("Wait", Range(0, 1)) = 0
        _Angle("Angle", float) = 0.1
        _Width("Width", Range(0, 1)) = 0.1
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
            fixed _Move;
            fixed _Breath;
            fixed _Wait;
            fixed _Angle;
            fixed _Width;
            // === Custom Property End ===

            static fixed2 k_RectCenter[9] =
            {
                fixed2(0, 0),
                fixed2(1, 0),
                fixed2(1, 1),
                fixed2(0, 1),

                fixed2(-1, 1),
                fixed2(-1, 0),
                fixed2(-1, -1),
                fixed2(0, -1),

                fixed2(1, -1),
            };

            fixed DrawRect(uint index, fixed2 uv, fixed2 center, fixed2 size)
            {
                fixed2 newUV = uv - center;
                fixed angle = step(1, index) * (_Time.y * _Breath + radians(index * 30));
                newUV = fixed2(newUV.x * cos(angle) - newUV.y * sin(angle), newUV.x * sin(angle) + newUV.y * cos(angle));
                return step(-0.5 * size.x, newUV.x) * step(newUV.x, 0.5 * size.x) * step(-0.5 * size.y, newUV.y) * step(newUV.y, 0.5 * size.y);
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

                half alpha = 0;
                for (uint i = 0; i < 9; ++i)
                {
                    fixed w = _Width;
                    fixed curTime = _Time.y - (i - 1) * _Wait;
                    curTime = step(0, curTime) * curTime;
                    fixed add = smoothstep(0, 1, abs(sin(curTime)));
                    fixed2 rectCenter = _Move * add * k_RectCenter[i];
                    add = smoothstep(0, 1, 1 - abs(sin(_Time.y)));
                    w += step(i, 0) * _Move * add;
                    alpha += DrawRect(i, IN.texcoord, rectCenter + fixed2(0.5, 0.5), fixed2(w, w));
                }
                color.a *= alpha;

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
