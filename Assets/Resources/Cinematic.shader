Shader "Afterglow/Cinematic" {
Properties { _MainTex("Source",2D)="white"{} _Glow("Glow",2D)="black"{} }
SubShader { Cull Off ZWrite Off ZTest Always
Pass {
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;float4 _MainTex_TexelSize;
fixed4 frag(v2f_img i):SV_Target {float3 c=0;for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++){float3 s=tex2D(_MainTex,i.uv+float2(x,y)*_MainTex_TexelSize.xy*2).rgb;c+=max(s-.85,0);}return float4(c/25,1);}
ENDCG
}
Pass {
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;sampler2D _Glow;
fixed4 frag(v2f_img i):SV_Target {float3 c=tex2D(_MainTex,i.uv).rgb;c+=tex2D(_Glow,i.uv).rgb*.28;c=(c*(2.51*c+.03))/(c*(2.43*c+.59)+.14);float2 p=i.uv-.5;c*=1-dot(p,p)*.36;return float4(c,1);}
ENDCG
} } }
