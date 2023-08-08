Shader "Custom/Line" 
{
    Properties 
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _LineWidth("Line Width", Range(0,1)) = 0.05
        _Speed("Speed", Range(0,10)) = 1.0
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

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                float time = _Time.y * _Speed;
                float linePos = fmod(time, 1); // loop line position
                float diff = abs(i.uv.x - linePos);
                
                if (diff < _LineWidth) {
                    return _LineColor;
                } else {
                    return fixed4(0, 0, 0, 1); // background color
                }
            }
            ENDCG
        }
    }
}
