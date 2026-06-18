Shader "Retro Shaders Pro/Post Processing/CRT"
{
    Properties
    {
        _TintColor ("Tint", Color) = (1, 1, 1, 1)
        _BackgroundColor ("Background", Color) = (0, 0, 0, 1)
        _Brightness ("Brightness", Float) = 1
        _Contrast ("Contrast", Float) = 1
        _DistortionStrength ("Distortion Strength", Float) = 0
        _DistortionSmoothing ("Distortion Smoothing", Float) = 0.01
        _RGBTex ("RGB Texture", 2D) = "white" {}
        _RGBStrength ("RGB Strength", Float) = 0
        _ScanlineTex ("Scanline Texture", 2D) = "white" {}
        _ScanlineStrength ("Scanline Strength", Float) = 0
        _ScrollSpeed ("Scroll Speed", Float) = 0
        _RandomWear ("Random Wear", Float) = 0
        _AberrationStrength ("Aberration Strength", Float) = 0
        _TrackingTex ("Tracking Texture", 2D) = "gray" {}
        _TrackingSize ("Tracking Size", Float) = 1
        _TrackingStrength ("Tracking Strength", Float) = 0
        _TrackingSpeed ("Tracking Speed", Float) = 0
        _TrackingJitter ("Tracking Jitter", Float) = 0
        _TrackingColorDamage ("Tracking Color Damage", Float) = 0
        _TrackingLinesThreshold ("Tracking Lines Threshold", Float) = 1
        _TrackingLinesColor ("Tracking Lines Color", Color) = (1, 1, 1, 0.5)
        _InputTexture ("Interlace Texture", 2D) = "white" {}
        _Size ("Scanline Size", Int) = 8
        _Interlacing ("Interlacing", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "CRT"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_local_fragment _ _INTERLACING_ON
            #pragma multi_compile_local_fragment _ _POINT_FILTERING_ON
            #pragma multi_compile_local_fragment _ _CHROMATIC_ABERRATION_ON
            #pragma multi_compile_local_fragment _ _TRACKING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_RGBTex);
            SAMPLER(sampler_RGBTex);
            TEXTURE2D(_ScanlineTex);
            SAMPLER(sampler_ScanlineTex);
            TEXTURE2D(_TrackingTex);
            SAMPLER(sampler_TrackingTex);
            TEXTURE2D_X(_InputTexture);
            SAMPLER(sampler_InputTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float4 _BackgroundColor;
                float4 _TrackingLinesColor;
                float _Brightness;
                float _Contrast;
                float _DistortionStrength;
                float _DistortionSmoothing;
                float _RGBStrength;
                float _ScanlineStrength;
                float _ScrollSpeed;
                float _RandomWear;
                float _AberrationStrength;
                float _TrackingSize;
                float _TrackingStrength;
                float _TrackingSpeed;
                float _TrackingJitter;
                float _TrackingColorDamage;
                float _TrackingLinesThreshold;
                int _Size;
                int _Interlacing;
            CBUFFER_END

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            bool OutsideScreen(float2 uv)
            {
                return any(uv < 0.0) || any(uv > 1.0);
            }

            float2 Distort(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float radius = dot(centered, centered);
                float strength = saturate(_DistortionStrength);
                float smoothing = max(_DistortionSmoothing, 0.0001);
                centered *= 1.0 + radius * strength * (0.18 + smoothing);
                return centered * 0.5 + 0.5;
            }

            float4 SampleSource(float2 uv)
            {
            #if defined(_POINT_FILTERING_ON)
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
            #else
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
            #endif
            }

            float4 ApplyTracking(float4 color, float2 uv)
            {
            #if defined(_TRACKING_ON)
                float trackUvY = uv.y * max(_TrackingSize, 0.001) + _Time.y * _TrackingSpeed;
                float track = SAMPLE_TEXTURE2D(_TrackingTex, sampler_TrackingTex, float2(uv.x, trackUvY)).r;
                float jitter = (Hash(float2(floor(uv.y * _ScreenParams.y), floor(_Time.y * 60.0))) - 0.5) * _TrackingJitter;
                float lineMask = step(_TrackingLinesThreshold, track);
                color.rgb = lerp(color.rgb, color.rgb + (track - 0.5) * _TrackingStrength * 0.04, saturate(_TrackingStrength));
                color.rgb = lerp(color.rgb, _TrackingLinesColor.rgb, lineMask * _TrackingLinesColor.a);
                color.rg += jitter * _TrackingColorDamage;
            #endif
                return color;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float2 distortedUv = Distort(uv);

                if (OutsideScreen(distortedUv))
                {
                    return _BackgroundColor;
                }

            #if defined(_CHROMATIC_ABERRATION_ON)
                float2 aberrationOffset = float2(_AberrationStrength, 0.0) * _BlitTexture_TexelSize.xy;
                float4 color = SampleSource(distortedUv);
                color.r = SampleSource(distortedUv + aberrationOffset).r;
                color.b = SampleSource(distortedUv - aberrationOffset).b;
            #else
                float4 color = SampleSource(distortedUv);
            #endif

                float scanlineRow = floor(uv.y * _ScreenParams.y / max(_Size, 1));
                float scanlinePhase = frac(scanlineRow * 0.5 + _Time.y * _ScrollSpeed);
                float scanlineSample = SAMPLE_TEXTURE2D(_ScanlineTex, sampler_ScanlineTex, float2(uv.x, scanlinePhase)).r;
                float scanline = lerp(1.0, scanlineSample, saturate(_ScanlineStrength));

                float3 rgbMask = SAMPLE_TEXTURE2D(_RGBTex, sampler_RGBTex, uv * _ScreenParams.xy / max(_Size, 1)).rgb;
                color.rgb *= lerp(1.0.xxx, rgbMask * 1.5, saturate(_RGBStrength));
                color.rgb *= scanline;

            #if defined(_INTERLACING_ON)
                float oddLine = fmod(floor(uv.y * _ScreenParams.y) + _Interlacing, 2.0);
                float4 previous = SAMPLE_TEXTURE2D_X(_InputTexture, sampler_LinearClamp, distortedUv);
                color.rgb = lerp(color.rgb, previous.rgb, oddLine * 0.45);
            #endif

                float noise = (Hash(float2(uv.x * _ScreenParams.x, uv.y * _ScreenParams.y + _Time.y * 60.0)) - 0.5) * _RandomWear * 0.02;
                color.rgb += noise;
                color = ApplyTracking(color, uv);
                color.rgb = (color.rgb - 0.5) * _Contrast + 0.5;
                color.rgb *= _Brightness;
                color.rgb *= _TintColor.rgb;
                color.a *= _TintColor.a;

                return saturate(color);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
