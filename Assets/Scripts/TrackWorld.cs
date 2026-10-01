using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Afterglow
{
public partial class TrackWorld
{
    public const int Samples=1400;
    public const float Width=15;
    public Vector3[] points=new Vector3[Samples+1];
    public float[] distances=new float[Samples+1];
    public float Length=>distances[Samples];
    public Transform root;
    public Material asphalt,ivory,orange,dark,metal,rock,foliage,concrete,wood,glass,teal;
    Material[] greens;
    readonly Dictionary<(Material,int,int),List<CombineInstance>> batches=new Dictionary<(Material,int,int),List<CombineInstance>>();
    readonly Dictionary<PrimitiveType,Mesh> primitives=new Dictionary<PrimitiveType,Mesh>();
    Mesh pineMesh,rockMesh;
    public readonly Vector2 LakeCenter=new Vector2(-270,-140);
    public readonly Vector2 LakeRadius=new Vector2(160,170);
    public Bounds MapBounds {get;private set;}
    public readonly string[] SectorNames={"GRAND PRIX PADDOCK","EMBER CANYON","CEDAR HIGHLANDS","AZURE LAKE","SUMMIT RETURN"};
    public string Sector(float d){float f=Mathf.Repeat(d,Length)/Length;return SectorNames[f<.17f?0:f<.43f?1:f<.66f?2:f<.83f?3:4];}
    public static Material Mat(string name,Color color,float metallic=0,float smooth=.3f,float glow=0){var m=new Material(Shader.Find("Standard")){name=name,color=color,enableInstancing=true};m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",smooth);if(glow>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*glow);}return m;}
    public TrackWorld()
    {
        Random.InitState(2047);root=new GameObject("Solstice / Grand Tour").transform;
        asphalt=Mat("Weathered race asphalt",new Color(.19f,.205f,.22f),.08f,.31f);
        var texture=new Texture2D(256,256);var pixels=new Color[256*256];
        for(int y=0;y<256;y++)for(int x=0;x<256;x++){float v=Random.Range(.68f,1f)+Mathf.PerlinNoise(x*.06f,y*.06f)*.10f;pixels[y*256+x]=new Color(v,v,v);}
        texture.SetPixels(pixels);texture.Apply();texture.wrapMode=TextureWrapMode.Repeat;texture.anisoLevel=8;asphalt.mainTexture=texture;asphalt.mainTextureScale=new Vector2(10,390);
        ivory=Mat("Porcelain road markings",new Color(.86f,.86f,.80f),.05f,.30f);
        orange=Mat("Race orange",new Color(.99f,.23f,.045f),.12f,.48f);
        dark=Mat("Structural graphite",new Color(.045f,.058f,.067f),.3f,.4f);
        metal=Mat("Galvanized steel",new Color(.45f,.54f,.58f),.68f,.6f);
        concrete=Mat("Precast concrete",new Color(.47f,.49f,.47f),0,.22f);
        wood=Mat("Cedar timber",new Color(.25f,.14f,.072f),0,.18f);
        glass=Mat("Architecture glazing",new Color(.065f,.18f,.23f),.72f,.9f);
        teal=Mat("Bridge painted steel",new Color(.19f,.40f,.44f),.6f,.55f);
        rock=new Material(Resources.Load<Shader>("TerrainColor")){name="Stratified canyon rock"};rock.SetColor("_Color",new Color(.41f,.235f,.125f,1));
        greens=new[]{Mat("Cedar shadow",new Color(.055f,.14f,.085f)),Mat("Pine green",new Color(.10f,.23f,.13f)),Mat("Sunlit foliage",new Color(.21f,.30f,.11f))};foliage=greens[1];
        Layout();pineMesh=Cone();rockMesh=RockShape();
        Terrain();
        Strip("Race surface",-7.5f,7.5f,.08f,asphalt);
        Strip("Gravel left",-10,-7.5f,.025f,concrete);Strip("Gravel right",7.5f,10,.025f,concrete);
        Strip("Left edge line",-7.17f,-7.01f,.095f,ivory);Strip("Right edge line",7.01f,7.17f,.095f,ivory);
        RoadFurniture();Populate();Lake();PitComplex();Grandstand();Bridge();Landmarks();Flush();
    }
    void Layout()
    {
        Vector3[] knots={new Vector3(0,12,360),new Vector3(230,15,350),new Vector3(420,24,270),new Vector3(465,38,110),new Vector3(340,47,20),new Vector3(430,42,-155),new Vector3(280,25,-330),new Vector3(60,14,-390),new Vector3(-170,10,-310),new Vector3(-300,12,-120),new Vector3(-400,20,40),new Vector3(-330,30,220),new Vector3(-160,18,310)};
        var bounds=new Bounds(knots[0],Vector3.zero);
        for(int i=0;i<=Samples;i++){float f=i/(float)Samples*knots.Length;int k=Mathf.FloorToInt(f);float t=f-k;int n=knots.Length;Vector3 a=knots[(k-1+n)%n],b=knots[k%n],c=knots[(k+1)%n],d=knots[(k+2)%n];points[i]=.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);if(i>0)distances[i]=distances[i-1]+Vector3.Distance(points[i],points[i-1]);bounds.Encapsulate(points[i]);}
        MapBounds=bounds;
    }
    public Vector3 Position(float distance){distance=Mathf.Repeat(distance,Length);int a=0,b=Samples;while(b-a>1){int m=(a+b)/2;if(distances[m]<=distance)a=m;else b=m;}return Vector3.Lerp(points[a],points[b],Mathf.InverseLerp(distances[a],distances[b],distance));}
    public Quaternion Rotation(float d)=>Quaternion.LookRotation((Position(d+1)-Position(d-1)).normalized,Vector3.up);
    public float LakeMask(float x,float z){float dx=(x-LakeCenter.x)/LakeRadius.x,dz=(z-LakeCenter.y)/LakeRadius.y;return Mathf.Sqrt(dx*dx+dz*dz);}
    public void RoadInfo(float x,float z,out float away,out float y)
    {
        float best=float.MaxValue;y=0;
        for(int i=0;i<Samples;i+=5){Vector3 a=points[i],b=points[Mathf.Min(i+5,Samples)];float dx=b.x-a.x,dz=b.z-a.z,t=Mathf.Clamp01(((x-a.x)*dx+(z-a.z)*dz)/(dx*dx+dz*dz));float xx=x-(a.x+dx*t),zz=z-(a.z+dz*t),dd=xx*xx+zz*zz;if(dd<best){best=dd;y=Mathf.Lerp(a.y,b.y,t);}}
        away=Mathf.Sqrt(best);
    }
    public float Ground(float x,float z)
    {
        RoadInfo(x,z,out float away,out float roadY);
        float large=Mathf.PerlinNoise(x*.0038f+51,z*.0038f+51);
        float detail=Mathf.PerlinNoise(x*.019f+17,z*.019f+32)*3+Mathf.PerlinNoise(x*.065f+33,z*.065f+67)*.65f;
        float radial=new Vector2(x*.91f,z).magnitude;
        float mountains=Mathf.SmoothStep(0,1,Mathf.InverseLerp(450,830,radial));
        float ridge=1-Mathf.Abs(Mathf.PerlinNoise(x*.007f+14,z*.007f+64)*2-1);
        float height=large*27-3+detail+mountains*(50+ridge*ridge*145);
        if(x>300)height+=Mathf.SmoothStep(0,1,Mathf.InverseLerp(310,560,x))*24;
        float nearRoad=Mathf.SmoothStep(0,1,Mathf.InverseLerp(13,65,away));
        height=Mathf.Lerp(roadY-.5f,height,nearRoad);
        // The lake bed stays below the bridge; roads elsewhere sit on graded shoulders.
        float water=LakeMask(x,z);
        height=Mathf.Lerp(-5+detail*.25f,height,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.88f,1.13f,water)));
        // A level foundation for the complete pit complex.
        if(x>-100&&x<165&&z>311&&z<435){float edge=Mathf.Min(Mathf.Min(x+100,165-x),Mathf.Min(z-311,435-z));height=Mathf.Lerp(height,roadY-.5f,Mathf.SmoothStep(0,1,edge/12));}
        return height;
    }
    void Terrain()
    {
        const int n=250;var v=new Vector3[(n+1)*(n+1)];var uv=new Vector2[v.Length];var colors=new Color[v.Length];var triangles=new int[n*n*6];
        for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){int i=z*(n+1)+x;float xx=(x-n/2)*8,zz=(z-n/2)*8,yy=Ground(xx,zz);v[i]=new Vector3(xx,yy,zz);uv[i]=new Vector2(xx*.025f,zz*.025f);float noise=Mathf.PerlinNoise(xx*.014f+91,zz*.014f+36);Color green=Color.Lerp(new Color(.13f,.23f,.115f),new Color(.34f,.35f,.20f),noise);Color canyon=Color.Lerp(new Color(.32f,.22f,.16f),new Color(.54f,.34f,.19f),noise);float biome=Mathf.SmoothStep(0,1,Mathf.InverseLerp(130,400,xx));colors[i]=Color.Lerp(green,canyon,biome);if(yy>65)colors[i]=Color.Lerp(colors[i],new Color(.36f,.37f,.34f),Mathf.InverseLerp(65,170,yy));if(yy>155)colors[i]=Color.Lerp(colors[i],new Color(.77f,.79f,.77f),Mathf.InverseLerp(155,220,yy));colors[i]=colors[i].linear;}
        int t=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;triangles[t++]=a;triangles[t++]=a+n+1;triangles[t++]=a+1;triangles[t++]=a+1;triangles[t++]=a+n+1;triangles[t++]=a+n+2;}
        var mesh=new Mesh{name="Grand Tour terrain",indexFormat=IndexFormat.UInt32};mesh.vertices=v;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateNormals();MeshObject("Layered alpine landscape",mesh,new Material(Resources.Load<Shader>("TerrainColor")));
    }
    void Strip(string name,float left,float right,float height,Material mat,float start=0,float end=-1)
    {
        if(end<0)end=Length;int n=Mathf.Max(2,Mathf.CeilToInt((end-start)/2));
        var v=new Vector3[(n+1)*2];var uv=new Vector2[v.Length];var tri=new int[n*6];
        for(int i=0;i<=n;i++){float d=Mathf.Lerp(start,end,i/(float)n);var q=Rotation(d);var p=Position(d);v[i*2]=p+q*new Vector3(left,height,0);v[i*2+1]=p+q*new Vector3(right,height,0);uv[i*2]=new Vector2(0,d/Length);uv[i*2+1]=new Vector2(1,d/Length);if(i<n){int a=i*2,t=i*6;tri[t]=a;tri[t+1]=a+2;tri[t+2]=a+1;tri[t+3]=a+1;tri[t+4]=a+2;tri[t+5]=a+3;}}
        var m=new Mesh{name=name};m.vertices=v;m.uv=uv;m.triangles=tri;m.RecalculateNormals();MeshObject(name,m,mat);
    }
    void RoadFurniture()
    {
        for(float d=0;d<Length;d+=4){var p=Position(d);var q=Rotation(d);int k=Mathf.RoundToInt(d/4);
            for(int s=-1;s<=1;s+=2){Box("Red white curb",p+q*new Vector3(s*7.72f,.08f,0),new Vector3(.80f,.12f,4.02f),k%2==0?orange:ivory,q);if(k%2==0){Box("Twin-beam barrier",p+q*new Vector3(s*9.6f,.80f,0),new Vector3(.16f,.31f,8.1f),metal,q);Box("Barrier lower beam",p+q*new Vector3(s*9.6f,.48f,0),new Vector3(.12f,.15f,8.1f),metal,q);Box("Barrier support",p+q*new Vector3(s*9.6f,.42f,0),new Vector3(.18f,1.12f,.18f),dark,q);}if(k%8==0)Box("Amber reflector",p+q*new Vector3(s*9.44f,.91f,0),new Vector3(.03f,.1f,.22f),orange,q);}
            if(k%8==0){Box("Track rubber left",p+q*new Vector3(-1.3f,.088f,0),new Vector3(.10f,.004f,9),dark,q);Box("Track rubber right",p+q*new Vector3(1.3f,.088f,0),new Vector3(.10f,.004f,9),dark,q);}
        }
        for(int i=0;i<10;i++){float d=Length*(i+.6f)/10;var p=Position(d);var q=Rotation(d);for(int j=0;j<3;j++){var at=Position(d-j*35);var r=Rotation(d-j*35);Box("Braking marker",at+r*new Vector3(-11,1.1f,0),new Vector3(.9f,1.3f,.08f),ivory,r);Sign(((j+1)*50).ToString(),at+r*new Vector3(-11,1.1f,-.06f),r,.075f,new Color(.035f,.045f,.05f));}}
    }
    void Populate()
    {
        for(int i=0;i<2100;i++){
            float d=Random.Range(0,Length),offset=Random.Range(17,180)*(Random.value>.5f?1:-1);var p=Position(d)+Rotation(d)*new Vector3(offset,0,0);
            RoadInfo(p.x,p.z,out float near,out float roadY);if(near<17||LakeMask(p.x,p.z)<1.17f||(p.z>302&&p.z<460&&p.x>-150&&p.x<230))continue;p.y=Ground(p.x,p.z);
            bool canyon=p.x>280;
            if(Random.value<(canyon?.25f:.88f)){float h=Random.Range(6,19);Tree(p,h,greens[Random.Range(0,greens.Length)]);}
            else {float size=Random.Range(3,13);if(near>size*.65f+12)Rock(p,new Vector3(size,Random.Range(.6f,1.6f)*size,size*.8f));}
        }
        // Larger exposed cliff faces frame the canyon without entering the racing surface.
        for(int i=0;i<60;i++){float d=Length*Random.Range(.18f,.43f);var p=Position(d)+Rotation(d)*new Vector3(-Random.Range(28,100),0,0);p.y=Ground(p.x,p.z)-2;RoadInfo(p.x,p.z,out float near,out float yy);if(near>40)Rock(p,new Vector3(Random.Range(14,29),Random.Range(20,52),Random.Range(12,25)));}
    }
    void Tree(Vector3 p,float h,Material leaves)
    {
        Add(GetPrimitive(PrimitiveType.Cylinder),p+Vector3.up*h*.25f,new Vector3(h*.034f,h*.25f,h*.034f),wood,Quaternion.identity);
        float rotate=Random.Range(0,360);for(int layer=0;layer<4;layer++){float width=h*(.245f-layer*.038f);Add(pineMesh,p+Vector3.up*h*(.22f+layer*.17f),new Vector3(width,h*.44f,width),leaves,Quaternion.Euler(0,rotate+layer*37,0));}
    }
    Mesh Cone()
    {
        const int n=12;var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;float r=i%2==0?1:.91f,r2=(i+1)%2==0?1:.91f;var left=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);var right=new Vector3(Mathf.Cos(b)*r2,0,Mathf.Sin(b)*r2);int k=v.Count;v.AddRange(new[]{left,Vector3.up,right,left,right,Vector3.zero});t.AddRange(new[]{k,k+1,k+2,k+3,k+4,k+5});}var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();return m;
    }
    Mesh RockShape()
    {
        const int n=11,levels=4;var p=new Vector3[n*levels+1];for(int h=0;h<levels;h++)for(int k=0;k<n;k++){float a=k*Mathf.PI*2/n,r=Random.Range(.40f,.60f)*(1-h*.16f);p[h*n+k]=new Vector3(Mathf.Cos(a)*r,h*.24f-.07f,Mathf.Sin(a)*r);}p[n*levels]=new Vector3(.05f,.92f,-.03f);var v=new List<Vector3>();var t=new List<int>();for(int h=0;h<levels-1;h++)for(int k=0;k<n;k++){int a=h*n+k,b=h*n+(k+1)%n,c=a+n,d=b+n;foreach(int index in new[]{a,c,b,b,c,d}){t.Add(v.Count);v.Add(p[index]);}}for(int k=0;k<n;k++)foreach(int index in new[]{(levels-1)*n+k,n*levels,(levels-1)*n+(k+1)%n}){t.Add(v.Count);v.Add(p[index]);}var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.colors=System.Array.ConvertAll(v.ToArray(),x=>Color.white);m.RecalculateNormals();return m;
    }
    void Rock(Vector3 p,Vector3 scale)=>Add(rockMesh,p,scale,rock,Quaternion.Euler(Random.Range(-8,8),Random.Range(0,360),Random.Range(-6,6)));
    Mesh GetPrimitive(PrimitiveType type){if(!primitives.ContainsKey(type)){var go=GameObject.CreatePrimitive(type);primitives[type]=go.GetComponent<MeshFilter>().sharedMesh;Object.Destroy(go);}return primitives[type];}
    void Box(string name,Vector3 p,Vector3 s,Material m,Quaternion q)=>Add(GetPrimitive(PrimitiveType.Cube),p,s,m,q);
    void Beam(Vector3 a,Vector3 b,float width,Material m)=>Box("Structural beam",(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),m,Quaternion.FromToRotation(Vector3.up,b-a));
    void Add(Mesh mesh,Vector3 p,Vector3 scale,Material m,Quaternion q){var key=(m,Mathf.FloorToInt(p.x/120),Mathf.FloorToInt(p.z/120));if(!batches.ContainsKey(key))batches[key]=new List<CombineInstance>();batches[key].Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,scale)});}
    void Flush(){foreach(var pair in batches){var mesh=new Mesh{name=pair.Key.Item1.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());MeshObject("Scenery cell / "+pair.Key.Item1.name,mesh,pair.Key.Item1);}batches.Clear();}
    void MeshObject(string name,Mesh mesh,Material material){var go=new GameObject(name);go.transform.parent=root;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;}
    void Sign(string text,Vector3 p,Quaternion r,float size,Color color){var go=new GameObject(text);go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,r);var tm=go.AddComponent<TextMesh>();tm.text=text;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.characterSize=size;tm.fontSize=64;tm.color=color;}
}
}
