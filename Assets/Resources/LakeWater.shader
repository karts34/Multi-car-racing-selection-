Shader "Afterglow/LakeWater" {
Properties { _Color("Water",Color)=(.025,.115,.14,1) }
SubShader { Tags { "RenderType"="Opaque" } LOD 200
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input { float3 worldPos; float2 uv_MainTex; };
fixed4 _Color;
void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float t=_Time.y;float a=sin(p.x*1.7+p.y*1.15+t*.8),b=cos(p.y*1.87-p.x*.70+t*.55);o.Albedo=_Color.rgb*(.95+(a*b)*.06);o.Normal=normalize(float3(a*.014,b*.018,1));o.Metallic=.15;o.Smoothness=.82;o.Emission=_Color.rgb*.10;o.Alpha=1;}
ENDCG
} FallBack "Diffuse" }
