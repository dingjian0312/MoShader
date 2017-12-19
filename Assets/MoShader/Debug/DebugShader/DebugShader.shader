Shader "Unlit/DebugShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ColorMask R

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 viewPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _CameraDepthTexture;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.viewPos = UnityObjectToClipPos(v.vertex);
				float4 viewPos = 
				o.screenPos = ComputeScreenPos(o.vertex);
				o.vertex.z = 2;
				//o.vertex.w = ;
				//o.viewPos.z = 1;
				//o.viewPos.z = o.viewPos.z / o.viewPos.w;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//float z = i.viewPos.z;
				float z = i.viewPos.z / i.viewPos.w;
				//col.rgb = float3(z, z, z);

				//float zBuffer = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos));
				//float zBuffer = Linear01Depth( SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos));
				float zBuffer = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
				col.rgb = zBuffer;

				col = 0;
				col.r = 0.1;
				col.gba = 1;

				return col;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
