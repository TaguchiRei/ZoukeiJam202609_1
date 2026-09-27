// UI（UGUI の Image）用の、ゴール時の画面演出。画面全体に広げた Image に使う。
// 奥から順に、画面の縁の光 → 集中線 → フラッシュを重ねる。
// 長さは画面の高さを 1 とした単位で指定する。_Center は Image の矩形の左下を (0, 0)、右上を (1, 1) とした位置。
// 集中線は _Center を中心に円周を _LineCount 個の区画に分け、区画ごとに乱数で太さ・位置のずれ・始まる半径・有無を決め、
// _FlickerRate 回/秒で乱数を引き直す。線は始まる半径から外側へ向かって太くなる。
// 線の始まる半径は、_LineIntensity が 0 のとき _GrowDistance だけ外側（画面外）になり、1 に近づくほど (1 - _LineIntensity)^2 に比例して内側へ伸びる
Shader "ZoukeiJam1/UI/GoalBurst"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Set By Script)]
        _Center ("Center (0-1 of rect)", Vector) = (0.5, 0.5, 0, 0)
        _Aspect ("Aspect (width / height of rect)", Float) = 1.7777778
        _LineIntensity ("Line Intensity", Range(0, 1)) = 1
        _Flash ("Flash", Range(0, 1)) = 0
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 1

        [Header(Concentration Lines)]
        _LineColor ("Line Color", Color) = (1, 1, 1, 0.9)
        _LineCount ("Line Count (divisions of circle)", Float) = 120
        _LineDensity ("Line Density (0-1 of divisions)", Range(0, 1)) = 0.55
        _LineWidth ("Line Half Width (x Min, y Max, 0-0.5 of division)", Vector) = (0.08, 0.3, 0, 0)
        _LineJitter ("Line Center Jitter (0-1 of division)", Range(0, 1)) = 0.5
        _InnerRadius ("Line Start Radius (x Min, y Max)", Vector) = (0.3, 0.55, 0, 0)
        _TaperLength ("Taper Length", Float) = 0.5
        _GrowDistance ("Grow Distance", Float) = 1.5
        _FlickerRate ("Flicker Rate (per second)", Float) = 12

        [Header(Edge Glow)]
        _GlowColor ("Glow Color", Color) = (1, 0.8, 0.25, 0.8)
        _GlowWidth ("Glow Width", Range(0.001, 0.5)) = 0.18
        _GlowPulseSpeed ("Glow Pulse Speed (per second)", Float) = 1.5
        _GlowPulseAmount ("Glow Pulse Amount (0-1)", Range(0, 1)) = 0.25

        [Header(Flash)]
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)

        [Header(UI)]
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
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
        // フラグメントは乗算済みアルファで出力する
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float4 _ClipRect;

            float4 _Center;
            float _Aspect;
            float _LineIntensity;
            float _Flash;
            float _GlowIntensity;

            float4 _LineColor;
            float _LineCount;
            float _LineDensity;
            float4 _LineWidth;
            float _LineJitter;
            float4 _InnerRadius;
            float _TaperLength;
            float _GrowDistance;
            float _FlickerRate;

            float4 _GlowColor;
            float _GlowWidth;
            float _GlowPulseSpeed;
            float _GlowPulseAmount;

            float4 _FlashColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            // 0 以上 1 未満の擬似乱数
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Pow2(float x)
            {
                return x * x;
            }

            // 乗算済みアルファの色 col の上に、色 rgb を不透明度 a で重ねる
            void BlendOver(inout float4 col, float3 rgb, float a)
            {
                col.rgb = rgb * a + col.rgb * (1.0 - a);
                col.a = a + col.a * (1.0 - a);
            }

            // 集中線の不透明度（0-1）。p は中心からの位置（画面の高さを 1 とする単位）、px は 1 ピクセルの長さ
            float ConcentrationLines(float2 p, float px)
            {
                float r = length(p);
                float cell = (atan2(p.y, p.x) / (2.0 * UNITY_PI) + 0.5) * _LineCount;
                float index = floor(cell);
                float seed = floor(_Time.y * _FlickerRate);

                float present = step(Hash(float2(index, seed)), _LineDensity);
                float halfWidth = lerp(_LineWidth.x, _LineWidth.y, Hash(float2(index + 17.3, seed)));
                float offset = (Hash(float2(index + 41.7, seed)) - 0.5) * _LineJitter;
                float startRadius = lerp(_InnerRadius.x, _InnerRadius.y, Hash(float2(index + 73.1, seed)))
                    + Pow2(1.0 - saturate(_LineIntensity)) * _GrowDistance;

                float taper = saturate((r - startRadius) / max(_TaperLength, 1e-4));
                float distance = abs(frac(cell) - 0.5 - offset);
                // 1 ピクセルが区画の何個分に当たるか。atan2 の折り返しで fwidth が壊れるため、半径から求める
                float aa = _LineCount * px / (2.0 * UNITY_PI * max(r, 1e-4));
                float w = halfWidth * taper;
                return present * (1.0 - smoothstep(w - aa, w + aa, distance)) * step(1e-4, w);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float px = max(fwidth(uv.y), 1e-5);
                float4 col = float4(0, 0, 0, 0);

                // 画面の縁の光
                float edge = min(min(uv.x, 1.0 - uv.x) * _Aspect, min(uv.y, 1.0 - uv.y));
                float glow = 1.0 - saturate(edge / _GlowWidth);
                float pulse = 1.0 - _GlowPulseAmount * (0.5 + 0.5 * sin(_Time.y * _GlowPulseSpeed * 2.0 * UNITY_PI));
                BlendOver(col, _GlowColor.rgb, glow * glow * pulse * _GlowColor.a * saturate(_GlowIntensity));

                // 集中線
                float2 p = uv - _Center.xy;
                p.x *= _Aspect;
                BlendOver(col, _LineColor.rgb, ConcentrationLines(p, px) * _LineColor.a * step(1e-4, _LineIntensity));

                // フラッシュ
                BlendOver(col, _FlashColor.rgb, saturate(_Flash) * _FlashColor.a);

                col.rgb *= i.color.rgb * i.color.a;
                col.a *= i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                col *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
