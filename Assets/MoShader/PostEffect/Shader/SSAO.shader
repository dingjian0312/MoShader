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

			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			int _SampleCount;
			float _Radius;
			float _Intensity;
			sampler2D _CameraGBufferTexture2;

			float4x4 _ViewMatrix;

			static const float kContrast = 0.6;
			#define UNITY_PI_2 (UNITY_PI * 2.0)

			float2 CosSin(float theta)
			{
				float sn, cs;
				sincos(theta, sn, cs);
				return float2(cs, sn);
			}
			float GradientNoise(float2 uv)
			{
				uv = floor(uv * _ScreenParams.xy);
				float f = dot(float2(0.06711056, 0.00583715), uv);
				return frac(52.9829189 * frac(f));
			}
			// Pseudo random number generator with 2D coordinates
			float UVRandom(float u, float v)
			{
				float f = dot(float2(12.9898, 78.233), float2(u, v));
				return frac(43758.5453 * sin(f));
			}
			float3 PickSamplePoint(float2 uv, float index)
			{
				float gn = GradientNoise(uv);
				// FIXME: This was added to avoid a NVIDIA driver issue.
				//                                   vvvvvvvvvvvv
				float u = frac(UVRandom(0.0, index + uv.x * 1e-10) + gn) * 2.0 - 1.0;
				float theta = (UVRandom(1.0, index + uv.x * 1e-10) + gn) * UNITY_PI_2;
				float3 v = float3(CosSin(theta) * sqrt(1.0 - u * u), u);
				// Make them distributed between [0, _Radius]
				float l = sqrt((index + 1.0) / _SampleCount) * _Radius;
				return v * l;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float zBuffer = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float eyeDepth = LinearEyeDepth(zBuffer);
				float linearDepth = Linear01Depth(zBuffer);

				//法线转换到相机空间
				float3 worldNormal = tex2D(_CameraGBufferTexture2, i.uv).xyz;
				worldNormal = worldNormal * 2 - any(worldNormal); // gets (0,0,0) when normal == 0
				float3 viewNormal = mul((float3x3)unity_WorldToCamera, worldNormal); //相机局部空间
				viewNormal.z = -viewNormal.z;

				//重建相机空间坐标
				float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
				float2 p13_23 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
				float3 vpos_o = float3((i.uv * 2 - 1) / p11_22 * eyeDepth, -eyeDepth);

				float ao = 0;
				for (int s = 0; s < _SampleCount; ++s)
				{
					float3 v_s1 = PickSamplePoint(i.uv, s);
					v_s1 = faceforward(v_s1, -viewNormal, v_s1); //faceforward(N,I,Ng)	如果Ng∙I<0，返回N；否则返回-N。
					//采样点的相机空间坐标
					float3 vpos_s1 = vpos_o + v_s1;
					float4 spos_s1 = mul(unity_CameraProjection, float4(vpos_s1, 1));
					float2 uv_s1_01 = (spos_s1.xy / spos_s1.w + 1.0) * 0.5;
					//采样点对应的深度图里的深度
					float depth_s1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv_s1_01));
					ao = ao + (depth_s1 < -vpos_s1.z);
				}
				ao = pow(ao*_Intensity/_SampleCount, kContrast);

				fixed4 col = tex2D(_MainTex, i.uv);
				//背景不处理
				col = (linearDepth >= 1 ? col : col * (1-ao));

				return col;
			}
			ENDCG
		}
	}
}
