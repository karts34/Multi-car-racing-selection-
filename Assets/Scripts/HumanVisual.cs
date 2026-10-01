using UnityEngine;
namespace Afterglow
{
public sealed class HumanVisual
{
    public readonly Transform Root;
    readonly Transform body,leftArm,rightArm,leftLeg,rightLeg,leftElbow,rightElbow,leftKnee,rightKnee;
    public bool Carrying;
    static Material skin,red,blue,black,eyes;
    static Transform Pivot(Transform parent,string name,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
    static Transform Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(type);g.name=name;Object.Destroy(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g.transform;}
    static void Thread(Transform parent,string name,Vector3[] points,float width,Material material){var g=new GameObject(name);g.transform.SetParent(parent,false);var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=line.endWidth=width;line.sharedMaterial=material;line.numCapVertices=2;}
    static readonly float[] heights={.91f,1.02f,1.15f,1.34f,1.46f,1.52f},widths={.17f,.16f,.165f,.235f,.245f,.15f},depths={.115f,.105f,.11f,.145f,.12f,.085f};
    static Vector3 TorsoPoint(int row,float angle,float padding=0)=>new Vector3(Mathf.Cos(angle)*(widths[row]+padding),heights[row],Mathf.Sin(angle)*(depths[row]+padding));
    static void Torso(Transform root,Material material)
    {
        const int sides=24;var vertices=new Vector3[heights.Length*sides+2];var triangles=new int[heights.Length*sides*6];int n=0;
        for(int row=0;row<heights.Length;row++)for(int j=0;j<sides;j++){vertices[row*sides+j]=TorsoPoint(row,j*Mathf.PI*2/sides);if(row==heights.Length-1)continue;int a=row*sides+j,b=row*sides+(j+1)%sides,c=a+sides,d=b+sides;triangles[n++]=a;triangles[n++]=c;triangles[n++]=b;triangles[n++]=b;triangles[n++]=c;triangles[n++]=d;}
        int bottom=vertices.Length-2,top=vertices.Length-1;vertices[bottom]=Vector3.up*heights[0];vertices[top]=Vector3.up*heights[heights.Length-1];
        for(int j=0;j<sides;j++){int next=(j+1)%sides;triangles[n++]=bottom;triangles[n++]=j;triangles[n++]=next;triangles[n++]=top;triangles[n++]=(heights.Length-1)*sides+next;triangles[n++]=(heights.Length-1)*sides+j;}
        var g=new GameObject("Anatomical tapered torso");g.transform.SetParent(root,false);var mesh=new Mesh{name="Chest waist and hips"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    static void Eye(Transform root,int side,float size,float z,Material mat)
    {
        Vector3[] points={new Vector3(.025f,1.758f,z),new Vector3(.115f,1.785f,z-.025f),new Vector3(.094f,1.708f,z-.008f),new Vector3(.054f,1.707f,z)};
        Vector3 center=new Vector3(.071f,1.738f,z);for(int i=0;i<points.Length;i++){points[i]=center+(points[i]-center)*size;points[i].x*=side;}
        var g=new GameObject("Pointed mask lens");g.transform.SetParent(root,false);var m=new Mesh();m.vertices=points;m.triangles=side==1?new[]{2,1,0,3,2,0}:new[]{0,1,2,0,2,3};m.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }
    public HumanVisual(Transform parent,string name,bool hero,int index=0)
    {
        if(skin==null){skin=TrackWorld.Mat("Skin",new Color(.65f,.40f,.26f));red=TrackWorld.Mat("Crimson suit fabric",new Color(.70f,.025f,.045f),0,.23f);blue=TrackWorld.Mat("Midnight blue fabric",new Color(.025f,.09f,.32f),0,.24f);black=TrackWorld.Mat("Suit web embroidery",new Color(.012f,.016f,.025f));eyes=TrackWorld.Mat("White eye lenses",new Color(.93f,.97f,1),.1f,.45f);}
        Root=Pivot(parent,name,Vector3.zero);body=Pivot(Root,"Animated skeleton",Vector3.zero);
        Material shirt=hero?red:TrackWorld.Mat("Citizen shirt "+index,Color.HSVToRGB(index*.137f%1,.45f,.64f)),pants=hero?blue:TrackWorld.Mat("Citizen trousers "+index,new Color(.08f,.11f,.17f));
        Torso(body,shirt);Part(body,"Pelvis",PrimitiveType.Sphere,new Vector3(0,.96f,0),new Vector3(.35f,.22f,.24f),pants);
        Part(body,"Neck",PrimitiveType.Capsule,new Vector3(0,1.56f,0),new Vector3(.115f,.085f,.12f),hero?red:skin);
        Part(body,"Human head",PrimitiveType.Sphere,new Vector3(0,1.72f,0),new Vector3(.24f,.30f,.255f),hero?red:skin);
        Transform[] arms=new Transform[2],elbows=new Transform[2],legs=new Transform[2],knees=new Transform[2];
        for(int i=0;i<2;i++){
            int side=i==0?-1:1;
            arms[i]=Pivot(body,"Shoulder joint",new Vector3(side*.255f,1.45f,0));Part(arms[i],"Deltoid",PrimitiveType.Sphere,Vector3.zero,new Vector3(.175f,.19f,.18f),shirt);
            Part(arms[i],"Upper arm",PrimitiveType.Capsule,new Vector3(0,-.145f,0),new Vector3(.13f,.16f,.14f),hero?blue:shirt);
            elbows[i]=Pivot(arms[i],"Elbow joint",new Vector3(0,-.29f,0));Part(elbows[i],"Elbow",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.115f,shirt);
            Part(elbows[i],"Forearm",PrimitiveType.Capsule,new Vector3(0,-.13f,0),new Vector3(.115f,.14f,.12f),shirt);
            Part(elbows[i],"Hand",PrimitiveType.Sphere,new Vector3(0,-.305f,.015f),new Vector3(.09f,.14f,.085f),hero?red:skin);
            legs[i]=Pivot(body,"Hip joint",new Vector3(side*.105f,.96f,0));Part(legs[i],"Thigh",PrimitiveType.Capsule,new Vector3(0,-.21f,0),new Vector3(.19f,.235f,.20f),pants);
            knees[i]=Pivot(legs[i],"Knee joint",new Vector3(0,-.43f,0));Part(knees[i],"Knee",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.14f,pants);
            Part(knees[i],"Calf",PrimitiveType.Capsule,new Vector3(0,-.205f,0),new Vector3(.14f,.215f,.15f),hero?red:pants);
            Part(knees[i],"Foot",PrimitiveType.Sphere,new Vector3(0,-.47f,.065f),new Vector3(.145f,.12f,.29f),hero?red:black);
        }
        leftArm=arms[0];rightArm=arms[1];leftElbow=elbows[0];rightElbow=elbows[1];leftLeg=legs[0];rightLeg=legs[1];leftKnee=knees[0];rightKnee=knees[1];
        if(hero){
            for(int s=-1;s<=1;s+=2){Eye(body,s,1.15f,.126f,black);Eye(body,s,.82f,.131f,eyes);}
            for(int row=1;row<heights.Length;row++){var p=new Vector3[25];for(int j=0;j<p.Length;j++)p[j]=TorsoPoint(row,j*Mathf.PI*2/24,.003f);Thread(body,"Torso web ring",p,.004f,black);}
            for(int j=0;j<12;j++){var p=new Vector3[heights.Length];for(int r=0;r<p.Length;r++)p[r]=TorsoPoint(r,j*Mathf.PI/6,.003f);Thread(body,"Torso web seam",p,.0035f,black);}
            for(int j=0;j<8;j++){var p=new Vector3[17];for(int k=0;k<p.Length;k++){float a=k*Mathf.PI/16,az=j*Mathf.PI/4;p[k]=new Vector3(Mathf.Sin(a)*Mathf.Cos(az)*.122f,1.72f+Mathf.Cos(a)*.152f,Mathf.Sin(a)*Mathf.Sin(az)*.130f);}Thread(body,"Mask web radial",p,.003f,black);}
            for(int row=1;row<5;row++){float a=row*Mathf.PI/5;var p=new Vector3[25];for(int k=0;k<p.Length;k++){float az=k*Mathf.PI/12;p[k]=new Vector3(Mathf.Sin(a)*Mathf.Cos(az)*.122f,1.72f+Mathf.Cos(a)*.152f,Mathf.Sin(a)*Mathf.Sin(az)*.130f);}Thread(body,"Mask web ring",p,.003f,black);}
            for(int face=-1;face<=1;face+=2){Part(body,"Spider emblem",PrimitiveType.Sphere,new Vector3(0,1.34f,face*.151f),new Vector3(.048f,.10f,.018f),black);for(int s=-1;s<=1;s+=2)for(int i=0;i<4;i++)Thread(body,"Spider emblem leg",new[]{new Vector3(0,1.32f+i*.012f,face*.153f),new Vector3(s*.07f,1.27f+i*.046f,face*.141f),new Vector3(s*.10f,1.23f+i*.066f,face*.128f)},.009f,black);}
        }else{Part(body,"Hair",PrimitiveType.Sphere,new Vector3(0,1.82f,-.015f),new Vector3(.25f,.13f,.26f),black);for(int s=-1;s<=1;s+=2)Part(body,"Eye",PrimitiveType.Sphere,new Vector3(s*.047f,1.745f,.121f),new Vector3(.022f,.016f,.012f),black);}
    }
    public void Animate(float speed,float time,bool air,bool web,bool climb)
    {
        float amount=Mathf.Clamp01(speed/3.5f),phase=time*(speed>6?11:7),s=Mathf.Sin(phase),stride=s*(speed>6?38:25)*amount;
        body.localPosition=Vector3.up*(air?0:Mathf.Abs(Mathf.Sin(phase))*amount*.025f);body.localRotation=Quaternion.Euler(climb?0:air?8:speed>6?7:0,Mathf.Sin(phase)*amount*3,0);
        leftLeg.localRotation=Quaternion.Euler(air?-24:stride,0,0);rightLeg.localRotation=Quaternion.Euler(air?15:-stride,0,0);
        leftKnee.localRotation=Quaternion.Euler(air?55:Mathf.Max(0,-s)*amount*55,0,0);rightKnee.localRotation=Quaternion.Euler(air?65:Mathf.Max(0,s)*amount*55,0,0);
        leftArm.localRotation=Quaternion.Euler(Carrying?-165:web?-160:climb?-135:air?-55:-stride*.8f,0,-6);rightArm.localRotation=Quaternion.Euler(Carrying?-165:climb?-135:air?-40:stride*.8f,0,6);
        leftElbow.localRotation=Quaternion.Euler(Carrying?-15:web?-10:climb?-30:-(speed>6?75:15),0,0);rightElbow.localRotation=Quaternion.Euler(Carrying?-15:climb?-30:-(speed>6?75:15),0,0);
    }
}
}
