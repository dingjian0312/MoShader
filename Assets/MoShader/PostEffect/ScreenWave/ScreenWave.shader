Shader "Mo/Screen Wave" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
	}
	SubShader 
	{
		Pass 
		{  
			ZTest Always Cull Off ZWrite Off
			
			CGPROGRAM
			
			#include "UnityCG.cginc"
			
			#pragma vertex vert  
			#pragma fragment frag
			
			sampler2D _MainTex;  
			float _speed;
			float _distanceFactor;
			float _amplitude;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};
			  
			v2f vert(appdata_img v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target 
			{
				float2 screenPos = i.screenPos.xy / i.screenPos.w;
				float2 delta = screenPos-float2(0.5, 0.5);
				delta = delta* float2(_ScreenParams.x/_ScreenParams.y, 1); //圆形扩散
				float dis = length(delta);
				float2 uv_offset = normalize(delta) * sin(dis*_distanceFactor + _Time.y*_speed)*_amplitude;
				float4 col = tex2D(_MainTex, i.uv+uv_offset);
				//col += length(uv_offset)*10;
				return col;
 			}
			
			ENDCG
		} 
	}
	FallBack Off
}
