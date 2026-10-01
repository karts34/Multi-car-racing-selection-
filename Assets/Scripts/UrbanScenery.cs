using System.Collections.Generic;
using UnityEngine;

namespace Afterglow
{
public sealed partial class OpenWorld
{
    readonly List<Bounds> urbanFootprints=new List<Bounds>();
    readonly List<Bounds> urbanLots=new List<Bounds>();
    public int BuildingCount {get;private set;}
    Material brick,plaster,ivoryWall,slate,window,frame,paving,roofTile,litWindow,signalRed,signalGreen;
    Mesh houseRoof;

    void InitializeUrbanMaterials()
    {
        brick=TrackWorld.Mat("Weathered terracotta brick",new Color(.63f,.34f,.25f),.05f,.25f);
        plaster=TrackWorld.Mat("Warm stucco",new Color(.72f,.67f,.55f),0,.26f);
        ivoryWall=TrackWorld.Mat("Ivory stone cladding",new Color(.77f,.78f,.73f),.1f,.35f);
        slate=TrackWorld.Mat("Charcoal architectural panels",new Color(.19f,.22f,.24f),.32f,.4f);
        window=TrackWorld.Mat("City blue reflective windows",new Color(.24f,.42f,.48f),.55f,.83f);
        frame=TrackWorld.Mat("Brushed aluminium window frames",new Color(.48f,.52f,.53f),.7f,.6f);
        paving=TrackWorld.Mat("Concrete sidewalk slabs",new Color(.44f,.46f,.45f),.03f,.25f);
        roofTile=TrackWorld.Mat("Clay roof tiles",new Color(.43f,.20f,.12f),.08f,.3f);
        litWindow=TrackWorld.Mat("Warm occupied windows",new Color(.55f,.47f,.30f),.3f,.55f,.25f);
        signalRed=TrackWorld.Mat("Signal red",new Color(.7f,.025f,.015f),0,.4f,.9f);
        signalGreen=TrackWorld.Mat("Signal green",new Color(.02f,.65f,.32f),0,.4f,.9f);
        var texture=new Texture2D(128,128);var pixels=new Color[128*128];
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){
            int row=y/16;bool mortar=y%16<2||(x+(row%2)*16)%32<2;
            float n=Mathf.PerlinNoise(x*.24f,y*.24f);pixels[y*128+x]=mortar?new Color(.67f,.65f,.61f):Color.Lerp(new Color(.68f,.55f,.48f),Color.white,n);
        }
        texture.SetPixels(pixels);texture.Apply();texture.wrapMode=TextureWrapMode.Repeat;brick.mainTexture=texture;brick.mainTextureScale=new Vector2(5,7);
        houseRoof=new Mesh();houseRoof.vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(0,1,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(0,1,.5f)};
        houseRoof.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3};houseRoof.RecalculateNormals();
    }
    bool IsUrbanLot(Vector3 p){foreach(var b in urbanLots)if(p.x>b.min.x-12&&p.x<b.max.x+12&&p.z>b.min.z-12&&p.z<b.max.z+12)return true;return false;}
    void Footprint(Vector3 p,Vector3 size,float wallInset=0){var b=new Bounds(p+Vector3.up*size.y*.5f,size);urbanFootprints.Add(b);wallSurfaceInsets[b]=wallInset;urbanLots.Add(new Bounds(p,size+new Vector3(12,0,12)));Obstacle(p,size);BuildingCount++;}
    void BuildCityStreets()
    {
        for(int i=-1;i<=1;i+=2){
            CityStreet(At(i*200,-400),At(i*200,400));CityStreet(At(-400,i*200),At(400,i*200));
        }
        for(int x=-400;x<=400;x+=200)for(int z=-400;z<=400;z+=200)CityJunction(At(x,z));
    }
    void CityStreet(Vector3 from,Vector3 to)
    {
        int n=Mathf.CeilToInt(Vector3.Distance(from,to)/7);var points=new Vector3[n+1];
        for(int i=0;i<=n;i++){var p=Vector3.Lerp(from,to,i/(float)n);points[i]=At(p.x,p.z);if(i>0)RoadLength+=Vector3.Distance(points[i-1],points[i]);}
        Roads.Add(points);Ribbon(points,-11.5f,11.5f,.035f,concrete);Ribbon(points,-10,10,.078f,asphalt);
        StreetDetails(points,10);
        for(int i=3;i<points.Length-3;i+=3){var p=points[i];if(Mathf.Abs(Mathf.Repeat(p.x+400,200))<20&&Mathf.Abs(Mathf.Repeat(p.z+400,200))<20)continue;Box(p+Vector3.up*.108f,new Vector3(.16f,.01f,5),white,Quaternion.LookRotation(to-from));}
    }
    void StreetDetails(Vector3[] road,float width)
    {
        // Raised pavements run alongside the street, with gaps at junctions.
        for(int i=2;i<road.Length-2;i+=2){var p=road[i];Vector3 f=(road[i+1]-road[i-1]).normalized,side=Vector3.Cross(Vector3.up,f);var q=Quaternion.LookRotation(f);
            bool junction=Mathf.Abs(p.x/200-Mathf.Round(p.x/200))<.095f&&Mathf.Abs(p.z/200-Mathf.Round(p.z/200))<.095f;
            if(junction)continue;
            for(int s=-1;s<=1;s+=2){var at=p+side*s*(width+3.4f);at.y=Height(at.x,at.z);Box(at+Vector3.up*.10f,new Vector3(6,.20f,14.2f),paving,q);Box(p+side*s*(width+.35f)+Vector3.up*.12f,new Vector3(.35f,.25f,14),ivoryWall,q);}
            if(i%10!=2)continue;
            for(int s=-1;s<=1;s+=2){Vector3 at=p+side*s*(width+5.8f);at.y=Height(at.x,at.z);Box(at+Vector3.up*4,new Vector3(.15f,8,.15f),dark);Box(at+Vector3.up*8-side*s*1.2f,new Vector3(2.8f,.13f,.14f),dark,q);Box(at+Vector3.up*7.94f-side*s*2.3f,new Vector3(.6f,.12f,1),litWindow,q);}
        }
    }
    void CityJunction(Vector3 p)
    {
        for(int a=0;a<4;a++){
            var q=Quaternion.Euler(0,a*90,0);
            for(int i=-4;i<=4;i++)Box(p+q*new Vector3(i*1.7f,.112f,15.8f),new Vector3(.8f,.015f,4.5f),white,q);
            Box(p+q*new Vector3(0,.11f,20),new Vector3(18,.015f,.3f),white,q);
            Vector3 at=p+q*new Vector3(13,0,13);at.y=Height(at.x,at.z);
            Box(at+Vector3.up*3.1f,new Vector3(.17f,6.2f,.17f),dark);
            Box(at+Vector3.up*5.7f,new Vector3(.48f,1.3f,.4f),dark,q);
            Box(at+Vector3.up*(a%2==0?6:5.35f)-q*Vector3.forward*.23f,new Vector3(.22f,.22f,.035f),a%2==0?signalRed:signalGreen,q);
        }
    }
    void BuildUrbanDistricts()
    {
        int[] lots={-345,-265,-135,-55,55,135,265,345};int index=0;
        foreach(int x in lots)foreach(int z in lots){
            Vector3 p=At(x,z);int type=(index+x/5+z/5)%4;type=(type+4)%4;
            int floors=type==0?UnityEngine.Random.Range(12,23):type==1?UnityEngine.Random.Range(4,8):type==2?UnityEngine.Random.Range(2,4):UnityEngine.Random.Range(5,10);
            // A taller business district and quieter low-rise western streets.
            if(x<0&&z<0&&type==0){type=1;floors=5;}
            float w=UnityEngine.Random.Range(36,48),d=UnityEngine.Random.Range(34,47);UrbanBuilding(p,w,d,floors,type,index++);
        }
        // Boulevard frontage and neighbourhood houses connect the downtown to the shore.
        for(int s=-1;s<=1;s+=2)for(int z=-320;z<=320;z+=90)UrbanBuilding(At(s*447,z),32,36,2+(z/90&1),2,index++);
        for(int z=-310;z<=310;z+=100)for(int x=-980;x<=-520;x+=105){
            Vector3 p=At(x,z);NearestRoad(p,out float near,out _);if(near<35||Lake(p.x,p.z)<1.28f)continue;House(p,index++);
        }
        for(int x=490;x<=930;x+=110){Vector3 p=At(x,-355);Warehouse(p,index++);}
        var garage=At(-37,-450);Box(garage+Vector3.up*4,new Vector3(32,8,28),ivoryWall);Footprint(garage,new Vector3(32,12,28));
        Box(garage+new Vector3(16.1f,3,0),new Vector3(.15f,5,20),slate);for(int i=0;i<10;i++)Box(garage+new Vector3(16.2f,.8f+i*.42f,0),new Vector3(.025f,.025f,20),frame);
        Box(garage+Vector3.up*8.3f,new Vector3(35,.6f,31),slate);Sign("APEX MOTORWORKS",garage+new Vector3(16.4f,6.8f,0),Quaternion.Euler(0,-90,0),.12f);
        Box(garage+new Vector3(22,.08f,0),new Vector3(10,.16f,32),paving);
        Sign("SOLSTICE CITY",At(31,-470)+Vector3.up*6,Quaternion.identity,.22f);
    }
    void UrbanBuilding(Vector3 p,float w,float d,int floors,int type,int seed)
    {
        float h=4.4f+floors*3.25f;Material wall=type==0?slate:type==1?brick:type==2?plaster:ivoryWall;
        Box(p+Vector3.up*(h/2),new Vector3(w,h,d),wall);Footprint(p,new Vector3(w+2.6f,h+.45f,d+2.6f),1.3f);
        Box(p+Vector3.up*.12f,new Vector3(w+8,.24f,d+8),paving);
        Box(p+Vector3.up*4.2f,new Vector3(w+1,.38f,d+1),ivoryWall);
        Box(p+Vector3.up*(h+.2f),new Vector3(w+1.2f,.45f,d+1.2f),slate);
        for(int face=0;face<4;face++){
            var q=Quaternion.Euler(0,face*90,0);float span=face%2==0?w:d,depth=face%2==0?d:w;
            int bays=Mathf.Max(3,Mathf.FloorToInt(span/4.4f));float bay=(span-4)/bays;
            for(int k=0;k<bays;k++){
                float x=-span/2+2+bay*(k+.5f);
                Box(p+q*new Vector3(x,2.1f,depth/2+.07f),new Vector3(bay-.35f,3.35f,.10f),window,q);
                Box(p+q*new Vector3(x-bay/2+.08f,2.1f,depth/2+.17f),new Vector3(.13f,3.65f,.16f),frame,q);
                for(int f=0;f<floors;f++){
                    float y=6.1f+f*3.25f;bool occupied=(seed+f+k)%13==0;
                    float ww=type==0?bay-.18f:bay*.68f,hh=type==0?2.92f:1.88f;
                    Box(p+q*new Vector3(x,y,depth/2+.10f),new Vector3(ww+.18f,hh+.18f,.15f),frame,q);
                    Box(p+q*new Vector3(x,y,depth/2+.20f),new Vector3(ww,hh,.06f),occupied?litWindow:window,q);
                    Box(p+q*new Vector3(x,y,depth/2+.25f),new Vector3(.06f,hh,.055f),frame,q);
                    if(type!=0)Box(p+q*new Vector3(x,y-hh/2-.15f,depth/2+.28f),new Vector3(ww+.5f,.15f,.5f),ivoryWall,q);
                    if(type==1&&face==0&&f>0&&k%2==0){
                        Box(p+q*new Vector3(x,y-.9f,depth/2+.85f),new Vector3(bay*.85f,.22f,1.8f),concrete,q);
                        Box(p+q*new Vector3(x,y-.05f,depth/2+1.7f),new Vector3(bay*.85f,.08f,.08f),frame,q);
                        for(int b=-2;b<=2;b++)Box(p+q*new Vector3(x+b*bay*.15f,y-.5f,depth/2+1.7f),new Vector3(.05f,.8f,.05f),frame,q);
                    }
                }
            }
            for(int f=0;f<floors;f++)if(type==0||f%3==0)Box(p+q*new Vector3(0,4.8f+f*3.25f,depth/2+.12f),new Vector3(span,.10f,.20f),frame,q);
            Box(p+q*new Vector3(0,1.85f,depth/2+.3f),new Vector3(2.9f,3.4f,.22f),slate,q);
            Box(p+q*new Vector3(0,1.90f,depth/2+.45f),new Vector3(2.4f,3.1f,.055f),window,q);
        }
        if(type==0){float capH=7;Box(p+Vector3.up*(h+capH/2),new Vector3(w*.65f,capH,d*.65f),window);Box(p+Vector3.up*(h+capH),new Vector3(w*.68f,.4f,d*.68f),slate);}
        else {Box(p+new Vector3(w*.22f,h+1.2f,d*.16f),new Vector3(5,2.2f,4),frame);for(int k=0;k<3;k++)Box(p+new Vector3(-w*.22f+k*3,h+.6f,0),new Vector3(2,.9f,3),slate);}
        string[] shops={"SOLSTICE COFFEE","PACIFIC MARKET","CITY MOTORS","AFTERGLOW HOTEL","WESTSIDE BANK","PALM RESIDENCES"};
        Box(p+new Vector3(0,3.75f,d/2+.35f),new Vector3(w*.60f,.82f,.22f),seed%2==0?ochre:slate);
        Sign(shops[seed%shops.Length],p+new Vector3(0,3.77f,d/2+.50f),Quaternion.Euler(0,180,0),.062f);
        if(type==2){Box(p+new Vector3(0,3.35f,d/2+1.3f),new Vector3(w*.8f,.15f,2.5f),seed%2==0?green:ochre,Quaternion.Euler(-8,0,0));}
    }
    void House(Vector3 p,int seed)
    {
        float w=17,d=23,h=seed%3==0?8:5;var wall=seed%2==0?plaster:ivoryWall;
        Box(p+Vector3.up*(h/2),new Vector3(w,h,d),wall);Footprint(p,new Vector3(w+2,h,d+2),1);
        Add(houseRoof,p+Vector3.up*h,new Vector3(w+2,3.8f,d+2),roofTile,Quaternion.identity);
        Box(p+new Vector3(4,h+2.6f,5),new Vector3(1.4f,4,1.5f),brick);
        for(int face=0;face<2;face++)for(int k=-1;k<=1;k+=2){var q=Quaternion.Euler(0,face*180,0);Box(p+q*new Vector3(k*4.7f,2.8f,d/2+.1f),new Vector3(3.1f,2,.16f),frame,q);Box(p+q*new Vector3(k*4.7f,2.8f,d/2+.23f),new Vector3(2.8f,1.7f,.05f),window,q);}
        Box(p+new Vector3(0,1.4f,d/2+.12f),new Vector3(1.4f,2.8f,.2f),wood);
        Box(p+new Vector3(0,3.7f,d/2+2),new Vector3(6,.22f,4.4f),ivoryWall);
        for(int s=-1;s<=1;s+=2)Box(p+new Vector3(s*2.7f,1.8f,d/2+3.8f),new Vector3(.24f,3.6f,.24f),ivoryWall);
        Box(p+new Vector3(0,.10f,d/2+5),new Vector3(7,.2f,10),paving);
        for(int s=-1;s<=1;s+=2)Box(p+new Vector3(s*(w/2+6),.7f,0),new Vector3(.15f,1.4f,d+12),ivoryWall);
    }
    void Warehouse(Vector3 p,int seed)
    {
        Box(p+Vector3.up*5,new Vector3(55,10,48),seed%2==0?slate:brick);Footprint(p,new Vector3(55,12,48));
        Box(p+Vector3.up*10.3f,new Vector3(58,.65f,51),frame);
        for(int k=-1;k<=1;k++){Box(p+new Vector3(k*16,3.3f,24.1f),new Vector3(12,6,.2f),slate);for(int s=0;s<12;s++)Box(p+new Vector3(k*16,.6f+s*.48f,24.25f),new Vector3(12,.035f,.02f),frame);}
        Sign("SOLSTICE / LOGISTICS",p+new Vector3(0,8.2f,24.4f),Quaternion.Euler(0,180,0),.11f);
    }
}
}
