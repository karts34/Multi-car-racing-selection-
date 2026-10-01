using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Afterglow
{
// A separate, freely traversable landscape. Driving never projects the car onto a road.
public sealed partial class OpenWorld
{
    public const float Extent=1350;
    public readonly Transform Root;
    public readonly List<Vector3[]> Roads=new List<Vector3[]>();
    public readonly List<SprintRoute> Events=new List<SprintRoute>();
    public readonly Vector3 Home;
    public readonly Texture2D Map;
    public float RoadLength {get;private set;}
    public readonly string[] DistrictNames={"SOLSTICE CITY","AZURE WATERFRONT","CEDAR HILLS","SUMMIT HEIGHTS"};
    public readonly Vector2[] DistrictPoints={new Vector2(160,-180),new Vector2(-720,-720),new Vector2(-620,700),new Vector2(930,850)};
    readonly Dictionary<(int,int),Vector3[]> edges=new Dictionary<(int,int),Vector3[]>();
    readonly Dictionary<(Material,int,int),List<CombineInstance>> batches=new Dictionary<(Material,int,int),List<CombineInstance>>();
    readonly Dictionary<(int,int),List<Bounds>> obstacles=new Dictionary<(int,int),List<Bounds>>();
    readonly float[] grid={-1050,-400,0,400,1050};
    Material asphalt,white,ochre,concrete,glass,dark,wood,green,stone,beacon;
    Mesh cube,cone;

    public OpenWorld()
    {
        var randomState=UnityEngine.Random.state;UnityEngine.Random.InitState(4819);
        Root=new GameObject("Solstice County / Open World").transform;
        asphalt=TrackWorld.Mat("County asphalt",new Color(.13f,.15f,.17f),.08f,.24f);
        white=TrackWorld.Mat("Road ivory",new Color(.85f,.84f,.72f));
        ochre=TrackWorld.Mat("Warm sandstone",new Color(.64f,.39f,.22f));
        concrete=TrackWorld.Mat("Town limestone",new Color(.57f,.60f,.57f));
        glass=TrackWorld.Mat("Town blue glazing",new Color(.08f,.25f,.32f),.65f,.8f);
        dark=TrackWorld.Mat("Street graphite",new Color(.06f,.08f,.10f),.4f,.45f);
        wood=TrackWorld.Mat("Cedar trunk",new Color(.23f,.13f,.07f));
        green=TrackWorld.Mat("Cedar leaves",new Color(.13f,.29f,.12f));
        stone=TrackWorld.Mat("Highlands rock",new Color(.40f,.37f,.29f));
        beacon=TrackWorld.Mat("Sprint turquoise",new Color(.05f,.7f,.9f),.2f,.6f,1.3f);
        var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=primitive.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.Destroy(primitive);
        cone=MakeCone();
        InitializeUrbanMaterials();BuildRoads();BuildCityStreets();BuildTerrain();BuildUrbanDistricts();BuildNature();BuildLandmarks();
        Home=At(0,-445);
        Flush();Map=MakeMap();UnityEngine.Random.state=randomState;
    }
    static int N(int x,int z)=>z*5+x;
    Vector3 Node(int n)=>At(grid[n%5],grid[n/5]);
    public float Height(float x,float z)
    {
        float hills=52*Mathf.SmoothStep(0,1,Mathf.InverseLerp(200,1200,x))+35*Mathf.SmoothStep(0,1,Mathf.InverseLerp(200,1200,z));
        float y=5+Mathf.PerlinNoise(x*.0017f+42,z*.0017f+19)*7+hills;
        float lake=Lake(x,z);return Mathf.Lerp(-4,y,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.92f,1.19f,lake)));
    }
    public static float Lake(float x,float z)=>new Vector2((x+730)/215,(z+720)/215).magnitude;
    public Vector3 At(float x,float z)=>new Vector3(x,Height(x,z),z);
    public string District(Vector3 p)=>p.x>650?DistrictNames[3]:p.z>480?DistrictNames[2]:p.x<-550&&p.z<100?DistrictNames[1]:DistrictNames[0];
    public Vector3 NearestRoad(Vector3 p,out float separation,out Vector3 forward)
    {
        float best=float.MaxValue;Vector3 nearest=Home;forward=Vector3.forward;
        foreach(var road in Roads)for(int i=1;i<road.Length;i++){
            Vector3 a=road[i-1],d=road[i]-a;d.y=0;Vector3 delta=p-a;delta.y=0;
            float t=Mathf.Clamp01(Vector3.Dot(delta,d)/Mathf.Max(.001f,d.sqrMagnitude));Vector3 q=a+d*t;float ds=(new Vector2(p.x-q.x,p.z-q.z)).sqrMagnitude;
            if(ds<best){best=ds;nearest=q;forward=d.normalized;}
        }
        nearest.y=Height(nearest.x,nearest.z);separation=Mathf.Sqrt(best);return nearest;
    }
    public bool Blocked(Vector3 p,float radius=1.4f)
    {
        if(Mathf.Abs(p.x)>Extent-15||Mathf.Abs(p.z)>Extent-15||Lake(p.x,p.z)<1.035f)return true;
        int cx=Mathf.FloorToInt(p.x/60),cz=Mathf.FloorToInt(p.z/60);
        for(int x=cx-1;x<=cx+1;x++)for(int z=cz-1;z<=cz+1;z++)if(obstacles.TryGetValue((x,z),out var list))foreach(var b in list){
            float dx=Mathf.Max(b.min.x-p.x,0,p.x-b.max.x),dz=Mathf.Max(b.min.z-p.z,0,p.z-b.max.z);
            if(dx*dx+dz*dz<radius*radius)return true;
        }
        return false;
    }
    void Obstacle(Vector3 p,Vector3 size){var b=new Bounds(p,size);int x=Mathf.FloorToInt(p.x/60),z=Mathf.FloorToInt(p.z/60);if(!obstacles.ContainsKey((x,z)))obstacles[(x,z)]=new List<Bounds>();obstacles[(x,z)].Add(b);}
    void BuildRoads()
    {
        for(int z=0;z<5;z++)for(int x=0;x<5;x++){
            if(x<4)Road(N(x,z),N(x+1,z),z==0||z==4);
            if(z<4)Road(N(x,z),N(x,z+1),x==0||x==4);
        }
        for(int z=0;z<5;z++)for(int x=0;x<5;x++){
            Vector3 p=Node(N(x,z));
            // Junctions are open in all four directions. No circuit barriers.
            for(int s=-1;s<=1;s+=2)for(int axis=0;axis<2;axis++){
                Vector3 at=p+(axis==0?new Vector3(s*18,0,13):new Vector3(13,0,s*18));at.y=Height(at.x,at.z);
                Box(at+Vector3.up*3,new Vector3(.18f,6,.18f),dark);
                Box(at+Vector3.up*5.7f,new Vector3(2,.20f,1),white);
            }
        }
    }
    void Road(int a,int b,bool scenic)
    {
        Vector3 start=Node(a),end=Node(b),side=Vector3.Cross(Vector3.up,(end-start).normalized);int count=Mathf.CeilToInt(Vector3.Distance(start,end)/7);
        var path=new Vector3[count+1];
        for(int i=0;i<=count;i++){float t=i/(float)count;Vector3 p=Vector3.Lerp(start,end,t)+side*(scenic?Mathf.Sin(t*Mathf.PI)*65:0);path[i]=At(p.x,p.z);if(i>0)RoadLength+=Vector3.Distance(path[i-1],path[i]);}
        Roads.Add(path);edges[(a,b)]=path;
        Ribbon(path,-11.5f,11.5f,.035f,concrete);Ribbon(path,-10,10,.075f,asphalt);
        if(Mathf.Abs(start.x)<=400&&Mathf.Abs(end.x)<=400&&Mathf.Abs(start.z)<=400&&Mathf.Abs(end.z)<=400)StreetDetails(path,10);
        // Keep line paint clear of junction mouths.
        var inner=new List<Vector3>();for(int i=3;i<path.Length-3;i++)inner.Add(path[i]);
        if(inner.Count>1){Ribbon(inner.ToArray(),-9.3f,-9.1f,.10f,white);Ribbon(inner.ToArray(),9.1f,9.3f,.10f,white);}
        for(int i=4;i<path.Length-4;i+=3){Vector3 p=path[i],d=path[i+1]-p;Box(p+Vector3.up*.105f,new Vector3(.15f,.015f,5),white,Quaternion.LookRotation(d));}
    }
    void Ribbon(Vector3[] path,float left,float right,float lift,Material material)
    {
        if(path.Length<2)return;var v=new Vector3[path.Length*2];var t=new int[(path.Length-1)*6];
        for(int i=0;i<path.Length;i++){
            Vector3 d=path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)];d.y=0;Vector3 side=Vector3.Cross(Vector3.up,d.normalized);
            Vector3 a=path[i]+side*left,b=path[i]+side*right;v[i*2]=At(a.x,a.z)+Vector3.up*lift;v[i*2+1]=At(b.x,b.z)+Vector3.up*lift;
            if(i<path.Length-1){int k=i*6,j=i*2;t[k]=j;t[k+1]=j+2;t[k+2]=j+1;t[k+3]=j+1;t[k+4]=j+2;t[k+5]=j+3;}
        }
        var mesh=new Mesh();mesh.vertices=v;mesh.triangles=t;mesh.RecalculateNormals();MeshObject("County road",mesh,material);
    }
    void BuildTerrain()
    {
        const int n=300;float step=Extent*2/n;var v=new Vector3[(n+1)*(n+1)];var c=new Color[v.Length];var t=new int[n*n*6];
        for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){
            int i=z*(n+1)+x;Vector3 p=At(-Extent+x*step,-Extent+z*step);v[i]=p;
            Color baseColor=Color.Lerp(new Color(.20f,.30f,.14f),new Color(.43f,.42f,.26f),Mathf.PerlinNoise(p.x*.01f+7,p.z*.01f+6));
            if(Mathf.Abs(p.x)<415&&Mathf.Abs(p.z)<415)baseColor=Color.Lerp(baseColor,new Color(.34f,.36f,.34f),.78f);
            if(p.x>700)baseColor=Color.Lerp(baseColor,new Color(.49f,.35f,.23f),Mathf.InverseLerp(700,1200,p.x));
            if(Lake(p.x,p.z)<1.24f)baseColor=new Color(.52f,.48f,.33f);c[i]=baseColor.linear;
            if(x<n&&z<n){int k=(z*n+x)*6;t[k]=i;t[k+1]=i+n+1;t[k+2]=i+1;t[k+3]=i+1;t[k+4]=i+n+1;t[k+5]=i+n+2;}
        }
        var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.vertices=v;mesh.colors=c;mesh.triangles=t;mesh.RecalculateNormals();MeshObject("Freely traversable county terrain",mesh,new Material(Resources.Load<Shader>("TerrainColor")));
        var water=new Material(Resources.Load<Shader>("LakeWater"));var verts=new List<Vector3>{new Vector3(-730,1,-720)};var tris=new List<int>();
        for(int i=0;i<=96;i++){float a=i*Mathf.PI*2/96;verts.Add(new Vector3(-730+Mathf.Cos(a)*221,1,-720+Mathf.Sin(a)*221));if(i>0)tris.AddRange(new[]{0,i+1,i});}
        var lakeMesh=new Mesh();lakeMesh.SetVertices(verts);lakeMesh.SetTriangles(tris,0);lakeMesh.RecalculateNormals();MeshObject("Azure lake",lakeMesh,water);
        // Distant ridges frame the playable region.
        for(int i=0;i<60;i++){float a=i*Mathf.PI*2/60;Vector3 p=new Vector3(Mathf.Cos(a)*2350,-20,Mathf.Sin(a)*2350);Add(cone,p,new Vector3(260,UnityEngine.Random.Range(180,380),260),stone,Quaternion.identity);}
    }
    void BuildTown()
    {
        for(int x=-330;x<=330;x+=110)for(int z=-330;z<=330;z+=110){
            var p=At(x,z);NearestRoad(p,out float near,out _);if(near<48)continue;
            float h=UnityEngine.Random.Range(9,34),w=UnityEngine.Random.Range(28,45),depth=UnityEngine.Random.Range(28,48);Vector3 size=new Vector3(w,h,depth);
            Box(p+Vector3.up*(h/2),size,(x+z)%3==0?ochre:concrete);Obstacle(p+Vector3.up*(h/2),size);
            Box(p+Vector3.up*(h+.3f),new Vector3(w+2,.65f,depth+2),dark);
            for(float y=3;y<h-1;y+=4){Box(p+new Vector3(0,y,-depth/2-.04f),new Vector3(w*.85f,1.6f,.08f),glass);Box(p+new Vector3(0,y,depth/2+.04f),new Vector3(w*.85f,1.6f,.08f),glass);Box(p+new Vector3(w/2+.04f,y,0),new Vector3(.08f,1.6f,depth*.85f),glass);}
            Box(p+new Vector3(0,.06f,0),new Vector3(w+8,.10f,depth+8),concrete);
        }
        var garage=At(-37,-450);Box(garage+Vector3.up*4,new Vector3(32,8,28),concrete);Obstacle(garage,new Vector3(32,12,28));
        Box(garage+new Vector3(16.1f,3,0),new Vector3(.15f,5,20),dark);
        Box(garage+Vector3.up*8.5f,new Vector3(35,.8f,31),ochre);
        Sign("APEX / COUNTY GARAGE",garage+new Vector3(16.3f,6.8f,0),Quaternion.Euler(0,-90,0),.13f);
    }
    void BuildNature()
    {
        for(int i=0;i<1800;i++){
            Vector3 p=At(UnityEngine.Random.Range(-1280,1280),UnityEngine.Random.Range(-1280,1280));
            if(Mathf.Abs(p.x)<550&&Mathf.Abs(p.z)<600||Lake(p.x,p.z)<1.24f||IsUrbanLot(p))continue;
            NearestRoad(p,out float near,out _);if(near<25)continue;
            if(p.x>720&&UnityEngine.Random.value>.35f){float s=UnityEngine.Random.Range(5,13);Add(cone,p,new Vector3(s,s*.8f,s),stone,Quaternion.identity);Obstacle(p,new Vector3(s*.9f,10,s*.9f));continue;}
            float h=UnityEngine.Random.Range(8,21);Box(p+Vector3.up*(h*.3f),new Vector3(.6f,h*.6f,.6f),wood);Obstacle(p,new Vector3(.8f,15,.8f));
            for(int k=0;k<3;k++)Add(cone,p+Vector3.up*(h*.2f+k*h*.19f),new Vector3(h*(.23f-k*.04f),h*.55f,h*(.23f-k*.04f)),green,Quaternion.identity);
        }
    }
    void BuildLandmarks()
    {
        var lookout=At(1110,1060);Box(lookout+Vector3.up*7,new Vector3(11,14,11),concrete);Obstacle(lookout,new Vector3(11,16,11));Box(lookout+Vector3.up*15,new Vector3(20,2,20),glass);Box(lookout+Vector3.up*17,new Vector3(23,.8f,23),dark);
        Sign("SUMMIT OBSERVATORY",lookout+new Vector3(0,19,-12),Quaternion.identity,.24f);
        var pier=At(-950,-720);Box(new Vector3(-949,3,-720),new Vector3(54,.65f,12),wood);Obstacle(new Vector3(-926,3,-720),new Vector3(3,3,12));
        Sign("AZURE SHORE",At(-1017,-635)+Vector3.up*5,Quaternion.Euler(0,90,0),.25f);
        for(int i=0;i<5;i++){var p=At(-460-i*35,970);Box(p+Vector3.up*3,new Vector3(20,6,16),wood);Obstacle(p,new Vector3(20,8,16));Add(cone,p+Vector3.up*6,new Vector3(16,5,13),dark,Quaternion.Euler(0,45,0));}
        Sign("CEDAR RESERVE",At(-436,1018)+Vector3.up*6,Quaternion.identity,.23f);
    }
    void AddEvent(string name,int[] nodes)
    {
        var path=new List<Vector3>();
        for(int n=1;n<nodes.Length;n++){
            bool reverse=!edges.TryGetValue((nodes[n-1],nodes[n]),out var points);if(reverse)points=edges[(nodes[n],nodes[n-1])];
            for(int k=0;k<points.Length;k++){int i=reverse?points.Length-1-k:k;if(path.Count==0||Vector3.Distance(path[path.Count-1],points[i])>.1f)path.Add(points[i]);}
        }
        var route=new SprintRoute(name,path.ToArray());Events.Add(route);
        Vector3 at=route.Points[0],forward=(route.Points[1]-at).normalized,side=Vector3.Cross(Vector3.up,forward);
        for(int s=-1;s<=1;s+=2){Box(at+side*s*13+Vector3.up*4,new Vector3(.5f,8,.5f),beacon);Box(at+side*s*13+Vector3.up*6,new Vector3(2,4,.12f),beacon,Quaternion.LookRotation(forward));}
        Sign(name+"\nE / START SPRINT",at+Vector3.up*9,Quaternion.LookRotation(forward),.25f);
    }
    public Vector2 MapPoint(Vector3 p)=>new Vector2((p.x+Extent)/(Extent*2),1-(p.z+Extent)/(Extent*2));
    public Vector3 MapPosition(Vector2 p)=>At(p.x*Extent*2-Extent,(1-p.y)*Extent*2-Extent);
    Texture2D MakeMap()
    {
        const int n=640;var texture=new Texture2D(n,n,TextureFormat.RGB24,false);var colors=new Color[n*n];
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){float xx=x/(float)(n-1)*Extent*2-Extent,zz=y/(float)(n-1)*Extent*2-Extent;colors[y*n+x]=Lake(xx,zz)<1.08f?new Color(.07f,.32f,.43f):Color.Lerp(new Color(.10f,.18f,.13f),new Color(.38f,.30f,.21f),Height(xx,zz)/100);}
        foreach(var b in urbanFootprints){int x0=Mathf.Clamp((int)((b.min.x+Extent)/(Extent*2)*n),0,n-1),x1=Mathf.Clamp((int)((b.max.x+Extent)/(Extent*2)*n),0,n-1),y0=Mathf.Clamp((int)((b.min.z+Extent)/(Extent*2)*n),0,n-1),y1=Mathf.Clamp((int)((b.max.z+Extent)/(Extent*2)*n),0,n-1);for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)colors[y*n+x]=new Color(.32f,.39f,.42f);}
        foreach(var road in Roads)foreach(var p in road){int x=Mathf.RoundToInt((p.x+Extent)/(Extent*2)*(n-1)),y=Mathf.RoundToInt((p.z+Extent)/(Extent*2)*(n-1));for(int dx=-2;dx<=2;dx++)for(int dy=-2;dy<=2;dy++)if(x+dx>=0&&x+dx<n&&y+dy>=0&&y+dy<n)colors[(y+dy)*n+x+dx]=new Color(.75f,.77f,.70f);}
        texture.SetPixels(colors);texture.Apply();return texture;
    }
    static Mesh MakeCone(){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<10;i++){float a=i*Mathf.PI/5,b=(i+1)*Mathf.PI/5;int n=v.Count;v.AddRange(new[]{new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),Vector3.up,new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))});t.AddRange(new[]{n,n+1,n+2});}var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();return mesh;}
    void Box(Vector3 p,Vector3 size,Material m,Quaternion? q=null)=>Add(cube,p,size,m,q??Quaternion.identity);
    void Add(Mesh mesh,Vector3 p,Vector3 size,Material material,Quaternion q){var key=(material,Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.ContainsKey(key))batches[key]=new List<CombineInstance>();batches[key].Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,size)});}
    void Flush(){foreach(var b in batches){var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(b.Value.ToArray());MeshObject("County scenery / "+b.Key.Item1.name,mesh,b.Key.Item1);}batches.Clear();}
    void MeshObject(string name,Mesh mesh,Material material){var go=new GameObject(name);go.transform.SetParent(Root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;}
    void Sign(string text,Vector3 p,Quaternion q,float size){var go=new GameObject(text);go.transform.SetParent(Root,false);go.transform.SetPositionAndRotation(p,q);var tm=go.AddComponent<TextMesh>();tm.text=text;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.characterSize=size;tm.fontSize=64;tm.color=Color.white;}
}

public sealed class SprintRoute
{
    public readonly string Name;
    public readonly Vector3[] Points;
    public readonly float[] Distances;
    public readonly Vector3[] Checkpoints;
    public readonly float[] CheckpointDistances;
    public float Length=>Distances[Distances.Length-1];
    public SprintRoute(string name,Vector3[] points){Name=name;Points=points;Distances=new float[points.Length];for(int i=1;i<points.Length;i++)Distances[i]=Distances[i-1]+Vector3.Distance(points[i-1],points[i]);var gates=new List<Vector3>();var progress=new List<float>();for(float d=65;d<Length-35;d+=65){gates.Add(Position(d));progress.Add(d);}gates.Add(points[points.Length-1]);progress.Add(Length);Checkpoints=gates.ToArray();CheckpointDistances=progress.ToArray();}
    public Vector3 Position(float distance){distance=Mathf.Clamp(distance,0,Length);int a=0,b=Distances.Length-1;while(b-a>1){int m=(a+b)/2;if(Distances[m]<=distance)a=m;else b=m;}return Vector3.Lerp(Points[a],Points[b],Mathf.InverseLerp(Distances[a],Distances[b],distance));}
    public Vector3 Direction(float distance)=>(Position(Mathf.Min(distance+2,Length))-Position(Mathf.Max(0,distance-2))).normalized;
}
}
