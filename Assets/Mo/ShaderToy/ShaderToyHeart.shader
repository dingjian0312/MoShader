Shader "Mo/Shadertoy/Heart" { 
    Properties{
        iMouse ("Mouse Pos", Vector) = (100, 100, 0, 0)
        iChannel0("iChannel0", 2D) = "white" {}  
        iChannelResolution0 ("iChannelResolution0", Vector) = (100, 100, 0, 0)
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

    fixed4 iMouse;
    sampler2D iChannel0;
    fixed4 iChannelResolution0;

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

    vec4 main(vec2 fragCoord) {
        vec2 p = (2.0 * fragCoord-iResolution.xy)/min(iResolution.y,iResolution.x);
	
		// background color
		vec3 bcol = vec3(1.0,0.8,0.7-0.07*p.y)*(1.0-0.25*length(p));

		// animate
		float tt = mod(iTime,1.5)/1.5;
		float ss = pow(tt,.2)*0.5 + 0.5;
		ss = 1.0 + ss*0.5*sin(tt*6.2831*3.0 + p.y*0.5)*exp(-tt*4.0);
		p *= vec2(0.5,1.5) + ss*vec2(0.5,-0.5);

		// shape
	#if 0
		p *= 0.8;
		p.y = -0.1 - p.y*1.2 + abs(p.x)*(1.0-abs(p.x));
		float r = length(p);
		float d = 0.5;
	#else
		p.y -= 0.25;
		float a = atan(p.x,p.y)/3.141593;
		float r = length(p);
		float h = abs(a);
		float d = (13.0*h - 22.0*h*h + 10.0*h*h*h)/(6.0-5.0*h);
	#endif
    
		// color
		float s = 0.75 + 0.75*p.x;
		s *= 1.0-0.4*r;
		s = 0.3 + 0.7*s;
		s *= 0.5+0.5*pow( 1.0-clamp(r/d, 0.0, 1.0 ), 0.1 );
		vec3 hcol = vec3(1.0,0.5*r,0.3)*s;
	
		vec3 col = mix( bcol, hcol, smoothstep(-0.01, 0.01, d-r) );

		return vec4(col,1.0);
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