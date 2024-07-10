// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

Shader "Toon"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_BaseColor ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		[HideInInspector] __BeginGroup_ShadowHSV ("Shadow HSV", Float) = 0
		_Shadow_HSV_H ("Hue", Range(-180,180)) = 0
		_Shadow_HSV_S ("Saturation", Range(-1,1)) = 0
		_Shadow_HSV_V ("Value", Range(-1,1)) = 0
		[HideInInspector] __EndGroup ("Shadow HSV", Float) = 0
		[MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2Separator]
		
		[TCP2HeaderHelp(Specular)]
		[Toggle(TCP2_SPECULAR)] _UseSpecular ("Enable Specular", Float) = 0
		[TCP2ColorNoAlpha] _SpecularColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_SpecularToonSize ("Toon Size", Range(0,1)) = 0.25
		_SpecularToonSmoothness ("Toon Smoothness", Range(0.001,0.5)) = 0.05
		[TCP2Separator]

		[TCP2HeaderHelp(Emission)]
		[TCP2ColorNoAlpha] [HDR] _Emission ("Emission Color", Color) = (0,0,0,1)
		[TCP2Separator]
		
		[TCP2HeaderHelp(Rim Lighting)]
		[Toggle(TCP2_RIM_LIGHTING)] _UseRim ("Enable Rim Lighting", Float) = 0
		[TCP2ColorNoAlpha] _RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.5)
		 [NoScaleOffset] _RimColor1 ("Rim Color Texture", 2D) = "white" {}
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1
		//Rim Direction
		_RimDir ("Rim Direction", Vector) = (0,0,1,1)
		[TCP2Separator]
		
		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff ("Receive Shadows", Float) = 1

		// Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType"="Opaque"
		}

		HLSLINCLUDE
		#define fixed half
		#define fixed2 half2
		#define fixed3 half3
		#define fixed4 half4

		#if UNITY_VERSION >= 202020
			#define URP_10_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202120
			#define URP_12_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202220
			#define URP_14_OR_NEWER
		#endif

		// Texture/Sampler abstraction
		#define TCP2_TEX2D_WITH_SAMPLER(tex)						TEXTURE2D(tex); SAMPLER(sampler##tex)
		#define TCP2_TEX2D_NO_SAMPLER(tex)							TEXTURE2D(tex)
		#define TCP2_TEX2D_SAMPLE(tex, samplertex, coord)			SAMPLE_TEXTURE2D(tex, sampler##samplertex, coord)
		#define TCP2_TEX2D_SAMPLE_LOD(tex, samplertex, coord, lod)	SAMPLE_TEXTURE2D_LOD(tex, sampler##samplertex, coord, lod)

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		// Uniforms

		// Shader Properties
		TCP2_TEX2D_WITH_SAMPLER(_BaseMap);
		TCP2_TEX2D_WITH_SAMPLER(_RimColor1);

		CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			float4 _BaseMap_ST;
			fixed4 _BaseColor;
			half4 _Emission;
			float _RampThreshold;
			float _RampSmoothing;
			float _Shadow_HSV_H;
			float _Shadow_HSV_S;
			float _Shadow_HSV_V;
			fixed4 _SColor;
			fixed4 _HColor;
			float4 _RimDir;
			float _RimMin;
			float _RimMax;
			fixed4 _RimColor;
			float _SpecularToonSize;
			float _SpecularToonSmoothness;
			fixed4 _SpecularColor;
		CBUFFER_END

		#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_DOTS_INSTANCING_ENABLED)
			#define unity_ObjectToWorld UNITY_MATRIX_M
			#define unity_WorldToObject UNITY_MATRIX_I_M
		#endif

		//--------------------------------
		// HSV HELPERS
		// source: http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
		
		float3 rgb2hsv(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
			float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
		
			float d = q.x - min(q.w, q.y);
			float e = 1.0e-10;
			return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}
		
		float3 hsv2rgb(float3 c)
		{
			c.g = max(c.g, 0.0); //make sure that saturation value is positive
			float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
			float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
			return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
		}
		
		float3 ApplyHSV_3(float3 color, float h, float s, float v)
		{
			float3 hsv = rgb2hsv(color.rgb);
			hsv += float3(h/360,s,v);
			return hsv2rgb(hsv);
		}
		float3 ApplyHSV_3(float color, float h, float s, float v) { return ApplyHSV_3(color.xxx, h, s ,v); }
		
		float4 ApplyHSV_4(float4 color, float h, float s, float v)
		{
			float3 hsv = rgb2hsv(color.rgb);
			hsv += float3(h/360,s,v);
			return float4(hsv2rgb(hsv), color.a);
		}
		float4 ApplyHSV_4(float color, float h, float s, float v) { return ApplyHSV_4(color.xxxx, h, s, v); }
		
		//Specular help functions (from UnityStandardBRDF.cginc)
		inline float3 SpecSafeNormalize(float3 inVec)
		{
			half dp3 = max(0.001f, dot(inVec, inVec));
			return inVec * rsqrt(dp3);
		}
		
		// Built-in renderer (CG) to SRP (HLSL) bindings
		#define UnityObjectToClipPos TransformObjectToHClip
		#define _WorldSpaceLightPos0 _MainLightPosition
		
		ENDHLSL

		Pass
		{
			Name "Main"
			Tags
			{
				"LightMode"="UniversalForward"
			}

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 3.0

			// -------------------------------------
			// Material keywords
			#pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			// -------------------------------------

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex Vertex
			#pragma fragment Fragment

			//--------------------------------------
			// Toony Colors Pro 2 keywords
			#pragma shader_feature_local_fragment TCP2_SPECULAR
			#pragma shader_feature_local_fragment TCP2_RIM_LIGHTING

			// vertex input
			struct Attributes
			{
				float4 vertex       : POSITION;
				float3 normal       : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// vertex output / fragment input
			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 worldPosAndFog : TEXCOORD0;
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord    : TEXCOORD1; // compute shadow coord per-vertex for the main light
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				half3 vertexLights : TEXCOORD2;
			#endif
				float4 screenPosition : TEXCOORD3;
				float2 pack1 : TEXCOORD4; /* pack1.xy = texcoord0 */
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Texture Coordinates
				output.pack1.xy.xy = input.texcoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

				float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif
				float4 clipPos = vertexInput.positionCS;

				float4 screenPos = ComputeScreenPos(clipPos);
				output.screenPosition.xyzw = screenPos;

				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normal);
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				// Vertex lighting
				output.vertexLights = VertexLighting(vertexInput.positionWS, vertexNormalInput.normalWS);
			#endif

				// world position
				output.worldPosAndFog = float4(vertexInput.positionWS.xyz, 0);

				// normal
				output.normal = normalize(vertexNormalInput.normalWS);

				// clip position
				output.positionCS = vertexInput.positionCS;

				return output;
			}

			half4 Fragment(Varyings input
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = normalize(input.normal);
				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);

				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack1.xy).rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __ambientIntensity = ( 1.0 );
				float3 __emission = ( _Emission.rgb );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float __shadowHue = ( _Shadow_HSV_H );
				float __shadowSaturation = ( _Shadow_HSV_S );
				float __shadowValue = ( _Shadow_HSV_V );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float3 __rimDir = ( _RimDir.xyz );
				float __rimMin = ( _RimMin );
				float __rimMax = ( _RimMax );
				float3 __rimColor = ( _RimColor.rgb * TCP2_TEX2D_SAMPLE(_RimColor1, _RimColor1, input.pack1.xy).rgb );
				float __rimStrength = ( 1.0 );
				float __specularToonSize = ( _SpecularToonSize );
				float __specularToonSmoothness = ( _SpecularToonSmoothness );
				float3 __specularColor = ( _SpecularColor.rgb );

				// main texture
				half3 albedo = __albedo.rgb;
				half alpha = __alpha;

				half3 emission = half3(0,0,0);
				
				albedo *= __mainColor.rgb;

				// main light: direction, color, distanceAttenuation, shadowAttenuation
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord = input.shadowCoord;
			#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
			#else
				float4 shadowCoord = float4(0, 0, 0, 0);
			#endif

			#if defined(URP_10_OR_NEWER)
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
					half4 shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
				#elif !defined (LIGHTMAP_ON)
					half4 shadowMask = unity_ProbesOcclusion;
				#else
					half4 shadowMask = half4(1, 1, 1, 1);
				#endif

				Light mainLight = GetMainLight(shadowCoord, positionWS, shadowMask);
			#else
				Light mainLight = GetMainLight(shadowCoord);
			#endif

				// ambient or lightmap
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
				half occlusion = 1;

				half3 indirectDiffuse = bakedGI;
				indirectDiffuse *= occlusion * albedo * __ambientIntensity;
				emission += __emission;

				half3 lightDir = mainLight.direction;
				half3 lightColor = mainLight.color.rgb;

				half atten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

				half ndl = dot(normalWS, lightDir);
				half3 ramp;
				
				half rampThreshold = __rampThreshold;
				half rampSmooth = __rampSmoothing * 0.5;
				ndl = saturate(ndl);
				ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

				// apply attenuation
				ramp *= atten;

				//Shadow HSV
				float3 albedoShadowHSV = ApplyHSV_3(albedo, __shadowHue, __shadowSaturation, __shadowValue);
				albedo = lerp(albedoShadowHSV, albedo, ramp);

				// highlight/shadow colors
				ramp = lerp(__shadowColor, __highlightColor, ramp);
				
				// output color
				half3 color = half3(0,0,0);
				// Rim Lighting
				#if defined(TCP2_RIM_LIGHTING)
				half3 rViewDir = viewDirWS;
				half3 rimDir = __rimDir;
				half3 screenPosOffset = (input.screenPosition.xyz / input.screenPosition.w) - 0.5;
				rimDir.xyz -= screenPosOffset.xyz;
				rViewDir = normalize(UNITY_MATRIX_V[0].xyz * rimDir.x + UNITY_MATRIX_V[1].xyz * rimDir.y + UNITY_MATRIX_V[2].xyz * rimDir.z);
				half rim = 1.0f - saturate(dot(rViewDir, normalWS));
				rim = ( rim );
				half rimMin = __rimMin;
				half rimMax = __rimMax;
				rim = smoothstep(rimMin, rimMax, rim);
				half3 rimColor = __rimColor;
				half rimStrength = __rimStrength;
				//Rim light mask
				emission.rgb += ndl * atten * rim * rimColor * rimStrength;
				#endif
				color += albedo * lightColor.rgb * ramp;

				half3 halfDir = SpecSafeNormalize(float3(lightDir) + float3(viewDirWS));
				
				#if defined(TCP2_SPECULAR)
				//Blinn-Phong Specular
				float ndh = max(0, dot (normalWS, halfDir));
				float spec = smoothstep(__specularToonSize + __specularToonSmoothness, __specularToonSize - __specularToonSmoothness,1 - (ndh / (1+__specularToonSmoothness)));
				spec *= ndl;
				spec *= atten;
				
				//Apply specular
				emission.rgb += spec * lightColor.rgb * __specularColor;
				#endif

				// Additional lights loop
			#ifdef _ADDITIONAL_LIGHTS
				uint pixelLightCount = GetAdditionalLightsCount();

				LIGHT_LOOP_BEGIN(pixelLightCount)
				{
					#if defined(URP_10_OR_NEWER)
						Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
					#else
						Light light = GetAdditionalLight(lightIndex, positionWS);
					#endif
					half atten = light.shadowAttenuation * light.distanceAttenuation;

					#if defined(_LIGHT_LAYERS)
						half3 lightDir = half3(0, 1, 0);
						half3 lightColor = half3(0, 0, 0);
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						{
							lightColor = light.color.rgb;
							lightDir = light.direction;
						}
					#else
						half3 lightColor = light.color.rgb;
						half3 lightDir = light.direction;
					#endif

					half ndl = dot(normalWS, lightDir);
					half3 ramp;
					
					ndl = saturate(ndl);
					ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

					// apply attenuation (shadowmaps & point/spot lights attenuation)
					ramp *= atten;

					// apply highlight color
					ramp = lerp(half3(0,0,0), __highlightColor, ramp);
					
					// output color
					color += albedo * lightColor.rgb * ramp;

					half3 halfDir = SpecSafeNormalize(float3(lightDir) + float3(viewDirWS));
					
					#if defined(TCP2_SPECULAR)
					//Blinn-Phong Specular
					float ndh = max(0, dot (normalWS, halfDir));
					float spec = smoothstep(__specularToonSize + __specularToonSmoothness, __specularToonSize - __specularToonSmoothness,1 - (ndh / (1+__specularToonSmoothness)));
					spec *= ndl;
					spec *= atten;
					
					//Apply specular
					emission.rgb += spec * lightColor.rgb * __specularColor;
					#endif
					#if defined(TCP2_RIM_LIGHTING)
					// Rim light mask
					half3 rimColor = __rimColor;
					half rimStrength = __rimStrength;
					emission.rgb += ndl * atten * rim * rimColor * rimStrength;
					#endif
				}
				LIGHT_LOOP_END
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				color += input.vertexLights * albedo;
			#endif

				// apply ambient
				color += indirectDiffuse;

				color += emission;

				return half4(color, alpha);
			}
			ENDHLSL
		}

		// Depth & Shadow Caster Passes
		HLSLINCLUDE

		#if defined(SHADOW_CASTER_PASS) || defined(DEPTH_ONLY_PASS)

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			float3 _LightDirection;
			float3 _LightPosition;

			struct Attributes
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float4 screenPosition : TEXCOORD1;
				float3 pack1 : TEXCOORD2; /* pack1.xyz = positionWS */
				float2 pack2 : TEXCOORD3; /* pack2.xy = texcoord0 */
			#if defined(DEPTH_ONLY_PASS)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			#endif
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normal);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif
				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			Varyings ShadowDepthPassVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				#if defined(DEPTH_ONLY_PASS)
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				#endif

				// Texture Coordinates
				output.pack2.xy.xy = input.texcoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

				float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

				// Screen Space UV
				float4 screenPos = ComputeScreenPos(vertexInput.positionCS);
				output.screenPosition.xyzw = screenPos;
				output.pack1.xyz = vertexInput.positionWS;

				#if defined(DEPTH_ONLY_PASS)
					output.positionCS = TransformObjectToHClip(input.vertex.xyz);
				#elif defined(SHADOW_CASTER_PASS)
					output.positionCS = GetShadowPositionHClip(input);
				#else
					output.positionCS = float4(0,0,0,0);
				#endif

				return output;
			}

			half4 ShadowDepthPassFragment(
				Varyings input
			) : SV_TARGET
			{
				#if defined(DEPTH_ONLY_PASS)
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				#endif

				float3 positionWS = input.pack1.xyz;

				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack2.xy).rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );

				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);
				half3 albedo = half3(1,1,1);
				half alpha = __alpha;
				half3 emission = half3(0,0,0);

				return 0;
			}

		#endif
		ENDHLSL

		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile SHADOW_CASTER_PASS

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
			}

			ZWrite On
			ColorMask 0

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.10";unity:"2022.3.30f1";tmplt:"SG2_Template_URP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","UNITY_2021_2","UNITY_2022_2","SHADOW_HSV","SHADOW_COLOR_MAIN_DIR","SPEC_LEGACY","SPECULAR","SPECULAR_TOON","SPECULAR_SHADER_FEATURE","EMISSION","RIM","RIM_DIR","RIM_DIR_PERSP_CORRECTION","RIM_LIGHTMASK","RIM_SHADER_FEATURE","TEMPLATE_LWRP"];flags:list[];flags_extra:dict[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",RIM_LABEL="Rim Lighting"];shaderProperties:list[,,,,,,,,,,,,,,sp(name:"Rim Color";imps:list[imp_mp_color(def:RGBA(0.8, 0.8, 0.8, 0.5);hdr:False;cc:3;chan:"RGB";prop:"_RimColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"fe3b3fdb-0524-4cfd-ac2e-fedae4e8b40a";op:Multiply;lbl:"Rim Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0),imp_mp_texture(uto:False;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"white";locked_uv:False;uv:0;cc:3;chan:"RGB";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_RimColor1";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"2cbd7206-f003-467f-b950-be245d916980";op:Multiply;lbl:"Rim Color Texture";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False)];customTextures:list[];codeInjection:codeInjection(injectedFiles:list[];mark:False);matLayers:list[]) */
/* TCP_HASH 7d83ebd1c43fb51b571b0c7e25f02011 */
