Shader "GRADOMRAZ/Props/Lit Paper"
{
    Properties
    {
        [PerRendererData] _BaseMap ("Paper or sign", 2D) = "white" {}
        _Tint ("Paper tint", Color) = (0.91, 0.89, 0.81, 1)
        _Cutoff ("Cutout threshold", Range(0, 1)) = 0.12
        _Roughness ("Paper scattering", Range(0, 1)) = 0.88
        _AmbientFloor ("Minimum ambient", Range(0, 1)) = 0.26
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Tint;
                half _Cutoff;
                half _Roughness;
                half _AmbientFloor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half4 color : COLOR;
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
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 paper = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * input.color * _Tint;
                clip(paper.a - _Cutoff);
                half3 normalWS = normalize(input.normalWS);
                normalWS = IS_FRONT_VFACE(frontFace, normalWS, -normalWS);
                half3 ambient = max(SampleSH(normalWS), half3(_AmbientFloor, _AmbientFloor, _AmbientFloor));
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half forwardLight = saturate(dot(normalWS, mainLight.direction));
                half backScatter = saturate(dot(-normalWS, mainLight.direction)) * 0.21h;
                half3 direct = mainLight.color * ((forwardLight + backScatter) *
                    mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                half3 finalColor = MixFog(paper.rgb * (ambient + direct * lerp(0.65h, 0.92h, _Roughness)), input.fogFactor);
                return half4(finalColor, paper.a);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    Fallback Off
}
