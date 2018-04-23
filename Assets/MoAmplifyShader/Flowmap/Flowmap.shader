// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Flowmap"
{
	Properties
	{
		[Normal]_NormalMap("NormalMap", 2D) = "bump" {}
		_Flowmap("Flowmap", 2D) = "white" {}
		_PeriodSec("PeriodSec", Range( 0 , 10)) = 0
		_Speed("Speed", Range( 0 , 1)) = 0
		_TimeScale("TimeScale", Range( 0 , 1)) = 0.2266474
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_DiffuseMap("DiffuseMap", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _Speed;
		uniform float _PeriodSec;
		uniform sampler2D _Flowmap;
		uniform float _TimeScale;
		uniform sampler2D _NormalMap;
		uniform sampler2D _DiffuseMap;
		uniform float _Metallic;
		uniform float _Smoothness;


		float3 CalculateFlowMap( float Speed , float PeriodSec , sampler2D FlowTex , float2 UV , float Time , sampler2D DiffuseTex )
		{
			float2 AdjustUV = UV;
			float HalfPeriod = PeriodSec * 0.5;
			float2 Flow = tex2D(FlowTex, AdjustUV).rg  * 0.5 + float2(0.5,0.5);
			float2 Bias = fmod(Time,PeriodSec) * Flow;
			float2 NewUV = Bias* Speed + AdjustUV;
			float4 C1 = tex2D(DiffuseTex, NewUV);
			Bias = fmod(Time + HalfPeriod,PeriodSec) * Flow;
			NewUV = Bias * Speed + AdjustUV;
			float4 C2 = tex2D(DiffuseTex, NewUV);
			float a = abs(HalfPeriod - fmod(Time, PeriodSec)) / HalfPeriod;
			return lerp(C1,C2, a);
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float Speed1 = _Speed;
			float PeriodSec1 = _PeriodSec;
			sampler2D FlowTex1 = _Flowmap;
			float2 uv_TexCoord2 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 UV1 = uv_TexCoord2;
			float temp_output_9_0 = ( _Time.y * _TimeScale );
			float Time1 = temp_output_9_0;
			sampler2D DiffuseTex1 = _NormalMap;
			float3 localCalculateFlowMap1 = CalculateFlowMap( Speed1 , PeriodSec1 , FlowTex1 , UV1 , Time1 , DiffuseTex1 );
			o.Normal = localCalculateFlowMap1;
			float Speed15 = _Speed;
			float PeriodSec15 = _PeriodSec;
			sampler2D FlowTex15 = _Flowmap;
			float2 UV15 = uv_TexCoord2;
			float Time15 = temp_output_9_0;
			sampler2D DiffuseTex15 = _DiffuseMap;
			float3 localCalculateFlowMap15 = CalculateFlowMap( Speed15 , PeriodSec15 , FlowTex15 , UV15 , Time15 , DiffuseTex15 );
			o.Albedo = localCalculateFlowMap15;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15201
7;162;1433;789;986.8485;383.3358;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;8;-833.1396,298.2296;Float;False;Property;_TimeScale;TimeScale;4;0;Create;True;0;0;False;0;0.2266474;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;7;-877.5797,30.22687;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;3;-478.0709,308.084;Float;True;Property;_NormalMap;NormalMap;0;1;[Normal];Create;True;0;0;False;0;None;5be367dffa6dac448b585fe12407f916;True;bump;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-518.7301,81.75125;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-546.4491,-52.6615;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;4;-569.4491,-263.6615;Float;True;Property;_Flowmap;Flowmap;1;0;Create;True;0;0;False;0;None;d215868230cb50340995a3bd63f40ccd;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-15.86561,-431.1566;Float;False;Property;_Speed;Speed;3;0;Create;True;0;0;False;0;0;0.035;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-259.4848,-356.3535;Float;False;Property;_PeriodSec;PeriodSec;2;0;Create;True;0;0;False;0;0;2.35;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;17;-96.31543,259.8662;Float;True;Property;_DiffuseMap;DiffuseMap;7;0;Create;True;0;0;False;0;None;c68296334e691ed45b62266cbc716628;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CustomExpressionNode;1;-50.87075,-30.14492;Float;False;float2 AdjustUV = UV@$float HalfPeriod = PeriodSec * 0.5@$float2 Flow = tex2D(FlowTex, AdjustUV).rg  * 0.5 + float2(0.5,0.5)@$float2 Bias = fmod(Time,PeriodSec) * Flow@$float2 NewUV = Bias* Speed + AdjustUV@$float4 C1 = tex2D(DiffuseTex, NewUV)@$Bias = fmod(Time + HalfPeriod,PeriodSec) * Flow@$NewUV = Bias * Speed + AdjustUV@$float4 C2 = tex2D(DiffuseTex, NewUV)@$float a = abs(HalfPeriod - fmod(Time, PeriodSec)) / HalfPeriod@$return lerp(C1,C2, a)@;3;False;6;True;Speed;FLOAT;0;In;True;PeriodSec;FLOAT;0;In;True;FlowTex;SAMPLER2D;;In;True;UV;FLOAT2;0,0;In;True;Time;FLOAT;0;In;True;DiffuseTex;SAMPLER2D;;In;CalculateFlowMap;False;False;6;0;FLOAT;0;False;1;FLOAT;0;False;2;SAMPLER2D;;False;3;FLOAT2;0,0;False;4;FLOAT;0;False;5;SAMPLER2D;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;10;267.9316,268.8468;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;0;0.24;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;15;343.2766,-308.6477;Float;False;float2 AdjustUV = UV@$float HalfPeriod = PeriodSec * 0.5@$float2 Flow = tex2D(FlowTex, AdjustUV).rg  * 0.5 + float2(0.5,0.5)@$float2 Bias = fmod(Time,PeriodSec) * Flow@$float2 NewUV = Bias* Speed + AdjustUV@$float4 C1 = tex2D(DiffuseTex, NewUV)@$Bias = fmod(Time + HalfPeriod,PeriodSec) * Flow@$NewUV = Bias * Speed + AdjustUV@$float4 C2 = tex2D(DiffuseTex, NewUV)@$float a = abs(HalfPeriod - fmod(Time, PeriodSec)) / HalfPeriod@$return lerp(C1,C2, a)@;3;False;6;True;Speed;FLOAT;0;In;True;PeriodSec;FLOAT;0;In;True;FlowTex;SAMPLER2D;;In;True;UV;FLOAT2;0,0;In;True;Time;FLOAT;0;In;True;DiffuseTex;SAMPLER2D;;In;CalculateFlowMap;False;False;6;0;FLOAT;0;False;1;FLOAT;0;False;2;SAMPLER2D;;False;3;FLOAT2;0,0;False;4;FLOAT;0;False;5;SAMPLER2D;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;11;248.9316,162.8468;Float;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;False;0;0;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;707.6313,-116.563;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Flowmap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;7;2
WireConnection;9;1;8;0
WireConnection;1;0;5;0
WireConnection;1;1;6;0
WireConnection;1;2;4;0
WireConnection;1;3;2;0
WireConnection;1;4;9;0
WireConnection;1;5;3;0
WireConnection;15;0;5;0
WireConnection;15;1;6;0
WireConnection;15;2;4;0
WireConnection;15;3;2;0
WireConnection;15;4;9;0
WireConnection;15;5;17;0
WireConnection;0;0;15;0
WireConnection;0;1;1;0
WireConnection;0;3;11;0
WireConnection;0;4;10;0
ASEEND*/
//CHKSM=A703D8BD8748A481C9D35280515CA43991802A5C