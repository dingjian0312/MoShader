Shader "Mo/ShadowMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalBias ("Normal Bias", float) = 0.01
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float depth : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _NormalBias;
			v2f vert (appdata_full v)
			{
				v2f o;
				v.vertex.xyz = v.vertex.xyz + -v.normal * 0;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
			
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = EncodeFloatRGBA(i.depth);
				return col;
			}
			ENDCG
		}
	}
}
