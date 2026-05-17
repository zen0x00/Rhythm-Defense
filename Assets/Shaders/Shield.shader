Shader "RhythmDefense/Shield"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Shield Color", Color) = (0.3, 0.7, 1.0, 0.8)
        _HexScale ("Hex Grid Scale", Float) = 8.0
        _HexLineWidth ("Hex Line Width", Range(0.01, 0.5)) = 0.08
        _RimWidth ("Rim Glow Width", Range(0.0, 0.5)) = 0.15
        _RimIntensity ("Rim Glow Intensity", Range(0, 3)) = 1.5
        _ScanSpeed ("Scan Speed", Float) = 1.5
        _ScanDensity ("Scan Density", Float) = 20.0
        _ScanBrightness ("Scan Brightness", Range(0, 1)) = 0.15
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
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
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
                float t = _Time.y;
                float4 texSample = tex2D(_MainTex, i.uv);
                float4 col = _Color * i.color * texSample;

                // UV centered at (0.5, 0.5)
                float2 centered = i.uv - 0.5;

                // Rim glow — brightest at UV edges
                float2 rimUV = abs(centered) * 2.0; // 0 center, 1 edge
                float rim = max(rimUV.x, rimUV.y);
                rim = smoothstep(1.0 - _RimWidth, 1.0, rim) * _RimIntensity;

                // Hex grid
                float hex = hexGrid(i.uv, _HexScale, _HexLineWidth);
                float hexLines = (1.0 - hex) * 0.6;

                // Scanlines
                float scan = sin(i.uv.y * _ScanDensity - t * _ScanSpeed) * 0.5 + 0.5;
                scan = pow(scan, 4.0) * _ScanBrightness;

                // Pulse
                float pulse = sin(t * _PulseSpeed) * _PulseIntensity + 1.0;

                // Impact ring from center
                float impactAge = t - _ImpactTime;
                float impactRing = 0.0;
                if (impactAge >= 0.0 && impactAge < 1.2)
                {
                    float dist = length(centered);
                    float radius = impactAge * 0.4;
                    float fade = 1.0 - smoothstep(0.0, 1.2, impactAge);
                    impactRing = smoothstep(0.03, 0.0, abs(dist - radius)) * fade * _ImpactIntensity;
                }

                float brightness = (hexLines + scan + rim + impactRing) * pulse;
                float3 rgb = col.rgb * brightness + col.rgb * rim;

                float alpha = col.a * (0.2 + hexLines * 0.5 + rim * 0.6 + impactRing * 0.7);
                alpha = saturate(alpha);

                return float4(rgb, alpha);
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
