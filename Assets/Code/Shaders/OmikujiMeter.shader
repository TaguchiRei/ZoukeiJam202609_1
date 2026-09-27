// UI（UGUI の Image）用の、おみくじの確率メーター。
// 矩形の中心を原点に、上を 0 として時計回りに _ArcDegrees 度のアーチを描き、アーチの左端を 0、右端を 1 とする位置 t で各要素を配置する。
// 奥から順に、背景の円 → 速度のトラック（確率が変わる範囲の強調・現在速度までの塗り・目盛り）→ 確率の帯（5 色・区切り線・光の筋）→ 針 → 中心の軸を重ねる。
// 半径・幅などの長さは、矩形の一辺を 1 とした単位で指定する（0.5 が矩形の端）。矩形は正方形にする
Shader "ZoukeiJam1/UI/OmikujiMeter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Arc)]
        _ArcDegrees ("Arc Degrees", Range(1, 360)) = 270

        [Header(Background)]
        _BackgroundColor ("Background Center Color", Color) = (0.05, 0.05, 0.09, 0.85)
        _BackgroundEdgeColor ("Background Edge Color", Color) = (0.13, 0.12, 0.2, 0.92)
        _BackgroundRadius ("Background Radius", Range(0, 0.5)) = 0.5

        [Header(Probability Band)]
        _BandRadius ("Band Radius (x Inner, y Outer)", Vector) = (0.4, 0.47, 0, 0)
        _SegmentBounds ("Segment Bounds (0-1, cumulative)", Vector) = (0.2, 0.4, 0.6, 0.8)
        _SegmentColor0 ("Segment Color 0", Color) = (0.45, 0.35, 0.6, 1)
        _SegmentColor1 ("Segment Color 1", Color) = (0.35, 0.7, 1, 1)
        _SegmentColor2 ("Segment Color 2", Color) = (0.95, 0.4, 0.65, 1)
        _SegmentColor3 ("Segment Color 3", Color) = (1, 0.45, 0.25, 1)
        _SegmentColor4 ("Segment Color 4", Color) = (1, 0.78, 0.2, 1)
        _BandShade ("Band Brightness (x Inner, y Outer)", Vector) = (0.65, 1.15, 0, 0)
        _SeparatorColor ("Separator Color", Color) = (0.04, 0.04, 0.07, 1)
        _SeparatorWidth ("Separator Width", Range(0, 0.05)) = 0.006
        _BandGlowSize ("Band Glow Size", Range(0.0001, 0.1)) = 0.02
        _BandGlowStrength ("Band Glow Strength", Range(0, 1)) = 0.55
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 0.35)
        _ShineSpeed ("Shine Speed (loops per second)", Float) = 0.35
        _ShineWidth ("Shine Width (0-1 of arc)", Range(0.001, 0.5)) = 0.06

        [Header(Speed Track)]
        _SpeedT ("Speed (0-1 of arc)", Range(0, 1)) = 0
        _TrackRadius ("Track Radius (x Inner, y Outer)", Vector) = (0.3, 0.36, 0, 0)
        _TrackColor ("Track Color", Color) = (0.18, 0.18, 0.26, 1)
        _ProbabilityRange ("Probability Range (x Start, y End, 0-1 of arc)", Vector) = (0.2, 0.75, 0, 0)
        _RangeColor ("Probability Range Color", Color) = (1, 1, 1, 0.12)
        _FillColorLow ("Fill Color Low", Color) = (0.2, 0.8, 1, 1)
        _FillColorHigh ("Fill Color High", Color) = (1, 0.78, 0.2, 1)

        [Header(Ticks)]
        _TickRadius ("Tick Radius (x Inner, y Outer)", Vector) = (0.245, 0.29, 0, 0)
        _TickWidth ("Tick Width (x Major, y Minor)", Vector) = (0.008, 0.004, 0, 0)
        _MajorTickCount ("Major Tick Divisions", Float) = 8
        _MinorTickCount ("Minor Tick Divisions per Major", Float) = 4
        _TickColor ("Tick Color", Color) = (0.55, 0.55, 0.65, 1)
        _TickLitColor ("Tick Lit Color", Color) = (1, 1, 1, 1)

        [Header(Needle)]
        _NeedleColor ("Needle Color", Color) = (1, 0.25, 0.2, 1)
        _NeedleLength ("Needle Length", Range(0, 0.5)) = 0.43
        _NeedleTail ("Needle Tail Length", Range(0, 0.2)) = 0.06
        _NeedleWidth ("Needle Width (x Base, y Tip)", Vector) = (0.022, 0.006, 0, 0)
        _NeedleGlowColor ("Needle Glow Color", Color) = (1, 0.3, 0.2, 0.6)
        _NeedleGlowSize ("Needle Glow Size", Range(0.0001, 0.1)) = 0.02

        [Header(Hub)]
        _HubRadius ("Hub Radius", Range(0, 0.2)) = 0.045
        _HubColor ("Hub Color", Color) = (0.85, 0.85, 0.9, 1)
        _HubEdgeColor ("Hub Edge Color", Color) = (0.3, 0.3, 0.35, 1)

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

            float _ArcDegrees;

            float4 _BackgroundColor;
            float4 _BackgroundEdgeColor;
            float _BackgroundRadius;

            float4 _BandRadius;
            float4 _SegmentBounds;
            float4 _SegmentColor0;
            float4 _SegmentColor1;
            float4 _SegmentColor2;
            float4 _SegmentColor3;
            float4 _SegmentColor4;
            float4 _BandShade;
            float4 _SeparatorColor;
            float _SeparatorWidth;
            float _BandGlowSize;
            float _BandGlowStrength;
            float4 _ShineColor;
            float _ShineSpeed;
            float _ShineWidth;

            float _SpeedT;
            float4 _TrackRadius;
            float4 _TrackColor;
            float4 _ProbabilityRange;
            float4 _RangeColor;
            float4 _FillColorLow;
            float4 _FillColorHigh;

            float4 _TickRadius;
            float4 _TickWidth;
            float _MajorTickCount;
            float _MinorTickCount;
            float4 _TickColor;
            float4 _TickLitColor;

            float4 _NeedleColor;
            float _NeedleLength;
            float _NeedleTail;
            float4 _NeedleWidth;
            float4 _NeedleGlowColor;
            float _NeedleGlowSize;

            float _HubRadius;
            float4 _HubColor;
            float4 _HubEdgeColor;

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

            // 符号付き距離 d（内側が負）から、1 ピクセル幅でなめらかにした塗りの割合を求める
            float Coverage(float d, float px)
            {
                return saturate(0.5 - d / px);
            }

            // 乗算済みアルファで dst の上に色を重ねる
            void BlendOver(inout float4 dst, float3 rgb, float alpha)
            {
                dst.rgb = rgb * alpha + dst.rgb * (1.0 - alpha);
                dst.a = alpha + dst.a * (1.0 - alpha);
            }

            // アーチの範囲（t が 0〜1）に収まった帯までの符号付き距離
            float ArcBandDistance(float r, float t, float inner, float outer, float arcRad)
            {
                float radial = abs(r - (inner + outer) * 0.5) - (outer - inner) * 0.5;
                float angular = max(-t, t - 1.0) * arcRad * r;
                return max(radial, angular);
            }

            // t を count 等分した目盛りのうち、最も近い目盛りまでの弧の長さ
            float TickDistance(float t, float count, float r, float arcRad)
            {
                float x = t * count;
                return abs(frac(x + 0.5) - 0.5) / count * arcRad * r;
            }

            // 区切り位置 bound までの t の差。アーチの両端にある区切りは描かないので、遠い値を返す
            float SeparatorDistance(float t, float bound)
            {
                return (bound > 0.0001 && bound < 0.9999) ? abs(t - bound) : 1e5;
            }

            // t の位置にある確率の帯の色。区切りの前後は 1 ピクセル幅で混ぜる
            float3 SegmentColor(float t, float tpx)
            {
                float4 b = _SegmentBounds;
                float3 c = _SegmentColor0.rgb;
                c = lerp(c, _SegmentColor1.rgb, saturate((t - b.x) / tpx + 0.5));
                c = lerp(c, _SegmentColor2.rgb, saturate((t - b.y) / tpx + 0.5));
                c = lerp(c, _SegmentColor3.rgb, saturate((t - b.z) / tpx + 0.5));
                c = lerp(c, _SegmentColor4.rgb, saturate((t - b.w) / tpx + 0.5));
                return c;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 p = i.texcoord - 0.5;
                float r = length(p);
                float px = max(max(fwidth(p.x), fwidth(p.y)), 1e-5);

                float arcRad = radians(_ArcDegrees);
                float angle = atan2(p.x, p.y);
                float t = (angle + arcRad * 0.5) / arcRad;
                // 1 ピクセルあたりの t の変化量
                float tpx = px / (arcRad * max(r, 1e-4));

                float4 col = 0;

                // 背景の円
                float bgD = r - _BackgroundRadius;
                float4 bgColor = lerp(_BackgroundColor, _BackgroundEdgeColor, saturate(r / max(_BackgroundRadius, 1e-4)));
                BlendOver(col, bgColor.rgb, bgColor.a * Coverage(bgD, px));

                // 速度のトラック
                float trackCov = Coverage(ArcBandDistance(r, t, _TrackRadius.x, _TrackRadius.y, arcRad), px);
                if (trackCov > 0.0)
                {
                    BlendOver(col, _TrackColor.rgb, _TrackColor.a * trackCov);

                    float rangeMask = saturate(min(t - _ProbabilityRange.x, _ProbabilityRange.y - t) / tpx + 0.5);
                    BlendOver(col, _RangeColor.rgb, _RangeColor.a * rangeMask * trackCov);

                    float fillMask = saturate((_SpeedT - t) / tpx + 0.5);
                    float trackN = saturate((r - _TrackRadius.x) / max(_TrackRadius.y - _TrackRadius.x, 1e-4));
                    float4 fillColor = lerp(_FillColorLow, _FillColorHigh, saturate(t));
                    fillColor.rgb *= lerp(0.75, 1.2, trackN);
                    BlendOver(col, fillColor.rgb, fillColor.a * fillMask * trackCov);
                }

                // 目盛り。小目盛りは外側の半分だけに描く
                float tickEnd = max(-t, t - 1.0) * arcRad * r;
                float tickMid = (_TickRadius.x + _TickRadius.y) * 0.5;
                float tickHalfLength = (_TickRadius.y - _TickRadius.x) * 0.5;
                float majorD = max(abs(r - tickMid) - tickHalfLength,
                    TickDistance(t, max(_MajorTickCount, 1.0), r, arcRad) - _TickWidth.x * 0.5);
                majorD = max(majorD, tickEnd - _TickWidth.x * 0.5);
                float minorMid = _TickRadius.y - tickHalfLength * 0.5;
                float minorD = max(abs(r - minorMid) - tickHalfLength * 0.5,
                    TickDistance(t, max(_MajorTickCount * _MinorTickCount, 1.0), r, arcRad) - _TickWidth.y * 0.5);
                minorD = max(minorD, tickEnd - _TickWidth.y * 0.5);
                float tickCov = max(Coverage(majorD, px), Coverage(minorD, px));
                float4 tickColor = t <= _SpeedT ? _TickLitColor : _TickColor;
                BlendOver(col, tickColor.rgb, tickColor.a * tickCov);

                // 確率の帯。帯の外側には、その位置の帯の色で光をにじませる
                float bandD = ArcBandDistance(r, t, _BandRadius.x, _BandRadius.y, arcRad);
                float3 segColor = SegmentColor(saturate(t), tpx);
                float glow = exp(-max(bandD, 0.0) / _BandGlowSize) * _BandGlowStrength;
                BlendOver(col, segColor, glow * (1.0 - Coverage(bandD, px)));

                float bandCov = Coverage(bandD, px);
                if (bandCov > 0.0)
                {
                    float bandN = saturate((r - _BandRadius.x) / max(_BandRadius.y - _BandRadius.x, 1e-4));
                    float3 bandColor = segColor * lerp(_BandShade.x, _BandShade.y, bandN);

                    float4 b = _SegmentBounds;
                    float sepT = min(min(SeparatorDistance(t, b.x), SeparatorDistance(t, b.y)),
                        min(SeparatorDistance(t, b.z), SeparatorDistance(t, b.w)));
                    float sepCov = Coverage(sepT * arcRad * r - _SeparatorWidth * 0.5, px);
                    bandColor = lerp(bandColor, _SeparatorColor.rgb, sepCov * _SeparatorColor.a);

                    BlendOver(col, bandColor, bandCov);

                    // アーチに沿って流れる光の筋。両端の外まで流してから戻す
                    float shinePos = frac(_Time.y * _ShineSpeed) * (1.0 + _ShineWidth * 4.0) - _ShineWidth * 2.0;
                    float shineX = (t - shinePos) / _ShineWidth;
                    float shine = exp(-shineX * shineX) * _ShineColor.a * (1.0 - sepCov);
                    col.rgb += _ShineColor.rgb * shine * bandCov;
                }

                // 針
                float needleAngle = (saturate(_SpeedT) - 0.5) * arcRad;
                float2 dir = float2(sin(needleAngle), cos(needleAngle));
                float h = clamp(dot(p, dir), -_NeedleTail, _NeedleLength);
                float halfWidth = lerp(_NeedleWidth.x, _NeedleWidth.y, saturate(h / max(_NeedleLength, 1e-4))) * 0.5;
                float needleD = length(p - dir * h) - halfWidth;
                float needleGlow = exp(-max(needleD, 0.0) / _NeedleGlowSize) * _NeedleGlowColor.a;
                BlendOver(col, _NeedleGlowColor.rgb, needleGlow * (1.0 - Coverage(needleD, px)));
                BlendOver(col, _NeedleColor.rgb, _NeedleColor.a * Coverage(needleD, px));

                // 中心の軸
                float hubD = r - _HubRadius;
                float hubEdge = Coverage(-(r - _HubRadius * 0.7), px);
                float3 hubColor = lerp(_HubEdgeColor.rgb, _HubColor.rgb, hubEdge);
                BlendOver(col, hubColor, _HubColor.a * Coverage(hubD, px));

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
