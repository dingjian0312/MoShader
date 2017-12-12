Shader "Mo/Shadertoy/Circle" { 
    Properties{
		_Parameters ("Circle Parameters", Vector) = (0.5, 0.5, 10, 0) // Center: (x, y), Radius: z  
		_Color ("Circle Color", Color) = (1, 1, 1, 1)  
		_BgColor("Bg Color", Color) = (0, 0, 0, 1)
    }

    CGINCLUDE    
    #include "UnityCG.cginc"   
    #pragma target 3.0      

    #define vec2 float2
    #define vec3 float3
    #define vec4 float4
    #define mat2 float2x2
    #define mat3 float3x3
    #define mat4 float4x4
    #define iTime _Time.y
    #define mod fmod
    #define mix lerp
    #define fract frac
    #define texture2D tex2D
    #define iResolution _ScreenParams
    #define gl_FragCoord ((_iParam.scrPos.xy/_iParam.scrPos.w) * _ScreenParams.xy)

    #define PI2 6.28318530718
    #define pi 3.14159265358979
    #define halfpi (pi * 0.5)
    #define oneoverpi (1.0 / pi)
	#define atan atan2

    float4 _Parameters;
    float4 _Color, _BgColor;

    struct v2f {    
        float4 pos : SV_POSITION;    
        float4 scrPos : TEXCOORD0;   
    };              

    v2f vert(appdata_base v) {  
        v2f o;
        o.pos = UnityObjectToClipPos (v.vertex);
        o.scrPos = ComputeScreenPos(o.pos);
        return o;
    }  

    vec4 main(vec2 fragCoord);

    fixed4 frag(v2f _iParam) : COLOR0 { 
        vec2 fragCoord = gl_FragCoord;
        return main(gl_FragCoord);
    }  

	vec4 circle(vec2 pos, vec2 center, float radius, float4 color, float antialias)
	{
		float d = length(pos - center) - radius;  
		float t = smoothstep(-antialias, antialias, d);  
		return mix(color, _BgColor, t);  //抗锯齿
	}

    vec4 main(vec2 fragCoord) 
	{
		return circle(fragCoord, _Parameters.xy*iResolution.xy, _Parameters.z, _Color, _Parameters.w);
    }

    ENDCG    

    SubShader {    
        Pass {    
            CGPROGRAM    

            #pragma vertex vert    
            #pragma fragment frag    
            #pragma fragmentoption ARB_precision_hint_fastest     

            ENDCG    
        }    
    }     
    FallBack Off    
}