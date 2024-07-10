// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

Shader "67 Bits/Toon"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_Color ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		[HideInInspector] __BeginGroup_ShadowHSV ("Shadow HSV", Float) = 0
		_Shadow_HSV_H ("Hue", Range(-180,180)) = 0
		_Shadow_HSV_S ("Saturation", Range(-1,1)) = 0
		_Shadow_HSV_V ("Value", Range(-1,1)) = 0
		[HideInInspector] __EndGroup ("Shadow HSV", Float) = 0
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2Separator]
		
		[TCP2HeaderHelp(Specular)]
		[Toggle(TCP2_SPECULAR)] _UseSpecular ("Enable Specular", Float) = 0
		[TCP2ColorNoAlpha] _SpecularColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_SpecularSmoothness ("Smoothness", Float) = 0.2
		[TCP2Separator]

		[TCP2HeaderHelp(Emission)]
		[TCP2ColorNoAlpha] [HDR] _Emission ("Emission Color", Color) = (0,0,0,1)
		[TCP2Separator]
		
		[TCP2HeaderHelp(Rim Lighting)]
		[Toggle(TCP2_RIM_LIGHTING)] _UseRim ("Enable Rim Lighting", Float) = 0
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1
		//Rim Direction
		_RimDir ("Rim Direction", Vector) = (0,0,1,0)
		[TCP2Separator]
		
		[TCP2HeaderHelp(Silhouette Pass)]
		_SilhouetteColor ("Silhouette Color", Color) = (0,0,0,0.33)
		[TCP2Separator]
		
		[TCP2HeaderHelp(Outline)]
		_OutlineWidth ("Width", Range(0.1,50)) = 1
		_OutlineColorVertex ("Color", Color) = (0,0,0,1)
		[Space]
		_OutlineZSmooth ("Z Correction", Range(-3,3)) = 0
		// Outline Normals
		[TCP2MaterialKeywordEnumNoPrefix(Regular, _, Vertex Colors, TCP2_COLORS_AS_NORMALS, Tangents, TCP2_TANGENT_AS_NORMALS, UV1, TCP2_UV1_AS_NORMALS, UV2, TCP2_UV2_AS_NORMALS, UV3, TCP2_UV3_AS_NORMALS, UV4, TCP2_UV4_AS_NORMALS)]
		_NormalsSource ("Outline Normals Source", Float) = 0
		[TCP2MaterialKeywordEnumNoPrefix(Full XYZ, TCP2_UV_NORMALS_FULL, Compressed XY, _, Compressed ZW, TCP2_UV_NORMALS_ZW)]
		_NormalsUVType ("UV Data Type", Float) = 0
		[TCP2Separator]
		// Custom Material Properties
		[MainTexture] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}

		// Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Geometry+10" // Make sure that the objects are rendered later to avoid sorting issues with the transparent silhouette
		}

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc"	// needed for LightColor

		// Texture/Sampler abstraction
		#define TCP2_TEX2D_WITH_SAMPLER(tex)						UNITY_DECLARE_TEX2D(tex)
		#define TCP2_TEX2D_NO_SAMPLER(tex)							UNITY_DECLARE_TEX2D_NOSAMPLER(tex)
		#define TCP2_TEX2D_SAMPLE(tex, samplertex, coord)			UNITY_SAMPLE_TEX2D_SAMPLER(tex, samplertex, coord)
		#define TCP2_TEX2D_SAMPLE_LOD(tex, samplertex, coord, lod)	UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex, samplertex, coord, lod)

		// Custom Material Properties
		TCP2_TEX2D_WITH_SAMPLER(_MainTex);

		// Instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// Custom Material Properties

			// Shader Properties
			float _OutlineZSmooth;
			float _OutlineWidth;
			fixed4 _OutlineColorVertex;
			fixed4 _SilhouetteColor;
			fixed4 _Color;
			half4 _Emission;
			float _RampThreshold;
			float _RampSmoothing;
			float _Shadow_HSV_H;
			float _Shadow_HSV_S;
			float _Shadow_HSV_V;
			fixed4 _HColor;
			fixed4 _SColor;
			float _SpecularSmoothness;
			fixed4 _SpecularColor;
			float4 _RimDir;
			float _RimMin;
			float _RimMax;
		UNITY_INSTANCING_BUFFER_END(Props)

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

		ENDCG

		// Outline Include
		CGINCLUDE

		struct appdata_outline
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord0 : TEXCOORD0;
			#if TCP2_UV2_AS_NORMALS
			float4 texcoord1 : TEXCOORD1;
		#elif TCP2_UV3_AS_NORMALS
			float4 texcoord2 : TEXCOORD2;
		#elif TCP2_UV4_AS_NORMALS
			float4 texcoord3 : TEXCOORD3;
		#endif
		#if TCP2_COLORS_AS_NORMALS
			float4 vertexColor : COLOR;
		#endif
		#if TCP2_TANGENT_AS_NORMALS
			float4 tangent : TANGENT;
		#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f_outline
		{
			float4 vertex : SV_POSITION;
			float4 screenPosition : TEXCOORD0;
			float4 vcolor : TEXCOORD1;
			float3 pack2 : TEXCOORD2; /* pack2.xyz = worldPos */
			float2 pack3 : TEXCOORD3; /* pack3.xy = texcoord0 */
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f_outline vertex_outline (appdata_outline v)
		{
			v2f_outline output;
			UNITY_INITIALIZE_OUTPUT(v2f_outline, output);
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			// Texture Coordinates
			output.pack3.xy = v.texcoord0.xy;
			// Shader Properties Sampling
			float __outlineZsmooth = ( UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineZSmooth) );
			float __outlineWidth = ( UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineWidth) );
			float4 __outlineColorVertex = ( UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineColorVertex).rgba );

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			output.pack2.xyz = worldPos;
			float4 clipPos = output.vertex;

			// Screen Position
			float4 screenPos = ComputeScreenPos(clipPos);
			output.screenPosition = screenPos;
		
		#ifdef TCP2_COLORS_AS_NORMALS
			//Vertex Color for Normals
			float3 normal = (v.vertexColor.xyz*2) - 1;
		#elif TCP2_TANGENT_AS_NORMALS
			//Tangent for Normals
			float3 normal = v.tangent.xyz;
		#elif TCP2_UV1_AS_NORMALS || TCP2_UV2_AS_NORMALS || TCP2_UV3_AS_NORMALS || TCP2_UV4_AS_NORMALS
			#if TCP2_UV1_AS_NORMALS
				#define uvChannel texcoord0
			#elif TCP2_UV2_AS_NORMALS
				#define uvChannel texcoord1
			#elif TCP2_UV3_AS_NORMALS
				#define uvChannel texcoord2
			#elif TCP2_UV4_AS_NORMALS
				#define uvChannel texcoord3
			#endif
		
			#if TCP2_UV_NORMALS_FULL
			//UV for Normals, full
			float3 normal = v.uvChannel.xyz;
			#else
			//UV for Normals, compressed
			#if TCP2_UV_NORMALS_ZW
				#define ch1 z
				#define ch2 w
			#else
				#define ch1 x
				#define ch2 y
			#endif
			float3 n;
			//unpack uvs
			v.uvChannel.ch1 = v.uvChannel.ch1 * 255.0/16.0;
			n.x = floor(v.uvChannel.ch1) / 15.0;
			n.y = frac(v.uvChannel.ch1) * 16.0 / 15.0;
			//- get z
			n.z = v.uvChannel.ch2;
			//- transform
			n = n*2 - 1;
			float3 normal = n;
			#endif
		#else
			float3 normal = v.normal;
		#endif
		
		#if TCP2_ZSMOOTH_ON
			//Correct Z artefacts
			normal = UnityObjectToViewPos(normal);
			normal.z = -_ZSmooth;
		#endif
		
			//Z correction in view space
			normal = mul(UNITY_MATRIX_V, float4(normal, 0)).xyz;
			normal.z += __outlineZsmooth;
			normal = mul(float4(normal, 0), UNITY_MATRIX_V).xyz;
			float size = 1;
		
		#if !defined(SHADOWCASTER_PASS)
			output.vertex = UnityObjectToClipPos(v.vertex.xyz);
			normal = mul(unity_ObjectToWorld, float4(normal, 0)).xyz;
			float2 clipNormals = normalize(mul(UNITY_MATRIX_VP, float4(normal,0)).xy);
			half2 outlineWidth = (__outlineWidth * output.vertex.w) / (_ScreenParams.xy / 2.0);
			output.vertex.xy += clipNormals.xy * outlineWidth;
			
			output.vertex.z += __outlineZsmooth * 0.0001;
		#else
			v.vertex = v.vertex + float4(normal,0) * __outlineWidth * size * 0.01;
		#endif
		
			output.vcolor.xyzw = __outlineColorVertex;

			return output;
		}

		float4 fragment_outline (v2f_outline input) : SV_Target
		{

			// Custom Material Properties Sampling
			half4 value__MainTex = TCP2_TEX2D_SAMPLE(_MainTex, _MainTex, input.pack3.xy).rgba;

			// Shader Properties Sampling
			float4 __outlineColor = ( value__MainTex.rgba );

			half4 outlineColor = __outlineColor * input.vcolor.xyzw;

			return outlineColor;
		}

		ENDCG
		// Outline Include End
		// Silhouette Pass
		Pass
		{
			Name "Silhouette"
			Tags
			{
				"LightMode"="ForwardBase"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater
			ZWrite Off

			CGPROGRAM
			#pragma vertex vertex_silhouette
			#pragma fragment fragment_silhouette
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nolodfade nolightprobe nolightmap
			#pragma target 3.0

			struct appdata_sil
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f_sil
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 screenPosition : TEXCOORD0;
				float3 pack1 : TEXCOORD1; /* pack1.xyz = worldPos */
			};

			v2f_sil vertex_silhouette (appdata_sil v)
			{
				v2f_sil output;
				UNITY_INITIALIZE_OUTPUT(v2f_sil, output);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				output.pack1.xyz = worldPos;
				output.vertex = UnityObjectToClipPos(v.vertex);
				float4 clipPos = output.vertex;

				// Screen Position
				float4 screenPos = ComputeScreenPos(clipPos);
				output.screenPosition = screenPos;

				return output;
			}

			half4 fragment_silhouette (v2f_sil input) : SV_Target
			{

				// Shader Properties Sampling
				float4 __silhouetteColor = ( UNITY_ACCESS_INSTANCED_PROP(Props, _SilhouetteColor).rgba );

				return __silhouetteColor;
			}
			ENDCG
		}

		// Main Surface Shader

		CGPROGRAM

		#pragma surface surf ToonyColorsCustom vertex:vertex_surface exclude_path:deferred exclude_path:prepass keepalpha noforwardadd interpolateview halfasview nolightmap nofog nolppv
		#pragma instancing_options assumeuniformscaling nolodfade nolightprobe nolightmap
		#pragma target 3.0

		//================================================================
		// SHADER KEYWORDS

		#pragma shader_feature_local_fragment TCP2_SPECULAR
		#pragma shader_feature_local_fragment TCP2_RIM_LIGHTING

		//================================================================
		// STRUCTS

		// Vertex input
		struct appdata_tcp2
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord0 : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
		#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
			half4 tangent : TANGENT;
		#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			half3 viewDir;
			float3 worldPos;
			float4 screenPosition;
			float2 texcoord0;
		};

		//================================================================

		// Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			half atten;
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Specular;
			half Gloss;
			half Alpha;

			Input input;

			// Shader Properties
			float __rampThreshold;
			float __rampSmoothing;
			float __shadowHue;
			float __shadowSaturation;
			float __shadowValue;
			float3 __highlightColor;
			float3 __shadowColor;
			float __ambientIntensity;
			float __specularSmoothness;
			float3 __specularColor;
			float3 __rimDir;
			float __rimMin;
			float __rimMax;
			float3 __rimColor;
			float __rimStrength;
		};

		//================================================================
		// VERTEX FUNCTION

		void vertex_surface(inout appdata_tcp2 v, out Input output)
		{
			UNITY_INITIALIZE_OUTPUT(Input, output);

			// Texture Coordinates
			output.texcoord0 = v.texcoord0.xy;

			float4 clipPos = UnityObjectToClipPos(v.vertex);

			// Screen Position
			float4 screenPos = ComputeScreenPos(clipPos);
			output.screenPosition = screenPos;

		}

		//================================================================
		// SURFACE FUNCTION

		void surf(Input input, inout SurfaceOutputCustom output)
		{
			// Custom Material Properties Sampling
			half4 value__MainTex = TCP2_TEX2D_SAMPLE(_MainTex, _MainTex, input.texcoord0.xy).rgba;

			// Shader Properties Sampling
			float4 __albedo = ( value__MainTex.rgba );
			float4 __mainColor = ( UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgba );
			float __alpha = ( __albedo.a * __mainColor.a );
			float3 __emission = ( UNITY_ACCESS_INSTANCED_PROP(Props, _Emission).rgb );
			output.__rampThreshold = ( UNITY_ACCESS_INSTANCED_PROP(Props, _RampThreshold) );
			output.__rampSmoothing = ( UNITY_ACCESS_INSTANCED_PROP(Props, _RampSmoothing) );
			output.__shadowHue = ( UNITY_ACCESS_INSTANCED_PROP(Props, _Shadow_HSV_H) );
			output.__shadowSaturation = ( UNITY_ACCESS_INSTANCED_PROP(Props, _Shadow_HSV_S) );
			output.__shadowValue = ( UNITY_ACCESS_INSTANCED_PROP(Props, _Shadow_HSV_V) );
			output.__highlightColor = ( UNITY_ACCESS_INSTANCED_PROP(Props, _HColor).rgb );
			output.__shadowColor = ( UNITY_ACCESS_INSTANCED_PROP(Props, _SColor).rgb );
			output.__ambientIntensity = ( 1.0 );
			output.__specularSmoothness = ( UNITY_ACCESS_INSTANCED_PROP(Props, _SpecularSmoothness) );
			output.__specularColor = ( UNITY_ACCESS_INSTANCED_PROP(Props, _SpecularColor).rgb );
			output.__rimDir = ( UNITY_ACCESS_INSTANCED_PROP(Props, _RimDir).xyz );
			output.__rimMin = ( UNITY_ACCESS_INSTANCED_PROP(Props, _RimMin) );
			output.__rimMax = ( UNITY_ACCESS_INSTANCED_PROP(Props, _RimMax) );
			output.__rimColor = ( value__MainTex.rgb );
			output.__rimStrength = ( 1.0 );

			output.input = input;

			output.Albedo = __albedo.rgb;
			output.Alpha = __alpha;

			output.Albedo *= __mainColor.rgb;
			output.Emission += __emission;

		}

		//================================================================
		// LIGHTING FUNCTION

		inline half4 LightingToonyColorsCustom(inout SurfaceOutputCustom surface, half3 viewDir, UnityGI gi)
		{

			half3 lightDir = gi.light.dir;
			#if defined(UNITY_PASS_FORWARDBASE)
				half3 lightColor = _LightColor0.rgb;
				half atten = surface.atten;
			#else
				// extract attenuation from point/spot lights
				half3 lightColor = _LightColor0.rgb;
				half atten = max(gi.light.color.r, max(gi.light.color.g, gi.light.color.b)) / max(_LightColor0.r, max(_LightColor0.g, _LightColor0.b));
			#endif

			half3 normal = normalize(surface.Normal);
			half ndl = dot(normal, lightDir);
			half3 ramp;
			
			#define		RAMP_THRESHOLD	surface.__rampThreshold
			#define		RAMP_SMOOTH		surface.__rampSmoothing
			ndl = saturate(ndl);
			ramp = smoothstep(RAMP_THRESHOLD - RAMP_SMOOTH*0.5, RAMP_THRESHOLD + RAMP_SMOOTH*0.5, ndl);

			// Apply attenuation (shadowmaps & point/spot lights attenuation)
			ramp *= atten;
			
			//Shadow HSV
			float3 albedoShadowHSV = ApplyHSV_3(surface.Albedo, surface.__shadowHue, surface.__shadowSaturation, surface.__shadowValue);
			surface.Albedo = lerp(albedoShadowHSV, surface.Albedo, ramp);

			// Highlight/Shadow Colors
			#if !defined(UNITY_PASS_FORWARDBASE)
				ramp = lerp(half3(0,0,0), surface.__highlightColor, ramp);
			#else
				ramp = lerp(surface.__shadowColor, surface.__highlightColor, ramp);
			#endif

			// Output color
			half4 color;
			color.rgb = surface.Albedo * lightColor.rgb * ramp;
			color.a = surface.Alpha;

			// Apply indirect lighting (ambient)
			half occlusion = 1;
			#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
				half3 ambient = gi.indirect.diffuse;
				ambient *= surface.Albedo * occlusion * surface.__ambientIntensity;

				color.rgb += ambient;
			#endif

			half3 halfDir = SpecSafeNormalize(float3(lightDir) + float3(viewDir));
			
			#if defined(TCP2_SPECULAR)
			//Blinn-Phong Specular
			float ndh = max(0, dot (normal, halfDir));
			float spec = pow(ndh, 1e-4h + surface.__specularSmoothness * 128.0);
			spec *= ndl;
			spec *= atten;
			
			//Apply specular
			color.rgb += spec * lightColor.rgb * surface.__specularColor;
			#endif
			// Rim Lighting
			#if defined(TCP2_RIM_LIGHTING)
			half3 rViewDir = viewDir;
			half3 rimDir = surface.__rimDir;
			half3 screenPosOffset = (surface.input.screenPosition.xyz / surface.input.screenPosition.w) - 0.5;
			rimDir.xyz -= screenPosOffset.xyz;
			rViewDir = normalize(UNITY_MATRIX_V[0].xyz * rimDir.x + UNITY_MATRIX_V[1].xyz * rimDir.y + UNITY_MATRIX_V[2].xyz * rimDir.z);
			half rim = 1.0f - saturate(dot(rViewDir, normal));
			rim = ( rim );
			half rimMin = surface.__rimMin;
			half rimMax = surface.__rimMax;
			rim = smoothstep(rimMin, rimMax, rim);
			half3 rimColor = surface.__rimColor;
			half rimStrength = surface.__rimStrength;
			//Rim light mask
			color.rgb += ndl * atten * rim * rimColor * rimStrength;
			#endif

			return color;
		}

		void LightingToonyColorsCustom_GI(inout SurfaceOutputCustom surface, UnityGIInput data, inout UnityGI gi)
		{
			half3 normal = surface.Normal;

			// GI without reflection probes
			gi = UnityGlobalIllumination(data, 1.0, normal); // occlusion is applied in the lighting function, if necessary

			surface.atten = data.atten; // transfer attenuation to lighting function
			gi.light.color = _LightColor0.rgb; // remove attenuation

		}

		ENDCG

		// Outline
		Pass
		{
			Name "Outline"
			Tags
			{
				"LightMode"="ForwardBase"
			}
			Cull Front
			Offset 0,0
			Blend Off

			CGPROGRAM
			#pragma vertex vertex_outline
			#pragma fragment fragment_outline
			#pragma target 3.0
			#pragma multi_compile _ TCP2_COLORS_AS_NORMALS TCP2_TANGENT_AS_NORMALS TCP2_UV1_AS_NORMALS TCP2_UV2_AS_NORMALS TCP2_UV3_AS_NORMALS TCP2_UV4_AS_NORMALS
			#pragma multi_compile _ TCP2_UV_NORMALS_FULL TCP2_UV_NORMALS_ZW
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nolodfade nolightprobe nolightmap
			ENDCG
		}
	}

	Fallback "Diffuse"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.10";unity:"2022.3.30f1";tmplt:"SG2_Template_Default";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","UNITY_2021_2","UNITY_2022_2","SHADOW_HSV","SPEC_LEGACY","SPECULAR","SPECULAR_SHADER_FEATURE","EMISSION","RIM","RIM_DIR","RIM_LIGHTMASK","RIM_SHADER_FEATURE","RIM_DIR_PERSP_CORRECTION","OUTLINE","OUTLINE_OPAQUE","OUTLINE_CLIP_SPACE","OUTLINE_CONSTANT_SIZE","OUTLINE_PIXEL_PERFECT","OUTLINE_ZSMOOTH","PASS_SILHOUETTE"];flags:list["noforwardadd","interpolateview","halfasview"];flags_extra:dict[pragma_gpu_instancing=list["assumeuniformscaling","nolodfade","nolightprobe","nolightmap"]];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",RIM_LABEL="Rim Lighting"];shaderProperties:list[sp(name:"Albedo";imps:list[imp_ct(lct:"_MainTex";cc:4;chan:"RGBA";avchan:"RGBA";guid:"0c0a481c-97de-4b0d-be06-46f21663a3f3";op:Multiply;lbl:"Albedo";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Main Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:4;chan:"RGBA";prop:"_Color";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"e21217a5-ee26-48c4-9846-46ca64b6218d";op:Multiply;lbl:"Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),,,sp(name:"Ramp Threshold";imps:list[imp_mp_range(def:0.5;min:0.01;max:1;prop:"_RampThreshold";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"9ad50b66-16bb-43ed-be4d-54804b931949";op:Multiply;lbl:"Threshold";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Ramp Smoothing";imps:list[imp_mp_range(def:0.5;min:0.001;max:1;prop:"_RampSmoothing";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"ba18899e-b91c-4fdf-962c-9c783ec6380f";op:Multiply;lbl:"Smoothing";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Highlight Color";imps:list[imp_mp_color(def:RGBA(0.75, 0.75, 0.75, 1);hdr:False;cc:3;chan:"RGB";prop:"_HColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"5fd6b4f4-beb7-4a21-b4ae-fd938076696b";op:Multiply;lbl:"Highlight Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Shadow Color";imps:list[imp_mp_color(def:RGBA(0.2, 0.2, 0.2, 1);hdr:False;cc:3;chan:"RGB";prop:"_SColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"54d2ec87-37b3-4a4c-b1cb-d69dd48bb1b1";op:Multiply;lbl:"Shadow Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Shadow Hue";imps:list[imp_mp_range(def:0;min:-180;max:180;prop:"_Shadow_HSV_H";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"b5630b73-9a55-4e89-ad03-04b81f75e2ae";op:Multiply;lbl:"Hue";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Shadow Saturation";imps:list[imp_mp_range(def:0;min:-1;max:1;prop:"_Shadow_HSV_S";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"8b9cde7d-7287-4265-a746-42c85fe95797";op:Multiply;lbl:"Saturation";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Shadow Value";imps:list[imp_mp_range(def:0;min:-1;max:1;prop:"_Shadow_HSV_V";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"ae10abc0-7b65-4e95-ace4-77699c1251a9";op:Multiply;lbl:"Value";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Specular Color";imps:list[imp_mp_color(def:RGBA(0.5, 0.5, 0.5, 1);hdr:False;cc:3;chan:"RGB";prop:"_SpecularColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"27f34ea9-b76f-40f4-b6bc-3c18c119171f";op:Multiply;lbl:"Specular Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Specular Smoothness";imps:list[imp_mp_float(def:0.2;prop:"_SpecularSmoothness";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"478b81c2-1db1-4005-989c-7ff3ea6691e3";op:Multiply;lbl:"Smoothness";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Rim Color";imps:list[imp_ct(lct:"_MainTex";cc:3;chan:"RGB";avchan:"RGBA";guid:"715cf0b0-9bb4-4de7-8d04-c6c7578cf23d";op:Multiply;lbl:"Rim Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),,sp(name:"Rim Min";imps:list[imp_mp_range(def:0.5;min:0;max:2;prop:"_RimMin";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"312e5ec1-270e-47f1-8b3a-3bbe24197602";op:Multiply;lbl:"Rim Min";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Rim Max";imps:list[imp_mp_range(def:1;min:0;max:2;prop:"_RimMax";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"99e997fb-e165-41d7-a4eb-90d2fa302e25";op:Multiply;lbl:"Rim Max";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Rim Dir";imps:list[imp_mp_vector(def:(0, 0, 1, 0);fp:float;cc:3;chan:"XYZ";prop:"_RimDir";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"b4a791b0-66fc-4563-8b5b-302919ee4343";op:Multiply;lbl:"Rim Direction";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),,sp(name:"Emission";imps:list[imp_mp_color(def:RGBA(0, 0, 0, 1);hdr:True;cc:3;chan:"RGB";prop:"_Emission";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"2bcb7c3d-ec48-4fa3-af39-e1ba9aef6441";op:Multiply;lbl:"Emission Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Outline Width";imps:list[imp_mp_range(def:1;min:0.1;max:50;prop:"_OutlineWidth";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"6eb172c4-3791-4f07-a3c3-ef6882c919b7";op:Multiply;lbl:"Width";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Outline Color";imps:list[imp_ct(lct:"_MainTex";cc:4;chan:"RGBA";avchan:"RGBA";guid:"b8a063be-63ef-4e9d-895a-90955840bc9d";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Outline Color Vertex";imps:list[imp_mp_color(def:RGBA(0, 0, 0, 1);hdr:False;cc:4;chan:"RGBA";prop:"_OutlineColorVertex";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"406530f6-e9c5-4614-b378-0ad7ab437eab";op:Multiply;lbl:"Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),sp(name:"Outline ZSmooth";imps:list[imp_mp_range(def:0;min:-3;max:3;prop:"_OutlineZSmooth";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"c30d14d5-39a9-4429-97db-4772b41a507a";op:Multiply;lbl:"Z Correction";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False),,,sp(name:"Silhouette Color";imps:list[imp_mp_color(def:RGBA(0, 0, 0, 0.33);hdr:False;cc:4;chan:"RGBA";prop:"_SilhouetteColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"c2c8e6cb-1331-49af-8e20-1eb98de43c6e";op:Multiply;lbl:"Silhouette Color";gpu_inst:True;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False)];customTextures:list[ct(cimp:imp_mp_texture(uto:False;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"white";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_MainTex";md:"[MainTexture]";gbv:False;custom:True;refs:"Albedo, Rim Color";pnlock:False;guid:"7cff599e-dbf1-45ae-9efa-dc72d8085172";op:Multiply;lbl:"MainTex";gpu_inst:True;dots_inst:False;locked:False;impl_index:-1);exp:True;uv_exp:False;imp_lbl:"Texture")];codeInjection:codeInjection(injectedFiles:list[];mark:False);matLayers:list[]) */
/* TCP_HASH ea2814d73b1c8ca9e105806cf84a6c98 */
