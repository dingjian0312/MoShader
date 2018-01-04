Shader "Mo/SSR"
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
		float _MaxRayDistance;
		float _RenderBufferSize;

		inline bool RayMarching( float3 rayOrigin, float3 rayDirection, float jitter, out float2 hitPixel, 
								out float3 hitPoint, out float iterationCount, bool debugHalf) 
		{
			float limitLength = (-_ProjectionParams.y - rayOrigin.z) / rayDirection.z;
			float rayLength;
			//限制在远近平面之内
			if (rayOrigin.z + rayDirection.z * _MaxRayDistance > -_ProjectionParams.y)
			{
				rayLength = limitLength;
			}
			else
			{
				rayLength = _MaxRayDistance;
			}
			
			float3 rayEnd = rayOrigin + rayDirection*rayLength;
			
			//两端点转换到齐次裁减空间
			float4 clipStart = mul(unity_CameraProjection, float4(rayOrigin, 1));
			float4 clipEnd = mul(unity_CameraProjection, float4(rayEnd, 1));

			//光栅化要对1/z进行插值
			float zStart = 1 / clipStart.w;
			float zEnd = 1 / clipEnd.w;

			
			float2 screenStart = clipStart.xy/clipStart.w;	
			float2 screenEnd = clipEnd.xy/clipEnd.w;
			
			float2 delta = screenEnd - screenStart;
			bool permute = false;
			if (abs(delta.x) < abs(delta.y)) 
			{ 
			    permute = true; 
				delta = delta.yx; 
				screenStart = screenStart.yx; 
				screenEnd = screenEnd.yx; 
			}
			
			float stepDir = sign(delta.x);	//如果x>0返回1, x<0返回-1, 否则返回0	

			return true;
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

			viewPos = normalize(viewPos);
			viewNormal = normalize(viewNormal);
			float3 reflectDir = reflect(viewPos, viewNormal);
			
			float2 uv2 = i.uv * _RenderBufferSize;
			float c = (uv2.x + uv2.y) * 0.25;
			float jitter = fmod( c, 1.0);
			float2 hitPixel; 
			float3 hitPoint;
			float iterationCount;
			//bool hit = RayMarching(viewPos, reflectDir, jitter, hitPixel, hitPoint, iterationCount, i.uv.x > 0.5);


			fixed4 col = tex2D(_MainTex, i.uv);
			// just invert the colors
			//col = 1 - col;
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
