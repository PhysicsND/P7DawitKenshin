// ============================================================================
// CCA_PropLite — General stylized prop shader (FREE / Starter edition)
// Cursed Cozy Village | Serwus Studio
//
// Soft cel shading for small props so they match the banners/cloth look.
// "_CelStrength" blends a smooth half-lambert gradient (0) to a crisp toon
// step (1+); the value is unclamped so you can push the band harder.
//
// Features (simple): Albedo + tint, optional Normal map, packed Mask (R = AO),
// optional Emission (lamps / windows), optional Alpha Cutout (foliage / rope).
//
// URP 17 (Unity 6), Forward+ compatible, SRP Batcher compatible.
// Self-contained (cel lighting inlined — no external includes), so this ships
// standalone in the Free Starter Pack.
// ============================================================================
Shader "Serwus Studio/Cursed Cozy Alley/Prop (Free)"
{
    Properties
    {
        [Header(Surface)]
        [Space(4)]
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1, 1, 1, 1)
        [Toggle(_NORMALMAP)] _NormalMapToggle("Use Normal Map", Float) = 0
        [Normal][NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 2)) = 1
        [NoScaleOffset]_MaskMap("Mask  R:AO  (G,B reserved)", 2D) = "white" {}
        _AOStrength("AO Strength", Range(0, 1)) = 1

        [Header(Cel Shading)]
        [Space(4)]
        _CelStrength("Cel Strength (0 soft - 1+ hard)", Float) = 0.5
        _ShadowTint("Shadow Tint", Color) = (0.55, 0.55, 0.62, 1)
        _ShadeThreshold("Shade Threshold", Range(0, 1)) = 0.5
        _ShadeSoftness("Shade Softness", Range(0.01, 0.6)) = 0.28

        [Header(Emission)]
        [Space(4)]
        [Toggle(_EMISSION)] _EmissionToggle("Enable Emission", Float) = 0
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionStrength("Emission Strength", Range(0, 8)) = 1

        [Header(Alpha Cutout)]
        [Space(4)]
        [Toggle(_ALPHATEST)] _AlphaTestToggle("Enable Alpha Cutout", Float) = 0
        _Cutoff("Cutoff", Range(0, 1)) = 0.5

        [Header(Rendering)]
        [Space(4)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap);     SAMPLER(sampler_BumpMap);
        TEXTURE2D(_MaskMap);     SAMPLER(sampler_MaskMap);
        TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4  _BaseColor;
            half4  _ShadowTint;
            half4  _EmissionColor;
            half   _BumpScale;
            half   _AOStrength;
            half   _CelStrength;
            half   _ShadeThreshold;
            half   _ShadeSoftness;
            half   _EmissionStrength;
            half   _Cutoff;
            half   _Cull;
        CBUFFER_END
        ENDHLSL

        // ===================================================================
        // ForwardLit
        // ===================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex PropVertex
            #pragma fragment PropFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _ALPHATEST

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #if UNITY_VERSION < 600000 // URP 14 (Unity 2022.3) Forward+
            #pragma multi_compile _ _FORWARD_PLUS
            #endif
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Inlined soft-cel lighting (self-contained — mirror of the premium
            // CCA_Lighting.hlsl::CCAToonLightingEx). Keeps the free pack standalone.
            half3 CCAToonLightingEx(float3 positionWS, float2 screenUV, half3 normalWS,
                                    half shadeThreshold, half shadeSoftness, half3 shadowTint, half toonStrength)
            {
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half ndl = dot(normalWS, mainLight.direction) * 0.5 + 0.5;
                half lit = ndl * mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                half toon = smoothstep(shadeThreshold - shadeSoftness, shadeThreshold + shadeSoftness, lit);
                half ramp = saturate(lerp(lit, toon, toonStrength));
                half3 lighting = lerp(shadowTint, half3(1, 1, 1), ramp) * mainLight.color;
                half3 ambient = SampleSH(normalWS);
                half3 addLights = half3(0, 0, 0);
                #if defined(_ADDITIONAL_LIGHTS) || defined(_CLUSTER_LIGHT_LOOP) || defined(_FORWARD_PLUS)
                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalizedScreenSpaceUV = screenUV;
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
                    half andl = saturate(dot(normalWS, light.direction) * 0.5 + 0.5);
                    half attenuated = light.distanceAttenuation * light.shadowAttenuation;
                    half shaped = saturate(lerp(andl, smoothstep(0.25, 0.75, andl), toonStrength));
                    addLights += light.color * attenuated * shaped;
                LIGHT_LOOP_END
                #endif
                return ambient + lighting + addLights;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float4 tangentWS  : TEXCOORD3;
                half   fogFactor  : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings PropVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   nrm = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS   = nrm.normalWS;
                output.tangentWS  = float4(nrm.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv         = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor  = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            half4 PropFragment(Varyings input, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
            #if defined(_ALPHATEST)
                clip(baseTex.a - _Cutoff);
            #endif

                half3 N = normalize(input.normalWS) * (isFront ? 1.0 : -1.0);
            #if defined(_NORMALMAP)
                half3 T  = normalize(input.tangentWS.xyz);
                half3 Bn = normalize(cross(N, T) * input.tangentWS.w);
                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                N = normalize(nTS.x * T + nTS.y * Bn + nTS.z * N);
            #endif

                half3 lighting = CCAToonLightingEx(
                    input.positionWS,
                    GetNormalizedScreenSpaceUV(input.positionCS),
                    N, _ShadeThreshold, _ShadeSoftness, _ShadowTint.rgb, _CelStrength);

                half ao = lerp(1.0, SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv).r, _AOStrength);
                half3 color = baseTex.rgb * lighting * ao;
            #if defined(_EMISSION)
                color += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb
                         * _EmissionColor.rgb * _EmissionStrength;
            #endif
                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        // ===================================================================
        // ShadowCaster
        // ===================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma shader_feature_local _ALPHATEST
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl" // LerpWhiteTo for URP 12
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 nWS   = TransformObjectToWorldNormal(input.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDir = normalize(_LightPosition - posWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, nWS, lightDir));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
            #if defined(_ALPHATEST)
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(a - _Cutoff);
            #endif
                return 0;
            }
            ENDHLSL
        }

        // ===================================================================
        // DepthOnly
        // ===================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment
            #pragma shader_feature_local _ALPHATEST
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half4 DepthFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
            #if defined(_ALPHATEST)
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(a - _Cutoff);
            #endif
                return 0;
            }
            ENDHLSL
        }

        // ===================================================================
        // DepthNormals
        // ===================================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma shader_feature_local _ALPHATEST
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthNormalsFragment(Varyings input, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
            #if defined(_ALPHATEST)
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(a - _Cutoff);
            #endif
                float3 n = normalize(input.normalWS) * (isFront ? 1.0 : -1.0);
                return half4(n, 0);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/Lit"
}
