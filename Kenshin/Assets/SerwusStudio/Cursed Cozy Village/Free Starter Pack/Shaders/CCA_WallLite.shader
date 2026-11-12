// ============================================================================
// CCA_WallLite — Two-Layer Brick / Plaster Wall (FREE / Starter edition)
// Cursed Cozy Village | Serwus Studio
//
// Bottom layer : BRICK   (Albedo + Normal + Mask[R:AO G:Rough B:Height])
// Top layer    : PLASTER (Albedo + Normal + Mask[R:AO G:Rough B:Height])
// Plaster crumbles away (slider + painted mask + procedural noise) to reveal
// the brick underneath.
//
// Soft cel shading (matches the props/cloth look). URP 17 (Unity 6), Forward+
// compatible, SRP Batcher compatible. Self-contained (no external includes).
//
// >>> This is the trimmed FREE version. The PREMIUM "Wall (Brick + Plaster)"
//     shader adds: Dirt, Moss/Damp, Edge Wear, Parallax-Occlusion depth and
//     Triplanar mapping. Property names match, so a material made with this
//     shader upgrades cleanly by switching to the premium shader. <<<
// ============================================================================
Shader "Serwus Studio/Cursed Cozy Alley/Wall (Free)"
{
    Properties
    {
        [Header(BRICK   bottom layer)]
        [Space(4)]
        _BrickMap("Brick Albedo", 2D) = "white" {}
        [Normal][NoScaleOffset]_BrickNormal("Brick Normal", 2D) = "bump" {}
        [NoScaleOffset]_BrickMask("Brick Mask  R:AO G:Rough B:Height", 2D) = "white" {}
        _BrickColor("Brick Tint", Color) = (1, 1, 1, 1)
        _BrickNormalScale("Brick Normal Strength", Range(0, 2)) = 1
        _BrickAOStrength("Brick AO Strength", Range(0, 1)) = 1

        [Header(PLASTER   top layer)]
        [Space(4)]
        _PlasterMap("Plaster Albedo", 2D) = "white" {}
        [Normal][NoScaleOffset]_PlasterNormal("Plaster Normal", 2D) = "bump" {}
        [NoScaleOffset]_PlasterMask("Plaster Mask  R:AO G:Rough B:Height", 2D) = "white" {}
        _PlasterColor("Plaster Tint", Color) = (1, 1, 1, 1)
        _PlasterNormalScale("Plaster Normal Strength", Range(0, 2)) = 1
        _PlasterAOStrength("Plaster AO Strength", Range(0, 1)) = 1

        [Header(PLASTER DAMAGE   reveal brick)]
        [Space(4)]
        _DamageAmount("Damage Amount (global)", Range(0, 1)) = 0.25
        [NoScaleOffset]_DamageMap("Damage Mask (R: white=plaster  black=brick)", 2D) = "white" {}
        _DamageMaskWeight("Painted Mask Influence", Range(0, 2)) = 1
        _DamageEdge("Crumble Edge Softness", Range(0.001, 2)) = 0.12
        [Toggle(_DAMAGE_NOISE)] _DamageNoise("Procedural Crumble Noise", Float) = 1
        _DamageNoiseScale("Crumble Noise Scale", Range(0.01, 200)) = 18
        _DamageNoiseStrength("Crumble Noise Strength", Float) = 0.5
        _BrickRecess("Exposed Brick Recess", Range(0, 2)) = 0.5
        _EdgeRimDarken("Crumble Rim Darkening", Range(0, 2)) = 0.35

        [Header(Cel Shading)]
        [Space(4)]
        _CelStrength("Cel Strength (0 soft - 1 hard)", Range(0, 1)) = 0.35
        _ShadowTint("Shadow Tint", Color) = (0.55, 0.55, 0.62, 1)
        _ShadeThreshold("Shade Threshold", Range(0, 1)) = 0.5
        _ShadeSoftness("Shade Softness", Range(0.01, 0.6)) = 0.28

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

        // ===================================================================
        // Shared declarations
        // ===================================================================
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BrickMap);      SAMPLER(sampler_BrickMap);
        TEXTURE2D(_BrickNormal);   SAMPLER(sampler_BrickNormal);
        TEXTURE2D(_BrickMask);     SAMPLER(sampler_BrickMask);
        TEXTURE2D(_PlasterMap);    SAMPLER(sampler_PlasterMap);
        TEXTURE2D(_PlasterNormal); SAMPLER(sampler_PlasterNormal);
        TEXTURE2D(_PlasterMask);   SAMPLER(sampler_PlasterMask);
        TEXTURE2D(_DamageMap);     SAMPLER(sampler_DamageMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BrickMap_ST;
            float4 _PlasterMap_ST;
            half4  _BrickColor;
            half4  _PlasterColor;
            half4  _ShadowTint;
            half   _BrickNormalScale;
            half   _BrickAOStrength;
            half   _PlasterNormalScale;
            half   _PlasterAOStrength;
            half   _CelStrength;
            half   _ShadeThreshold;
            half   _ShadeSoftness;
            half   _DamageAmount;
            half   _DamageMaskWeight;
            half   _DamageEdge;
            half   _DamageNoiseScale;
            half   _DamageNoiseStrength;
            half   _BrickRecess;
            half   _EdgeRimDarken;
            half   _Cull;
        CBUFFER_END

        // --- inline value-noise FBM (self-contained, no external include) -----
        float Hash21(float2 p)
        {
            p = frac(p * float2(123.34, 345.45));
            p += dot(p, p + 34.345);
            return frac(p.x * p.y);
        }
        float ValueNoise(float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
            float a = Hash21(i);
            float b = Hash21(i + float2(1, 0));
            float c = Hash21(i + float2(0, 1));
            float d = Hash21(i + float2(1, 1));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }
        float Fbm(float2 uv)
        {
            return ValueNoise(uv) * 0.6 + ValueNoise(uv * 2.7 + 13.7) * 0.4;
        }
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
            #pragma vertex WallVertex
            #pragma fragment WallFragment

            #pragma shader_feature_local _DAMAGE_NOISE

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
                float3 viewDirWS  : TEXCOORD4;
                half   fogFactor  : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings WallVertex(Attributes input)
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
                output.viewDirWS  = GetWorldSpaceViewDir(pos.positionWS);
                output.uv         = input.uv;
                output.fogFactor  = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            void UnpackMask(half4 m, out half ao, out half rough, out half height)
            {
                ao = m.r; rough = m.g; height = m.b;
            }

            half3 SampleNormalWS(TEXTURE2D_PARAM(tex, samp), float2 uv, half scale,
                                 half3 T, half3 B, half3 N)
            {
                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(tex, samp, uv), scale);
                return normalize(nTS.x * T + nTS.y * B + nTS.z * N);
            }

            half4 WallFragment(Varyings input, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 posWS  = input.positionWS;
                half3  N      = normalize(input.normalWS) * (isFront ? 1.0 : -1.0);
                half3  T      = normalize(input.tangentWS.xyz);
                half3  Bn     = normalize(cross(N, T) * input.tangentWS.w);
                half3  viewWS = normalize(input.viewDirWS);

                float2 uvBrick   = TRANSFORM_TEX(input.uv, _BrickMap);
                float2 uvPlaster = TRANSFORM_TEX(input.uv, _PlasterMap);

                half4 brickAlb   = SAMPLE_TEXTURE2D(_BrickMap,   sampler_BrickMap,   uvBrick)   * _BrickColor;
                half4 plasterAlb = SAMPLE_TEXTURE2D(_PlasterMap, sampler_PlasterMap, uvPlaster) * _PlasterColor;
                half4 brickM     = SAMPLE_TEXTURE2D(_BrickMask,  sampler_BrickMask,  uvBrick);
                half4 plasterM   = SAMPLE_TEXTURE2D(_PlasterMask,sampler_PlasterMask,uvPlaster);

                half bAO, bRough, bH, pAO, pRough, pH;
                UnpackMask(brickM,   bAO, bRough, bH);
                UnpackMask(plasterM, pAO, pRough, pH);

                half3 brickN   = SampleNormalWS(TEXTURE2D_ARGS(_BrickNormal,   sampler_BrickNormal),   uvBrick,   _BrickNormalScale,   T, Bn, N);
                half3 plasterN = SampleNormalWS(TEXTURE2D_ARGS(_PlasterNormal, sampler_PlasterNormal), uvPlaster, _PlasterNormalScale, T, Bn, N);

                // ---- plaster coverage (reveal brick) ------------------------
                // _DamageMap: WHITE (R=1) = intact plaster, BLACK (R=0) = brick.
                half painted = SAMPLE_TEXTURE2D(_DamageMap, sampler_DamageMap, input.uv).r;
                half erosion = saturate(_DamageAmount + (1.0 - painted) * _DamageMaskWeight);
            #if defined(_DAMAGE_NOISE)
                half crumble = Fbm(input.uv * _DamageNoiseScale);
                half noiseWindow = erosion * (1.0 - erosion) * 4.0;
                erosion = saturate(erosion + (crumble - 0.5) * _DamageNoiseStrength * noiseWindow);
            #endif
                half cut = lerp(-_DamageEdge, 1.0 + _DamageEdge, erosion);
                half coverage = smoothstep(cut - _DamageEdge, cut + _DamageEdge, pH);

                half rim = saturate(1.0 - abs(coverage - 0.5) * 2.0);
                half brickExposeAO = lerp(1.0, bAO, _BrickRecess);

                // ---- blend the two layers -----------------------------------
                half3 albedo   = lerp(brickAlb.rgb * brickExposeAO, plasterAlb.rgb, coverage);
                half3 normalWS = normalize(lerp(brickN, plasterN, coverage));
                half  ao       = lerp(bAO, pAO, coverage);
                ao = lerp(1.0, ao, lerp(_BrickAOStrength, _PlasterAOStrength, coverage));

                albedo *= lerp(1.0, 1.0 - _EdgeRimDarken, rim);
                albedo = max(albedo, 0.0); // guard against extreme rim/recess values

                // ===================== Cel lighting ==========================
                half3 lighting = CCAToonLightingEx(
                    posWS, GetNormalizedScreenSpaceUV(input.positionCS), normalWS,
                    _ShadeThreshold, _ShadeSoftness, _ShadowTint.rgb, _CelStrength);

                half3 color = albedo * lighting * ao; // AO baked into the result
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
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
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target { return 0; }
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
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };

            Varyings DepthVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            half4 DepthFragment(Varyings input) : SV_Target { return 0; }
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
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 DepthNormalsFragment(Varyings input, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 n = normalize(input.normalWS) * (isFront ? 1.0 : -1.0);
                return half4(n, 0);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/Lit"
}
