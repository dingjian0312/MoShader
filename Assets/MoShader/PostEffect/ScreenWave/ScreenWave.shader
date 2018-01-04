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
			float _speed;	//速度
			float _density; //密度
			float _amplitude; //振幅
			float _radius;	//当前扩散的半径
			float _width;	//水圈的宽度
			float2 _startPos;


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
				float2 delta = screenPos-_startPos;
				delta = delta* float2(_ScreenParams.x/_ScreenParams.y, 1); //圆形扩散
				float dis = length(delta);
				float2 uv_offset = 0;
				if (abs(_radius-dis) < _width) 
				{
					uv_offset = normalize(delta) * sin(dis*_density + _Time.y*_speed)*_amplitude;
				}
				float4 col = tex2D(_MainTex, i.uv+uv_offset);
				return col;
 			}
			
			ENDCG
		} 
	}
	FallBack Off
}
