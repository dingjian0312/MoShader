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
			
			float _width;	//水圈的宽度

			#define MAX_POINT_NUMBER 20
			float4 _startPos[MAX_POINT_NUMBER];
			float _radius[MAX_POINT_NUMBER];	//当前扩散的半径
			int _pointNumber;

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

				float4 col = tex2D(_MainTex, i.uv);
				//序号越小越新
				for (int index = _pointNumber-1; index >= 0 ; --index)
				{
					float2 delta = screenPos-_startPos[index];
					delta = delta* float2(_ScreenParams.x/_ScreenParams.y, 1); //圆形扩散
					float dis = length(delta);
					float2 uv_offset = 0;
					if (abs(_radius[index]-dis) < _width)
					{
						uv_offset = normalize(delta) * sin(dis*_density + _Time.y*_speed)*_amplitude;
					}

					//保持前面的水波
					if (length(uv_offset) > 0.001) 
					{
						col = tex2D(_MainTex, i.uv+uv_offset);
					}
				}
				return col;
 			}
			
			ENDCG
		} 
	}
	FallBack Off
}
