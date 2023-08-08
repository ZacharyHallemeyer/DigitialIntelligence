Shader "Unlit/Testing2"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _DistFactor("Dist Factor", Float) = 20.0
    }
        SubShader
        {
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

                sampler2D _MainTex;
                float _DistFactor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    //float2 uv = (i.uv * 2.0 - 1.0);  // In Unity, the UV coordinates are already normalized
                    float2 uv = i.uv * 2.0 - 1.0;  // In Unity, the UV coordinates are already normalized
                    uv.x *= _ScreenParams.x / _ScreenParams.y; // Adjust the aspect ratio

                    float dist = length(uv);

                    //float3 colors = float3(0.0, 0.0, 1.0);
                    float3 colors = float3(1.0, 0.0, 1.0);
                    dist = sin(dist * _DistFactor + _Time.y / 2) / _DistFactor;

                    dist = abs(dist);

                    //dist = .01 / dist;

                    colors *= dist;

                    return float4(colors, 1.0);
                }
                ENDCG
            }
        }
}
