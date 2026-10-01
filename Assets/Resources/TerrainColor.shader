Shader "Afterglow/TerrainColor" {
Properties { _Color("Tint", Color) = (1,1,1,1) }
SubShader { Tags { "RenderType"="Opaque" } LOD 200
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input { float4 color:COLOR; float3 worldPos; float3 worldNormal; INTERNAL_DATA };
fixed4 _Color;
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
void surf(Input IN,inout SurfaceOutputStandard o) {
float n=noise(IN.worldPos.xz*.7),detail=noise(IN.worldPos.xz*8);float strata=sin(IN.worldPos.y*2.2+noise(IN.worldPos.xz*.12)*2)*.5+.5;
float slope=1-saturate(IN.worldNormal.y);float3 tint=IN.color.rgb*_Color.rgb;
o.Albedo=tint*lerp(.78,1.15,n)*lerp(.90,1.05,detail)*lerp(1,.82+strata*.25,slope);
o.Smoothness=.12;o.Metallic=0;
}
ENDCG
} FallBack "Diffuse" }
