// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NewWaterfall"
{
	Properties
	{
		_WaterfallStartNoise("Waterfall Start Noise", 2D) = "white" {}
		_WaterfallStartNoiseSpeed("Waterfall Start Noise Speed", Float) = 0.2842633
		_WaterfallStartNoisePosition("Waterfall Start Noise Position", Range( 0 , 1)) = 0.22
		_WaterfallStartNoiseOpacity("Waterfall Start Noise Opacity", Range( 0 , 1)) = 0.22
		_WaterfallStartNoiseExtend("Waterfall Start Noise Extend", Range( 0 , 1)) = 0.22
		_WaterfallStartNoisePow("Waterfall Start Noise Pow", Range( 0 , 1)) = 1
		_CloudNoiseSpeed("Cloud Noise Speed", Float) = -0.1
		_CloudNoise("Cloud Noise", 2D) = "white" {}
		_WaterfallStartNoiseDistortion("Waterfall Start Noise Distortion", Range( 0 , 0.3)) = 0.037
		_WaterfallStartNoiseDepth("Waterfall Start Noise Depth", Range( 0 , 1)) = 0.037
		_ColorVariationSpeed("Color Variation Speed", Float) = -0.1
		_ColorVariationTexture("Color Variation Texture", 2D) = "white" {}
		_ColorVariationDepth("Color Variation Depth", Range( 0 , 1)) = 0.054
		_Color1("Color 1", Color) = (0,0.8273995,1,0)
		_Color3("Color 3", Color) = (0,0.5989819,0.8867924,0)
		_Color4("Color 4", Color) = (0,0.5989819,0.8867924,0)
		_Color2("Color 2", Color) = (0,0.5989819,0.8867924,0)
		_ColorVariationContrast("Color Variation Contrast", Float) = 1
		_WaterfallEdgeSpeed("Waterfall Edge Speed", Float) = 0.2
		_WaterfallEdge("Waterfall Edge", 2D) = "white" {}
		_WaterfallEdgeFoamOpacity("Waterfall Edge Foam Opacity", Range( 0 , 1)) = 0.5
		_TopVoronoi("Top Voronoi", 2D) = "white" {}
		_SmallDots1Scale("Small Dots 1 Scale", Vector) = (1,1,0,0)
		_SmallDots2Scale("Small Dots 2 Scale", Vector) = (1,1,0,0)
		_SmallDots2Speed("Small Dots 2 Speed", Float) = -0.1
		_SmallDots1Speed("Small Dots 1 Speed", Float) = -0.1
		_SmallDots1Step("Small Dots 1 Step", Range( 0 , 1)) = 0.8
		_SmallDots2Step("Small Dots 2 Step", Range( 0 , 1)) = 0.8
		_SmallDots1Opacity("Small Dots 1 Opacity", Range( 0 , 1)) = 0.8
		_SmallDots2Opacity("Small Dots 2 Opacity", Range( 0 , 1)) = 0.8
		_SmallDots1Distortion("Small Dots 1 Distortion", Range( 0 , 0.2)) = 0.045
		_SmallDots2Distortion("Small Dots 2 Distortion", Range( 0 , 0.2)) = 0.045
		_NoiseLinesPow("Noise Lines Pow", Float) = 1
		_NoiseLinesOpacity("Noise Lines Opacity", Float) = 0.8
		_NoiseLinesReveal("Noise Lines Reveal", Float) = 0.8
		_NoiseLinesDistortion("Noise Lines Distortion", Range( 0 , 0.3)) = 0.042
		_NoiseLinesSpeed("Noise Lines Speed", Float) = -0.2
		_NoiseLines("Noise Lines", 2D) = "white" {}
		_BottomFoamSpeed("Bottom Foam Speed", Float) = -0.1
		_BottomFoamWidthPow("Bottom Foam Width Pow", Float) = 0
		_BottomFoamExtendMin("Bottom Foam Extend Min", Float) = 0
		_BottomFoamExtendMax("Bottom Foam Extend Max", Range( 0 , 1)) = 0.15
		_BottomFoamStep("Bottom Foam Step", Range( 0 , 1)) = 0.5
		_BottomFoamDistortion("Bottom Foam Distortion", Range( 0 , 0.3)) = 0.05
		_StartNoiseHarshTiling("Start Noise Harsh Tiling", Vector) = (1,1,0,0)
		_StartNoiseHarshSpeed("Start Noise Harsh Speed", Float) = -1
		_StartNoiseHarshDistortion("Start Noise Harsh Distortion", Range( 0 , 0.2)) = 0.026
		_StartNoiseharshTopPosition("Start Noise harsh Top Position", Range( 0 , 1)) = 0.24
		_StartNoiseharshBottomPosition("Start Noise harsh Bottom Position", Range( 0 , 1)) = 0.4435484
		_StartNoiseharshTopBlend("Start Noise harsh Top Blend", Range( 0 , 0.5)) = 0.07522548
		_StartNoiseharshBottomBlend("Start Noise harsh Bottom Blend", Range( 0 , 0.5)) = 0.07522548
		_StartNoiseHarshStep("Start Noise Harsh Step", Range( 0 , 1)) = 0.51
		_InitialOpacityGradience("Initial Opacity Gradience", Float) = 10
		_BottomOpacityCutout("Bottom Opacity Cutout", Range( 0 , 1)) = 0.9
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_NormalStrength("Normal Strength", Float) = 1
		[Toggle(_DEBUG_ON)] _DEBUG("DEBUG", Float) = 0
		_DebugWaterColor("DebugWaterColor", Range( 0 , 1)) = 0
		_DebugNormals("DebugNormals", Range( 0 , 1)) = 0
		_DebugCloudNoise("Debug Cloud Noise", Range( 0 , 1)) = 0
		_EdgeFoamDistance("Edge Foam Distance", Float) = 1
		_EdgeFoamOpacity("Edge Foam Opacity", Float) = 1
		_EdgeFoamStep("Edge Foam Step", Range( 0 , 1)) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
		#pragma shader_feature_local _DEBUG_ON
		#define ASE_VERSION 19801
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float4 screenPos;
		};

		uniform float _NormalStrength;
		uniform sampler2D _ColorVariationTexture;
		uniform float4 _ColorVariationTexture_ST;
		uniform float _ColorVariationSpeed;
		float4 _ColorVariationTexture_TexelSize;
		uniform float4 _Color2;
		uniform float4 _Color1;
		uniform float _ColorVariationDepth;
		uniform float _ColorVariationContrast;
		uniform float4 _Color3;
		uniform sampler2D _NoiseLines;
		uniform float4 _NoiseLines_ST;
		uniform float _NoiseLinesSpeed;
		uniform sampler2D _CloudNoise;
		uniform float4 _CloudNoise_ST;
		uniform float _CloudNoiseSpeed;
		uniform float _NoiseLinesDistortion;
		uniform float _NoiseLinesOpacity;
		uniform float _NoiseLinesReveal;
		uniform float _NoiseLinesPow;
		uniform float4 _Color4;
		uniform sampler2D _WaterfallStartNoise;
		uniform float4 _WaterfallStartNoise_ST;
		uniform float _WaterfallStartNoiseSpeed;
		uniform float _WaterfallStartNoiseDistortion;
		uniform float _WaterfallStartNoiseDepth;
		uniform float _WaterfallStartNoiseExtend;
		uniform float _WaterfallStartNoisePow;
		uniform float _WaterfallStartNoisePosition;
		uniform float _WaterfallStartNoiseOpacity;
		uniform float _BottomFoamStep;
		uniform float _BottomFoamSpeed;
		uniform float _BottomFoamDistortion;
		uniform float _BottomFoamExtendMax;
		uniform float _BottomFoamExtendMin;
		uniform float _BottomFoamWidthPow;
		uniform sampler2D _WaterfallEdge;
		uniform float4 _WaterfallEdge_ST;
		uniform float _WaterfallEdgeSpeed;
		uniform float _WaterfallEdgeFoamOpacity;
		uniform float _SmallDots1Step;
		uniform sampler2D _TopVoronoi;
		uniform float2 _SmallDots1Scale;
		uniform float _SmallDots1Speed;
		uniform float _SmallDots1Distortion;
		uniform float _SmallDots1Opacity;
		uniform float _StartNoiseHarshStep;
		uniform float2 _StartNoiseHarshTiling;
		uniform float _StartNoiseHarshSpeed;
		uniform float _StartNoiseHarshDistortion;
		uniform float _StartNoiseharshTopPosition;
		uniform float _StartNoiseharshTopBlend;
		uniform float _StartNoiseharshBottomPosition;
		uniform float _StartNoiseharshBottomBlend;
		uniform float _SmallDots2Step;
		uniform float2 _SmallDots2Scale;
		uniform float _SmallDots2Speed;
		uniform float _SmallDots2Distortion;
		uniform float _SmallDots2Opacity;
		uniform float _EdgeFoamStep;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _EdgeFoamDistance;
		uniform float _EdgeFoamOpacity;
		uniform float _DebugWaterColor;
		uniform float _DebugNormals;
		uniform float _DebugCloudNoise;
		uniform float _Smoothness;
		uniform float _InitialOpacityGradience;
		uniform float _BottomOpacityCutout;


		void CalculateUVsSmooth46_g2( float2 UV, float4 TexelSize, out float2 UV0, out float2 UV1, out float2 UV2, out float2 UV3, out float2 UV4, out float2 UV5, out float2 UV6, out float2 UV7, out float2 UV8 )
		{
			{
			    float3 pos = float3( TexelSize.xy, 0 );
			    float3 neg = float3( -pos.xy, 0 );
			    UV0 = UV + neg.xy;
			    UV1 = UV + neg.zy;
			    UV2 = UV + float2( pos.x, neg.y );
			    UV3 = UV + neg.xz;
			    UV4 = UV;
			    UV5 = UV + pos.xz;
			    UV6 = UV + float2( neg.x, pos.y );
			    UV7 = UV + pos.zy;
			    UV8 = UV + pos.xy;
			    return;
			}
		}


		float3 CombineSamplesSmooth58_g2( float Strength, float S0, float S1, float S2, float S3, float S4, float S5, float S6, float S7, float S8 )
		{
			{
			    float3 normal;
			    normal.x = Strength * ( S0 - S2 + 2 * S3 - 2 * S5 + S6 - S8 );
			    normal.y = Strength * ( S0 + 2 * S1 + S2 - S6 - 2 * S7 - S8 );
			    normal.z = 1.0;
			    return normalize( normal );
			}
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_91_0_g2 = _NormalStrength;
			float Strength58_g2 = temp_output_91_0_g2;
			float localCalculateUVsSmooth46_g2 = ( 0.0 );
			float2 uv_ColorVariationTexture = i.uv_texcoord * _ColorVariationTexture_ST.xy + _ColorVariationTexture_ST.zw;
			float mulTime52 = _Time.y * _ColorVariationSpeed;
			float2 appendResult54 = (float2(0.0 , mulTime52));
			float2 temp_output_55_0 = ( uv_ColorVariationTexture + appendResult54 );
			float2 temp_output_85_0_g2 = temp_output_55_0;
			float2 UV46_g2 = temp_output_85_0_g2;
			float4 TexelSize46_g2 = _ColorVariationTexture_TexelSize;
			float2 UV046_g2 = float2( 0,0 );
			float2 UV146_g2 = float2( 0,0 );
			float2 UV246_g2 = float2( 0,0 );
			float2 UV346_g2 = float2( 0,0 );
			float2 UV446_g2 = float2( 0,0 );
			float2 UV546_g2 = float2( 0,0 );
			float2 UV646_g2 = float2( 0,0 );
			float2 UV746_g2 = float2( 0,0 );
			float2 UV846_g2 = float2( 0,0 );
			CalculateUVsSmooth46_g2( UV46_g2 , TexelSize46_g2 , UV046_g2 , UV146_g2 , UV246_g2 , UV346_g2 , UV446_g2 , UV546_g2 , UV646_g2 , UV746_g2 , UV846_g2 );
			float4 break140_g2 = tex2D( _ColorVariationTexture, UV046_g2 );
			float S058_g2 = break140_g2.g;
			float4 break142_g2 = tex2D( _ColorVariationTexture, UV146_g2 );
			float S158_g2 = break142_g2.g;
			float4 break146_g2 = tex2D( _ColorVariationTexture, UV246_g2 );
			float S258_g2 = break146_g2.g;
			float4 break148_g2 = tex2D( _ColorVariationTexture, UV346_g2 );
			float S358_g2 = break148_g2.g;
			float4 break150_g2 = tex2D( _ColorVariationTexture, UV446_g2 );
			float S458_g2 = break150_g2.g;
			float4 break152_g2 = tex2D( _ColorVariationTexture, UV546_g2 );
			float S558_g2 = break152_g2.g;
			float4 break154_g2 = tex2D( _ColorVariationTexture, UV646_g2 );
			float S658_g2 = break154_g2.g;
			float4 break156_g2 = tex2D( _ColorVariationTexture, UV746_g2 );
			float S758_g2 = break156_g2.g;
			float4 break158_g2 = tex2D( _ColorVariationTexture, UV846_g2 );
			float S858_g2 = break158_g2.g;
			float3 localCombineSamplesSmooth58_g2 = CombineSamplesSmooth58_g2( Strength58_g2 , S058_g2 , S158_g2 , S258_g2 , S358_g2 , S458_g2 , S558_g2 , S658_g2 , S758_g2 , S858_g2 );
			float3 Normals366 = localCombineSamplesSmooth58_g2;
			#ifdef _DEBUG_ON
				float3 staticSwitch364 = float3(0,0,1);
			#else
				float3 staticSwitch364 = Normals366;
			#endif
			o.Normal = staticSwitch364;
			float4 tex2DNode48 = tex2D( _ColorVariationTexture, temp_output_55_0 );
			float3 ase_positionWS = i.worldPos;
			float3 ase_normalWS = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_tangentWS = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_bitangentWS = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
			float3 ase_viewVectorTS = mul( ase_worldToTangent, ( _WorldSpaceCameraPos.xyz - ase_positionWS ) );
			float3 normalizeResult59 = normalize( ase_viewVectorTS );
			float2 paralaxOffset47 = ParallaxOffset( tex2DNode48.g , _ColorVariationDepth , normalizeResult59 );
			float4 tex2DNode57 = tex2D( _ColorVariationTexture, ( paralaxOffset47 + temp_output_55_0 ) );
			float4 lerpResult62 = lerp( _Color2 , _Color1 , pow( tex2DNode57.g , _ColorVariationContrast ));
			float2 uv_NoiseLines = i.uv_texcoord * _NoiseLines_ST.xy + _NoiseLines_ST.zw;
			float mulTime188 = _Time.y * _NoiseLinesSpeed;
			float2 appendResult190 = (float2(0.0 , mulTime188));
			float2 uv_CloudNoise = i.uv_texcoord * _CloudNoise_ST.xy + _CloudNoise_ST.zw;
			float mulTime32 = _Time.y * _CloudNoiseSpeed;
			float2 appendResult33 = (float2(mulTime32 , 0.0));
			float4 Cloud_Noise37 = tex2D( _CloudNoise, ( uv_CloudNoise + appendResult33 ) );
			float4 lerpResult199 = lerp( lerpResult62 , _Color3 , pow( saturate( ( tex2D( _NoiseLines, ( uv_NoiseLines + appendResult190 + ( (Cloud_Noise37).rg * _NoiseLinesDistortion ) ) ).g * _NoiseLinesOpacity * pow( tex2DNode48.g , _NoiseLinesReveal ) ) ) , _NoiseLinesPow ));
			float2 uv_WaterfallStartNoise = i.uv_texcoord * _WaterfallStartNoise_ST.xy + _WaterfallStartNoise_ST.zw;
			float mulTime3 = _Time.y * _WaterfallStartNoiseSpeed;
			float2 appendResult5 = (float2(0.0 , mulTime3));
			float2 temp_output_6_0 = ( uv_WaterfallStartNoise + appendResult5 + ( (Cloud_Noise37).rg * _WaterfallStartNoiseDistortion ) );
			float3 normalizeResult236 = normalize( ase_viewVectorTS );
			float2 paralaxOffset234 = ParallaxOffset( tex2D( _WaterfallStartNoise, temp_output_6_0 ).g , _WaterfallStartNoiseDepth , normalizeResult236 );
			float smoothstepResult19 = smoothstep( _WaterfallStartNoiseExtend , _WaterfallStartNoisePow , abs( ( i.uv_texcoord.y - _WaterfallStartNoisePosition ) ));
			float4 lerpResult238 = lerp( lerpResult199 , float4( _Color4.rgb , 0.0 ) , saturate( ( tex2D( _WaterfallStartNoise, ( temp_output_6_0 + paralaxOffset234 ) ).g * ( smoothstepResult19 * _WaterfallStartNoiseOpacity ) ) ));
			float4 Water_Color73 = lerpResult238;
			float mulTime252 = _Time.y * _BottomFoamSpeed;
			float2 appendResult254 = (float2(0.0 , mulTime252));
			float smoothstepResult258 = smoothstep( _BottomFoamExtendMax , _BottomFoamExtendMin , ( 1.0 - i.uv_texcoord.y ));
			float Bottom_Foam264 = step( _BottomFoamStep , ( tex2D( _WaterfallStartNoise, ( uv_WaterfallStartNoise + appendResult254 + ( (Cloud_Noise37).rg * _BottomFoamDistortion ) ) ).g * smoothstepResult258 * pow( ( 1.0 - ( abs( ( i.uv_texcoord.x - 0.5 ) ) * 2.0 ) ) , _BottomFoamWidthPow ) ) );
			float2 uv_WaterfallEdge = i.uv_texcoord * _WaterfallEdge_ST.xy + _WaterfallEdge_ST.zw;
			float mulTime79 = _Time.y * -_WaterfallEdgeSpeed;
			float2 appendResult81 = (float2(0.0 , mulTime79));
			float4 tex2DNode83 = tex2D( _WaterfallEdge, ( uv_WaterfallEdge + appendResult81 ) );
			float Watefal_Edge87 = ( step( 0.6 , tex2DNode83.r ) * _WaterfallEdgeFoamOpacity );
			float mulTime133 = _Time.y * _SmallDots1Speed;
			float2 appendResult134 = (float2(0.0 , mulTime133));
			float Small_Dots_1139 = ( step( _SmallDots1Step , tex2D( _TopVoronoi, ( ( i.uv_texcoord * _SmallDots1Scale ) + appendResult134 + ( ( (Cloud_Noise37).rg - float2( 0.5,0.5 ) ) * _SmallDots1Distortion ) + float2( 0.32,0.27 ) ) ).g ) * _SmallDots1Opacity );
			float mulTime295 = _Time.y * _StartNoiseHarshSpeed;
			float2 appendResult296 = (float2(0.0 , mulTime295));
			float smoothstepResult303 = smoothstep( _StartNoiseharshTopPosition , ( _StartNoiseharshTopPosition + _StartNoiseharshTopBlend ) , i.uv_texcoord.y);
			float smoothstepResult304 = smoothstep( _StartNoiseharshBottomPosition , ( _StartNoiseharshBottomPosition + _StartNoiseharshBottomBlend ) , i.uv_texcoord.y);
			float Start_Noise_Harsh316 = step( _StartNoiseHarshStep , ( tex2D( _WaterfallStartNoise, ( ( i.uv_texcoord * _StartNoiseHarshTiling ) + appendResult296 + ( (Cloud_Noise37).rg * _StartNoiseHarshDistortion ) ) ).g * ( smoothstepResult303 * ( 1.0 - smoothstepResult304 ) ) ) );
			float mulTime156 = _Time.y * _SmallDots2Speed;
			float2 appendResult161 = (float2(0.0 , mulTime156));
			float Small_Dots_2168 = ( step( _SmallDots2Step , tex2D( _TopVoronoi, ( ( i.uv_texcoord * _SmallDots2Scale ) + appendResult161 + ( ( (Cloud_Noise37).rg - float2( 0.5,0.5 ) ) * _SmallDots2Distortion ) ) ).g ) * _SmallDots2Opacity );
			float4 ase_positionSS = float4( i.screenPos.xyz , i.screenPos.w + 1e-7 );
			float4 ase_positionSSNorm = ase_positionSS / ase_positionSS.w;
			ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
			float depthLinearEye370 = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy ) );
			float Scene_Depth372 = ( depthLinearEye370 - ase_positionSS.w );
			float Edge_Foam382 = ( step( _EdgeFoamStep , saturate( ( saturate( ( ( ( Scene_Depth372 - _EdgeFoamDistance ) / _EdgeFoamDistance ) * -1.0 ) ) * tex2DNode57.g ) ) ) * _EdgeFoamOpacity );
			float Foam_Mask71 = max( max( max( Bottom_Foam264 , Watefal_Edge87 ) , Small_Dots_1139 ) , max( max( Start_Noise_Harsh316 , Small_Dots_2168 ) , Edge_Foam382 ) );
			float4 lerpResult67 = lerp( Water_Color73 , float4( 1,1,1,0 ) , Foam_Mask71);
			float4 temp_cast_1 = (0.0).xxxx;
			#ifdef _DEBUG_ON
				float4 staticSwitch358 = temp_cast_1;
			#else
				float4 staticSwitch358 = lerpResult67;
			#endif
			o.Albedo = staticSwitch358.rgb;
			float4 temp_cast_3 = (0.0).xxxx;
			#ifdef _DEBUG_ON
				float4 staticSwitch357 = ( ( Water_Color73 * _DebugWaterColor ) + float4( ( Normals366 * _DebugNormals ) , 0.0 ) + ( Cloud_Noise37 * _DebugCloudNoise ) );
			#else
				float4 staticSwitch357 = temp_cast_3;
			#endif
			o.Emission = staticSwitch357.rgb;
			#ifdef _DEBUG_ON
				float staticSwitch362 = 0.0;
			#else
				float staticSwitch362 = _Smoothness;
			#endif
			o.Smoothness = staticSwitch362;
			float Opacity93 = step( 0.1 , tex2DNode83.r );
			#ifdef _DEBUG_ON
				float staticSwitch360 = 1.0;
			#else
				float staticSwitch360 = ( Opacity93 * saturate( ( i.uv_texcoord.y * _InitialOpacityGradience ) ) * ( 1.0 - step( _BottomOpacityCutout , i.uv_texcoord.y ) ) );
			#endif
			o.Alpha = staticSwitch360;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "Polyart.WaterfallMaterialCustomInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.RangedFloatNode;34;-1376,-2000;Inherit;False;Property;_CloudNoiseSpeed;Cloud Noise Speed;6;0;Create;True;0;0;0;False;0;False;-0.1;-0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;26;-1408,-2272;Inherit;True;Property;_CloudNoise;Cloud Noise;7;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;32;-1152,-2000;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-1168,-2144;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;33;-960,-2032;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;368;-6945.269,-2405.87;Inherit;False;692;339;Scene Depth;4;372;371;370;369;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-784,-2144;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;369;-6895.269,-2275.87;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;370;-6895.269,-2355.87;Inherit;False;0;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-4016,2016;Inherit;False;Property;_ColorVariationSpeed;Color Variation Speed;10;0;Create;True;0;0;0;False;0;False;-0.1;-0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;27;-656,-2272;Inherit;True;Property;_TextureSample1;Texture Sample 1;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;3;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleSubtractOpNode;371;-6639.269,-2291.87;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;52;-3776,2016;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;49;-4096,1648;Inherit;True;Property;_ColorVariationTexture;Color Variation Texture;11;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-368,-2272;Inherit;False;Cloud Noise;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;372;-6495.269,-2291.87;Inherit;False;Scene Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;50;-3840,1856;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;54;-3584,1984;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;373;-2608,-1664;Inherit;False;372;Scene Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;375;-2608,-1568;Inherit;False;Property;_EdgeFoamDistance;Edge Foam Distance;60;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-7552,96;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-4064,3136;Inherit;False;Property;_WaterfallEdgeSpeed;Waterfall Edge Speed;18;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;257;-3520,-1088;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;55;-3392,1856;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewVectorNode;58;-3296,1984;Inherit;False;Tangent;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;142;-7504,-560;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;374;-2368,-1664;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;377;-2256,-1536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;253;-4096,-1328;Inherit;False;Property;_BottomFoamSpeed;Bottom Foam Speed;38;0;Create;True;0;0;0;False;0;False;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;286;-4080,-1232;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-4288,-1936;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;297;-4160,-2032;Inherit;False;Property;_StartNoiseHarshSpeed;Start Noise Harsh Speed;45;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;153;-7376,96;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-7424,-16;Inherit;False;Property;_SmallDots2Speed;Small Dots 2 Speed;24;0;Create;True;0;0;0;False;0;False;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;78;-3808,3136;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;268;-3248,-1056;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-4304,-352;Inherit;False;Property;_WaterfallStartNoiseSpeed;Waterfall Start Noise Speed;1;0;Create;True;0;0;0;False;0;False;0.2842633;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4304,-224;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;309;-3585.854,-1749.999;Inherit;False;Property;_StartNoiseharshBottomPosition;Start Noise harsh Bottom Position;48;0;Create;True;0;0;0;False;0;False;0.4435484;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;311;-3552,-1632;Inherit;False;Property;_StartNoiseharshBottomBlend;Start Noise harsh Bottom Blend;50;0;Create;True;0;0;0;False;0;False;0.07522548;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;48;-3264,1648;Inherit;True;Property;_TextureSample2;Texture Sample 2;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;61;-3248,1872;Inherit;False;Property;_ColorVariationDepth;Color Variation Depth;12;0;Create;True;0;0;0;False;0;False;0.054;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;59;-3104,1984;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-7376,-672;Inherit;False;Property;_SmallDots1Speed;Small Dots 1 Speed;25;0;Create;True;0;0;0;False;0;False;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;143;-7328,-560;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;376;-2160,-1664;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;189;-3888,1024;Inherit;False;Property;_NoiseLinesSpeed;Noise Lines Speed;36;0;Create;True;0;0;0;False;0;False;-0.2;-0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;191;-3888,1168;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-4448,-624;Inherit;True;Property;_WaterfallStartNoise;Waterfall Start Noise;0;0;Create;True;0;0;0;False;0;False;5592d5353472a1840880192b0a423e4c;5592d5353472a1840880192b0a423e4c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;252;-3872,-1328;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;287;-3904,-1232;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;285;-3968,-1168;Inherit;False;Property;_BottomFoamDistortion;Bottom Foam Distortion;43;0;Create;True;0;0;0;False;0;False;0.05;0;0;0.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;290;-3920,-2304;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;299;-4096,-1936;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;301;-4160,-1856;Inherit;False;Property;_StartNoiseHarshDistortion;Start Noise Harsh Distortion;46;0;Create;True;0;0;0;False;0;False;0.026;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;293;-3952,-2176;Inherit;False;Property;_StartNoiseHarshTiling;Start Noise Harsh Tiling;44;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;295;-3904,-2032;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-7440,192;Inherit;False;Property;_SmallDots2Distortion;Small Dots 2 Distortion;31;0;Create;True;0;0;0;False;0;False;0.045;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;156;-7200,-16;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;157;-7248,-144;Inherit;False;Property;_SmallDots2Scale;Small Dots 2 Scale;23;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;159;-7168,96;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;154;-7264,-272;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;91;-4176,2848;Inherit;True;Property;_WaterfallEdge;Waterfall Edge;19;0;Create;True;0;0;0;False;0;False;6c6e2a23ad2379b439393259ddaff19d;6c6e2a23ad2379b439393259ddaff19d;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;79;-3680,3136;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;282;-3104,-1056;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-4192,-128;Inherit;False;Property;_WaterfallStartNoiseDistortion;Waterfall Start Noise Distortion;8;0;Create;True;0;0;0;False;0;False;0.037;0;0;0.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-4032,-352;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;41;-4128,-224;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;310;-3270.854,-1680.999;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;305;-3536,-1984;Inherit;False;Property;_StartNoiseharshTopPosition;Start Noise harsh Top Position;47;0;Create;True;0;0;0;False;0;False;0.24;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;306;-3664,-1856;Inherit;False;Property;_StartNoiseharshTopBlend;Start Noise harsh Top Blend;49;0;Create;True;0;0;0;False;0;False;0.07522548;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;47;-2928,1760;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;133;-7152,-672;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;128;-7200,-800;Inherit;False;Property;_SmallDots1Scale;Small Dots 1 Scale;22;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;141;-7392,-464;Inherit;False;Property;_SmallDots1Distortion;Small Dots 1 Distortion;30;0;Create;True;0;0;0;False;0;False;0.045;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;146;-7120,-560;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;129;-7216,-928;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;378;-2016,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;195;-3968,624;Inherit;True;Property;_NoiseLines;Noise Lines;37;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;188;-3680,1040;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;192;-3712,1168;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;194;-3744,1248;Inherit;False;Property;_NoiseLinesDistortion;Noise Lines Distortion;35;0;Create;True;0;0;0;False;0;False;0.042;0;0;0.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;246;-4096,-1472;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;288;-3680,-1232;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;254;-3696,-1360;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-3888,-1936;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;291;-3664,-2304;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;296;-3712,-2048;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;160;-7024,-272;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;162;-7008,96;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;161;-7024,-32;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-3888,2960;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;81;-3472,3136;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-2992,-1056;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-4112,-496;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;5;-3856,-352;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-3904,-224;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;304;-3168,-1760;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;307;-3296,-1920;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-2672,1840;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-6976,-928;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-6960,-560;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;202;-6992,-448;Inherit;False;Constant;_Vector7;Vector 7;45;0;Create;True;0;0;0;False;0;False;0.32,0.27;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;134;-6976,-688;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;95;-7488,-912;Inherit;True;Property;_TopVoronoi;Top Voronoi;21;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SaturateNode;379;-1856,-1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;184;-3712,752;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;190;-3488,1024;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;-3488,1184;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;251;-3488,-1456;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;294;-3472,-2176;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;163;-6832,-144;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-3296,3104;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;259;-2608,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;260;-2704,-816;Inherit;False;Property;_BottomFoamExtendMin;Bottom Foam Extend Min;40;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;261;-2736,-736;Inherit;False;Property;_BottomFoamExtendMax;Bottom Foam Extend Max;41;0;Create;True;0;0;0;False;0;False;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;281;-2832,-1056;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;283;-3072,-944;Inherit;False;Property;_BottomFoamWidthPow;Bottom Foam Width Pow;39;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-3520,-384;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewVectorNode;235;-3728,-96;Inherit;False;Tangent;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-3712,96;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-3776,224;Inherit;False;Property;_WaterfallStartNoisePosition;Waterfall Start Noise Position;2;0;Create;True;0;0;0;False;0;False;0.22;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;303;-3168,-1984;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;308;-2976,-1760;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;57;-2512,1680;Inherit;True;Property;_TextureSample3;Texture Sample 3;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;131;-6784,-800;Inherit;False;4;4;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;380;-1680,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;187;-3280,848;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;247;-3200,-1440;Inherit;True;Property;_TextureSample9;Texture Sample 9;48;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;289;-3248,-2192;Inherit;True;Property;_TextureSample10;Texture Sample 10;55;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;150;-6512,-400;Inherit;True;Property;_TextureSample6;Texture Sample 5;26;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;83;-3216,2848;Inherit;True;Property;_T_WaterfalEdgeOpacity;T_WaterfalEdgeOpacity;14;0;Create;True;0;0;0;False;0;False;-1;6c6e2a23ad2379b439393259ddaff19d;6c6e2a23ad2379b439393259ddaff19d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SmoothstepOpNode;258;-2432,-896;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;330;-2672,-1056;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-3216,-624;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.NormalizeNode;236;-3536,-96;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;237;-3632,-208;Inherit;False;Property;_WaterfallStartNoiseDepth;Waterfall Start Noise Depth;9;0;Create;True;0;0;0;False;0;False;0.037;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;15;-3424,160;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;-3195.621,1182.456;Inherit;False;Property;_NoiseLinesReveal;Noise Lines Reveal;34;0;Create;True;0;0;0;False;0;False;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;164;-6224,-384;Inherit;False;Property;_SmallDots2Step;Small Dots 2 Step;27;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;312;-2848,-1888;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;125;-6528,-880;Inherit;True;Property;_TextureSample5;Texture Sample 5;26;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;182;-3120,624;Inherit;True;Property;_TextureSample7;Texture Sample 7;39;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;137;-6224,-896;Inherit;False;Property;_SmallDots1Step;Small Dots 1 Step;26;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;85;-2848,2784;Inherit;False;2;0;FLOAT;0.6;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;92;-2720,2848;Inherit;False;Property;_WaterfallEdgeFoamOpacity;Waterfall Edge Foam Opacity;20;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;279;-2224,-1104;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;263;-2352,-1216;Inherit;False;Property;_BottomFoamStep;Bottom Foam Step;42;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;234;-3344,-208;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;204;-2944,1088;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;-3008,976;Inherit;False;Property;_NoiseLinesOpacity;Noise Lines Opacity;33;0;Create;True;0;0;0;False;0;False;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;16;-3264,160;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-3424,352;Inherit;False;Property;_WaterfallStartNoisePow;Waterfall Start Noise Pow;5;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-3424,272;Inherit;False;Property;_WaterfallStartNoiseExtend;Waterfall Start Noise Extend;4;0;Create;True;0;0;0;False;0;False;0.22;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;165;-5936,-352;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;166;-6064,-240;Inherit;False;Property;_SmallDots2Opacity;Small Dots 2 Opacity;29;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;313;-2658.065,-2068.372;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;315;-2767.15,-2251.955;Inherit;False;Property;_StartNoiseHarshStep;Start Noise Harsh Step;51;0;Create;True;0;0;0;False;0;False;0.51;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-1584,-1824;Inherit;False;Property;_EdgeFoamStep;Edge Foam Step;62;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;385;-1536,-1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;136;-5936,-864;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-6064,-752;Inherit;False;Property;_SmallDots1Opacity;Small Dots 1 Opacity;28;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-2432,2784;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;262;-2048,-1168;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;243;-3112.361,-374.8425;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;177;-2704,896;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2480,1888;Inherit;False;Property;_ColorVariationContrast;Color Variation Contrast;17;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;19;-3104,160;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;327;-3056,304;Inherit;False;Property;_WaterfallStartNoiseOpacity;Waterfall Start Noise Opacity;3;0;Create;True;0;0;0;False;0;False;0.22;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;-5792,-352;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;314;-2464,-2144;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;386;-1312,-1744;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;381;-1408,-1632;Inherit;False;Property;_EdgeFoamOpacity;Edge Foam Opacity;61;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-5792,-864;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;-2256,2784;Inherit;False;Watefal Edge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;264;-1936,-1168;Inherit;False;Bottom Foam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;233;-2912,-320;Inherit;True;Property;_TextureSample8;Texture Sample 8;46;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;198;-2544,896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-2640,1008;Inherit;False;Property;_NoiseLinesPow;Noise Lines Pow;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;75;-2208,1760;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;328;-2768,192;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;65;-2448,1472;Inherit;False;Property;_Color1;Color 1;13;0;Create;True;0;0;0;False;0;False;0,0.8273995,1,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;66;-2448,1248;Inherit;False;Property;_Color2;Color 2;16;0;Create;True;0;0;0;False;0;False;0,0.5989819,0.8867924,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-5632,-352;Inherit;False;Small Dots 2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;316;-2304,-2144;Inherit;False;Start Noise Harsh;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;388;-1184,-1712;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;139;-5632,-864;Inherit;False;Small Dots 1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-912,-1472;Inherit;False;264;Bottom Foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-912,-1376;Inherit;False;87;Watefal Edge;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-2549.641,49.97424;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;62;-2096,1408;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;196;-2368,896;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;201;-2448,1040;Inherit;False;Property;_Color3;Color 3;14;0;Create;True;0;0;0;False;0;False;0,0.5989819,0.8867924,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;171;-896,-1008;Inherit;False;168;Small Dots 2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;180;-928,-1120;Inherit;False;316;Start Noise Harsh;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;382;-1024,-1712;Inherit;False;Edge Foam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;140;-912,-1200;Inherit;False;139;Small Dots 1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;147;-656,-1424;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;239;-2320,320;Inherit;False;Property;_Color4;Color 4;15;0;Create;True;0;0;0;False;0;False;0,0.5989819,0.8867924,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;242;-2336,48;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;199;-2001.13,1112.294;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;333;-1520,512;Inherit;False;Property;_NormalStrength;Normal Strength;55;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;169;-656,-1120;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-672,-992;Inherit;False;382;Edge Foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;149;-512,-1344;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;238;-1968,256;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;334;-1152,576;Inherit;False;Normal From Texture;-1;;2;9728ee98a55193249b513caf9a0f1676;13,149,1,147,1,143,1,141,1,139,1,151,1,137,1,153,1,159,1,157,1,155,1,135,1,108,1;4;87;SAMPLER2D;0;False;85;FLOAT2;0,0;False;74;SAMPLERSTATE;0;False;91;FLOAT;1.5;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;383;-496,-1088;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;181;-352,-1200;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;84;-2848,2944;Inherit;False;2;0;FLOAT;0.1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-1808,256;Inherit;False;Water Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;366;-848,576;Inherit;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;317;-1120,-128;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;325;-1216,96;Inherit;False;Property;_BottomOpacityCutout;Bottom Opacity Cutout;53;0;Create;True;0;0;0;False;0;False;0.9;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;321;-1184,0;Inherit;False;Property;_InitialOpacityGradience;Initial Opacity Gradience;52;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-224,-1200;Inherit;False;Foam Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-2704,2944;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;319;-864,-96;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;323;-832,16;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;337;-960,1584;Inherit;False;73;Water Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;338;-1056,1664;Inherit;False;Property;_DebugWaterColor;DebugWaterColor;57;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;339;-1056,1824;Inherit;False;Property;_DebugNormals;DebugNormals;58;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;340;-960,1744;Inherit;False;366;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;347;-960,1904;Inherit;False;37;Cloud Noise;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;348;-1056,1984;Inherit;False;Property;_DebugCloudNoise;Debug Cloud Noise;59;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-560,-768;Inherit;False;73;Water Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-560,-672;Inherit;False;71;Foam Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;320;-704,-96;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;324;-704,16;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;-736,-192;Inherit;False;93;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;349;-768,1584;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;350;-768,1744;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;354;-768,1904;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;67;-368,-768;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;331;-592,-544;Inherit;False;Property;_Smoothness;Smoothness;54;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;355;-496.6146,1684.933;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;356;-384.6146,1396.933;Inherit;False;Constant;_Float2;Float 2;71;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-227.2147,-639.9574;Inherit;False;Constant;_Float0;Float 0;71;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;363;-480,-416;Inherit;False;Constant;_Float3;Float 3;73;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;365;-256,-352;Inherit;False;Constant;_Vector0;Vector 0;74;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;367;-256,-416;Inherit;False;366;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;318;-528,-128;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;361;-384,-64;Inherit;False;Constant;_Float1;Float 1;72;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;332;-1136,416;Inherit;False;Normal From Height;-1;;3;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;358;-176,-768;Inherit;False;Property;_DEBUG;DEBUG;56;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;357;-224.6146,1412.933;Inherit;False;Property;_DEBUG5;DEBUG;56;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;358;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;362;-192,-512;Inherit;False;Property;_DEBUG7;DEBUG;56;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;358;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;364;-48,-384;Inherit;False;Property;_DEBUG8;DEBUG;56;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;358;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;360;-224,-128;Inherit;False;Property;_DEBUG6;DEBUG;56;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;358;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;320,-688;Float;False;True;-1;3;Polyart.WaterfallMaterialCustomInspector;0;0;Standard;NewWaterfall;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;32;0;34;0
WireConnection;28;2;26;0
WireConnection;33;0;32;0
WireConnection;29;0;28;0
WireConnection;29;1;33;0
WireConnection;27;0;26;0
WireConnection;27;1;29;0
WireConnection;27;7;26;1
WireConnection;371;0;370;0
WireConnection;371;1;369;4
WireConnection;52;0;56;0
WireConnection;37;0;27;0
WireConnection;372;0;371;0
WireConnection;50;2;49;0
WireConnection;54;1;52;0
WireConnection;55;0;50;0
WireConnection;55;1;54;0
WireConnection;374;0;373;0
WireConnection;374;1;375;0
WireConnection;377;0;375;0
WireConnection;153;0;151;0
WireConnection;78;0;77;0
WireConnection;268;0;257;1
WireConnection;48;0;49;0
WireConnection;48;1;55;0
WireConnection;48;7;49;1
WireConnection;59;0;58;0
WireConnection;143;0;142;0
WireConnection;376;0;374;0
WireConnection;376;1;377;0
WireConnection;252;0;253;0
WireConnection;287;0;286;0
WireConnection;299;0;298;0
WireConnection;295;0;297;0
WireConnection;156;0;152;0
WireConnection;159;0;153;0
WireConnection;79;0;78;0
WireConnection;282;0;268;0
WireConnection;3;0;1;0
WireConnection;41;0;39;0
WireConnection;310;0;309;0
WireConnection;310;1;311;0
WireConnection;47;0;48;2
WireConnection;47;1;61;0
WireConnection;47;2;59;0
WireConnection;133;0;132;0
WireConnection;146;0;143;0
WireConnection;378;0;376;0
WireConnection;188;0;189;0
WireConnection;192;0;191;0
WireConnection;246;2;2;0
WireConnection;288;0;287;0
WireConnection;288;1;285;0
WireConnection;254;1;252;0
WireConnection;300;0;299;0
WireConnection;300;1;301;0
WireConnection;291;0;290;0
WireConnection;291;1;293;0
WireConnection;296;1;295;0
WireConnection;160;0;154;0
WireConnection;160;1;157;0
WireConnection;162;0;159;0
WireConnection;162;1;158;0
WireConnection;161;1;156;0
WireConnection;80;2;91;0
WireConnection;81;1;79;0
WireConnection;329;0;282;0
WireConnection;4;2;2;0
WireConnection;5;1;3;0
WireConnection;42;0;41;0
WireConnection;42;1;43;0
WireConnection;304;0;290;2
WireConnection;304;1;309;0
WireConnection;304;2;310;0
WireConnection;307;0;305;0
WireConnection;307;1;306;0
WireConnection;60;0;47;0
WireConnection;60;1;55;0
WireConnection;127;0;129;0
WireConnection;127;1;128;0
WireConnection;145;0;146;0
WireConnection;145;1;141;0
WireConnection;134;1;133;0
WireConnection;379;0;378;0
WireConnection;184;2;195;0
WireConnection;190;1;188;0
WireConnection;193;0;192;0
WireConnection;193;1;194;0
WireConnection;251;0;246;0
WireConnection;251;1;254;0
WireConnection;251;2;288;0
WireConnection;294;0;291;0
WireConnection;294;1;296;0
WireConnection;294;2;300;0
WireConnection;163;0;160;0
WireConnection;163;1;161;0
WireConnection;163;2;162;0
WireConnection;82;0;80;0
WireConnection;82;1;81;0
WireConnection;259;0;257;2
WireConnection;281;0;329;0
WireConnection;6;0;4;0
WireConnection;6;1;5;0
WireConnection;6;2;42;0
WireConnection;303;0;290;2
WireConnection;303;1;305;0
WireConnection;303;2;307;0
WireConnection;308;0;304;0
WireConnection;57;0;49;0
WireConnection;57;1;60;0
WireConnection;57;7;49;1
WireConnection;131;0;127;0
WireConnection;131;1;134;0
WireConnection;131;2;145;0
WireConnection;131;3;202;0
WireConnection;380;0;379;0
WireConnection;380;1;57;2
WireConnection;187;0;184;0
WireConnection;187;1;190;0
WireConnection;187;2;193;0
WireConnection;247;0;2;0
WireConnection;247;1;251;0
WireConnection;247;7;2;1
WireConnection;289;0;2;0
WireConnection;289;1;294;0
WireConnection;150;0;95;0
WireConnection;150;1;163;0
WireConnection;150;7;95;1
WireConnection;83;0;91;0
WireConnection;83;1;82;0
WireConnection;83;7;91;1
WireConnection;258;0;259;0
WireConnection;258;1;261;0
WireConnection;258;2;260;0
WireConnection;330;0;281;0
WireConnection;330;1;283;0
WireConnection;7;0;2;0
WireConnection;7;1;6;0
WireConnection;7;7;2;1
WireConnection;236;0;235;0
WireConnection;15;0;17;2
WireConnection;15;1;18;0
WireConnection;312;0;303;0
WireConnection;312;1;308;0
WireConnection;125;0;95;0
WireConnection;125;1;131;0
WireConnection;125;7;95;1
WireConnection;182;0;195;0
WireConnection;182;1;187;0
WireConnection;182;7;195;1
WireConnection;85;1;83;1
WireConnection;279;0;247;2
WireConnection;279;1;258;0
WireConnection;279;2;330;0
WireConnection;234;0;7;2
WireConnection;234;1;237;0
WireConnection;234;2;236;0
WireConnection;204;0;48;2
WireConnection;204;1;205;0
WireConnection;16;0;15;0
WireConnection;165;0;164;0
WireConnection;165;1;150;2
WireConnection;313;0;289;2
WireConnection;313;1;312;0
WireConnection;385;0;380;0
WireConnection;136;0;137;0
WireConnection;136;1;125;2
WireConnection;86;0;85;0
WireConnection;86;1;92;0
WireConnection;262;0;263;0
WireConnection;262;1;279;0
WireConnection;243;0;6;0
WireConnection;243;1;234;0
WireConnection;177;0;182;2
WireConnection;177;1;178;0
WireConnection;177;2;204;0
WireConnection;19;0;16;0
WireConnection;19;1;22;0
WireConnection;19;2;25;0
WireConnection;167;0;165;0
WireConnection;167;1;166;0
WireConnection;314;0;315;0
WireConnection;314;1;313;0
WireConnection;386;0;387;0
WireConnection;386;1;385;0
WireConnection;135;0;136;0
WireConnection;135;1;138;0
WireConnection;87;0;86;0
WireConnection;264;0;262;0
WireConnection;233;0;2;0
WireConnection;233;1;243;0
WireConnection;233;7;2;1
WireConnection;198;0;177;0
WireConnection;75;0;57;2
WireConnection;75;1;76;0
WireConnection;328;0;19;0
WireConnection;328;1;327;0
WireConnection;168;0;167;0
WireConnection;316;0;314;0
WireConnection;388;0;386;0
WireConnection;388;1;381;0
WireConnection;139;0;135;0
WireConnection;245;0;233;2
WireConnection;245;1;328;0
WireConnection;62;0;66;0
WireConnection;62;1;65;0
WireConnection;62;2;75;0
WireConnection;196;0;198;0
WireConnection;196;1;197;0
WireConnection;382;0;388;0
WireConnection;147;0;69;0
WireConnection;147;1;88;0
WireConnection;242;0;245;0
WireConnection;199;0;62;0
WireConnection;199;1;201;0
WireConnection;199;2;196;0
WireConnection;169;0;180;0
WireConnection;169;1;171;0
WireConnection;149;0;147;0
WireConnection;149;1;140;0
WireConnection;238;0;199;0
WireConnection;238;1;239;5
WireConnection;238;2;242;0
WireConnection;334;87;49;0
WireConnection;334;85;55;0
WireConnection;334;74;49;1
WireConnection;334;91;333;0
WireConnection;383;0;169;0
WireConnection;383;1;384;0
WireConnection;181;0;149;0
WireConnection;181;1;383;0
WireConnection;84;1;83;1
WireConnection;73;0;238;0
WireConnection;366;0;334;40
WireConnection;71;0;181;0
WireConnection;93;0;84;0
WireConnection;319;0;317;2
WireConnection;319;1;321;0
WireConnection;323;0;325;0
WireConnection;323;1;317;2
WireConnection;320;0;319;0
WireConnection;324;0;323;0
WireConnection;349;0;337;0
WireConnection;349;1;338;0
WireConnection;350;0;340;0
WireConnection;350;1;339;0
WireConnection;354;0;347;0
WireConnection;354;1;348;0
WireConnection;67;0;74;0
WireConnection;67;2;72;0
WireConnection;355;0;349;0
WireConnection;355;1;350;0
WireConnection;355;2;354;0
WireConnection;318;0;94;0
WireConnection;318;1;320;0
WireConnection;318;2;324;0
WireConnection;332;20;57;2
WireConnection;332;110;333;0
WireConnection;358;1;67;0
WireConnection;358;0;359;0
WireConnection;357;1;356;0
WireConnection;357;0;355;0
WireConnection;362;1;331;0
WireConnection;362;0;363;0
WireConnection;364;1;367;0
WireConnection;364;0;365;0
WireConnection;360;1;318;0
WireConnection;360;0;361;0
WireConnection;0;0;358;0
WireConnection;0;1;364;0
WireConnection;0;2;357;0
WireConnection;0;4;362;0
WireConnection;0;9;360;0
ASEEND*/
//CHKSM=01082C3E8650090E2861FE8D82A3BBDCEB2829D4