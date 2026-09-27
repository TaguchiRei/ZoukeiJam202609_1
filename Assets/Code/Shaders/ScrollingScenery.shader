// 横スクロールの背景を、テクスチャを使わずに関数とノイズで描くシェーダー。正投影カメラの前に置いた Quad に使う。
// 奥から順に、空のグラデーション → 雲 → 遠くの山 → 近くの丘 → 野原 → 電柱 → 縁石 → 道路（路面の粒・白線）→ 手前の草 を重ねる。
// 位置と長さはワールド座標のユニットで指定し、高さは _GroundY（タイヤの接地位置）を基準にする。
// 各層は、そのピクセルのワールド X に「_ScrollDistance × 層ごとの視差」を足した位置の模様を描く。視差 1 の層はバイクと同じ速さで流れる。
// モーションブラーは、スクロール量を _ScrollSpeed × _ShutterTime の幅でずらした複数の位置の平均で表す
Shader "ZoukeiJam1/Background/ScrollingScenery"
{
    Properties
    {
        [Header(Scroll)]
        _ScrollDistance ("Scroll Distance (set by script)", Float) = 0
        _ScrollSpeed ("Scroll Speed (units per second, set by script)", Float) = 0
        _GroundY ("Ground Y (world, tire contact)", Float) = -3

        [Header(Motion Blur)]
        _ShutterTime ("Shutter Time (seconds)", Range(0, 0.1)) = 0.025
        _MaxBlurLength ("Max Blur Length (units)", Float) = 2

        [Header(Sky)]
        _SkyTopColor ("Sky Top Color", Color) = (0.3, 0.55, 0.92, 1)
        _SkyHorizonColor ("Sky Horizon Color", Color) = (0.86, 0.93, 1, 1)
        _SkyGradientHeight ("Sky Gradient Height (above horizon)", Float) = 8
        _HorizonHeight ("Horizon Height (above ground)", Float) = 2

        [Header(Clouds)]
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.9)
        _CloudParallax ("Cloud Parallax", Float) = 0.03
        _CloudDriftSpeed ("Cloud Drift Speed (units per second)", Float) = 0.3
        _CloudRange ("Cloud Range (x Bottom, y Top, above horizon)", Vector) = (3, 8, 0, 0)
        _CloudScale ("Cloud Scale", Float) = 0.18
        _CloudCoverage ("Cloud Coverage", Range(0, 1)) = 0.45

        [Header(Far Mountains)]
        _FarColor ("Far Color", Color) = (0.55, 0.68, 0.85, 1)
        _FarParallax ("Far Parallax", Float) = 0.08
        _FarHeight ("Far Height (x Base, y Amplitude, above horizon)", Vector) = (0.8, 3.2, 0, 0)
        _FarFrequency ("Far Frequency", Float) = 0.09

        [Header(Near Hills)]
        _NearColor ("Near Color", Color) = (0.42, 0.62, 0.45, 1)
        _NearParallax ("Near Parallax", Float) = 0.25
        _NearHeight ("Near Height (x Base, y Amplitude, above horizon)", Vector) = (0.2, 1.6, 0, 0)
        _NearFrequency ("Near Frequency", Float) = 0.22

        [Header(Field)]
        _FieldColor ("Field Color", Color) = (0.5, 0.72, 0.36, 1)
        _FieldStripeColor ("Field Stripe Color", Color) = (0.44, 0.65, 0.31, 1)
        _FieldParallax ("Field Parallax", Float) = 0.55
        _FieldStripeFrequency ("Field Stripe Frequency", Float) = 0.6

        [Header(Poles)]
        _PoleColor ("Pole Color", Color) = (0.35, 0.3, 0.28, 1)
        _PoleParallax ("Pole Parallax", Float) = 0.7
        _PoleInterval ("Pole Interval (units)", Float) = 9
        _PoleHeight ("Pole Height (units)", Float) = 5.5
        _PoleWidth ("Pole Width (units)", Float) = 0.16
        _PoleBarSize ("Pole Bar Size (x Width, y Height)", Vector) = (1.2, 0.1, 0, 0)

        [Header(Road)]
        _RoadColor ("Road Color", Color) = (0.33, 0.33, 0.36, 1)
        _RoadRange ("Road Range (x Top above ground, y Bottom below ground)", Vector) = (0.5, 1.6, 0, 0)
        _RoadGrainStrength ("Road Grain Strength", Range(0, 1)) = 0.12
        _RoadGrainFrequency ("Road Grain Frequency", Float) = 4

        [Header(Curb)]
        _CurbColorA ("Curb Color A", Color) = (0.9, 0.25, 0.2, 1)
        _CurbColorB ("Curb Color B", Color) = (0.95, 0.95, 0.95, 1)
        _CurbHeight ("Curb Height (units)", Float) = 0.22
        _CurbLength ("Curb Stripe Length (units)", Float) = 1
        _CurbParallax ("Curb Parallax", Float) = 0.9

        [Header(Lane)]
        _LaneColor ("Lane Color", Color) = (0.95, 0.95, 0.9, 1)
        _LaneDepth ("Lane Depth (below ground)", Float) = 0.9
        _LaneHeight ("Lane Height (units)", Float) = 0.12
        _LaneDash ("Lane Dash (x Length, y Gap)", Vector) = (2.5, 2.5, 0, 0)
        _LaneParallax ("Lane Parallax", Float) = 1.15

        [Header(Foreground Grass)]
        _GrassColor ("Grass Color", Color) = (0.3, 0.52, 0.25, 1)
        _GrassTipColor ("Grass Tip Color", Color) = (0.4, 0.62, 0.3, 1)
        _GrassParallax ("Grass Parallax", Float) = 1.4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Background"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            Name "Unlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // モーションブラーで平均するサンプル数
            #define BLUR_SAMPLES 8

            CBUFFER_START(UnityPerMaterial)
                float _ScrollDistance;
                float _ScrollSpeed;
                float _GroundY;

                float _ShutterTime;
                float _MaxBlurLength;

                float4 _SkyTopColor;
                float4 _SkyHorizonColor;
                float _SkyGradientHeight;
                float _HorizonHeight;

                float4 _CloudColor;
                float _CloudParallax;
                float _CloudDriftSpeed;
                float4 _CloudRange;
                float _CloudScale;
                float _CloudCoverage;

                float4 _FarColor;
                float _FarParallax;
                float4 _FarHeight;
                float _FarFrequency;

                float4 _NearColor;
                float _NearParallax;
                float4 _NearHeight;
                float _NearFrequency;

                float4 _FieldColor;
                float4 _FieldStripeColor;
                float _FieldParallax;
                float _FieldStripeFrequency;

                float4 _PoleColor;
                float _PoleParallax;
                float _PoleInterval;
                float _PoleHeight;
                float _PoleWidth;
                float4 _PoleBarSize;

                float4 _RoadColor;
                float4 _RoadRange;
                float _RoadGrainStrength;
                float _RoadGrainFrequency;

                float4 _CurbColorA;
                float4 _CurbColorB;
                float _CurbHeight;
                float _CurbLength;
                float _CurbParallax;

                float4 _LaneColor;
                float _LaneDepth;
                float _LaneHeight;
                float4 _LaneDash;
                float _LaneParallax;

                float4 _GrassColor;
                float4 _GrassTipColor;
                float _GrassParallax;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Hash21(float2 p)
            {
                float3 p3 = frac(p.xyx * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // 0〜1 の 1 次元のバリューノイズ
            float Noise1(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(Hash11(i), Hash11(i + 1.0), u);
            }

            // 0〜1 の 2 次元のバリューノイズ
            float Noise2(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // 3 オクターブ重ねた 0〜1 のノイズ
            float Fbm1(float x)
            {
                return Noise1(x) * 0.55 + Noise1(x * 2.03 + 17.1) * 0.3 + Noise1(x * 4.01 + 41.7) * 0.15;
            }

            float Fbm2(float2 p)
            {
                return Noise2(p) * 0.55 + Noise2(p * 2.03 + 17.1) * 0.3 + Noise2(p * 4.01 + 41.7) * 0.15;
            }

            // 境界からの符号付き距離 d（内側が正）を、1 ピクセル幅 aa でなめらかにした塗りの割合にする
            float Coverage(float d, float aa)
            {
                return saturate(d / aa + 0.5);
            }

            // 間隔 period で並ぶ、幅 width の区間の内側までの符号付き距離（内側が正）。区間の中心は u = 0 に合わせる
            float RepeatedSegment(float u, float period, float width)
            {
                float local = (frac(u / period + 0.5) - 0.5) * period;
                return width * 0.5 - abs(local);
            }

            // スクロール量 scroll のときの、ワールド座標 (x, y) の背景の色
            float3 SampleScenery(float x, float y, float scroll, float aa)
            {
                float ground = _GroundY;
                float horizon = ground + _HorizonHeight;
                float roadTop = ground + _RoadRange.x;
                float roadBottom = ground - _RoadRange.y;
                float curbTop = roadTop + _CurbHeight;

                // 空
                float3 color = lerp(_SkyHorizonColor.rgb, _SkyTopColor.rgb, saturate((y - horizon) / _SkyGradientHeight));

                // 雲
                float cloudY = y - horizon;
                float cloudBand = smoothstep(_CloudRange.x, _CloudRange.x + 1.5, cloudY)
                    * (1.0 - smoothstep(_CloudRange.y - 1.5, _CloudRange.y, cloudY));
                float cloudU = (x + scroll * _CloudParallax + _Time.y * _CloudDriftSpeed) * _CloudScale;
                float cloudDensity = Fbm2(float2(cloudU, cloudY * _CloudScale * 2.0));
                float cloudThreshold = 1.0 - _CloudCoverage;
                float cloudAlpha = smoothstep(cloudThreshold, cloudThreshold + 0.12, cloudDensity) * cloudBand;
                color = lerp(color, _CloudColor.rgb, cloudAlpha * _CloudColor.a);

                // 遠くの山
                float farTop = horizon + _FarHeight.x + Fbm1((x + scroll * _FarParallax) * _FarFrequency) * _FarHeight.y;
                color = lerp(color, _FarColor.rgb, Coverage(farTop - y, aa));

                // 近くの丘
                float nearTop = horizon + _NearHeight.x + Fbm1((x + scroll * _NearParallax) * _NearFrequency + 73.3) * _NearHeight.y;
                color = lerp(color, _NearColor.rgb, Coverage(nearTop - y, aa));

                // 野原
                float fieldU = x + scroll * _FieldParallax;
                float3 fieldColor = lerp(_FieldColor.rgb, _FieldStripeColor.rgb,
                    smoothstep(0.45, 0.55, Noise1(fieldU * _FieldStripeFrequency)));
                color = lerp(color, fieldColor, Coverage(horizon - y, aa));

                // 電柱（縁石の上に立つ柱と、上端付近の横木）
                float poleU = x + scroll * _PoleParallax;
                float poleTop = curbTop + _PoleHeight;
                float pole = Coverage(RepeatedSegment(poleU, _PoleInterval, _PoleWidth), aa)
                    * Coverage(y - curbTop, aa) * Coverage(poleTop - y, aa);
                float barCenter = poleTop - _PoleBarSize.y * 3.0;
                float bar = Coverage(RepeatedSegment(poleU, _PoleInterval, _PoleBarSize.x), aa)
                    * Coverage(_PoleBarSize.y * 0.5 - abs(y - barCenter), aa);
                color = lerp(color, _PoleColor.rgb, max(pole, bar));

                // 縁石（色 A と B を交互に並べる）
                float curbU = x + scroll * _CurbParallax;
                float curbA = Coverage(RepeatedSegment(curbU, _CurbLength * 2.0, _CurbLength), aa);
                float3 curbColor = lerp(_CurbColorB.rgb, _CurbColorA.rgb, curbA);
                color = lerp(color, curbColor, Coverage(curbTop - y, aa));

                // 道路（路面の粒は視差 1 で流す）
                float roadU = x + scroll;
                float grain = Noise2(float2(roadU, y * 1.5) * _RoadGrainFrequency) - 0.5;
                float3 roadColor = _RoadColor.rgb * (1.0 + grain * _RoadGrainStrength * 2.0);
                float laneU = x + scroll * _LaneParallax;
                float lane = Coverage(RepeatedSegment(laneU, _LaneDash.x + _LaneDash.y, _LaneDash.x), aa)
                    * Coverage(_LaneHeight * 0.5 - abs(y - (ground - _LaneDepth)), aa);
                roadColor = lerp(roadColor, _LaneColor.rgb, lane);
                color = lerp(color, roadColor, Coverage(roadTop - y, aa));

                // 手前の草（上端をノイズで波打たせる）
                float grassU = x + scroll * _GrassParallax;
                float grassTop = roadBottom + (Noise1(grassU * 3.0) - 0.5) * 0.25;
                float3 grassColor = lerp(_GrassColor.rgb, _GrassTipColor.rgb,
                    smoothstep(0.4, 0.7, Noise1(grassU * 1.3 + 11.0)));
                color = lerp(color, grassColor, Coverage(grassTop - y, aa));

                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float x = input.positionWS.x;
                float y = input.positionWS.y;
                float aa = max(fwidth(y), 1e-4);
                float blurLength = min(_ScrollSpeed * _ShutterTime, _MaxBlurLength);

                if (blurLength <= aa)
                {
                    return half4(SampleScenery(x, y, _ScrollDistance, aa), 1.0);
                }

                float3 sum = 0;
                [unroll]
                for (int i = 0; i < BLUR_SAMPLES; i++)
                {
                    float t = (i + 0.5) / BLUR_SAMPLES - 0.5;
                    sum += SampleScenery(x, y, _ScrollDistance + t * blurLength, aa);
                }
                return half4(sum / BLUR_SAMPLES, 1.0);
            }
            ENDHLSL
        }
    }
}
