Shader "UI/DynamicBackground"
{
    Properties
    {
        [Header(Colors)]
        _Color1 ("Color 1", Color) = (0.353, 0.427, 0.561, 1) // #5a6d8f
        _Color2 ("Color 2", Color) = (0.137, 0.184, 0.243, 1) // #232f3e
        _RimColor ("Rim Color", Color) = (0.878, 0.969, 0.98, 1) // #e0f7fa

        [Header(Animation)]
        _AnimSpeed ("Animation Speed", Range(0, 2)) = 0.3
        _ColorAnimSpeed ("Color Animation Speed", Range(0, 5)) = 1.0

        [Header(Effects)]
        _Opacity ("Shape Opacity", Range(0, 1)) = 0.5
        _RimLight ("Rim Light Intensity", Range(0, 1)) = 0.1
        _Blur ("Blur Amount", Range(0, 10)) = 0
        _Vignette ("Vignette Strength", Range(0, 1)) = 0.72

        [Header(Shape Settings)]
        _NumShapes ("Number of Shapes", Range(1, 10)) = 5
        _Curvature ("Shape Curvature", Range(0, 2)) = 0.7

        [Header(Advanced)]
        _ShapeData ("Shape Data Texture", 2D) = "white" {}
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.01
        _TimeOffset ("Time Offset", Vector) = (0, 0, 0, 0)
        // Added for UI compatibility
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "PreviewType"="Plane"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            // Added for UI compatibility
            sampler2D _MainTex;


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

            float4 _Color1;
            float4 _Color2;
            float4 _RimColor;
            float _AnimSpeed;
            float _ColorAnimSpeed;
            float _Opacity;
            float _RimLight;
            float _Blur;
            float _Vignette;
            float _NumShapes;
            float _Curvature;
            sampler2D _ShapeData;
            float _NoiseIntensity;
            float4 _TimeOffset;

            // Hash function for pseudo-random values
            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            // Noise function
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float2 a = hash22(i);
                float2 b = hash22(i + float2(1.0, 0.0));
                float2 c = hash22(i + float2(0.0, 1.0));
                float2 d = hash22(i + float2(1.0, 1.0));

                float x1 = lerp(dot(a, f - float2(0.0, 0.0)),
                               dot(b, f - float2(1.0, 0.0)), u.x);
                float x2 = lerp(dot(c, f - float2(0.0, 1.0)),
                               dot(d, f - float2(1.0, 1.0)), u.x);

                return lerp(x1, x2, u.y) * 0.5 + 0.5;
            }

            // Bezier curve
            float2 bezier(float2 p1, float2 ctrl, float2 p2, float t)
            {
                float oneMinusT = 1.0 - t;
                return oneMinusT * oneMinusT * p1 + 2.0 * oneMinusT * t * ctrl + t * t * p2;
            }

            // SDF for a line segment
            float lineSDF(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                float2 d = pa - ba * h;
                return length(d);
            }

            // SDF for a bezier curve (approximation)
            float bezierSDF(float2 p, float2 p1, float2 ctrl, float2 p2, int segments)
            {
                float minDist = 1000.0;
                float2 lastPoint = p1;

                for (int i = 1; i <= segments; i++)
                {
                    float t = float(i) / float(segments);
                    float2 currentPoint = bezier(p1, ctrl, p2, t);
                    float dist = lineSDF(p, lastPoint, currentPoint);
                    minDist = min(minDist, dist);
                    lastPoint = currentPoint;
                }

                return minDist;
            }

            // Point in polygon test using winding number
            bool isPointInPolygon(float2 p, float2 p1, float2 ctrl, float2 p2,
                                  float2 corner1, float2 corner2, float2 corner3)
            {
                bool inside = false;

                // Test line segments
                float2 points[7];
                // Approximate bezier with 4 points
                points[0] = p1;
                points[1] = bezier(p1, ctrl, p2, 0.33);
                points[2] = bezier(p1, ctrl, p2, 0.66);
                points[3] = p2;
                points[4] = corner1;
                points[5] = corner2;
                points[6] = p1;

                for (int i = 0; i < 7; i++)
                {
                    int j = (i + 1) % 7;
                    if (((points[i].y > p.y) != (points[j].y > p.y)) &&
                        (p.x < (points[j].x - points[i].x) * (p.y - points[i].y) /
                         (points[j].y - points[i].y) + points[i].x))
                    {
                        inside = !inside;
                        //if (!inside) return inside;
                    }
                }

                return inside;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _AnimSpeed + _TimeOffset.x;

                // Background gradient with animated angle
                float gradAngle = time * 0.01;
                float2 gradCenter = float2(0.5, 0.5) + float2(cos(gradAngle), sin(gradAngle)) * 0.2;
                float2 gradVec = i.uv - gradCenter;
                float gradT = length(gradVec) * 2.0;
                float4 bgColor = lerp(_Color1, _Color2, smoothstep(0.0, 1.0, gradT));

                // Add subtle noise
                float noiseVal = noise(i.uv * 10.0 + time * 0.1) * _NoiseIntensity;
                bgColor.rgb += noiseVal;

                // Shape rendering
                for (int shapeIdx = 0; shapeIdx < int(_NumShapes); shapeIdx++)
                {
                    // Each shape has a random seed based on its index
                    float seed = float(shapeIdx) * 0.1;

                    // Determine two random sides
                    float side1Rand = frac(sin(seed * 78.233) * 43758.5453);
                    float side2Rand = frac(sin((seed + 0.1) * 78.233) * 43758.5453);

                    int side1 = int(side1Rand * 4.0);
                    int side2 = int(side2Rand * 4.0);
                    // Ensure different sides
                    if (side2 == side1) side2 = (side1 + 1) % 4;

                    // Generate points on the sides
                    float2 p1, p2;
                    float2 corners[4];
                    corners[0] = float2(-0.1, -0.1); // top-left
                    corners[1] = float2(1.1, -0.1);  // top-right
                    corners[2] = float2(1.1, 1.1);   // bottom-right
                    corners[3] = float2(-0.1, 1.1);  // bottom-left

                    // For each side, pick a random point along that side
                    float p1Pos = frac(sin(seed * 25.123) * 43758.5453);
                    float p2Pos = frac(sin((seed + 0.2) * 25.123) * 43758.5453);

                    // Top
                    if (side1 == 0) p1 = lerp(corners[0], corners[1], p1Pos);
                    // Right
                    else if (side1 == 1) p1 = lerp(corners[1], corners[2], p1Pos);
                    // Bottom
                    else if (side1 == 2) p1 = lerp(corners[2], corners[3], p1Pos);
                    // Left
                    else p1 = lerp(corners[3], corners[0], p1Pos);

                    // Top
                    if (side2 == 0) p2 = lerp(corners[0], corners[1], p2Pos);
                    // Right
                    else if (side2 == 1) p2 = lerp(corners[1], corners[2], p2Pos);
                    // Bottom
                    else if (side2 == 2) p2 = lerp(corners[2], corners[3], p2Pos);
                    // Left
                    else p2 = lerp(corners[3], corners[0], p2Pos);

                    // Random control point
                    float2 ctrlOffset = float2(
                        frac(sin(seed * 36.7654) * 43758.5453) - 0.5,
                        frac(sin((seed + 0.3) * 36.7654) * 43758.5453) - 0.5
                    ) * _Curvature;

                    float2 midPoint = (p1 + p2) * 0.5;
                    float2 ctrlPoint = midPoint + ctrlOffset;

                    // Animation
                    float waveSpeed = frac(sin(seed * 45.678) * 43758.5453) * 0.8 + 0.2;
                    float waveAmp = frac(sin((seed + 0.4) * 45.678) * 43758.5453) * 0.04 + 0.01;
                    float wave = sin(time * waveSpeed + seed * 10.0) * waveAmp;

                    ctrlPoint += float2(
                        sin(time * waveSpeed * 0.7 + seed * 5.0) * waveAmp,
                        cos(time * waveSpeed * 0.5 + seed * 7.0) * waveAmp
                    );

                    // Calculate shape
                    float distToBezier = bezierSDF(i.uv, p1, ctrlPoint, p2, 55);

                    // --- Begin True Polygon Fill ---
                    // 1. Sample the Bezier curve into N points
                    const int N = 20;
                    float2 poly[24]; // N + up to 4 corners
                    for (int t = 0; t < N; ++t) {
                        float tt = float(t) / float(N - 1);
                        poly[t] = bezier(p1, ctrlPoint, p2, tt);
                    }
                    int polyCount = N;
                    // 2. Append corners between side2 and side1 (nearest path)
                    int idxA = side2;
                    int idxB = side1;
                    int cwCount = (idxB - idxA + 4) % 4;
                    int ccwCount = (idxA - idxB + 4) % 4;
                    if (cwCount <= ccwCount) {
                        for (int k = 1; k <= cwCount; ++k) {
                            int idx = (idxA + k) % 4;
                            poly[polyCount++] = corners[idx];
                        }
                    } else {
                        for (int k = 1; k <= ccwCount; ++k) {
                            int idx = (idxA - k + 4) % 4;
                            poly[polyCount++] = corners[idx];
                        }
                    }
                    // 3. Winding number point-in-polygon test
                    bool insideShape = false;
                    for (int m = 0, n = polyCount - 1; m < polyCount; n = m++) {
                        if (((poly[m].y > i.uv.y) != (poly[n].y > i.uv.y)) &&
                            (i.uv.x < (poly[n].x - poly[m].x) * (i.uv.y - poly[m].y) / (poly[n].y - poly[m].y) + poly[m].x))
                            insideShape = !insideShape;
                    }
                    // 4. SDF for rim highlight
                    float minDist = bezierSDF(i.uv, p1, ctrlPoint, p2, 55);
                    float2 prev = poly[N-1];
                    for (int k = N; k < polyCount; ++k) {
                        float2 next = poly[k];
                        float dist = lineSDF(i.uv, prev, next);
                        minDist = min(minDist, dist);
                        prev = next;
                    }
                    float edgeGlow = smoothstep(0.005 + _Blur * 0.001, 0.0, minDist);
                    // --- End True Polygon Fill ---

                    if (insideShape)
                    {
                        // Gradient inside shape
                        float2 bezierMid = bezier(p1, ctrlPoint, p2, 0.5);
                        float2 shapeDist = i.uv - bezierMid;
                        float shapeGradT = length(shapeDist) * 1.5;
                        float4 shapeColor = lerp(_Color1, _Color2, smoothstep(0.0, 1.0, shapeGradT));

                        // Blend shape with background
                        bgColor = lerp(bgColor, shapeColor, _Opacity);
                    }

                    // Add rim highlight
                    bgColor.rgb += edgeGlow * _RimColor.rgb * _RimLight;
                }

                // Apply vignette
                float vignetteAmount = _Vignette;
                float2 vignetteUV = i.uv - 0.5;
                float vignette = 1.0 - dot(vignetteUV, vignetteUV) * vignetteAmount;
                bgColor.rgb *= vignette;

                return bgColor;
            }
            ENDCG
        }
    }
}