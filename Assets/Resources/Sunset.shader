Shader "Afterglow/Sunset" {
Properties { _SunDir("Sun direction", Vector) = (-0.5,0.18,0.8,0) }
SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata { float4 vertex:POSITION; }; struct v2f { float4 vertex:SV_POSITION;float3 dir:TEXCOORD0; };
float4 _SunDir;
v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
fixed4 frag(v2f i):SV_Target {
float3 d=normalize(i.dir);float h=saturate(d.y);float3 c=lerp(float3(.90,.54,.32),float3(.10,.27,.43),pow(saturate(h*3),.65));
float sun=dot(d,normalize(_SunDir.xyz));c+=float3(1,.53,.2)*pow(saturate(sun),24)*.45;c+=float3(4,2.8,1.5)*smoothstep(.9995,.9998,sun);
float cloud=sin(d.x*22+d.z*7+sin(d.z*32)*.3)*sin(d.z*43-d.x*8);c+=float3(.24,.17,.15)*smoothstep(.38,.9,cloud)*smoothstep(.04,.15,d.y)*(1-smoothstep(.18,.35,d.y));
return float4(GammaToLinearSpace(c),1);}
ENDCG
} } }
