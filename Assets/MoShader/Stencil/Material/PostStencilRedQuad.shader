﻿Shader "Mo/PostStencilRedQuad"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Stencil
		{
			Ref 2
			Comp Equal
		}
		
		ZTest Always ZWrite Off Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
				o.vertex = float4(v.vertex.xy * 2, 1, 1);
				o.uv = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return half4(0.6, 0, 0, 1);
			}
			ENDCG
		}
	}
}
