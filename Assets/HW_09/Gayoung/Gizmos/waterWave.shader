Shader "Custom/waterWave"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.3, 0.5, 0.8)
        _DeepColor ("Deep Color", Color) = (0.0, 0.15, 0.35, 1.0)
        _WaveHeight ("Wave Height", Float) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFreq ("Wave Frequency", Float) = 1.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.95
        _NormalStrength ("Normal Strength", Range(0,3)) = 1.0
        _FresnelPower ("Fresnel Power", Range(0,5)) = 2.0
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecularPower ("Specular Power", Range(1,256)) = 64
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf StandardSpecular vertex:vert alpha fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _DeepColor;
        float _WaveHeight;
        float _WaveSpeed;
        float _WaveFreq;
        half _Glossiness;
        float _NormalStrength;
        float _FresnelPower;
        fixed4 _SpecularColor;
        float _SpecularPower;

        struct Input {
            float3 worldPos;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
        };

        float getWave(float x, float z) {
            float wave1 = sin(x * _WaveFreq + _Time.y * _WaveSpeed) * _WaveHeight;
            float wave2 = sin(z * _WaveFreq * 0.8 + _Time.y * _WaveSpeed * 1.2) * _WaveHeight * 0.6;
            float wave3 = sin((x + z) * _WaveFreq * 0.5 + _Time.y * _WaveSpeed * 0.8) * _WaveHeight * 0.4;
            float wave4 = sin(x * _WaveFreq * 1.5 - _Time.y * _WaveSpeed * 0.7) * _WaveHeight * 0.3;
            return wave1 + wave2 + wave3 + wave4;
        }

        void vert(inout appdata_full v)
        {
            float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.vertex.y += getWave(wp.x, wp.z);

            float eps = 0.2;
            float dx = getWave(wp.x + eps, wp.z) - getWave(wp.x - eps, wp.z);
            float dz = getWave(wp.x, wp.z + eps) - getWave(wp.x, wp.z - eps);
            float3 normal = normalize(float3(-dx * _NormalStrength, 2.0, -dz * _NormalStrength));
            v.normal = mul((float3x3)unity_WorldToObject, normal);
        }

        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // fresnel로 깊이감 표현
            float3 worldNormal = WorldNormalVector(IN, o.Normal);
            float fresnel = pow(1.0 - saturate(dot(worldNormal, IN.viewDir)), _FresnelPower);
            
            // 얕은곳/깊은곳 색상 블렌드
            fixed4 col = lerp(_Color, _DeepColor, fresnel * 0.6);
            
            o.Albedo = col.rgb;
            o.Alpha = lerp(_Color.a, 1.0, fresnel * 0.3);
            o.Specular = _SpecularColor.rgb;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
}