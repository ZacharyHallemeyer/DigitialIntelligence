Shader "Custom/Line2" 
{
    Properties 
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _LineWidth("Line Width", Range(0,1)) = 0.05
        _Speed("Speed", Range(0,10)) = 1.0
        _GlowWidth("Glow Width", Range(0,1)) = .075
        _GlowIntensity("Glow Intensity", Range(0,1)) = .075
        _NumLines("Number of Lines", Range(0,10)) = 5
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            float4 _LineColor;
            float _LineWidth;
            float _Speed;
            float _GlowWidth;
            float _GlowIntensity;
            float _NumLines;

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;

                fixed4 color = fixed4(0, 0, 0, 1); // Start with background color
                float glowAmountMax = 0;

                // For three lines in both x and y direction
                for (int lineIndex = 0; lineIndex < _NumLines; ++lineIndex) {
                    float linePos = fmod(time + lineIndex * 0.33, 1); // Offset each line by 0.33

                    float diffX = abs(i.uv.x - linePos); // difference along x axis
                    float diffY = abs(i.uv.y - linePos); // difference along y axis

                    if (diffX < _LineWidth || diffY < _LineWidth) {
                        return _LineColor; // If we hit a line, no need to check for glow
                    }
                    else {
                        float glowAmountX = diffX < _GlowWidth ? 1 - ((diffX - _LineWidth) / (_GlowWidth - _LineWidth)) : 0;
                        float glowAmountY = diffY < _GlowWidth ? 1 - ((diffY - _LineWidth) / (_GlowWidth - _LineWidth)) : 0;

                        glowAmountMax = max(glowAmountX, glowAmountMax);
                        glowAmountMax = max(glowAmountY, glowAmountMax); // Use max glow found
                    }
                }

                // If we didn't hit a line, blend the color using the max glow found
                if (glowAmountMax > 0) {
                    color = lerp(fixed4(0, 0, 0, 1), _LineColor * _GlowIntensity, glowAmountMax);
                }

                return color;
            }



            ENDCG
        }
    }
}
