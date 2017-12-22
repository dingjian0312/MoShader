Shader "Mo/BackDepth" 
{
	Properties 
	{
		
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float depth : TEXCOORD0;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return float4(i.depth, 0, 0, 0);
			}
			ENDCG
		}
	}
}
