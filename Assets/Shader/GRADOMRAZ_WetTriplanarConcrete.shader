Shader "GRADOMRAZ/Environment/Wet Triplanar Concrete"
{
    Properties
    {
        _BaseMap ("Seamless concrete / stone", 2D) = "gray" {}
        _Tint ("Concrete color", Color) = (0.54, 0.59, 0.6, 1)
        _WorldTiling ("World-space tiling", Range(0.02, 4)) = 0.5
        _BlendSharpness ("Triplanar edge", Range(1, 8)) = 4
        _Dampness ("Dampness", Range(0, 1)) = 0.35
        _Smoothness ("Dry smoothness", Range(0, 1)) = 0.12
        _WetSpecular ("Wet highlights", Range(0, 1)) = 0.65
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _WorldTiling;
                half _BlendSharpness;
                half _Dampness;
                half _Smoothness;
                half _WetSpecular;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
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
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS);
                half3 w = pow(abs(normalWS), _BlendSharpness);
                w /= max(w.x + w.y + w.z, 0.0001h);
                float3 coord = input.positionWS * _WorldTiling;
                half3 x = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, coord.zy).rgb;
                half3 y = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, coord.xz).rgb;
                half3 z = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, coord.xy).rgb;
                half3 albedo = (x * w.x + y * w.y + z * w.z) * _Tint.rgb;
                half damp = saturate(_Dampness);
                albedo *= lerp(1.0h, 0.58h, damp);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 ambient = max(SampleSH(normalWS), half3(0.09h, 0.1h, 0.11h));
                half nDotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * (nDotL * mainLight.shadowAttenuation *
                    mainLight.distanceAttenuation);
                half3 viewWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfVector = SafeNormalize(mainLight.direction + viewWS);
                half glossy = lerp(_Smoothness, 0.9h, damp);
                half highlight = pow(saturate(dot(normalWS, halfVector)),
                    lerp(12.0h, 110.0h, glossy));
                half3 lit = albedo * (ambient + diffuse) +
                    mainLight.color * highlight * saturate(damp * _WetSpecular) *
                    mainLight.shadowAttenuation * nDotL;
                return half4(MixFog(lit, input.fogFactor), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    Fallback Off
}
