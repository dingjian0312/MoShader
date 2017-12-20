﻿Shader "Mo/SSAO"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
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

		fixed4 fragAO (v2f i) : SV_Target
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
				float3 s_dir = PickSamplePoint(i.uv, s);
				s_dir = faceforward(s_dir, -viewNormal, s_dir); //faceforward(N,I,Ng)	如果Ng∙I<0，返回N；否则返回-N。
				//采样点的相机空间坐标
				float3 vpos_s1 = vpos_o + s_dir;
				float depth_s1 = -vpos_s1.z; //采样点的相机深度
				float4 clip_pos_s1 = mul(unity_CameraProjection, float4(vpos_s1, 1));
				float2 uv_s1 = (clip_pos_s1.xy / clip_pos_s1.w + 1.0) * 0.5;
				//采样点对应的深度图里的深度
				float depth_tex = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv_s1));

				//深度差太大的不连续空间
				float rangeCheck= abs(depth_s1 - depth_tex) < _Radius ? 1.0 : 0.0;
				ao = ao + ((depth_s1 > depth_tex) ? rangeCheck : 0);
			}
			ao = pow(ao*_Intensity/_SampleCount, kContrast);

			//背景不处理
			ao = (linearDepth >= 1 ? 0 : ao);

			return ao;
		}

		sampler2D _AOTex;
		fixed4 fragCombineAO (v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);
			fixed ao = tex2D(_AOTex, i.uv);
			col.rgb = col.rgb * (1-ao);
			return col;
		}
		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragAO
			ENDCG
		}

		UsePass "Mo/Gaussian Blur/GAUSSIAN_BLUR_VERTICAL"
		
		UsePass "Mo/Gaussian Blur/GAUSSIAN_BLUR_HORIZONTAL"

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragCombineAO
			ENDCG
		}
	}
}
