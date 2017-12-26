Shader "Mo/RenderLightDepth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
				float4 shadowPos : TEXCOORD0;
				//float lightDepth : TEXCOORD2;
			};

			sampler2D _ShadowMap;
			float4x4 _LightMatrixV;
			float4x4 _LightMatrixP;
			//float _lightCameraFar;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 lightPos = mul(_LightMatrixV, worldPos);
				o.shadowPos = mul(_LightMatrixP, lightPos);
				//o.lightDepth = -lightPos.z / _lightCameraFar;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 shadowUV = i.shadowPos * 0.5 + 0.5;
				fixed4 col = tex2D(_ShadowMap, shadowUV);

				if (shadowUV.x <0 || shadowUV.x > 1)
					col = 0;
				if (shadowUV.y <0 || shadowUV.y > 1)
					col = 1;
				//col = i.lightDepth;
				return col;
			}
			ENDCG
		}
	}
}
