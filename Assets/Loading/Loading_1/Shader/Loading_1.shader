Shader "Hidden/Loading/Loading_1"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)

		// === Custom Property Begin ===
		_InsideRadius("Inside Center Radius(uv size)", Range(0, 0.5)) = 0.1
		_Width("Width(uv size)", Range(0, 0.5)) = 0.1
		_BreathSpeed("Breath Speed", Float) = 1
		_Num("Num", Int) = 1
		_Dir("Dir", Int) = 1
		// === Custom Property End ===

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}


	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend One OneMinusSrcAlpha
		ColorMask[_ColorMask]

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
				fixed4 color	: COLOR;
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
			half _InsideRadius;
			half _Width;
			half _BreathSpeed;
			half _Dir;
			half _Num;
			// === Custom Property End ===

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float4 vPosition = UnityObjectToClipPos(v.vertex);
				o.worldPosition = v.vertex;
				o.vertex = vPosition;

				float2 pixelSize = vPosition.w;
				pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

				float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
				float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
				o.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
				o.texcoord = v.texcoord;
				o.color = v.color * _Color;
				return o;
			}



			half DrawRing(half rMax, half rMin, half2 uv)
			{
				half r = length(uv);
				// equal step(rMin, r) * step(r, rMax)
				if (r >= rMin && r <= rMax)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}

			half getZeroToOneLoop()
			{
				return fmod(_Time.y * _BreathSpeed, 1);
			}

			half4 frag(v2f i) : SV_Target
			{
				half2 newUv = i.texcoord.xy - half2(0.5, 0.5);
				half curAngle = degrees(atan2(newUv.y, newUv.x)) + 180;
				float angle = 360.0 / _Num;
				half interval = floor(curAngle / angle);
				half odd = fmod(interval, 2);
				interval += step(_Dir, 0) *_Num + _Dir * _Num * getZeroToOneLoop();
				interval = fmod(interval, _Num);
				half4 finalColor = i.color;
				finalColor.a = odd * DrawRing(_InsideRadius + _Width, _InsideRadius, newUv) * interval / _Num;


#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
				finalColor.a *= m.x * m.y;
#endif
#ifdef UNITY_UI_ALPHACLIP
				clip(finalColor.a - 0.001);
#endif
				finalColor.rgb *= finalColor.a;
				return finalColor;
			}
		ENDCG
		}
	}
}