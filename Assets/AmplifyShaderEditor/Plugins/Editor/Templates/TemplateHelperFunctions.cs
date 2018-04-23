// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{
	public enum TemplateSemantics
	{
		NONE,
		POSITION,
		SV_POSITION,
		COLOR,
		COLOR0,
		COLOR1,
		TEXCOORD0,
		TEXCOORD1,
		TEXCOORD2,
		TEXCOORD3,
		TEXCOORD4,
		TEXCOORD5,
		TEXCOORD6,
		TEXCOORD7,
		TEXCOORD8,
		TEXCOORD9,
		TEXCOORD10,
		TEXCOORD11,
		TEXCOORD12,
		TEXCOORD13,
		TEXCOORD14,
		TEXCOORD15,
		NORMAL,
		TANGENT,
		VFACE
	}

	public enum TemplateInfoOnSematics
	{
		NONE,
		POSITION,
		SCREEN_POSITION,
		COLOR,
		TEXTURE_COORDINATES0,
		TEXTURE_COORDINATES1,
		TEXTURE_COORDINATES2,
		TEXTURE_COORDINATES3,
		NORMAL,
		TANGENT,
		WORLD_NORMAL,
		WORLD_TANGENT,
		WORLD_BITANGENT,
		WORLD_VIEW_DIR,
		WORLD_POSITION,
		OTHER
	}

	public enum TemplateShaderPropertiesIdx
	{
		Name = 2,
		InspectorName,
		Type
	}

	public enum TemplateShaderGlobalsIdx
	{
		Type = 1,
		Name = 2
	}
	public enum TemplateDataCheck
	{
		Valid,
		Invalid,
		Unreadable
	}

	public class TemplateReplaceHelper
	{
		public TemplateMultiPassMasterNode MasterNode = null;
		public bool Used = false;
		public TemplateReplaceHelper( TemplateMultiPassMasterNode masterNode ) { MasterNode = masterNode; }
	}

	[Serializable]
	public class TemplatesTagData
	{
		public string Name;
		public string Value;
		public TemplatesTagData( string name, string value )
		{
			Name = name;
			Value = value;
		}
	}

	[Serializable]
	public class TemplateModuleData
	{
		public TemplateDataCheck DataCheck = TemplateDataCheck.Invalid;
		public int StartIdx;
	}

	[Serializable]
	public sealed class TemplateTagsModuleData : TemplateModuleData
	{
		public string TagsId;
		public List<TemplatesTagData> Tags = new List<TemplatesTagData>();
		public void Destroy()
		{
			Tags.Clear();
			Tags = null;
		}

		public void Reset()
		{
			Tags.Clear();
		}

		public void Dump()
		{
			string dump = string.Empty;
			for ( int i = 0; i < Tags.Count; i++ )
			{
				dump += string.Format( "[{0}] Name: {1} Value: {2}\n", i, Tags[ i ].Name, Tags[ i ].Value );
			}
			Debug.Log( dump );
		}
	}

	[Serializable]
	public class TemplateShaderModelData : TemplateModuleData
	{
		public string Id = string.Empty;
		public string Value = "2.5";
		public int InterpolatorAmount = 8;
	}

	[Serializable]
	public sealed class TemplateDepthData : TemplateModuleData
	{
		public bool ValidZWrite;
		public string ZWriteModeId;
		public ZWriteMode ZWriteModeValue;
		public int ZWriteStartIndex;

		public bool ValidZTest;
		public string ZTestModeId;
		public ZTestMode ZTestModeValue;
		public int ZTestStartIndex;

		public bool ValidOffset;
		public string OffsetId;
		public float OffsetFactor;
		public float OffsetUnits;
		public int OffsetStartIndex;
	}

	[Serializable]
	public sealed class TemplateStencilData : TemplateModuleData
	{
		public string StencilBufferId;
		public int Reference;
		public int ReadMask = 255;
		public int WriteMask = 255;

		public string ComparisonFront;
		public string PassFront;
		public string FailFront;
		public string ZFailFront;

		public string ComparisonBack;
		public string PassBack;
		public string FailBack;
		public string ZFailBack;
	}

	[Serializable]
	public sealed class TemplateBlendData : TemplateModuleData
	{
		public bool ValidBlendMode = false;
		public bool BlendModeOff = true;

		public string BlendModeId;
		public bool SeparateBlendFactors = false;
		public AvailableBlendFactor SourceFactorRGB = AvailableBlendFactor.One;
		public AvailableBlendFactor DestFactorRGB = AvailableBlendFactor.Zero;
		public int BlendModeStartIndex;

		public AvailableBlendFactor SourceFactorAlpha = AvailableBlendFactor.One;
		public AvailableBlendFactor DestFactorAlpha = AvailableBlendFactor.Zero;

		public bool ValidBlendOp = false;
		public string BlendOpId;
		public bool SeparateBlendOps = false;
		public AvailableBlendOps BlendOpRGB = AvailableBlendOps.OFF;
		public AvailableBlendOps BlendOpAlpha = AvailableBlendOps.OFF;
		public int BlendOpStartIndex;
	}

	[Serializable]
	public sealed class TemplateCullModeData : TemplateModuleData
	{
		public string CullModeId;
		public CullMode CullModeData = CullMode.Front;
	}

	[Serializable]
	public sealed class TemplateColorMaskData : TemplateModuleData
	{
		public string ColorMaskId;
		public bool[] ColorMaskData = { true, true, true, true };
	}

	public static class TemplateHelperFunctions
	{
		public static string[] VectorSwizzle = { "x", "y", "z", "w" };
		public static string[] ColorSwizzle = { "r", "g", "b", "a" };

		public static readonly Dictionary<string, WirePortDataType> PropertyToWireType = new Dictionary<string, WirePortDataType>
		{
			{"Float",WirePortDataType.FLOAT},
			{"Range",WirePortDataType.FLOAT},
			{"Int",WirePortDataType.INT},
			{"Color",WirePortDataType.COLOR},
			{"Vector",WirePortDataType.FLOAT4},
			{"2D",WirePortDataType.SAMPLER2D},
			{"3D",WirePortDataType.SAMPLER3D},
			{"Cube",WirePortDataType.SAMPLERCUBE}
		};

		public static readonly Dictionary<WirePortDataType, int> DataTypeChannelUsage = new Dictionary<WirePortDataType, int>
		{
			{WirePortDataType.OBJECT,0 },
			{WirePortDataType.FLOAT,1 },
			{WirePortDataType.FLOAT2,2 },
			{WirePortDataType.FLOAT3,3 },
			{WirePortDataType.FLOAT4,4 },
			{WirePortDataType.FLOAT3x3,0 },
			{WirePortDataType.FLOAT4x4,0 },
			{WirePortDataType.COLOR,4 },
			{WirePortDataType.INT,1 },
			{WirePortDataType.SAMPLER1D,0 },
			{WirePortDataType.SAMPLER2D,0 },
			{WirePortDataType.SAMPLER3D,0 },
			{WirePortDataType.SAMPLERCUBE,0 }
		};

		public static readonly Dictionary<TemplateSemantics, string> SemanticsDefaultName = new Dictionary<TemplateSemantics, string>
		{
			{TemplateSemantics.COLOR        ,"ase_color"},
			{TemplateSemantics.NORMAL       ,"ase_normal"},
			{TemplateSemantics.POSITION     ,"ase_position"},
			{TemplateSemantics.SV_POSITION  ,"ase_sv_position"},
			{TemplateSemantics.TANGENT      ,"ase_tangent"},
			{TemplateSemantics.VFACE        ,"ase_vface"},
			{TemplateSemantics.TEXCOORD0    ,"ase_tex_coord0"},
			{TemplateSemantics.TEXCOORD1    ,"ase_tex_coord1"},
			{TemplateSemantics.TEXCOORD2    ,"ase_tex_coord2"},
			{TemplateSemantics.TEXCOORD3    ,"ase_tex_coord3"},
			{TemplateSemantics.TEXCOORD4    ,"ase_tex_coord4"},
			{TemplateSemantics.TEXCOORD5    ,"ase_tex_coord5"},
			{TemplateSemantics.TEXCOORD6    ,"ase_tex_coord6"},
			{TemplateSemantics.TEXCOORD7    ,"ase_tex_coord7"},
			{TemplateSemantics.TEXCOORD8    ,"ase_tex_coord8"},
			{TemplateSemantics.TEXCOORD9    ,"ase_tex_coord9"},
			{TemplateSemantics.TEXCOORD10    ,"ase_tex_coord10"},
			{TemplateSemantics.TEXCOORD11    ,"ase_tex_coord11"},
			{TemplateSemantics.TEXCOORD12    ,"ase_tex_coord12"},
			{TemplateSemantics.TEXCOORD13    ,"ase_tex_coord13"},
			{TemplateSemantics.TEXCOORD14    ,"ase_tex_coord14"},
			{TemplateSemantics.TEXCOORD15    ,"ase_tex_coord15"},
		};

		public static readonly Dictionary<int, TemplateInfoOnSematics> IntToInfo = new Dictionary<int, TemplateInfoOnSematics>
		{
			{0,TemplateInfoOnSematics.TEXTURE_COORDINATES0 },
			{1,TemplateInfoOnSematics.TEXTURE_COORDINATES1 },
			{2,TemplateInfoOnSematics.TEXTURE_COORDINATES2 },
			{3,TemplateInfoOnSematics.TEXTURE_COORDINATES3 },
		};

		public static readonly Dictionary<string, TemplateInfoOnSematics> ShortcutToInfo = new Dictionary<string, TemplateInfoOnSematics>
		{
			{"p"    ,TemplateInfoOnSematics.POSITION },
			{"sp"   ,TemplateInfoOnSematics.SCREEN_POSITION },
			{"c"    ,TemplateInfoOnSematics.COLOR },
			{"uv0"  ,TemplateInfoOnSematics.TEXTURE_COORDINATES0 },
			{"uv1"  ,TemplateInfoOnSematics.TEXTURE_COORDINATES1 },
			{"uv2"  ,TemplateInfoOnSematics.TEXTURE_COORDINATES2 },
			{"uv3"  ,TemplateInfoOnSematics.TEXTURE_COORDINATES3 },
			{"n"    ,TemplateInfoOnSematics.NORMAL },
			{"t"    ,TemplateInfoOnSematics.TANGENT },
			{"wn"   ,TemplateInfoOnSematics.WORLD_NORMAL},
			{"wt"   ,TemplateInfoOnSematics.WORLD_TANGENT},
			{"wbt"  ,TemplateInfoOnSematics.WORLD_BITANGENT},
			{"wvd"  ,TemplateInfoOnSematics.WORLD_VIEW_DIR},
			{"wp"   ,TemplateInfoOnSematics.WORLD_POSITION}
		};


		public static readonly Dictionary<TemplateInfoOnSematics, string> InfoToLocalVar = new Dictionary<TemplateInfoOnSematics, string>
		{
			{TemplateInfoOnSematics.POSITION,GeneratorUtils.VertexPosition4Str },
			{TemplateInfoOnSematics.SCREEN_POSITION,GeneratorUtils.ScreenPositionStr },
			{TemplateInfoOnSematics.COLOR, "ase_color" },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES0, "ase_uv0" },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES1, "ase_uv1" },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES2, "ase_uv2" },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES3, "ase_uv3" },
			{TemplateInfoOnSematics.NORMAL, GeneratorUtils.VertexNormalStr },
			{TemplateInfoOnSematics.TANGENT, GeneratorUtils.VertexTangentStr },
			{TemplateInfoOnSematics.WORLD_NORMAL, GeneratorUtils.WorldNormalStr},
			{TemplateInfoOnSematics.WORLD_TANGENT, GeneratorUtils.WorldTangentStr},
			{TemplateInfoOnSematics.WORLD_BITANGENT, GeneratorUtils.WorldBitangentStr},
			{TemplateInfoOnSematics.WORLD_VIEW_DIR, GeneratorUtils.WorldViewDirectionStr},
			{TemplateInfoOnSematics.WORLD_POSITION, GeneratorUtils.WorldPositionStr}
		};


		public static readonly Dictionary<TemplateInfoOnSematics, WirePortDataType> InfoToWirePortType = new Dictionary<TemplateInfoOnSematics, WirePortDataType>
		{
			{TemplateInfoOnSematics.POSITION,WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.SCREEN_POSITION,WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.COLOR, WirePortDataType.COLOR },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES0, WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES1, WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES2, WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.TEXTURE_COORDINATES3, WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.NORMAL, WirePortDataType.FLOAT3 },
			{TemplateInfoOnSematics.TANGENT, WirePortDataType.FLOAT4 },
			{TemplateInfoOnSematics.WORLD_NORMAL, WirePortDataType.FLOAT3},
			{TemplateInfoOnSematics.WORLD_TANGENT, WirePortDataType.FLOAT3},
			{TemplateInfoOnSematics.WORLD_BITANGENT, WirePortDataType.FLOAT3},
			{TemplateInfoOnSematics.WORLD_VIEW_DIR, WirePortDataType.FLOAT3},
			{TemplateInfoOnSematics.WORLD_POSITION, WirePortDataType.FLOAT3}
		};
		public static readonly Dictionary<int, TemplateInfoOnSematics> IntToUVChannelInfo = new Dictionary<int, TemplateInfoOnSematics>
		{
			{0,TemplateInfoOnSematics.TEXTURE_COORDINATES0 },
			{1,TemplateInfoOnSematics.TEXTURE_COORDINATES1 },
			{2,TemplateInfoOnSematics.TEXTURE_COORDINATES2 },
			{3,TemplateInfoOnSematics.TEXTURE_COORDINATES3 }
		};

		public static readonly Dictionary<int, TemplateSemantics> IntToSemantic = new Dictionary<int, TemplateSemantics>
		{
			{ 0,TemplateSemantics.TEXCOORD0 },
			{ 1,TemplateSemantics.TEXCOORD1 },
			{ 2,TemplateSemantics.TEXCOORD2 },
			{ 3,TemplateSemantics.TEXCOORD3 },
			{ 4,TemplateSemantics.TEXCOORD4 },
			{ 5,TemplateSemantics.TEXCOORD5 },
			{ 6,TemplateSemantics.TEXCOORD6 },
			{ 7,TemplateSemantics.TEXCOORD7 },
			{ 8,TemplateSemantics.TEXCOORD8 },
			{ 9,TemplateSemantics.TEXCOORD9 },
			{ 10,TemplateSemantics.TEXCOORD10 },
			{ 11,TemplateSemantics.TEXCOORD11 },
			{ 12,TemplateSemantics.TEXCOORD12 },
			{ 13,TemplateSemantics.TEXCOORD13 },
			{ 14,TemplateSemantics.TEXCOORD14 },
			{ 15,TemplateSemantics.TEXCOORD15 }
		};

		public static readonly Dictionary<TemplateSemantics, int> SemanticToInt = new Dictionary<TemplateSemantics, int>
		{
			{ TemplateSemantics.TEXCOORD0,0 },
			{ TemplateSemantics.TEXCOORD1,1 },
			{ TemplateSemantics.TEXCOORD2,2 },
			{ TemplateSemantics.TEXCOORD3,3 },
			{ TemplateSemantics.TEXCOORD4,4 },
			{ TemplateSemantics.TEXCOORD5,5 },
			{ TemplateSemantics.TEXCOORD6,6 },
			{ TemplateSemantics.TEXCOORD7,7 },
			{ TemplateSemantics.TEXCOORD8,8 },
			{ TemplateSemantics.TEXCOORD9,9 },
			{ TemplateSemantics.TEXCOORD10,10 },
			{ TemplateSemantics.TEXCOORD11,11 },
			{ TemplateSemantics.TEXCOORD12,12 },
			{ TemplateSemantics.TEXCOORD13,13 },
			{ TemplateSemantics.TEXCOORD14,14 },
			{ TemplateSemantics.TEXCOORD15,15 },
		};

		public static readonly Dictionary<string, TemplateSemantics> ShortcutToSemantic = new Dictionary<string, TemplateSemantics>
		{
			{ "p"   ,TemplateSemantics.POSITION },
			{ "sp"  ,TemplateSemantics.SV_POSITION },
			{ "c"   ,TemplateSemantics.COLOR },
			{ "n"   ,TemplateSemantics.NORMAL },
			{ "t"   ,TemplateSemantics.TANGENT },
			{ "tc0" ,TemplateSemantics.TEXCOORD0 },
			{ "tc1" ,TemplateSemantics.TEXCOORD1 },
			{ "tc2" ,TemplateSemantics.TEXCOORD2 },
			{ "tc3" ,TemplateSemantics.TEXCOORD3 },
			{ "tc4" ,TemplateSemantics.TEXCOORD4 },
			{ "tc5" ,TemplateSemantics.TEXCOORD5 },
			{ "tc6" ,TemplateSemantics.TEXCOORD6 },
			{ "tc7" ,TemplateSemantics.TEXCOORD7 },
			{ "tc8" ,TemplateSemantics.TEXCOORD8 },
			{ "tc9" ,TemplateSemantics.TEXCOORD9 },
			{ "tc10" ,TemplateSemantics.TEXCOORD10 },
			{ "tc11" ,TemplateSemantics.TEXCOORD11 },
			{ "tc12" ,TemplateSemantics.TEXCOORD12 },
			{ "tc13" ,TemplateSemantics.TEXCOORD13 },
			{ "tc14" ,TemplateSemantics.TEXCOORD14 },
			{ "tc15" ,TemplateSemantics.TEXCOORD15 }
		};

		public static readonly Dictionary<string, WirePortDataType> CgToWirePortType = new Dictionary<string, WirePortDataType>()
		{
			{"float"            ,WirePortDataType.FLOAT},
			{"float2"           ,WirePortDataType.FLOAT2},
			{"float3"           ,WirePortDataType.FLOAT3},
			{"float4"           ,WirePortDataType.FLOAT4},
			{"float3x3"         ,WirePortDataType.FLOAT3x3},
			{"float4x4"         ,WirePortDataType.FLOAT4x4},
			{"half"             ,WirePortDataType.FLOAT},
			{"half2"            ,WirePortDataType.FLOAT2},
			{"half3"            ,WirePortDataType.FLOAT3},
			{"half4"            ,WirePortDataType.FLOAT4},
			{"half3x3"          ,WirePortDataType.FLOAT3x3},
			{"half4x4"          ,WirePortDataType.FLOAT4x4},
			{"fixed"            ,WirePortDataType.FLOAT},
			{"fixed2"           ,WirePortDataType.FLOAT2},
			{"fixed3"           ,WirePortDataType.FLOAT3},
			{"fixed4"           ,WirePortDataType.FLOAT4},
			{"fixed3x3"         ,WirePortDataType.FLOAT3x3},
			{"fixed4x4"         ,WirePortDataType.FLOAT4x4},
			{"int"              ,WirePortDataType.INT},
			{"sampler1D"        ,WirePortDataType.SAMPLER1D},
			{"sampler2D"        ,WirePortDataType.SAMPLER2D},
			{"sampler2D_float"  ,WirePortDataType.SAMPLER2D},
			{"sampler3D"        ,WirePortDataType.SAMPLER3D},
			{"samplerCUBE"      ,WirePortDataType.SAMPLERCUBE}
		};

		public static readonly Dictionary<string, int> AvailableInterpolators = new Dictionary<string, int>()
		{
			{"2.0",8 },
			{"2.5",8 },
			{"3.0",10},
			{"3.5",10},
			{"4.0",16},
			{"4.5",16},
			{"4.6",16},
			{"5.0",16}
		};

		public static readonly string[] AvailableShaderModels =
		{ "2.0", "2.5", "3.0", "3.5", "4.0", "4.5", "4.6", "5.0" };

		public static readonly Dictionary<string, int> ShaderModelToArrayIdx = new Dictionary<string, int>()
		{
			{"2.0",0},
			{"2.5",1},
			{"3.0",2},
			{"3.5",3},
			{"4.0",4},
			{"4.5",5},
			{"4.6",6},
			{"5.0",7}
		};

		public static readonly Dictionary<string, TemplateSRPType> TagToRenderPipeline = new Dictionary<string, TemplateSRPType>()
		{
			{ "LightweightPipeline",TemplateSRPType.Lightweight },
			{ "HighDefinitionPipeline",TemplateSRPType.HD }
		};

		public static string CustomInspectorPattern = "^\\s*CustomEditor\\s+\\\"(\\w*)\\\"";
		public static string DefinesPattern = @"^\s*#define\s+([\w .]*)";
		public static string PragmasPattern = @"^\s*#pragma\s+([\w .]*)";
		public static string IncludesPattern = "^\\s*#include\\s+\"([\\w.]*)\"";

		public static string VertexPragmaPattern = @"#pragma vertex\s+(\w+)";
		public static string FragmentPragmaPattern = @"#pragma fragment\s+(\w+)";
		public static string FunctionBodyStartPattern = @"\s+{0}\s*\(";

		public static string ShaderModelPattern = @"#pragma\s+target\s+([0-9]*[.]*[0-9]*)";

		public static readonly string LocalVarPattern = @"\/\*ase_local_var[:]*(\w*)\*\/\s*(\w*)\s+(\w*)";

		public static readonly string SubShaderLODPattern = @"LOD\s+(\w+)";

		//public static readonly string PassNamePattern = "Name\\s+\\\"(\\w+)\\\"";
		public static readonly string PassNamePattern = "Name\\s+\\\"([\\w\\+\\-\\*\\/\\(\\) ]*)\\\"";

		public static readonly string TagsPattern = "\"(\\w+)\"\\s*=\\s*\"(\\w+\\+*\\w*)\"";
		public static readonly string ZTestPattern = @"\s*ZTest\s+(\w+)";
		public static readonly string ZWritePattern = @"\s*ZWrite\s+(\w+)";
		public static readonly string ZOffsetPattern = @"\s*Offset\s+([-+]?[0-9]*\.?[0-9]+)\s*,\s*([-+]?[0-9]*\.?[0-9]+)";

		//public static readonly string VertexDataPattern = @"(\w+)[ \t](\w+)[ \t]:[ \t]([A-Z0-9_]+);";
		public static readonly string VertexDataPattern = @"(\w+)\s*(\w+)\s*:\s*([A-Z0-9_]+);";
		public static readonly string InterpRangePattern = @"ase_interp\((\d\.{0,1}\w{0,4}),(\d*)\)";
		//public static readonly string PropertiesPattern = @"(\w*)\s*\(\s*\""([\w ] *)\""\s*\,\s*(\w*)\s*.*\)";
		public static readonly string PropertiesPatternB = "(\\w*)\\s*\\(\\s*\"([\\w ]*)\"\\s*\\,\\s*(\\w*)\\s*.*\\)";
		public static readonly string PropertiesPatternC = "^\\s*(\\w*)\\s*\\(\\s*\"([\\w\\(\\)\\+\\-\\\\* ]*)\"\\s*\\,\\s*(\\w*)\\s*.*\\)";
		public static readonly string PropertiesPatternD = "(\\/\\/\\s*)*(\\w*)\\s*\\(\\s*\"([\\w\\(\\)\\+\\-\\\\* ]*)\"\\s*\\,\\s*(\\w*)\\s*.*\\)";
		public static readonly string CullModePattern = @"\s*Cull\s+(\w+)";
		public static readonly string ColorMaskPattern = @"\s*ColorMask\s+(\w+)";
		public static readonly string BlendModePattern = @"\s*Blend\s+(\w+)\s+(\w+)(?:[\s,]+(\w+)\s+(\w+)|)";
		public static readonly string BlendOpPattern = @"\s*BlendOp\s+(\w+)[\s,]*(?:(\w+)|)";

		//public static readonly string StencilOpGlobalPattern = @"Stencil\s{([\w\W\s]*)}\/\*ase_stencil\*\/";
		public static readonly string StencilOpGlobalPattern = @"Stencil\s*{([\w\W\s]*)}";
		public static readonly string StencilOpLinePattern = @"(\w+)\s*(\w+)";

		//public static readonly string ShaderGlobalsOverallPattern = "[\\}\\#][\\w\\s\\;\\/\\*\\.\\\"]*\\/\\*ase_globals\\*\\/";
		public static readonly string ShaderGlobalsOverallPattern = "(?:\\/\\*ase_pragma\\*\\/|[\\}\\#])[\\w\\s\\;\\/\\*\\.\\\"]*\\/\\*ase_globals\\*\\/";
		public static readonly string ShaderGlobalsMultilinePattern = @"^\s*(?:uniform\s*)*(\w*)\s*(\w*);$";

		public static readonly string TexSemantic = "float4 {0} : TEXCOORD{1};";
		public static readonly string TexFullSemantic = "float4 {0} : {1};";
		public static readonly string InterpFullSemantic = "{0} {1} : {2};";
		public static readonly string BaseInterpolatorName = "ase_texcoord";
		public static readonly string InterpMacro = "{0}({1})";

		public static readonly string InterpolatorDecl = Constants.VertexShaderOutputStr + ".{0} = " + Constants.VertexShaderInputStr + ".{0};";
		public static readonly string TemplateVariableDecl = "{0} = {1};";
		public static readonly string TemplateVarFormat = "{0}.{1}";

		public static string ReplaceAt( this string body, string oldStr, string newStr, int startIndex )
		{
			return body.Remove( startIndex, oldStr.Length ).Insert( startIndex, newStr );
		}

		static public string GenerateTextureSemantic( ref MasterNodeDataCollector dataCollector, int uv )
		{
			string texCoordName = BaseInterpolatorName;
			if ( uv > 0 )
			{
				texCoordName += uv.ToString();
			}

			string texCoordData = string.Format( TexSemantic, texCoordName, uv );
			dataCollector.AddToVertexInput( texCoordData );
			dataCollector.AddToInterpolators( texCoordData );
			dataCollector.AddToVertexInterpolatorsDecl( string.Format( InterpolatorDecl, texCoordName ) );
			return texCoordName;
		}

		public static void CreatePragmaIncludeList( string data, TemplateIncludePragmaContainter includePragmaContainer )
		{
			foreach ( Match match in Regex.Matches( data, PragmasPattern,RegexOptions.Multiline ) )
			{
				if ( match.Groups.Count == 2 )
				{
					includePragmaContainer.AddPragma( match.Groups[ 1 ].Value );
				} 
			}

			foreach ( Match match in Regex.Matches( data, DefinesPattern, RegexOptions.Multiline ) )
			{
				if ( match.Groups.Count == 2 )
				{
					includePragmaContainer.AddDefine( match.Groups[ 1 ].Value );
				}
			}

			foreach ( Match match in Regex.Matches( data, IncludesPattern, RegexOptions.Multiline ) )
			{
				if ( match.Groups.Count == 2 )
				{
					includePragmaContainer.AddInclude( match.Groups[ 1 ].Value );
				}
			}
		}

		public static void CreateShaderPropertiesList( string propertyData, ref List<TemplateShaderPropertyData> propertiesList, ref Dictionary<string, TemplateShaderPropertyData> duplicatesHelper )
		{
			int nameIdx = ( int ) TemplateShaderPropertiesIdx.Name;
			int typeIdx = (int)TemplateShaderPropertiesIdx.Type;
			int inspectorNameIdx = (int)TemplateShaderPropertiesIdx.InspectorName;
			
			foreach ( Match match in Regex.Matches( propertyData, PropertiesPatternD ) )
			{
				if ( match.Groups.Count > 1 )
				{
					if( !match.Groups[ 1 ].Value.Contains( "//" ) )
					{
						if( !duplicatesHelper.ContainsKey( match.Groups[ nameIdx ].Value ) && PropertyToWireType.ContainsKey( match.Groups[ typeIdx ].Value ) )
						{
							TemplateShaderPropertyData newData = new TemplateShaderPropertyData( match.Groups[ inspectorNameIdx ].Value,
																									match.Groups[ nameIdx ].Value,
																									PropertyToWireType[ match.Groups[ typeIdx ].Value ],
																									PropertyType.Property );
							propertiesList.Add( newData );
							duplicatesHelper.Add( newData.PropertyName, newData );
						}
					}
				}
			}
		}

		public static void CreateShaderGlobalsList( string propertyData, ref List<TemplateShaderPropertyData> propertiesList, ref Dictionary<string, TemplateShaderPropertyData> duplicatesHelper )
		{
			int typeIdx = ( int ) TemplateShaderGlobalsIdx.Type;
			int nameIdx = ( int ) TemplateShaderGlobalsIdx.Name;
			MatchCollection matchCollection = Regex.Matches( propertyData, ShaderGlobalsOverallPattern );
			string value = ( matchCollection.Count > 0 ) ? matchCollection[ 0 ].Groups[ 0 ].Value : propertyData;
			foreach ( Match lineMatch in Regex.Matches( value, ShaderGlobalsMultilinePattern, RegexOptions.Multiline ) )
			{
				if ( lineMatch.Groups.Count > 1 )
				{
					if ( !duplicatesHelper.ContainsKey( lineMatch.Groups[ nameIdx ].Value ) && CgToWirePortType.ContainsKey( lineMatch.Groups[ typeIdx ].Value ) )
					{
						TemplateShaderPropertyData newData = new TemplateShaderPropertyData( string.Empty, lineMatch.Groups[ nameIdx ].Value,
																								CgToWirePortType[ lineMatch.Groups[ typeIdx ].Value ],
																								PropertyType.Global );
						duplicatesHelper.Add( newData.PropertyName, newData );
						propertiesList.Add( newData );
					}
				}
			}
		}

		public static void CreateStencilOps( string stencilData, ref TemplateStencilData stencilDataObj )
		{
			stencilDataObj.DataCheck = TemplateDataCheck.Unreadable;
			MatchCollection overallGlobalMatch = Regex.Matches( stencilData, StencilOpGlobalPattern );
			if ( overallGlobalMatch.Count == 1 && overallGlobalMatch[ 0 ].Groups.Count == 2 )
			{
				string value = overallGlobalMatch[ 0 ].Groups[ 1 ].Value;
				foreach ( Match match in Regex.Matches( value, StencilOpLinePattern ) )
				{
					stencilDataObj.DataCheck = TemplateDataCheck.Valid;
					if ( match.Groups.Count == 3 )
					{
						switch ( match.Groups[ 1 ].Value )
						{
							default:
							{
								stencilDataObj.DataCheck = TemplateDataCheck.Unreadable;
								return;
							}
							case "Ref":
							{
								try
								{
									stencilDataObj.Reference = Convert.ToInt32( match.Groups[ 2 ].Value );
								}
								catch ( Exception e )
								{
									Debug.LogException( e );
									stencilDataObj.DataCheck = TemplateDataCheck.Unreadable;
									return;
								}
							}
							break;
							case "ReadMask":
							{
								try
								{
									stencilDataObj.ReadMask = Convert.ToInt32( match.Groups[ 2 ].Value );
								}
								catch ( Exception e )
								{
									Debug.LogException( e );
									stencilDataObj.DataCheck = TemplateDataCheck.Unreadable;
									return;
								}
							}
							break;
							case "WriteMask":
							{
								try
								{
									stencilDataObj.WriteMask = Convert.ToInt32( match.Groups[ 2 ].Value );
								}
								catch ( Exception e )
								{
									Debug.LogException( e );
									stencilDataObj.DataCheck = TemplateDataCheck.Unreadable;
									return;
								}
							}
							break;
							case "CompFront":
							case "Comp":
							{
								stencilDataObj.ComparisonFront = match.Groups[ 2 ].Value;
							}
							break;
							case "PassFront":
							case "Pass":
							{
								stencilDataObj.PassFront = match.Groups[ 2 ].Value;
							}
							break;
							case "FailFront":
							case "Fail":
							{
								stencilDataObj.FailFront = match.Groups[ 2 ].Value;
							}
							break;
							case "ZFail":
							case "ZFailFront":
							{
								stencilDataObj.ZFailFront = match.Groups[ 2 ].Value;
							}
							break;
							case "CompBack":
							{
								stencilDataObj.ComparisonBack = match.Groups[ 2 ].Value;
							}
							break;
							case "PassBack":
							{
								stencilDataObj.PassBack = match.Groups[ 2 ].Value;
							}
							break;
							case "FailBack":
							{
								stencilDataObj.ZFailBack = match.Groups[ 2 ].Value;
							}
							break;
							case "ZFailBack":
							{
								stencilDataObj.ZFailBack = match.Groups[ 2 ].Value;
							}
							break;
						}
					}
				}
			}
		}

		public static void CreateColorMask( string colorMaskData, ref TemplateColorMaskData colorMaskObj )
		{
			colorMaskObj.DataCheck = TemplateDataCheck.Unreadable;
			foreach ( Match match in Regex.Matches( colorMaskData, ColorMaskPattern ) )
			{
				if ( match.Groups.Count == 2 )
				{
					for ( int i = 0; i < 4; i++ )
					{
						colorMaskObj.ColorMaskData[ i ] = false;
					}

					colorMaskObj.DataCheck = TemplateDataCheck.Valid;
					try
					{
						for ( int i = 0; i < match.Groups[ 1 ].Value.Length; i++ )
						{
							switch ( Char.ToLower( match.Groups[ 1 ].Value[ i ] ) )
							{
								case 'r': colorMaskObj.ColorMaskData[ 0 ] = true; break;
								case 'g': colorMaskObj.ColorMaskData[ 1 ] = true; break;
								case 'b': colorMaskObj.ColorMaskData[ 2 ] = true; break;
								case 'a': colorMaskObj.ColorMaskData[ 3 ] = true; break;
								case '0':
								{
									for ( int j = 0; j < 4; j++ )
									{
										colorMaskObj.ColorMaskData[ j ] = false;
									}
									return;
								}
								default:
								{
									colorMaskObj.DataCheck = TemplateDataCheck.Unreadable;
									return;
								}
							}
						}
					}
					catch ( Exception e )
					{
						Debug.LogException( e );
						colorMaskObj.DataCheck = TemplateDataCheck.Unreadable;
						return;
					}
				}
			}

		}

		public static void CreateCullMode( string cullModeData, ref TemplateCullModeData cullDataObj )
		{
			cullDataObj.DataCheck = TemplateDataCheck.Unreadable;
			foreach ( Match match in Regex.Matches( cullModeData, CullModePattern ) )
			{
				if ( match.Groups.Count == 2 )
				{
					cullDataObj.DataCheck = TemplateDataCheck.Valid;
					try
					{
						cullDataObj.CullModeData = ( CullMode ) Enum.Parse( typeof( CullMode ), match.Groups[ 1 ].Value );
					}
					catch ( Exception e )
					{
						cullDataObj.DataCheck = TemplateDataCheck.Unreadable;
						Debug.LogException( e );
						return;
					}
				}
			}
		}

		public static void CreateBlendMode( string blendModeData, ref TemplateBlendData blendDataObj )
		{
			blendDataObj.ValidBlendMode = true;
			// TODO: OPTIMIZE REGEX EXPRESSIONS TO NOT CATCH EMPTY GROUPS 
			foreach ( Match match in Regex.Matches( blendModeData, BlendModePattern ) )
			{
				if ( match.Groups.Count == 3 )
				{
					try
					{
						AvailableBlendFactor sourceAll = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 1 ].Value );
						AvailableBlendFactor destAll = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 2 ].Value );
						blendDataObj.SeparateBlendFactors = false;
						blendDataObj.SourceFactorRGB = sourceAll;
						blendDataObj.DestFactorRGB = destAll;
						blendDataObj.BlendModeOff = false;
					}
					catch ( Exception e )
					{
						Debug.LogException( e );
						blendDataObj.DataCheck = TemplateDataCheck.Unreadable;
						return;

					}
					break;
				}
				else if ( match.Groups.Count == 5 )
				{
					try
					{
						AvailableBlendFactor sourceRGB = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 1 ].Value );
						blendDataObj.SourceFactorRGB = sourceRGB;
						AvailableBlendFactor destRGB = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 2 ].Value );
						blendDataObj.DestFactorRGB = destRGB;

						if ( match.Groups[ 3 ].Success && match.Groups[ 4 ].Success )
						{
							AvailableBlendFactor sourceA = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 3 ].Value );
							blendDataObj.SourceFactorAlpha = sourceA;
							AvailableBlendFactor destA = ( AvailableBlendFactor ) Enum.Parse( typeof( AvailableBlendFactor ), match.Groups[ 4 ].Value );
							blendDataObj.DestFactorAlpha = destA;
							blendDataObj.SeparateBlendFactors = true;
						}
						else
						{
							blendDataObj.SeparateBlendFactors = false;
						}
						blendDataObj.BlendModeOff = false;
					}
					catch ( Exception e )
					{
						Debug.LogException( e );
						blendDataObj.DataCheck = TemplateDataCheck.Unreadable;
						return;
					}
					break;
				}
			}
		}

		public static void CreateBlendOp( string blendOpData, ref TemplateBlendData blendDataObj )
		{
			blendDataObj.ValidBlendOp = true;
			// TODO: OPTIMIZE REGEX EXPRESSIONS TO NOT CATCH EMPTY GROUPS 
			foreach ( Match match in Regex.Matches( blendOpData, BlendOpPattern, RegexOptions.None ) )
			{
				if ( match.Groups.Count == 2 )
				{
					try
					{
						AvailableBlendOps blendOpsAll = ( AvailableBlendOps ) Enum.Parse( typeof( AvailableBlendOps ), match.Groups[ 1 ].Value );
						blendDataObj.SeparateBlendOps = false;
						blendDataObj.BlendOpRGB = blendOpsAll;
					}
					catch ( Exception e )
					{
						Debug.LogException( e );
						blendDataObj.DataCheck = TemplateDataCheck.Unreadable;
						return;
					}
					break;
				}
				else if ( match.Groups.Count == 3 )
				{
					try
					{
						AvailableBlendOps blendOpsRGB = ( AvailableBlendOps ) Enum.Parse( typeof( AvailableBlendOps ), match.Groups[ 1 ].Value );
						blendDataObj.BlendOpRGB = blendOpsRGB;
						if ( match.Groups[ 2 ].Success )
						{
							AvailableBlendOps blendOpsA = ( AvailableBlendOps ) Enum.Parse( typeof( AvailableBlendOps ), match.Groups[ 2 ].Value );
							blendDataObj.BlendOpAlpha = blendOpsA;
							blendDataObj.SeparateBlendOps = true;
						}
						else
						{
							blendDataObj.SeparateBlendOps = false;
						}

					}
					catch ( Exception e )
					{
						Debug.LogException( e );
						blendDataObj.DataCheck = TemplateDataCheck.Unreadable;
						return;
					}
					break;
				}
			}
		}

		public static void CreateZWriteMode( string zWriteData, ref TemplateDepthData depthDataObj )
		{
			depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
			MatchCollection matchColl = Regex.Matches( zWriteData, ZWritePattern );
			if ( matchColl.Count > 0 )
			{
				if ( matchColl[ 0 ].Groups.Count == 2 )
				{
					try
					{
						depthDataObj.ZWriteModeValue = ( ZWriteMode ) Enum.Parse( typeof( ZWriteMode ), matchColl[ 0 ].Groups[ 1 ].Value );
						depthDataObj.DataCheck = TemplateDataCheck.Valid;
						depthDataObj.ValidZWrite = true;
					}
					catch
					{
						depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
					}
				}
			}
		}

		public static void FetchLocalVars( string body, ref List<TemplateLocalVarData> localVarList, TemplateFunctionData vertexFunction, TemplateFunctionData fragFunction )
		{
			foreach ( Match match in Regex.Matches( body, LocalVarPattern ) )
			{
				if ( match.Groups.Count == 4 )
				{
					if ( CgToWirePortType.ContainsKey( match.Groups[ 2 ].Value ) )
					{
						MasterNodePortCategory category;
						if ( fragFunction.MainBodyLocalIdx > vertexFunction.MainBodyLocalIdx )
						{
							if ( match.Index < fragFunction.MainBodyLocalIdx )
							{
								category = MasterNodePortCategory.Vertex;
							}
							else
							{
								category = MasterNodePortCategory.Fragment;
							}
						}
						else
						{
							if ( match.Index < vertexFunction.MainBodyLocalIdx )
							{
								category = MasterNodePortCategory.Fragment;
							}
							else
							{
								category = MasterNodePortCategory.Vertex;
							}
						}

						if ( !string.IsNullOrEmpty( match.Groups[ 1 ].Value ) && ShortcutToInfo.ContainsKey( match.Groups[ 1 ].Value ) )
						{
							string id = match.Groups[ 0 ].Value.Substring( 0, match.Groups[ 0 ].Value.IndexOf( "*/" ) + 2 );
							TemplateLocalVarData data = new TemplateLocalVarData( ShortcutToInfo[ match.Groups[ 1 ].Value ], id, CgToWirePortType[ match.Groups[ 2 ].Value ], category, match.Groups[ 3 ].Value, match.Index );
							localVarList.Add( data );
						}
						else
						{
							TemplateLocalVarData data = new TemplateLocalVarData( CgToWirePortType[ match.Groups[ 2 ].Value ], category, match.Groups[ 3 ].Value, match.Index );
							localVarList.Add( data );
						}

					}
				}
			}
		}

		public static TemplateSRPType CreateTags( ref TemplateTagsModuleData tagsObj, bool isSubShader )
		{
			TemplateSRPType srpType = TemplateSRPType.BuiltIn;
			MatchCollection matchColl = Regex.Matches( tagsObj.TagsId, TagsPattern, RegexOptions.IgnorePatternWhitespace );
			int count = matchColl.Count;
			if ( count > 0 )
			{
				for ( int i = 0; i < count; i++ )
				{
					if ( matchColl[ i ].Groups.Count == 3 )
					{
						if ( isSubShader && matchColl[ i ].Groups[ 1 ].Value.Equals( "RenderPipeline" ) )
						{
							if ( TagToRenderPipeline.ContainsKey( matchColl[ i ].Groups[ 2 ].Value ) )
								srpType = TagToRenderPipeline[ matchColl[ i ].Groups[ 2 ].Value ];
						}
						tagsObj.Tags.Add( new TemplatesTagData( matchColl[ i ].Groups[ 1 ].Value, matchColl[ i ].Groups[ 2 ].Value ) );
					}
				}
			}
			return srpType;
		}

		public static void CreateZTestMode( string zTestData, ref TemplateDepthData depthDataObj )
		{
			depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
			MatchCollection matchColl = Regex.Matches( zTestData, ZTestPattern );
			if ( matchColl.Count > 0 )
			{
				if ( matchColl[ 0 ].Groups.Count == 2 )
				{
					try
					{
						depthDataObj.ZTestModeValue = ( ZTestMode ) Enum.Parse( typeof( ZTestMode ), matchColl[ 0 ].Groups[ 1 ].Value );
						depthDataObj.DataCheck = TemplateDataCheck.Valid;
						depthDataObj.ValidZTest = true;
					}
					catch
					{
						depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
					}
				}
			}
		}

		public static void CreateZOffsetMode( string zOffsetData, ref TemplateDepthData depthDataObj )
		{
			depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
			MatchCollection matchColl = Regex.Matches( zOffsetData, ZOffsetPattern );
			if ( matchColl.Count > 0 )
			{
				if ( matchColl[ 0 ].Groups.Count == 3 )
				{
					try
					{
						depthDataObj.OffsetFactor = Convert.ToSingle( matchColl[ 0 ].Groups[ 1 ].Value );
						depthDataObj.OffsetUnits = Convert.ToSingle( matchColl[ 0 ].Groups[ 2 ].Value );
						depthDataObj.ValidOffset = true;
						depthDataObj.DataCheck = TemplateDataCheck.Valid;
					}
					catch
					{
						depthDataObj.DataCheck = TemplateDataCheck.Unreadable;
					}
				}
			}
		}


		public static List<TemplateVertexData> CreateVertexDataList( string vertexData, string parametersBody )
		{
			List<TemplateVertexData> vertexDataList = null;
			Dictionary<TemplateSemantics, TemplateVertexData> vertexDataDict = null;

			foreach ( Match match in Regex.Matches( vertexData, VertexDataPattern ) )
			{
				if ( match.Groups.Count > 1 )
				{
					if ( vertexDataList == null )
					{
						vertexDataList = new List<TemplateVertexData>();
						vertexDataDict = new Dictionary<TemplateSemantics, TemplateVertexData>();
					}

					WirePortDataType dataType = CgToWirePortType[ match.Groups[ 1 ].Value ];
					string varName = match.Groups[ 2 ].Value;
					TemplateSemantics semantics = ( TemplateSemantics ) Enum.Parse( typeof( TemplateSemantics ), match.Groups[ 3 ].Value );
					TemplateVertexData templateVertexData = new TemplateVertexData( semantics, dataType, varName );
					vertexDataList.Add( templateVertexData );
					vertexDataDict.Add( semantics, templateVertexData );
				}
			}

			string[] paramsArray = parametersBody.Split( IOUtils.FIELD_SEPARATOR );
			if ( paramsArray.Length > 0 )
			{
				for ( int i = 0; i < paramsArray.Length; i++ )
				{
					string[] paramDataArr = paramsArray[ i ].Split( IOUtils.VALUE_SEPARATOR );
					if ( paramDataArr.Length == 2 )
					{
						string[] swizzleInfoArr = paramDataArr[ 1 ].Split( IOUtils.FLOAT_SEPARATOR );
						TemplateSemantics semantic = ShortcutToSemantic[ swizzleInfoArr[ 0 ] ];
						if ( vertexDataDict.ContainsKey( semantic ) )
						{
							TemplateVertexData templateVertexData = vertexDataDict[ semantic ];
							if ( templateVertexData != null )
							{
								if ( swizzleInfoArr.Length > 1 )
								{
									templateVertexData.DataSwizzle = "." + swizzleInfoArr[ 1 ];
								}
								templateVertexData.DataInfo = ShortcutToInfo[ paramDataArr[ 0 ] ];
								templateVertexData.Available = true;
							}
						}
					}
				}
			}

			vertexDataDict.Clear();
			vertexDataDict = null;
			return vertexDataList;
		}

		public static TemplateInterpData CreateInterpDataList( string interpData, string fullLine, int maxInterpolators )
		{
			TemplateInterpData interpDataObj = null;
			List<TemplateVertexData> interpDataList = null;
			Dictionary<TemplateSemantics, TemplateVertexData> interpDataDict = null;
			Match rangeMatch = Regex.Match( fullLine, InterpRangePattern );
			if ( rangeMatch.Groups.Count > 0 )
			{
				interpDataObj = new TemplateInterpData();
				// Get range of available interpolators
				int minVal = 0;
				int maxVal = 0;
				try
				{
					string[] minValArgs = rangeMatch.Groups[ 1 ].Value.Split( IOUtils.FLOAT_SEPARATOR );
					minVal = Convert.ToInt32( minValArgs[ 0 ] );
					if ( string.IsNullOrEmpty( rangeMatch.Groups[ 2 ].Value ) )
					{
						maxVal = maxInterpolators - 1;
						interpDataObj.DynamicMax = true;
					}
					else
					{
						maxVal = Convert.ToInt32( rangeMatch.Groups[ 2 ].Value );
					}
					if ( minVal > maxVal )
					{
						int aux = minVal;
						minVal = maxVal;
						maxVal = aux;
					}
					for ( int i = minVal; i <= maxVal; i++ )
					{
						interpDataObj.AvailableInterpolators.Add( new TemplateInterpElement( IntToSemantic[ i ] ) );
					}
					if ( minValArgs.Length > 1 )
					{
						interpDataObj.AvailableInterpolators[ 0 ].SetAvailableChannelsFromString( minValArgs[ 1 ] );
					}
				}
				catch ( Exception e )
				{
					Debug.LogException( e );
				}

				interpDataList = new List<TemplateVertexData>();
				interpDataDict = new Dictionary<TemplateSemantics, TemplateVertexData>();

				//Get Current interpolators
				int parametersBeginIdx = fullLine.IndexOf( ":" ) + 1;
				int parametersEnd = fullLine.IndexOf( TemplatesManager.TemplateEndOfLine );
				string parametersBody = fullLine.Substring( parametersBeginIdx, parametersEnd - parametersBeginIdx );

				foreach ( Match match in Regex.Matches( interpData, VertexDataPattern ) )
				{
					if ( match.Groups.Count > 1 )
					{
						WirePortDataType dataType = CgToWirePortType[ match.Groups[ 1 ].Value ];
						string varName = match.Groups[ 2 ].Value;
						TemplateSemantics semantics = ( TemplateSemantics ) Enum.Parse( typeof( TemplateSemantics ), match.Groups[ 3 ].Value );
						TemplateVertexData templateVertexData = new TemplateVertexData( semantics, dataType, varName );
						//interpDataList.Add( templateVertexData );
						interpDataDict.Add( semantics, templateVertexData );
						interpDataObj.RawInterpolators.Add( templateVertexData );
						//Check if they are also on the free channels list and update their names
						interpDataObj.ReplaceNameOnInterpolator( semantics, varName );
					}
				}

				Dictionary<string, TemplateVertexData> auxDict = new Dictionary<string, TemplateVertexData>();
				// Get info for available interpolators
				string[] paramsArray = parametersBody.Split( IOUtils.FIELD_SEPARATOR );
				if ( paramsArray.Length > 0 )
				{
					for ( int i = 0; i < paramsArray.Length; i++ )
					{
						string[] paramDataArr = paramsArray[ i ].Split( IOUtils.VALUE_SEPARATOR );
						if ( paramDataArr.Length == 2 )
						{
							string[] swizzleInfoArr = paramDataArr[ 1 ].Split( IOUtils.FLOAT_SEPARATOR );
							TemplateSemantics semantic = ShortcutToSemantic[ swizzleInfoArr[ 0 ] ];
							if ( interpDataDict.ContainsKey( semantic ) )
							{
								if ( interpDataDict[ semantic ] != null )
								{
									string[] multiComponent = paramDataArr[ 0 ].Split( IOUtils.FLOAT_SEPARATOR );

									if ( multiComponent.Length > 1 )
									{
										TemplateVertexData templateInterpData = null;
										if ( auxDict.ContainsKey( multiComponent[ 0 ] ) )
										{
											templateInterpData = auxDict[ multiComponent[ 0 ] ];
										}
										else
										{
											templateInterpData = new TemplateVertexData( interpDataDict[ semantic ] );
											//if( swizzleInfoArr.Length > 1 )
											//{
											//	templateInterpData.DataSwizzle = "." + swizzleInfoArr[ 1 ];
											//}
											templateInterpData.DataInfo = ShortcutToInfo[ multiComponent[ 0 ] ];
											templateInterpData.Available = true;
											interpDataList.Add( templateInterpData );
											auxDict.Add( multiComponent[ 0 ], templateInterpData );
										}

										if ( swizzleInfoArr[ 1 ].Length == multiComponent[ 1 ].Length )
										{
											for ( int channelIdx = 0; channelIdx < swizzleInfoArr[ 1 ].Length; channelIdx++ )
											{
												templateInterpData.RegisterComponent( multiComponent[ 1 ][ channelIdx ], interpDataDict[ semantic ].VarName + "." + swizzleInfoArr[ 1 ][ channelIdx ] );
											}
										}
									}
									else
									{
										TemplateVertexData templateInterpData = new TemplateVertexData( interpDataDict[ semantic ] );
										if ( swizzleInfoArr.Length > 1 )
										{
											templateInterpData.DataSwizzle = "." + swizzleInfoArr[ 1 ];
										}
										templateInterpData.DataInfo = ShortcutToInfo[ paramDataArr[ 0 ] ];
										templateInterpData.Available = true;
										interpDataList.Add( templateInterpData );
									}
								}
							}
						}
					}
				}

				/*TODO: 
				1) Remove interpDataList.Add( templateVertexData ); from initial foreach 
				2) When looping though each foreach array element, create a new TemplateVertexData
				from the one containted on the interpDataDict and add it to interpDataList
				*/
				for ( int i = 0; i < interpDataList.Count; i++ )
				{
					interpDataList[ i ].BuildVar();
				}

				auxDict.Clear();
				auxDict = null;

				interpDataObj.Interpolators = interpDataList;
				interpDataDict.Clear();
				interpDataDict = null;
			}
			return interpDataObj;
		}

		public static void FetchCustomInspector( TemplateInfoContainer inspectorContainer, string body )
		{
			Match match = Regex.Match( body, CustomInspectorPattern, RegexOptions.Multiline );
			if( match != null && match.Groups.Count > 1 )
			{
				inspectorContainer.Index = match.Index;
				inspectorContainer.Id = match.Groups[ 0 ].Value;
				inspectorContainer.Data = match.Groups[ 1 ].Value;
			}
		}

		public static string AutoSwizzleData( string dataVar, WirePortDataType dataType, WirePortDataType swizzle )
		{
			switch ( dataType )
			{
				case WirePortDataType.COLOR:
				case WirePortDataType.FLOAT4:
				{
					switch ( swizzle )
					{
						case WirePortDataType.FLOAT3: dataVar += ".xyz"; break;
						case WirePortDataType.FLOAT2: dataVar += ".xy"; break;
						case WirePortDataType.INT:
						case WirePortDataType.FLOAT: dataVar += ".x"; break;
					}
				}
				break;
				case WirePortDataType.FLOAT3:
				{
					switch ( swizzle )
					{
						case WirePortDataType.FLOAT4: dataVar = string.Format( "float4({0},0)", dataVar ); break;
						case WirePortDataType.FLOAT2: dataVar += ".xy"; break;
						case WirePortDataType.INT:
						case WirePortDataType.FLOAT: dataVar += ".x"; break;
					}
				}
				break;
				case WirePortDataType.FLOAT2:
				{
					switch ( swizzle )
					{
						case WirePortDataType.FLOAT4: dataVar = string.Format( "float4({0},0,0)", dataVar ); break;
						case WirePortDataType.FLOAT3: dataVar = string.Format( "float3({0},0)", dataVar ); break;
						case WirePortDataType.INT:
						case WirePortDataType.FLOAT: dataVar += ".x"; break;
					}
				}
				break;
				case WirePortDataType.FLOAT:
				{
					switch ( swizzle )
					{
						case WirePortDataType.FLOAT4: dataVar = string.Format( "float4({0},0,0,0)", dataVar ); break;
						case WirePortDataType.FLOAT3: dataVar = string.Format( "float3({0},0,0)", dataVar ); break;
						case WirePortDataType.FLOAT2: dataVar = string.Format( "float2({0},0)", dataVar ); break;
					}
				}
				break;
			}
			return dataVar;
		}

		public static bool CheckIfTemplate( string assetPath )
		{
			Shader shader = AssetDatabase.LoadAssetAtPath<Shader>( assetPath );
			if ( shader != null )
			{
				string body = IOUtils.LoadTextFileFromDisk( assetPath );
				return ( body.IndexOf( TemplatesManager.TemplateShaderNameBeginTag ) > -1 );
			}
			return false;
		}

		public static bool CheckIfCompatibles( WirePortDataType first, WirePortDataType second )
		{
			switch ( first )
			{
				case WirePortDataType.OBJECT:
				return true;
				case WirePortDataType.FLOAT:
				case WirePortDataType.FLOAT2:
				case WirePortDataType.FLOAT3:
				case WirePortDataType.FLOAT4:
				case WirePortDataType.COLOR:
				case WirePortDataType.INT:
				{
					switch ( second )
					{
						case WirePortDataType.FLOAT3x3:
						case WirePortDataType.FLOAT4x4:
						case WirePortDataType.SAMPLER1D:
						case WirePortDataType.SAMPLER2D:
						case WirePortDataType.SAMPLER3D:
						case WirePortDataType.SAMPLERCUBE:
						return false;
					}
				}
				break;
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					switch ( second )
					{
						case WirePortDataType.FLOAT:
						case WirePortDataType.FLOAT2:
						case WirePortDataType.FLOAT3:
						case WirePortDataType.FLOAT4:
						case WirePortDataType.COLOR:
						case WirePortDataType.INT:
						case WirePortDataType.SAMPLER1D:
						case WirePortDataType.SAMPLER2D:
						case WirePortDataType.SAMPLER3D:
						case WirePortDataType.SAMPLERCUBE:
						return false;
					}
				}
				break;
				case WirePortDataType.SAMPLER1D:
				case WirePortDataType.SAMPLER2D:
				case WirePortDataType.SAMPLER3D:
				case WirePortDataType.SAMPLERCUBE:
				{
					switch ( second )
					{
						case WirePortDataType.FLOAT:
						case WirePortDataType.FLOAT2:
						case WirePortDataType.FLOAT3:
						case WirePortDataType.FLOAT4:
						case WirePortDataType.FLOAT3x3:
						case WirePortDataType.FLOAT4x4:
						case WirePortDataType.COLOR:
						case WirePortDataType.INT:
						return false;
					}
				}
				break;
			}
			return true;
		}
	}
}
