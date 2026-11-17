Shader "GRADOMRAZ/Atmosphere/Weathered Glass"
{
    Properties
    {
        _BaseMap ("Glass tint texture", 2D) = "white" {}
        _DirtMap ("Dust / grime mask", 2D) = "gray" {}
        _Tint ("Glass tint", Color) = (0.64, 0.76, 0.82, 1)
        _Opacity ("Base opacity", Range(0, 1)) = 0.19
        _FresnelPower ("Edge Fresnel", Range(1, 8)) = 4
        _DirtStrength ("Grime visibility", Range(0, 1)) = 0.3
        _Smoothness ("Reflection smoothness", Range(0, 1)) = 0.75
        _ReflectionStrength ("Reflection visibility", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+10" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DirtMap); SAMPLER(sampler_DirtMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Tint;
                half _Opacity;
                half _FresnelPower;
                half _DirtStrength;
                half _Smoothness;
                half _ReflectionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS);
                half3 viewWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewWS)), _FresnelPower);
                half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _Tint.rgb;
                half grime = SAMPLE_TEXTURE2D(_DirtMap, sampler_DirtMap, input.uv).r * _DirtStrength;
                half3 ambient = max(SampleSH(normalWS), half3(0.10h, 0.11h, 0.12h));
                half3 reflection = GlossyEnvironmentReflection(reflect(-viewWS, normalWS), 1.0h - _Smoothness, 1.0h);
                half3 color = baseColor * (ambient + 0.15h);
                color = lerp(color, reflection * baseColor, saturate(fresnel * _ReflectionStrength));
                color *= 1.0h - grime * 0.55h;
                color = MixFog(color, input.fogFactor);
                half alpha = saturate(_Opacity + fresnel * 0.48h + grime * 0.34h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
