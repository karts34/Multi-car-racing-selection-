using System.Collections.Generic;
using UnityEngine;

namespace Afterglow
{
public partial class TrackWorld
{
    void PitComplex()
    {
        Strip("Pit lane apron",-25,-10,.04f,asphalt,-70,145);
        Strip("Pit lane edge",-24.4f,-24.2f,.07f,ivory,-70,145);
        var p=Position(20);var q=Rotation(20);
        Box("Paddock foundation",p+q*new Vector3(-39,-.4f,0),new Vector3(27,.8f,114),concrete,q);
        Box("Garage building",p+q*new Vector3(-37,2.9f,0),new Vector3(18,5.8f,102),ivory,q);
        Box("Glass hospitality level",p+q*new Vector3(-37,7,0),new Vector3(17.8f,2.9f,100),glass,q);
        Box("Floating roof",p+q*new Vector3(-36.3f,8.8f,0),new Vector3(20,.45f,105),dark,q);
        Box("Roof orange ribbon",p+q*new Vector3(-26.3f,8.9f,0),new Vector3(.12f,.34f,104),orange,q);
        for(int i=0;i<9;i++){
            float z=-44+i*11;
            Box("Garage roller door",p+q*new Vector3(-27.94f,2.3f,z),new Vector3(.06f,4.1f,8.6f),dark,q);
            for(int k=0;k<8;k++)Box("Door panel seam",p+q*new Vector3(-27.88f,.6f+k*.45f,z),new Vector3(.025f,.026f,8.5f),metal,q);
            Box("Garage number panel",p+q*new Vector3(-27.84f,5.05f,z),new Vector3(.035f,.64f,8.5f),i%2==0?orange:teal,q);
            Sign((i+1).ToString("00")+"   /   APEX",p+q*new Vector3(-27.77f,5.07f,z),q*Quaternion.Euler(0,-90,0),.056f,Color.white);
            Box("Glass mullion",p+q*new Vector3(-28.05f,7,z+4.9f),new Vector3(.14f,3,.12f),metal,q);
            for(int s=-1;s<=1;s+=2)Box("Pit box line",p+q*new Vector3(-20,.07f,z+s*4.7f),new Vector3(7,.018f,.11f),ivory,q);
        }
        var tower=p+q*new Vector3(-49,0,68);
        Box("Race control plinth",tower+Vector3.up*7,new Vector3(12,14,13),concrete,q);
        Box("Race control glazing",tower+Vector3.up*14.2f,new Vector3(15,5,16),glass,q);
        Box("Control roof",tower+Vector3.up*16.9f,new Vector3(17,.5f,18),dark,q);
        for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)Beam(tower+q*new Vector3(s*7.4f,11.5f,z*7.9f),tower+q*new Vector3(s*7.4f,16.7f,z*7.9f),.24f,metal);
        Beam(tower+Vector3.up*17,tower+Vector3.up*26,.16f,metal);
        for(int i=0;i<6;i++){
            float d=-60+i*34;var at=Position(d);var r=Rotation(d);var pole=at+r*new Vector3(-25,0,0);
            Beam(pole,pole+Vector3.up*10,.19f,metal);Beam(pole+Vector3.up*10,pole+Vector3.up*10+r*Vector3.right*4,.14f,metal);
            Box("Pit floodlight",pole+Vector3.up*9.9f+r*Vector3.right*4,new Vector3(1,.14f,1.7f),ivory,r);
        }
        var start=Position(0);var rot=Rotation(0);
        for(int x=0;x<12;x++)for(int z=0;z<3;z++)Box("Start checker",start+rot*new Vector3((x-5.5f)*1.18f,.10f,z*.8f),new Vector3(1.18f,.015f,.8f),(x+z)%2==0?ivory:dark,rot);
        for(int side=-1;side<=1;side+=2){Beam(start+rot*new Vector3(side*9,0,0),start+rot*new Vector3(side*9,8.8f,0),.60f,dark);Beam(start+rot*new Vector3(side*9,0,2),start+rot*new Vector3(side*9,8.8f,0),.18f,metal);}
        Box("Start gantry",start+Vector3.up*8.5f,new Vector3(19,1.4f,.65f),dark,rot);
        Sign("A P E X   /   G R A N D   T O U R",start+Vector3.up*8.5f-rot*Vector3.forward*.34f,rot,.071f,Color.white);
        var lamps=Mat("Start lamps",new Color(.9f,.035f,.01f),0,.4f,1.5f);
        for(int i=0;i<5;i++)Box("Start light pod",start+rot*new Vector3((i-2)*.72f,7.40f,-.15f),new Vector3(.40f,.18f,.16f),lamps,rot);
        for(int i=0;i<8;i++){float d=-10-i*9;var at=Position(d);var r=Rotation(d);for(int s=-1;s<=1;s+=2){Box("Grid slot",at+r*new Vector3(s*2.8f,.101f,0),new Vector3(2.4f,.013f,.08f),ivory,r);Box("Grid slot side",at+r*new Vector3(s*4,.101f,-.8f),new Vector3(.08f,.013f,1.6f),ivory,r);}}
    }
    void Grandstand()
    {
        var p=Position(15);var q=Rotation(15);
        Box("Grandstand foundation",p+q*new Vector3(32,-.15f,0),new Vector3(29,.5f,91),concrete,q);
        for(int row=0;row<9;row++){
            float x=21+row*2.1f,y=.35f+row*.72f;
            Box("Seating terrace",p+q*new Vector3(x,y*.5f,0),new Vector3(2.1f,y,85),concrete,q);
            for(int seat=0;seat<43;seat++){
                float z=(seat-21)*1.8f;if(seat%11==0)continue;
                Box("Seat",p+q*new Vector3(x,y+.28f,z),new Vector3(.65f,.12f,.78f),row%3==0?orange:teal,q);
                Box("Seat back",p+q*new Vector3(x+.32f,y+.51f,z),new Vector3(.10f,.48f,.78f),row%3==0?orange:teal,q);
                if(Random.value<.28f){Box("Spectator",p+q*new Vector3(x,y+.71f,z),new Vector3(.38f,.61f,.42f),seat%3==0?ivory:dark,q);Add(GetPrimitive(PrimitiveType.Sphere),p+q*new Vector3(x,y+1.16f,z),new Vector3(.32f,.35f,.32f),wood,q);}
            }
        }
        Box("Grandstand canopy",p+q*new Vector3(30.8f,11.3f,0),new Vector3(29,.34f,94),ivory,q*Quaternion.Euler(0,0,-6));
        Box("Canopy fascia",p+q*new Vector3(16.4f,12.5f,0),new Vector3(.20f,.72f,94),dark,q);
        for(int i=-3;i<=3;i++){Beam(p+q*new Vector3(43,0,i*14),p+q*new Vector3(43,10,i*14),.33f,metal);Beam(p+q*new Vector3(43,9.4f,i*14),p+q*new Vector3(17,12.1f,i*14),.27f,metal);}
        for(int i=-2;i<=2;i++){var flag=p+q*new Vector3(48,0,i*19);Beam(flag,flag+Vector3.up*14,.13f,metal);Box("Team flag",flag+Vector3.up*12.8f+q*Vector3.forward*1.7f,new Vector3(.035f,2.1f,3.4f),i%2==0?orange:ivory,q);}
    }
    void Lake()
    {
        const int count=100;var v=new List<Vector3>{new Vector3(LakeCenter.x,2,LakeCenter.y)};var uv=new List<Vector2>{new Vector2(.5f,.5f)};var t=new List<int>();
        for(int i=0;i<=count;i++){float a=i*Mathf.PI*2/count;v.Add(new Vector3(LakeCenter.x+Mathf.Cos(a)*LakeRadius.x*.99f,2,LakeCenter.y+Mathf.Sin(a)*LakeRadius.y*.99f));uv.Add(new Vector2(.5f+Mathf.Cos(a)*.5f,.5f+Mathf.Sin(a)*.5f));if(i<count)t.AddRange(new[]{0,i+2,i+1});}
        var mesh=new Mesh{name="Azure lake water"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();MeshObject("Azure lake",mesh,new Material(Resources.Load<Shader>("LakeWater")));
        for(int i=0;i<90;i++){float a=Random.Range(0,Mathf.PI*2),r=Random.Range(1.03f,1.13f);var p=new Vector3(LakeCenter.x+Mathf.Cos(a)*LakeRadius.x*r,0,LakeCenter.y+Mathf.Sin(a)*LakeRadius.y*r);RoadInfo(p.x,p.z,out float near,out float yy);if(near<17)continue;p.y=Ground(p.x,p.z);Rock(p,new Vector3(Random.Range(2,6),Random.Range(1,3),Random.Range(2,6)));}
        var dock=new Vector3(LakeCenter.x+110,3,LakeCenter.y+70);
        for(int i=0;i<28;i++)Box("Dock plank",dock+new Vector3(0,0,-i*.65f),new Vector3(4.8f,.20f,.56f),wood,Quaternion.identity);
        for(int i=0;i<4;i++)for(int s=-1;s<=1;s+=2)Beam(dock+new Vector3(s*2,-7,-i*5.5f),dock+new Vector3(s*2,.7f,-i*5.5f),.18f,wood);
    }
    void Bridge()
    {
        int span=0;
        for(float d=0;d<Length;d+=5){var p=Position(d);if(LakeMask(p.x,p.z)>1.04f)continue;var q=Rotation(d);
            Box("Bridge reinforced deck",p+Vector3.down*.52f,new Vector3(19.6f,1,5.1f),concrete,q);
            for(int s=-1;s<=1;s+=2){Box("Bridge edge girder",p+q*new Vector3(s*9.9f,-.35f,0),new Vector3(.38f,1.1f,5.1f),teal,q);Beam(p+q*new Vector3(s*9.9f,.1f,0),p+q*new Vector3(s*9.9f,2.2f,0),.11f,teal);Box("Bridge top rail",p+q*new Vector3(s*9.9f,2.2f,0),new Vector3(.12f,.12f,5.1f),teal,q);}
            if(span%6==0){for(int s=-1;s<=1;s+=2){float h=p.y+5;Box("Bridge pier",new Vector3(p.x,p.y-h*.5f-.6f,p.z)+q*Vector3.right*s*6,new Vector3(2.1f,h,2.8f),concrete,q);}}
            if(span%10==0){for(int s=-1;s<=1;s+=2)Beam(p+q*new Vector3(s*10,-.5f,0),p+q*new Vector3(s*10,10,0),.50f,teal);Beam(p+q*new Vector3(-10,10,0),p+q*new Vector3(10,10,0),.5f,teal);Beam(p+q*new Vector3(-10,7,0),p+q*new Vector3(-6.5f,10,0),.25f,ivory);Beam(p+q*new Vector3(10,7,0),p+q*new Vector3(6.5f,10,0),.25f,ivory);}
            span++;
        }
    }
    void Landmarks()
    {
        for(int i=0;i<12;i++){float d=Length*(i+.35f)/12;var p=Position(d);var q=Rotation(d);
            Box("Sponsor board",p+q*new Vector3(-12.8f,1.9f,0),new Vector3(.20f,2.2f,9),dark,q);
            Sign(i%2==0?"AFTERGLOW":"SOLSTICE GT",p+q*new Vector3(-12.67f,1.9f,0),q*Quaternion.Euler(0,-90,0),.085f,Color.white);
            float turn=Mathf.DeltaAngle(Rotation(d).eulerAngles.y,Rotation(d+65).eulerAngles.y);int side=turn>0?-1:1;
            for(int j=0;j<3;j++){var at=Position(d+j*10);var r=Rotation(d+j*10);var board=at+r*new Vector3(side*11.6f,1.6f,0);Box("Corner chevron board",board,new Vector3(1.3f,1.2f,.08f),dark,r);Beam(board+r*new Vector3(-side*.35f,.38f,-.055f),board+r*new Vector3(side*.17f,0,-.055f),.11f,ivory);Beam(board+r*new Vector3(side*.17f,0,-.055f),board+r*new Vector3(-side*.35f,-.38f,-.055f),.11f,ivory);}
        }
        // Solar-powered marshal huts make remote sections feel like an operated circuit.
        for(int i=1;i<=6;i++){float d=Length*i/7;var p=Position(d)+Rotation(d)*new Vector3(18,0,0);if(LakeMask(p.x,p.z)<1.2f)continue;p.y=Ground(p.x,p.z);var q=Rotation(d);Box("Marshal hut",p+Vector3.up*1.8f,new Vector3(4,3.6f,5),ivory,q);Box("Hut glazing",p+Vector3.up*2.65f-q*Vector3.right*2.02f,new Vector3(.04f,1.1f,4.5f),glass,q);Box("Solar roof",p+Vector3.up*3.9f,new Vector3(5,.15f,6),glass,q*Quaternion.Euler(9,0,0));}
        // A distant communications tower marks the high point of the canyon.
        var peak=new Vector3(500,Ground(500,40),40);
        for(int i=0;i<4;i++){float h=i*8;for(int s=-1;s<=1;s+=2){Beam(peak+new Vector3(s*(4-i*.6f),h,0),peak+new Vector3(s*(3.4f-i*.6f),h+8,0),.19f,metal);Beam(peak+new Vector3(-s*(4-i*.6f),h,0),peak+new Vector3(s*(3.4f-i*.6f),h+8,0),.1f,metal);}}
        Add(GetPrimitive(PrimitiveType.Sphere),peak+Vector3.up*33,new Vector3(3,3,3),ivory,Quaternion.identity);
    }
}
}
