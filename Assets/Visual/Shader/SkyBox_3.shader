// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SkyBox_3"
{
    Properties
    {
        _Color1("Top Color", Color) = (1, 1, 1, 0)
        _Color2("Horizon Color", Color) = (1, 1, 1, 0)
        _Color3("Bottom Color", Color) = (1, 1, 1, 0)

        _Color4("Night Color", Color) = (1, 1, 1, 0)

        _Intensity("Intensity Amplifier", Float) = 1.0

        _SunPosition("Sun Position", Vector) = (1,1,1)
        _SunInnerSize("Sun In Size", Float) = 1.0
        _SunOuterSize("Sun Out Size", Float) = 1.0
        _SunColor("Sun Color ", Color) = (1, 1, 1, 0)

        _StarMap("Star Map", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            half4 _Color1;
            half4 _Color2;
            half4 _Color3;
            half4 _Color4;
            half _Intensity;

            half3 _SunPosition;
            half _SunInnerSize;
            half _SunOuterSize;
            half4 _SunColor;

            Texture2D _StarMap;
            float4 _StarMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.position);
                o.texcoord = v.texcoord;
                o.uv = TRANSFORM_TEX(v.texcoord, _StarMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float p = normalize(i.texcoord).y * 2;
                float p1 = clamp( (p) / (1), 0, 1);
                float p2 = clamp( 1 - abs(p), 0, 1);
                float p3 = clamp( (p) / (-1), 0, 1);

                float dotSun = 1 - dot(normalize(i.texcoord), normalize(_SunPosition));
                float sunIntensity = smoothstep(_SunInnerSize, _SunOuterSize, dotSun);

                float4 skyCol = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3) * _Intensity;
                float4 finCol = lerp(_SunColor, skyCol, sunIntensity);


                //if (col.r > 0.9)
                //{
                //    float moy = (finCol.x + finCol.y + finCol.z + finCol.w) / 4;
                //    finCol = lerp(finCol, (1, 1, 1, 1), moy);
                //}

                return finCol;
            }
            ENDCG
        }
    }
}
