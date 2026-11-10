// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Foam"
{
	Properties
	{
		_WPO1Intensity("WPO 1 Intensity", Range( 0 , 10)) = 0.007
		_Color0("Color 0", Color) = (1,1,1,0)
		_WPO2Intensity1("WPO 2 Intensity", Range( 0 , 10)) = 0.0174
		_Color1("Color 1", Color) = (0.256675,0.4971698,0.5283019,0)
		_WPOSpeed2("WPO Speed 2", Float) = -1
		_WPO1Speed("WPO 1 Speed", Float) = -1
		_LightDir("Light Dir", Range( -1 , 1)) = 0
		_VerticalMask("Vertical Mask", Range( -1 , 1)) = 0
		_LightBlending("Light Blending", Range( -1 , 1)) = 0.1
		_Texture0("Texture 0", 2D) = "white" {}
		_Texture1("Texture 1", 2D) = "white" {}
		_NormalStrength("Normal Strength", Float) = 1
		_NormalStrength1("Normal Strength", Float) = 1
		_WPO2SubractValue("WPO 2 Subract Value", Range( 0 , 1)) = 0
		_WPO1SubractValue("WPO 1 Subract Value", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
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
			float3 worldPos;
			float4 ase_positionOS4f;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Texture0;
		uniform float4 _Texture0_ST;
		uniform float _WPO1Speed;
		uniform float _WPO1SubractValue;
		uniform float _WPO1Intensity;
		uniform sampler2D _Texture1;
		uniform float4 _Texture1_ST;
		uniform float _WPOSpeed2;
		uniform float _WPO2SubractValue;
		uniform float _WPO2Intensity1;
		uniform float4 _Color1;
		uniform float4 _Color0;
		uniform float _LightDir;
		uniform float _LightBlending;
		uniform float _NormalStrength;
		uniform float _NormalStrength1;
		uniform float _VerticalMask;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_Texture0 = v.texcoord.xy * _Texture0_ST.xy + _Texture0_ST.zw;
			float mulTime157 = _Time.y * _WPO1Speed;
			float2 appendResult158 = (float2(0.0 , mulTime157));
			float4 tex2DNode3 = tex2Dlod( _Texture0, float4( ( uv_Texture0 + appendResult158 ), 0, 0.0) );
			float3 ase_normalOS = v.normal.xyz;
			float2 uv_Texture1 = v.texcoord.xy * _Texture1_ST.xy + _Texture1_ST.zw;
			float mulTime203 = _Time.y * _WPOSpeed2;
			float2 appendResult211 = (float2(0.0 , mulTime203));
			float4 tex2DNode217 = tex2Dlod( _Texture1, float4( ( uv_Texture1 + appendResult211 ), 0, 0.0) );
			v.vertex.xyz += ( ( ( ( tex2DNode3.g - _WPO1SubractValue ) * _WPO1Intensity * ase_normalOS ) + ( ( tex2DNode217.g - _WPO2SubractValue ) * ase_normalOS * _WPO2Intensity1 ) ) * saturate( ( ( 1.0 - abs( ( ( v.texcoord.xy.y - 0.5 ) * 2.0 ) ) ) * 4.0 ) ) );
			v.vertex.w = 1;
			float4 ase_positionOS4f = v.vertex;
			o.ase_positionOS4f = ase_positionOS4f;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float4 ase_positionOS4f = i.ase_positionOS4f;
			float3 ase_lightDirOS = normalize( ObjSpaceLightDir( ase_positionOS4f ) );
			float2 uv_Texture0 = i.uv_texcoord * _Texture0_ST.xy + _Texture0_ST.zw;
			float mulTime157 = _Time.y * _WPO1Speed;
			float2 appendResult158 = (float2(0.0 , mulTime157));
			float4 tex2DNode3 = tex2D( _Texture0, ( uv_Texture0 + appendResult158 ) );
			float2 temp_cast_0 = (tex2DNode3.g).xx;
			float mulTime163 = _Time.y * _WPO1Speed;
			float2 appendResult165 = (float2(0.0 , mulTime163));
			float2 appendResult168 = (float2(0.0169 , 0.0));
			float mulTime174 = _Time.y * _WPO1Speed;
			float2 appendResult176 = (float2(0.0 , mulTime174));
			float2 appendResult177 = (float2(0.0 , 0.0169));
			float2 appendResult171 = (float2(tex2D( _Texture0, ( uv_Texture0 + appendResult165 + appendResult168 ) ).g , tex2D( _Texture0, ( uv_Texture0 + appendResult176 + appendResult177 ) ).g));
			float2 temp_output_170_0 = ( temp_cast_0 - appendResult171 );
			float2 temp_cast_1 = (tex2DNode3.g).xx;
			float2 temp_cast_2 = (tex2DNode3.g).xx;
			float dotResult189 = dot( temp_output_170_0 , temp_output_170_0 );
			float3 appendResult193 = (float3(temp_output_170_0 , sqrt( ( 1.0 - saturate( dotResult189 ) ) )));
			float3 normalizeResult184 = normalize( appendResult193 );
			float3 lerpResult181 = lerp( float3( 0,0,1 ) , normalizeResult184 , _NormalStrength);
			float2 uv_Texture1 = i.uv_texcoord * _Texture1_ST.xy + _Texture1_ST.zw;
			float mulTime203 = _Time.y * _WPOSpeed2;
			float2 appendResult211 = (float2(0.0 , mulTime203));
			float4 tex2DNode217 = tex2D( _Texture1, ( uv_Texture1 + appendResult211 ) );
			float2 temp_cast_3 = (tex2DNode217.g).xx;
			float mulTime200 = _Time.y * _WPOSpeed2;
			float2 appendResult206 = (float2(0.0 , mulTime200));
			float2 appendResult204 = (float2(0.0169 , 0.0));
			float mulTime201 = _Time.y * _WPOSpeed2;
			float2 appendResult208 = (float2(0.0 , mulTime201));
			float2 appendResult209 = (float2(0.0 , 0.0169));
			float2 appendResult218 = (float2(tex2D( _Texture1, ( uv_Texture1 + appendResult206 + appendResult204 ) ).g , tex2D( _Texture1, ( uv_Texture1 + appendResult208 + appendResult209 ) ).g));
			float2 temp_output_219_0 = ( temp_cast_3 - appendResult218 );
			float2 temp_cast_4 = (tex2DNode217.g).xx;
			float2 temp_cast_5 = (tex2DNode217.g).xx;
			float dotResult220 = dot( temp_output_219_0 , temp_output_219_0 );
			float3 appendResult195 = (float3(temp_output_219_0 , sqrt( ( 1.0 - saturate( dotResult220 ) ) )));
			float3 normalizeResult196 = normalize( appendResult195 );
			float3 lerpResult197 = lerp( float3( 0,0,1 ) , normalizeResult196 , _NormalStrength1);
			float3 ase_normalWS = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_tangentWS = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_bitangentWS = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_tangentWS.x,ase_bitangentWS.x,ase_normalWS.x,ase_tangentWS.y,ase_bitangentWS.y,ase_normalWS.y,ase_tangentWS.z,ase_bitangentWS.z,ase_normalWS.z);
			float3 tangentToWorldDir233 = mul( ase_tangentToWorldFast, BlendNormals( lerpResult181 , lerpResult197 ) );
			float3 normalizeResult232 = normalize( tangentToWorldDir233 );
			float dotResult34 = dot( -ase_lightDirOS , normalizeResult232 );
			float smoothstepResult35 = smoothstep( _LightDir , ( _LightDir + _LightBlending ) , dotResult34);
			float dotResult38 = dot( normalizeResult232 , float3(0,1,0) );
			float smoothstepResult40 = smoothstep( _VerticalMask , ( _VerticalMask + _LightBlending ) , dotResult38);
			float3 lerpResult47 = lerp( _Color1.rgb , _Color0.rgb , max( smoothstepResult35 , smoothstepResult40 ));
			o.Emission = lerpResult47;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float2 customPack2 : TEXCOORD2;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xyzw = customInputData.ase_positionOS4f;
				o.customPack2.xy = customInputData.uv_texcoord;
				o.customPack2.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.ase_positionOS4f = IN.customPack1.xyzw;
				surfIN.uv_texcoord = IN.customPack2.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.RangedFloatNode;159;-4128,-1376;Inherit;False;Property;_WPO1Speed;WPO 1 Speed;9;0;Create;True;0;0;0;False;0;False;-1;-0.48;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-4176,64;Inherit;False;Property;_WPOSpeed2;WPO Speed 2;8;0;Create;True;0;0;0;False;0;False;-1;-0.18;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;154;-4096,-1664;Inherit;True;Property;_Texture0;Texture 0;19;0;Create;True;0;0;0;False;0;False;None;f40728e0bbda94a4ab38de34275e31e9;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;163;-3968,-1040;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;174;-3920,-592;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;199;-4144,-224;Inherit;True;Property;_Texture1;Texture 1;20;0;Create;True;0;0;0;False;0;False;None;f40728e0bbda94a4ab38de34275e31e9;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;200;-4016,400;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;201;-3968,848;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-4256,528;Inherit;False;Constant;_Float2;Float 2;19;0;Create;True;0;0;0;False;0;False;0.0169;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-4224,-896;Inherit;False;Constant;_Float1;Float 1;18;0;Create;True;0;0;0;False;0;False;0.0169;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;157;-3952,-1376;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;168;-3920,-912;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;164;-3872,-1216;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;165;-3776,-1056;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;175;-3824,-768;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;176;-3728,-608;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;177;-3888,-464;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;203;-4000,64;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;204;-3968,528;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;205;-3920,224;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;206;-3824,384;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;207;-3872,672;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;208;-3776,832;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;209;-3936,976;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;155;-3856,-1552;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;158;-3760,-1392;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-3632,-1168;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-3584,-720;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;210;-3904,-112;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;211;-3808,48;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;212;-3680,272;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;213;-3632,720;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;156;-3616,-1504;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;161;-3472,-1184;Inherit;True;Property;_TextureSample3;Texture Sample 3;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;173;-3424,-736;Inherit;True;Property;_TextureSample4;Texture Sample 3;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;214;-3664,-64;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;215;-3520,256;Inherit;True;Property;_TextureSample5;Texture Sample 3;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;216;-3472,704;Inherit;True;Property;_TextureSample6;Texture Sample 3;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;3;-3472,-1648;Inherit;True;Property;_Noise1;Noise1;2;0;Create;True;0;0;0;False;0;False;-1;5fa555fb8d3ad694cabefbd47cb9970d;5fa555fb8d3ad694cabefbd47cb9970d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DynamicAppendNode;171;-3104,-1056;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;217;-3520,-208;Inherit;True;Property;_Noise2;Noise1;2;0;Create;True;0;0;0;False;0;False;-1;5fa555fb8d3ad694cabefbd47cb9970d;5fa555fb8d3ad694cabefbd47cb9970d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DynamicAppendNode;218;-3152,384;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;170;-2848,-1152;Inherit;False;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;219;-2896,288;Inherit;False;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;189;-2672,-1184;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;220;-2720,256;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;190;-2544,-1184;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;221;-2592,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;191;-2384,-1184;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;222;-2432,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;192;-2224,-1184;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;194;-2272,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;195;-2112,208;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;193;-2096,-1152;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;196;-1936,352;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;228;-2000,432;Inherit;False;Property;_NormalStrength1;Normal Strength;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;184;-1952,-1152;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;182;-2016,-1056;Inherit;False;Property;_NormalStrength;Normal Strength;21;0;Create;True;0;0;0;False;0;False;1;5.34;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;197;-1760,320;Inherit;False;3;0;FLOAT3;0,0,1;False;1;FLOAT3;0,0,1;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;236;-2432,-224;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;181;-1776,-1152;Inherit;False;3;0;FLOAT3;0,0,1;False;1;FLOAT3;0,0,1;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;231;-1648,-208;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;238;-2238.315,-64.16473;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjSpaceLightDirHlpNode;33;-1120,-800;Inherit;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;237;-2096,-64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;233;-1360,-208;Inherit;False;Tangent;World;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;36;-784,-512;Inherit;False;Property;_LightDir;Light Dir;15;0;Create;True;0;0;0;False;0;False;0;0.598;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-800,-368;Inherit;False;Property;_LightBlending;Light Blending;17;0;Create;True;0;0;0;False;0;False;0.1;0.02;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-344.6542,-219.0604;Inherit;False;Property;_VerticalMask;Vertical Mask;16;0;Create;True;0;0;0;False;0;False;0;0.24;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;39;-416,128;Inherit;False;Constant;_Vector0;Vector 0;9;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.AbsOpNode;239;-1952,-64;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;-3216,-320;Inherit;False;Property;_WPO2SubractValue;WPO 2 Subract Value;23;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;246;-3472,-1456;Inherit;False;Property;_WPO1SubractValue;WPO 1 Subract Value;24;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;232;-1088,-208;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;234;-848,-800;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;224;-3024,80;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;38;-192,16;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;34;-560,-624;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-480,-448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-208,-112;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;229;-2992,-48;Inherit;False;Property;_WPO2Intensity1;WPO 2 Intensity;4;0;Create;True;0;0;0;False;0;False;0.0174;0.98;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;225;-2944,-272;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;242;-2176,-176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;-3072,-1568;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;147;-3136,-1456;Inherit;False;Property;_WPO1Intensity;WPO 1 Intensity;0;0;Create;True;0;0;0;False;0;False;0.007;2.11;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;149;-3040,-1376;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;40;-16.57495,7.671814;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;35;-368,-544;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;-2688,-128;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;-2032,-192;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-2800,-1568;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;2;-48,-1008;Inherit;False;Property;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;48;-48,-800;Inherit;False;Property;_Color1;Color 1;5;0;Create;True;0;0;0;False;0;False;0.256675,0.4971698,0.5283019,0;0.6944642,0.9202456,0.9622641,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;230;-2256,-496;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;241;-1888,-192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;42;-144,-544;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;243;-2720,96;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;160;-1058.976,-407.4517;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;47;224,-592;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;0,224;Inherit;False;67;Pos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;140;108.2722,-245.522;Inherit;False;Constant;_Float0;Float 0;17;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;74;-7376,2208;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;75;-7152,2208;Inherit;False;World;Tangent;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;78;-7088,2368;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;126;-7072,2928;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-6112,1936;Inherit;False;Property;_WPO2Speed;WPO 2 Speed;11;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-6880,2208;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.01,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;125;-6896,2912;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;25;-5936,1936;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;21;-6384,1744;Inherit;False;Property;_WPO2PositionMultiplier;WPO 2Position Multiplier;13;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;12;-6672,1392;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;83;-6320,2576;Inherit;False;Property;_WPO2PositionMultiplier1;WPO 2Position Multiplier;14;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;124;-6704,2208;Inherit;False;Tangent;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;127;-6704,2912;Inherit;False;Tangent;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;105;-6288,3232;Inherit;False;Property;_WPO2PositionMultiplier2;WPO 2Position Multiplier;12;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;17;-6352,1520;Inherit;False;Property;_WPOSpeed;WPO Speed;10;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;16;-6176,1520;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-5888,1712;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;-5712,1872;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-5824,2544;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;86;-5648,2704;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;104;-6224,2240;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-5904,3200;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;107;-5728,3360;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;121;-6304,2912;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-5936,1440;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-5680,1712;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-5872,2272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-5616,2544;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;-5696,3200;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-5968,2928;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-5888,1584;Inherit;False;Property;_WPOScale;WPO Scale;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;-5792,1408;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;23;-5520,1712;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;90;-5728,2240;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;91;-5456,2544;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;110;-5808,2896;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;111;-5536,3200;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;10;-5632,1440;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;31;-5280,1664;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;92;-5568,2272;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;95;-5216,2496;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;112;-5648,2928;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;114;-5296,3152;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-5440,1568;Inherit;False;Property;_WPOIntensity;WPO Intensity;1;0;Create;True;0;0;0;False;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;32;-5152,1536;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;7;-5280,1456;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-5184,1776;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;97;-5216,2288;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;98;-5120,2608;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;116;-5296,2944;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;117;-5200,3264;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;115;-5232,3440;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;96;-5168,2752;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-5104,1456;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-4960,1792;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-5040,2288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-4896,2624;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-5120,2944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;-4976,3280;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-4928,1632;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;133;-4592,1936;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;101;-4864,2464;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;120;-4944,3120;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;141;-4720,2704;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;142;-4800,2944;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-4432,1600;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;103;-4464,2464;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;123;-4624,3136;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;128;-4144,2064;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;129;-4112,2560;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;137;-3968,2560;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;136;-3984,2048;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-4272,1600;Inherit;False;Pos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CrossProductOpNode;138;-3744,2304;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;132;-3520,2288;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalizeNode;139;-3600,2176;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-7264,2368;Inherit;False;Property;_DeviationOffset;Deviation Offset;18;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;227;-2624,432;Inherit;False;Normal Reconstruct Z;-1;;4;63ba85b764ae0c84ab3d698b86364ae9;1,15,1;1;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-5504,1920;Inherit;False;Property;_WPO2Intensity;WPO 2 Intensity;6;0;Create;True;0;0;0;False;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;235;-2053.376,-401.5244;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;179;-2672,-1088;Inherit;False;Normal Reconstruct Z;-1;;5;63ba85b764ae0c84ab3d698b86364ae9;1,15,1;1;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;336,-80;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;Unlit;Foam;False;False;False;False;True;True;True;True;True;True;True;True;False;False;False;True;False;False;False;False;False;Back;1;False;;0;False;;False;1;False;;-1;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;2;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;163;0;159;0
WireConnection;174;0;159;0
WireConnection;200;0;198;0
WireConnection;201;0;198;0
WireConnection;157;0;159;0
WireConnection;168;0;167;0
WireConnection;164;2;154;0
WireConnection;165;1;163;0
WireConnection;175;2;154;0
WireConnection;176;1;174;0
WireConnection;177;1;167;0
WireConnection;203;0;198;0
WireConnection;204;0;202;0
WireConnection;205;2;199;0
WireConnection;206;1;200;0
WireConnection;207;2;199;0
WireConnection;208;1;201;0
WireConnection;209;1;202;0
WireConnection;155;2;154;0
WireConnection;158;1;157;0
WireConnection;166;0;164;0
WireConnection;166;1;165;0
WireConnection;166;2;168;0
WireConnection;172;0;175;0
WireConnection;172;1;176;0
WireConnection;172;2;177;0
WireConnection;210;2;199;0
WireConnection;211;1;203;0
WireConnection;212;0;205;0
WireConnection;212;1;206;0
WireConnection;212;2;204;0
WireConnection;213;0;207;0
WireConnection;213;1;208;0
WireConnection;213;2;209;0
WireConnection;156;0;155;0
WireConnection;156;1;158;0
WireConnection;161;0;154;0
WireConnection;161;1;166;0
WireConnection;161;7;154;1
WireConnection;173;0;154;0
WireConnection;173;1;172;0
WireConnection;173;7;154;1
WireConnection;214;0;210;0
WireConnection;214;1;211;0
WireConnection;215;0;199;0
WireConnection;215;1;212;0
WireConnection;215;7;199;1
WireConnection;216;0;199;0
WireConnection;216;1;213;0
WireConnection;216;7;199;1
WireConnection;3;0;154;0
WireConnection;3;1;156;0
WireConnection;3;7;154;1
WireConnection;171;0;161;2
WireConnection;171;1;173;2
WireConnection;217;0;199;0
WireConnection;217;1;214;0
WireConnection;217;7;199;1
WireConnection;218;0;215;2
WireConnection;218;1;216;2
WireConnection;170;0;3;2
WireConnection;170;1;171;0
WireConnection;219;0;217;2
WireConnection;219;1;218;0
WireConnection;189;0;170;0
WireConnection;189;1;170;0
WireConnection;220;0;219;0
WireConnection;220;1;219;0
WireConnection;190;0;189;0
WireConnection;221;0;220;0
WireConnection;191;0;190;0
WireConnection;222;0;221;0
WireConnection;192;0;191;0
WireConnection;194;0;222;0
WireConnection;195;0;219;0
WireConnection;195;2;194;0
WireConnection;193;0;170;0
WireConnection;193;2;192;0
WireConnection;196;0;195;0
WireConnection;184;0;193;0
WireConnection;197;1;196;0
WireConnection;197;2;228;0
WireConnection;181;1;184;0
WireConnection;181;2;182;0
WireConnection;231;0;181;0
WireConnection;231;1;197;0
WireConnection;238;0;236;2
WireConnection;237;0;238;0
WireConnection;233;0;231;0
WireConnection;239;0;237;0
WireConnection;232;0;233;0
WireConnection;234;0;33;0
WireConnection;38;0;232;0
WireConnection;38;1;39;0
WireConnection;34;0;234;0
WireConnection;34;1;232;0
WireConnection;45;0;36;0
WireConnection;45;1;37;0
WireConnection;46;0;43;0
WireConnection;46;1;37;0
WireConnection;225;0;217;2
WireConnection;225;1;245;0
WireConnection;242;0;239;0
WireConnection;9;0;3;2
WireConnection;9;1;246;0
WireConnection;40;0;38;0
WireConnection;40;1;43;0
WireConnection;40;2;46;0
WireConnection;35;0;34;0
WireConnection;35;1;36;0
WireConnection;35;2;45;0
WireConnection;226;0;225;0
WireConnection;226;1;224;0
WireConnection;226;2;229;0
WireConnection;240;0;242;0
WireConnection;148;0;9;0
WireConnection;148;1;147;0
WireConnection;148;2;149;0
WireConnection;230;0;148;0
WireConnection;230;1;226;0
WireConnection;241;0;240;0
WireConnection;42;0;35;0
WireConnection;42;1;40;0
WireConnection;243;0;224;0
WireConnection;47;0;48;5
WireConnection;47;1;2;5
WireConnection;47;2;42;0
WireConnection;75;0;74;0
WireConnection;78;0;77;0
WireConnection;126;1;77;0
WireConnection;76;0;75;0
WireConnection;76;1;78;0
WireConnection;125;0;75;0
WireConnection;125;1;126;0
WireConnection;25;0;24;0
WireConnection;124;0;76;0
WireConnection;127;0;125;0
WireConnection;16;0;17;0
WireConnection;19;0;12;0
WireConnection;19;1;21;0
WireConnection;27;1;25;0
WireConnection;85;0;124;0
WireConnection;85;1;83;0
WireConnection;86;1;25;0
WireConnection;104;0;124;0
WireConnection;106;0;127;0
WireConnection;106;1;105;0
WireConnection;107;1;25;0
WireConnection;121;0;127;0
WireConnection;14;0;12;2
WireConnection;14;1;16;0
WireConnection;22;0;19;0
WireConnection;22;1;27;0
WireConnection;87;0;104;1
WireConnection;87;1;16;0
WireConnection;88;0;85;0
WireConnection;88;1;86;0
WireConnection;109;0;106;0
WireConnection;109;1;107;0
WireConnection;108;0;121;1
WireConnection;108;1;16;0
WireConnection;15;0;12;1
WireConnection;15;1;14;0
WireConnection;15;2;12;3
WireConnection;23;0;22;0
WireConnection;90;0;104;0
WireConnection;90;1;87;0
WireConnection;90;2;104;2
WireConnection;91;0;88;0
WireConnection;110;0;121;0
WireConnection;110;1;108;0
WireConnection;110;2;121;2
WireConnection;111;0;109;0
WireConnection;10;0;15;0
WireConnection;10;1;13;0
WireConnection;31;0;23;0
WireConnection;92;0;90;0
WireConnection;92;1;13;0
WireConnection;95;0;91;0
WireConnection;112;0;110;0
WireConnection;112;1;13;0
WireConnection;114;0;111;0
WireConnection;7;0;10;0
WireConnection;7;1;8;0
WireConnection;28;0;31;0
WireConnection;28;1;20;0
WireConnection;97;0;92;0
WireConnection;97;1;8;0
WireConnection;98;0;95;0
WireConnection;98;1;20;0
WireConnection;116;0;112;0
WireConnection;116;1;8;0
WireConnection;117;0;114;0
WireConnection;117;1;20;0
WireConnection;30;0;7;0
WireConnection;30;1;32;0
WireConnection;49;0;32;0
WireConnection;49;1;28;0
WireConnection;99;0;97;0
WireConnection;99;1;96;0
WireConnection;100;0;96;0
WireConnection;100;1;98;0
WireConnection;118;0;116;0
WireConnection;118;1;115;0
WireConnection;119;0;115;0
WireConnection;119;1;117;0
WireConnection;29;0;30;0
WireConnection;29;1;49;0
WireConnection;101;0;99;0
WireConnection;101;1;100;0
WireConnection;120;0;118;0
WireConnection;120;1;119;0
WireConnection;141;0;124;0
WireConnection;142;0;127;0
WireConnection;66;0;29;0
WireConnection;66;1;133;0
WireConnection;103;0;101;0
WireConnection;103;1;141;0
WireConnection;123;0;120;0
WireConnection;123;1;142;0
WireConnection;128;0;66;0
WireConnection;128;1;103;0
WireConnection;129;0;66;0
WireConnection;129;1;123;0
WireConnection;137;0;129;0
WireConnection;136;0;128;0
WireConnection;67;0;66;0
WireConnection;138;0;137;0
WireConnection;138;1;136;0
WireConnection;139;0;138;0
WireConnection;227;1;219;0
WireConnection;235;0;230;0
WireConnection;235;1;241;0
WireConnection;179;1;170;0
WireConnection;0;2;47;0
WireConnection;0;11;235;0
ASEEND*/
//CHKSM=AEE836EC9D28479710789085E69BBF79020B97DD