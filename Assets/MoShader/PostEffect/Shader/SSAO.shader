Shader "Mo/SSAO"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			float rand(float2 co)
			{
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			#define MAX_SAMPLE_COUNT 64

			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float3 samplePoint[MAX_SAMPLE_COUNT];
			float sampleCount;
			sampler2D _CameraGBufferTexture2;


			fixed4 frag (v2f i) : SV_Target
			{
				float zBuffer = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float eyeDepth = LinearEyeDepth(zBuffer);


				//法线转换到相机空间
				float3 worldNormal = tex2D(_CameraGBufferTexture2, i.uv).xyz;
				worldNormal = worldNormal * 2 - any(worldNormal); // gets (0,0,0) when normal == 0
				float3 viewNormal = mul((float3x3)unity_WorldToCamera, worldNormal);

				//重建相机空间坐标


				float2 ndc_pos = i.uv * 2 - 1;
				for (int s = 0; s < sampleCount; ++s)
				{

				}

				fixed4 col = tex2D(_MainTex, i.uv);
				//col = 1 - col;
				return col;
			}
			ENDCG
		}
	}
}
