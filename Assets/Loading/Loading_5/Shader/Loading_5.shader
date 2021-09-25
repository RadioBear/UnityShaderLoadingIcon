
Shader "Hidden/Loading/Loading_5"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // === Custom Property Begin ===
        _Color1("Color1",Color) = (1,1,1,1)
        _Color2("Color2",Color) = (1,1,1,1)
        _RingColor("_RingColor",Color) = (1,1,1,1)
        _Speed("Speed",float) = 1
        _Radiu("Radiu",Range(0.01,0.5)) = 0.25
        _Width("Width",Range(0.01,0.5)) = 0.1
        _Interval("Interval",Range(1,20)) = 8
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

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            // === Custom Property Begin ===
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _RingColor;
            fixed _Radiu;
            fixed _Width;
            fixed _Interval;
            fixed _Speed;
            // === Custom Property End ===

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

                fixed2 uv = IN.texcoord - 0.5;
                fixed distance = length(uv);
                //ring
                fixed ring = (1 - smoothstep(_Radiu, _Radiu, distance)) * smoothstep(_Radiu - _Width, _Radiu - _Width, distance);
                //interval
                fixed angle = atan2(uv.x, uv.y) / UNITY_PI * _Interval;
                fixed fract = frac(angle);
                fixed interval = 1 - step(smoothstep(0, 0.1, fract) * smoothstep(1, 0.9, fract), 0.1);
                //light
                angle = floor(fmod(_Time.y * _Speed, _Interval * 2)) * UNITY_PI / _Interval;
                uv = mul(uv, float2x2(cos(angle), sin(angle), -sin(angle), cos(angle)));
                fixed lightEnd = atan2(uv.x, uv.y);
                angle = UNITY_PI / _Interval;
                uv = mul(uv, float2x2(cos(angle), sin(angle), -sin(angle), cos(angle)));
                fixed lightStart = atan2(uv.x, uv.y);
                fixed light = sign(lightStart - lightEnd) * smoothstep(_Radiu - _Width, _Radiu, length(uv));

                //color
                fixed ringColorMask = step(lightStart - lightEnd, 1) * ring * interval;
                fixed4 ringCol = _RingColor * ringColorMask;

                fixed tickColorMask = (1 - ringColorMask) * ring * interval;
                fixed4 tickColor = lerp(_Color1, _Color2, light) * tickColorMask;

                color = tickColor + ringCol;


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
