Shader "Mo/SSRSimple"
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
			
		sampler2D _MainTex;
		float step;
		sampler2D _CameraGBufferTexture1;
		sampler2D _CameraGBufferTexture2;
		sampler2D _CameraDepthTexture;
		sampler2D _BackFaceDepthTex;

		float _MaxRayDistance;
		float _RenderBufferSize;
		int _Iterations;
		float _PixelZSize;
		float _thickness;
		float _stepLength;
		#define MAX_ITERATION 20


		inline bool RayMarching(float3 rayStart, float3 rayDirection, inout float2 hitPixel) 
		{
			bool intersect = false;

			float3 rayPoint = rayStart;

			for (int i = 0; i < MAX_ITERATION; ++i)
			{
				rayPoint = rayPoint + rayDirection * _stepLength;

				float4 clipPos = mul(unity_CameraProjection, rayPoint);
				clipPos.xy = clipPos.xy/clipPos.w;
				float2 curUV = (clipPos.xy + 1) * 0.5;

				if (curUV.x < 0 || curUV.x > 1 || curUV.y < 0 || curUV.y > 1) break;

				float frontDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, curUV));
				float backDepth = tex2D(_BackFaceDepthTex, curUV) * _ProjectionParams.z;
				float rayDepth = -rayPoint.z;

				if (rayDepth >= frontDepth && rayDepth <= backDepth)
				{
					intersect = true;
					hitPixel = curUV;
					break;
				}
			}

			//二分搜索提高精度
			if (intersect)
			{
				float3 left = rayPoint - rayDirection * _stepLength;
				float3 right = rayPoint;
				for (int i = 0; i < 30; ++i)
				{
					float3 mid = (left+right)/2;
					float4 clipPos = mul(unity_CameraProjection, mid);
					clipPos.xy = clipPos.xy/clipPos.w;
					float2 curUV = (clipPos.xy + 1) * 0.5;

					float frontDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, curUV));
					//float backDepth = tex2D(_BackFaceDepthTex, curUV) * _ProjectionParams.z;
					float rayDepth = -mid.z;

					if (rayDepth < frontDepth) left = mid;
					else if (rayDepth > frontDepth) right = mid;
					hitPixel = curUV;
				}
			}

			return intersect;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			float zBuffer = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			float eyeDepth = LinearEyeDepth(zBuffer);
			float linearDepth = Linear01Depth(zBuffer);

			float smoothness = tex2D(_CameraGBufferTexture1, i.uv).a;

			//法线转换到相机空间
			float3 worldNormal = tex2D(_CameraGBufferTexture2, i.uv).xyz;
			worldNormal = worldNormal * 2 - any(worldNormal); // gets (0,0,0) when normal == 0
			float3 viewNormal = mul((float3x3)unity_WorldToCamera, worldNormal); //相机局部空间
			viewNormal.z = -viewNormal.z;

			//获取相机空间坐标
			float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
			float2 p13_23 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
			float3 viewPos = float3((i.uv * 2 - 1) / p11_22 * eyeDepth, -eyeDepth);

			float3 viewDir = normalize(viewPos);
			viewNormal = normalize(viewNormal);
			float3 reflectDir = normalize(reflect(viewDir, viewNormal));
			
			fixed4 col = tex2D(_MainTex, i.uv);
			if (smoothness > 0.001)
			{
				float2 hitPixel = i.uv;
				bool hit = RayMarching(viewPos, reflectDir, hitPixel);
				col = tex2D(_MainTex, hitPixel);
			}

			
			//float frontDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv));
			//float backDepth = tex2D(_BackFaceDepthTex, i.uv);
			//col = backDepth/_ProjectionParams.z;
			
			return col;
		}
		ENDCG
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
