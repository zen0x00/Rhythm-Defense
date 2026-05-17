Shader "RhythmDefense/ShieldDome"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Shield Color", Color) = (0.3, 0.7, 1.0, 0.8)
        _OuterRadius ("Outer Radius", Range(0.1, 0.5)) = 0.48
        _Thickness ("Shell Thickness", Range(0.01, 0.4)) = 0.08
        _HexScale ("Hex Grid Scale", Float) = 6.0
        _HexLineWidth ("Hex Line Width", Range(0.01, 0.4)) = 0.08
        _RimWidth ("Rim Glow Width", Range(0.001, 0.1)) = 0.02
        _RimIntensity ("Rim Glow Intensity", Range(0, 4)) = 2.0
        _ScanSpeed ("Scan Speed", Float) = 1.2
        _ScanDensity ("Scan Density", Float) = 12.0
        _ScanBrightness ("Scan Brightness", Range(0, 1)) = 0.2
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _PulseIntensity ("Pulse Intensity", Range(0, 0.5)) = 0.12
        _ImpactTime ("Impact Time", Float) = -999.0
        _ImpactIntensity ("Impact Ring Intensity", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float  _OuterRadius;
            float  _Thickness;
            float  _HexScale;
            float  _HexLineWidth;
            float  _RimWidth;
            float  _RimIntensity;
            float  _ScanSpeed;
            float  _ScanDensity;
            float  _ScanBrightness;
            float  _PulseSpeed;
            float  _PulseIntensity;
            float  _ImpactTime;
            float  _ImpactIntensity;

            // Hex grid in polar space (angle, radius)
            float hexGrid(float2 uv, float scale, float lineWidth)
            {
                uv *= scale;
                float2 r = float2(1.0, sqrt(3.0));
                float2 h = 0.5 * r;
                float2 a = fmod(uv, r) - h;
                float2 b = fmod(uv - h, r) - h;
                float2 gv = (dot(a, a) < dot(b, b)) ? a : b;
                float hex = max(abs(gv.x) * sqrt(3.0) + gv.y, gv.y * 2.0) / sqrt(3.0);
                return smoothstep(0.5 - lineWidth, 0.5 - lineWidth * 0.5, hex);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Dome center at bottom-center of quad
                float2 center = float2(0.5, 0.0);
                float2 delta  = i.uv - center;

                float r     = length(delta);
                float angle = atan2(delta.x, delta.y); // -PI..PI, 0 = straight up

                // Dome = semicircle above center: y >= 0 and angle in (-PI/2, PI/2)
                // atan2(x,y): left = -PI/2, right = PI/2, top = 0
                float halfPi = 3.14159265 * 0.5;
                clip(halfPi - abs(angle) - 0.001); // discard outside 180° arc
                clip(delta.y - 0.001);             // discard below center

                float innerRadius = _OuterRadius - _Thickness;

                // Discard outside shell
                clip(_OuterRadius - r);
                clip(r - innerRadius);

                float t = _Time.y;

                // Normalized position within shell thickness (0=inner, 1=outer)
                float shellT = (r - innerRadius) / _Thickness;

                // Polar UV for hex grid: (angle mapped 0..1, radius mapped 0..1)
                float2 polarUV = float2(
                    (angle / (3.14159265)) * 0.5 + 0.5,  // angle → 0..1
                    r * 4.0                               // radius (repeating)
                );
                float hex = hexGrid(polarUV, _HexScale, _HexLineWidth);
                float hexLines = (1.0 - hex) * 0.6;

                // Rim glow: inner and outer edges
                float innerRim = smoothstep(_RimWidth, 0.0, shellT) * _RimIntensity;
                float outerRim = smoothstep(1.0 - _RimWidth, 1.0, shellT) * _RimIntensity;
                float rim = innerRim + outerRim;

                // Angular scanlines sweeping around arc
                float scan = sin(angle * _ScanDensity - t * _ScanSpeed) * 0.5 + 0.5;
                scan = pow(scan, 4.0) * _ScanBrightness;

                // Pulse
                float pulse = sin(t * _PulseSpeed) * _PulseIntensity + 1.0;

                // Impact ring: expands radially from center
                float impactAge = t - _ImpactTime;
                float impactRing = 0.0;
                if (impactAge >= 0.0 && impactAge < 1.2)
                {
                    float targetR = innerRadius + impactAge * _Thickness * 2.5;
                    float fade = 1.0 - smoothstep(0.0, 1.2, impactAge);
                    impactRing = smoothstep(0.015, 0.0, abs(r - targetR)) * fade * _ImpactIntensity;
                }

                float4 col = _Color * i.color * tex2D(_MainTex, i.uv);

                float brightness = (hexLines + rim + scan + impactRing) * pulse;
                float3 rgb = col.rgb * brightness + col.rgb * outerRim;

                float alpha = col.a * (0.25 + hexLines * 0.45 + rim * 0.55 + impactRing * 0.7);
                alpha = saturate(alpha);

                return float4(rgb, alpha);
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
