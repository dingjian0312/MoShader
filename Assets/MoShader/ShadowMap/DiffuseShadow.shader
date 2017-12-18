Shader "Mo/DiffuseShadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShadowBias ("ShadowBias", float) = 0.01
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 shadowPos : TEXCOORD1;
				float lightDepth : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ShadowBias;

			sampler2D _ShadowMap;
			float4x4 _LightMatrixV;
			float4x4 _LightMatrixP;
			float _lightCameraSize;
			float _lightCameraFar;
			float _ShadowMapSize;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 lightPos = mul(_LightMatrixV, worldPos);
				o.shadowPos = mul(_LightMatrixP, lightPos);
				o.lightDepth = -lightPos.z / _lightCameraFar;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float2 shadowUV = i.shadowPos * 0.5 + 0.5;

				float shadow = 0;
				for (int s1 = -1; s1 <= 1; ++s1)
				{
					for (int s2 = -1; s2 <= 1; ++s2)
					{
						shadowUV = shadowUV + float2(s1, s2) * (1/_ShadowMapSize);
						//shadow += i.lightDepth > DecodeFloatRG(tex2D(_ShadowMap, shadowUV)) + _ShadowBias;
						shadow += i.lightDepth > tex2D(_ShadowMap, shadowUV).b + _ShadowBias;
					}
				}

				shadow = shadow / 9;
				col = col * (1- shadow * 0.8);
				return col;
			}
			ENDCG
		}
	}
}
