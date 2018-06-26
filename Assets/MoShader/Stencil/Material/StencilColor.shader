Shader "Mo/StencilColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		CGINCLUDE
		#include "UnityCG.cginc"
		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		v2f vert (appdata_img v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}

		fixed4 frag1 (v2f i) : SV_Target
		{
			float bit = 1.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag2 (v2f i) : SV_Target
		{
			float bit = 2.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag4 (v2f i) : SV_Target
		{
			float bit = 4.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag8 (v2f i) : SV_Target
		{
			float bit = 8.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag16 (v2f i) : SV_Target
		{
			float bit = 16.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag32 (v2f i) : SV_Target
		{
			float bit = 32.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag64 (v2f i) : SV_Target
		{
			float bit = 64.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		fixed4 frag128 (v2f i) : SV_Target
		{
			float bit = 128.0 / 255;
			return half4(bit, bit, bit, 1);
		}
		ENDCG
		
		
		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Stencil
			{
				Ref 1
				ReadMask 1
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag1
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 2
				ReadMask 2
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag2
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 4
				ReadMask 4
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag4
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 8
				ReadMask 8
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag8
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 16
				ReadMask 16
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag16
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 32
				ReadMask 32
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag32
			ENDCG
		}

		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 64
				ReadMask 64
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag64
			ENDCG
		}


		Pass
		{
			ZTest Always ZWrite Off Cull Off
			Blend One One
			Stencil
			{
				Ref 128
				ReadMask 128
				Comp Equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag128
			ENDCG
		}
	}
}
