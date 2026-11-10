// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WaterfallImpact"
{
	Properties
	{
		_VoronoiSpeed("Voronoi Speed", Float) = 0.5
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Voronoi("Voronoi", 2D) = "white" {}
		_VoronoiStep("Voronoi Step", Range( 0 , 0.5)) = 0.5
		_Color0("Color 0", Color) = (1,1,1,0)
		_LengthPow("Length Pow", Float) = 0
		_LengthMul("Length Mul", Float) = 1.5
		_ClouNoise("Clou Noise", 2D) = "white" {}
		_DistortionIntensity("Distortion Intensity", Range( 0 , 0.2)) = 0.04
		_DistortionSpeed("Distortion Speed", Float) = 0.1
		_WidthPow("Width Pow", Float) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#define ASE_VERSION 19801
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color0;
		uniform float _VoronoiStep;
		uniform sampler2D _Voronoi;
		uniform float4 _Voronoi_ST;
		uniform float _VoronoiSpeed;
		uniform sampler2D _ClouNoise;
		uniform float4 _ClouNoise_ST;
		uniform float _DistortionSpeed;
		uniform float _DistortionIntensity;
		uniform float _LengthMul;
		uniform float _LengthPow;
		uniform float _WidthPow;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Color0.rgb;
			o.Alpha = 1;
			float2 uv_Voronoi = i.uv_texcoord * _Voronoi_ST.xy + _Voronoi_ST.zw;
			float mulTime5 = _Time.y * _VoronoiSpeed;
			float2 appendResult6 = (float2(0.0 , mulTime5));
			float2 uv_ClouNoise = i.uv_texcoord * _ClouNoise_ST.xy + _ClouNoise_ST.zw;
			float mulTime21 = _Time.y * _DistortionSpeed;
			float2 appendResult22 = (float2(0.0 , mulTime21));
			float4 tex2DNode17 = tex2D( _ClouNoise, ( uv_ClouNoise + appendResult22 ) );
			float2 appendResult24 = (float2(tex2DNode17.r , tex2DNode17.g));
			clip( step( _VoronoiStep , ( tex2D( _Voronoi, ( uv_Voronoi + appendResult6 + ( ( appendResult24 - float2( 0.5,0.5 ) ) * _DistortionIntensity ) ) ).g * pow( saturate( ( i.uv_texcoord.y * _LengthMul ) ) , _LengthPow ) * pow( ( 1.0 - abs( ( ( i.uv_texcoord.x - 0.5 ) * 2.0 ) ) ) , _WidthPow ) ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.RangedFloatNode;23;-2496,576;Inherit;False;Property;_DistortionSpeed;Distortion Speed;9;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;21;-2288,592;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;18;-2384,288;Inherit;True;Property;_ClouNoise;Clou Noise;7;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-2160,400;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;22;-2096,560;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-1920,448;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;17;-1776,288;Inherit;True;Property;_DistortionNoise;Distortion Noise;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-1232,704;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-1408,224;Inherit;False;Property;_VoronoiSpeed;Voronoi Speed;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;24;-1472,336;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;-784,688;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1328,-16;Inherit;True;Property;_Voronoi;Voronoi;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleTimeNode;5;-1216,224;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;-1328,336;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1504,480;Inherit;False;Property;_DistortionIntensity;Distortion Intensity;8;0;Create;True;0;0;0;False;0;False;0.04;0.04;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-624,688;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1216,560;Inherit;False;Property;_LengthMul;Length Mul;6;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1088,80;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;6;-1024,208;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1168,336;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;30;-480,688;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-1008,496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-848,144;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;31;-368,688;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-368,768;Inherit;False;Property;_WidthPow;Width Pow;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-912,592;Inherit;False;Property;_LengthPow;Length Pow;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;39;-864,432;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-688,-16;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.PowerNode;32;-192,688;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;36;-736,496;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-544,512;Inherit;False;Property;_VoronoiStep;Voronoi Step;3;0;Create;True;0;0;0;False;0;False;0.5;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-416,224;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-288,-352;Inherit;False;Property;_Color0;Color 0;4;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StepOpNode;9;-144,240;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;Standard;WaterfallImpact;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;23;0
WireConnection;19;2;18;0
WireConnection;22;1;21;0
WireConnection;20;0;19;0
WireConnection;20;1;22;0
WireConnection;17;0;18;0
WireConnection;17;1;20;0
WireConnection;17;7;18;1
WireConnection;24;0;17;1
WireConnection;24;1;17;2
WireConnection;28;0;35;1
WireConnection;5;0;7;0
WireConnection;27;0;24;0
WireConnection;29;0;28;0
WireConnection;3;2;2;0
WireConnection;6;1;5;0
WireConnection;25;0;27;0
WireConnection;25;1;26;0
WireConnection;30;0;29;0
WireConnection;37;0;35;2
WireConnection;37;1;38;0
WireConnection;4;0;3;0
WireConnection;4;1;6;0
WireConnection;4;2;25;0
WireConnection;31;0;30;0
WireConnection;39;0;37;0
WireConnection;1;0;2;0
WireConnection;1;1;4;0
WireConnection;1;7;2;1
WireConnection;32;0;31;0
WireConnection;32;1;33;0
WireConnection;36;0;39;0
WireConnection;36;1;15;0
WireConnection;13;0;1;2
WireConnection;13;1;36;0
WireConnection;13;2;32;0
WireConnection;9;0;10;0
WireConnection;9;1;13;0
WireConnection;0;0;12;5
WireConnection;0;10;9;0
ASEEND*/
//CHKSM=A5715A8336A2FC97B05419FBCDAF1F66A12051D0