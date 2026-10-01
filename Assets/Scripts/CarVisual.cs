using System;
using System.Collections.Generic;
using UnityEngine;

namespace Afterglow
{
public class CarVisual
{
    public Transform root;
    public readonly List<Transform> wheels = new List<Transform>();
    readonly List<Transform> steeringKnuckles = new List<Transform>();
    public Material brakeMaterial, paint;
    public Transform glow;
    float wheelAngle;
    readonly CarBody body;
    float wheelbase=1.48f;

    public CarVisual(string name, Color color, CarBody body=CarBody.Sport)
    {
        this.body=body;
        root = new GameObject(name).transform;
        paint=TrackWorld.Mat(name+" ceramic metallic",color,.68f,.92f);
        var glass=TrackWorld.Mat("Obsidian blue glazing",new Color(.055f,.115f,.155f),.72f,.96f);
        var carbon=TrackWorld.Mat("Satin exposed carbon",new Color(.036f,.043f,.05f),.42f,.48f);
        var rubber=TrackWorld.Mat("Sport tire compound",new Color(.027f,.031f,.035f),.02f,.18f);
        var alloy=TrackWorld.Mat("Satin titanium alloy",new Color(.56f,.61f,.64f),.85f,.77f);
        var disk=TrackWorld.Mat("Carbon ceramic brakes",new Color(.24f,.25f,.26f),.62f,.45f);
        var caliper=TrackWorld.Mat("Vermilion caliper",new Color(.95f,.12f,.035f),.48f,.65f);
        var led=TrackWorld.Mat("Ice white LED",new Color(.64f,.84f,1),.25f,.65f,2.4f);
        brakeMaterial=TrackWorld.Mat("Ruby light blades",new Color(1,.025f,.008f),.25f,.65f,1.4f);
        if(body!=CarBody.Sport){BuildAlternative(glass,carbon,rubber,alloy,disk,caliper,led);FinishBuild(alloy,carbon,body==CarBody.Luxury?2.84f:2.43f);return;}
        Body();
        Loft("Glasshouse",new[]{-1.22f,-.94f,-.58f,.28f,.65f,1.08f},new[]{.78f,.76f,.70f,.70f,.75f,.83f},new[]{.88f,1.20f,1.42f,1.42f,1.22f,.87f},.81f,glass);
        Loft("Sculpted roof",new[]{-.71f,-.52f,.25f,.46f},new[]{.64f,.705f,.705f,.64f},new[]{1.39f,1.45f,1.45f,1.38f},1.35f,paint);
        for(int s=-1;s<=1;s+=2) {
            Beam("A pillar",new Vector3(s*.74f,1.36f,.45f),new Vector3(s*.85f,.89f,1.10f),.043f,paint);
            Beam("Rear sail",new Vector3(s*.72f,1.38f,-.64f),new Vector3(s*.88f,.87f,-1.37f),.07f,paint);
            Beam("Window sill",new Vector3(s*.84f,.90f,-1.13f),new Vector3(s*.87f,.89f,1.03f),.035f,carbon);
            Part("Door handle",new Vector3(s*1.015f,.81f,-.41f),new Vector3(.017f,.025f,.20f),alloy);
            Part("Mirror stalk",new Vector3(s*.94f,1.02f,.73f),new Vector3(.22f,.035f,.06f),carbon);
            Part("Aero mirror",new Vector3(s*1.095f,1.055f,.72f),new Vector3(.28f,.11f,.29f),paint,Quaternion.Euler(0,s*12,0),PrimitiveType.Sphere);
            Part("Mirror glass",new Vector3(s*1.105f,1.058f,.62f),new Vector3(.20f,.075f,.018f),glass);
            Part("Side skirt",new Vector3(s*1.04f,.265f,0),new Vector3(.15f,.085f,2.03f),carbon,Quaternion.Euler(0,s*-2,0));
            Panel("Side intake",new[]{new Vector3(s*1.08f,.38f,-.86f),new Vector3(s*1.08f,.77f,-.90f),new Vector3(s*1.04f,.82f,-.42f),new Vector3(s*1.04f,.48f,-.59f)},carbon);
            Beam("Intake rim",new Vector3(s*1.085f,.79f,-.92f),new Vector3(s*1.045f,.84f,-.43f),.025f,paint);
            for(int k=0;k<3;k++)Part("Fender gill",new Vector3(s*.94f,1.007f,-1.40f+k*.11f),new Vector3(.16f,.015f,.037f),carbon);
            Part("Hood heat extractor",new Vector3(s*.48f,.854f,1.38f),new Vector3(.23f,.016f,.49f),carbon,Quaternion.Euler(6,0,s*2));
            for(int k=0;k<5;k++)Part("Extractor slat",new Vector3(s*.48f,.867f,1.20f+k*.08f),new Vector3(.21f,.017f,.022f),alloy);
            Part("Headlight housing",new Vector3(s*.77f,.66f,2.30f),new Vector3(.48f,.145f,.16f),carbon,Quaternion.Euler(0,-s*15,0),PrimitiveType.Sphere);
            for(int k=0;k<3;k++)Part("LED projector",new Vector3(s*(.61f+k*.11f),.69f,2.365f-k*.025f),new Vector3(.064f,.043f,.031f),led);
            Beam("Daytime light signature",new Vector3(s*.55f,.748f,2.335f),new Vector3(s*.97f,.731f,2.26f),.026f,led);
            Beam("Vertical DRL",new Vector3(s*.98f,.724f,2.26f),new Vector3(s*1.01f,.54f,2.24f),.023f,led);
            Part("Front brake duct",new Vector3(s*.84f,.41f,2.31f),new Vector3(.27f,.14f,.065f),carbon,Quaternion.Euler(0,-s*12,0));
            Part("Rear light recess",new Vector3(s*.60f,.76f,-2.41f),new Vector3(.73f,.145f,.028f),carbon);
            Beam("Rear upper light",new Vector3(s*.23f,.805f,-2.44f),new Vector3(s*.955f,.805f,-2.44f),.029f,brakeMaterial);
            Beam("Rear light return",new Vector3(s*.955f,.805f,-2.44f),new Vector3(s*.91f,.697f,-2.44f),.026f,brakeMaterial);
            Part("Wing support",new Vector3(s*.63f,1.05f,-1.93f),new Vector3(.037f,.28f,.09f),carbon,Quaternion.Euler(-17,0,0));
            Part("Wing endplate",new Vector3(s*1.02f,1.205f,-2.09f),new Vector3(.03f,.15f,.34f),carbon,Quaternion.Euler(0,0,s*8));
            for(int axle=-1;axle<=1;axle+=2)Wheel(s,axle,rubber,alloy,disk,caliper,carbon);
        }
        Loft("Front splitter",new[]{1.99f,2.39f,2.48f},new[]{1.07f,1.04f,.91f},new[]{.28f,.28f,.26f},.23f,carbon);
        Part("Front center intake",new Vector3(0,.43f,2.40f),new Vector3(.93f,.15f,.033f),carbon);
        for(int i=-5;i<=5;i++)Part("Intake vanes",new Vector3(i*.077f,.43f,2.42f),new Vector3(.012f,.13f,.02f),alloy);
        Part("Rear grille",new Vector3(0,.51f,-2.408f),new Vector3(1.58f,.14f,.023f),carbon);
        for(int i=-8;i<=8;i++)Part("Rear grille slat",new Vector3(i*.085f,.51f,-2.425f),new Vector3(.008f,.11f,.010f),alloy);
        Loft("Diffuser tray",new[]{-2.47f,-2.12f,-1.96f},new[]{.97f,1.02f,.96f},new[]{.28f,.28f,.23f},.19f,carbon);
        for(int i=-3;i<=3;i++)Part("Diffuser strake",new Vector3(i*.25f,.24f,-2.27f),new Vector3(.018f,.15f,.43f),carbon,Quaternion.Euler(-9,0,0));
        Loft("Rear aerofoil",new[]{-2.27f,-2.15f,-1.94f},new[]{1.035f,1.04f,1.02f},new[]{1.205f,1.22f,1.19f},1.17f,carbon);
        for(int k=0;k<7;k++)Part("Engine cover louver",new Vector3(0,.916f,-1.19f-k*.10f),new Vector3(1.10f,.018f,.033f),carbon);
        Part("Rear marque",new Vector3(0,.80f,-2.386f),new Vector3(.21f,.034f,.018f),alloy);
        Part("Hood crest",new Vector3(0,.746f,2.10f),new Vector3(.07f,.009f,.105f),alloy);
        FinishBuild(alloy,carbon,2.43f);
    }
    void FinishBuild(Material alloy,Material carbon,float rear)
    {
        var nitro=TrackWorld.Mat("Blue exhaust core",new Color(.07f,.45f,1),.1f,.7f,2.8f);
        glow=new GameObject("Nitro exhaust").transform;glow.SetParent(root,false);
        for(int s=-1;s<=1;s+=2){
            var exhaust=new GameObject("Titanium exhaust").transform;exhaust.SetParent(root,false);exhaust.localPosition=new Vector3(s*.72f,.36f,-rear);exhaust.localRotation=Quaternion.Euler(0,90,0);
            Torus("Rolled exhaust lip",exhaust,.079f,.013f,.023f,0,alloy);
            Part("Exhaust interior",new Vector3(s*.72f,.36f,-rear+.015f),new Vector3(.13f,.13f,.015f),carbon,Quaternion.identity,PrimitiveType.Sphere);
            Part("Boost flame",new Vector3(s*.72f,.36f,-rear-.20f),new Vector3(.095f,.095f,.38f),nitro,Quaternion.identity,PrimitiveType.Sphere,glow);
        }
        glow.gameObject.SetActive(false);
        CombineParts(root,true);
        foreach(var wheel in wheels)CombineParts(wheel,false);
    }
    void BuildAlternative(Material glass,Material carbon,Material rubber,Material alloy,Material disk,Material caliper,Material led)
    {
        bool luxury=body==CarBody.Luxury, muscle=body==CarBody.Muscle;
        wheelbase=luxury?1.85f:1.48f;
        Body();
        float front=luxury?2.84f:2.43f,rear=front;
        var silver=TrackWorld.Mat("Polished coachwork",new Color(.73f,.77f,.80f),.88f,.91f);
        if(luxury){
            Loft("Tall formal glasshouse",new[]{-1.92f,-1.40f,.36f,.92f},new[]{.88f,.83f,.83f,.92f},new[]{1.13f,1.85f,1.85f,1.17f},1.08f,glass);
            Loft("Silver coach roof",new[]{-1.49f,-1.30f,.32f,.48f},new[]{.80f,.84f,.84f,.79f},new[]{1.83f,1.91f,1.91f,1.82f},1.80f,silver);
            Loft("Long silver bonnet",new[]{.94f,1.50f,2.76f},new[]{.81f,.82f,.72f},new[]{1.24f,1.23f,1.16f},1.10f,silver);
            Part("Upright chrome grille surround",new Vector3(0,.83f,front),new Vector3(1.05f,.76f,.10f),silver);
            Part("Deep grille shadow",new Vector3(0,.83f,front+.057f),new Vector3(.91f,.65f,.018f),carbon);
            for(int i=-8;i<=8;i++)Part("Vertical polished grille vane",new Vector3(i*.050f,.83f,front+.077f),new Vector3(.017f,.64f,.028f),silver);
            Part("Grille crown",new Vector3(0,1.24f,front-.015f),new Vector3(1.08f,.10f,.18f),silver);
            // An original geometric bonnet ornament; no manufacturer badge.
            Part("Bonnet ornament stem",new Vector3(0,1.32f,2.62f),new Vector3(.025f,.15f,.025f),silver);
            Part("Diamond bonnet ornament",new Vector3(0,1.43f,2.62f),new Vector3(.10f,.10f,.035f),silver,Quaternion.Euler(0,0,45));
            for(int s=-1;s<=1;s+=2){
                Beam("Chrome A pillar",new Vector3(s*.84f,1.83f,.38f),new Vector3(s*.94f,1.16f,.96f),.048f,silver);
                Beam("Formal rear pillar",new Vector3(s*.82f,1.83f,-1.40f),new Vector3(s*.94f,1.13f,-1.97f),.12f,paint);
                Beam("B pillar",new Vector3(s*.85f,1.84f,-.56f),new Vector3(s*.94f,1.12f,-.56f),.065f,carbon);
                Beam("Chrome window sill",new Vector3(s*.94f,1.14f,-1.91f),new Vector3(s*.94f,1.14f,.97f),.028f,silver);
                Beam("Coachline",new Vector3(s*1.075f,1.05f,-2.53f),new Vector3(s*1.075f,1.05f,2.57f),.018f,silver);
                Beam("Rear coach door seam",new Vector3(s*1.077f,.41f,-.55f),new Vector3(s*1.077f,1.10f,-.55f),.009f,carbon);
                for(int k=0;k<2;k++)Part("Coach door handle",new Vector3(s*1.088f,1.02f,-.37f-k*.37f),new Vector3(.035f,.035f,.22f),silver);
                Part("Rectangular headlamp bezel",new Vector3(s*.80f,.99f,front-.015f),new Vector3(.48f,.23f,.09f),silver);
                Part("Headlamp dark inset",new Vector3(s*.80f,.99f,front+.039f),new Vector3(.41f,.16f,.02f),carbon);
                Part("Crystal LED headlamp",new Vector3(s*.80f,1.025f,front+.055f),new Vector3(.36f,.055f,.02f),led);
                Part("Rear vertical ruby lamp",new Vector3(s*.90f,.91f,-rear),new Vector3(.17f,.33f,.05f),brakeMaterial);
                Part("Chrome rocker rail",new Vector3(s*1.08f,.34f,0),new Vector3(.07f,.07f,2.8f),silver);
            }
            Part("Chrome bumper blade",new Vector3(0,.39f,front),new Vector3(2.05f,.045f,.08f),silver);
            Part("Rear chrome blade",new Vector3(0,.47f,-rear),new Vector3(1.96f,.045f,.06f),silver);
        }else if(muscle){
            Loft("Muscle coupe glazing",new[]{-1.45f,-.91f,.32f,.94f},new[]{.87f,.78f,.78f,.90f},new[]{.98f,1.55f,1.55f,1.04f},.92f,glass);
            Loft("Fastback roof",new[]{-1.04f,-.83f,.30f,.46f},new[]{.74f,.80f,.80f,.74f},new[]{1.49f,1.60f,1.60f,1.48f},1.46f,paint);
            Part("Full width grille",new Vector3(0,.78f,front),new Vector3(1.97f,.29f,.08f),carbon);
            Part("Hood power bulge",new Vector3(0,1.095f,1.48f),new Vector3(.73f,.11f,1.12f),paint);
            Part("Hood scoop",new Vector3(0,1.14f,1.94f),new Vector3(.53f,.09f,.08f),carbon);
            for(int s=-1;s<=1;s+=2){
                for(int k=0;k<2;k++)Part("Round projector",new Vector3(s*(.64f+k*.23f),.80f,front+.055f),new Vector3(.18f,.18f,.045f),led,Quaternion.identity,PrimitiveType.Sphere);
                Beam("Coupe A pillar",new Vector3(s*.79f,1.54f,.35f),new Vector3(s*.92f,1.01f,.96f),.065f,paint);
                Beam("Fastback sail",new Vector3(s*.80f,1.54f,-.91f),new Vector3(s*.95f,.96f,-1.55f),.13f,paint);
                for(int k=0;k<3;k++)Part("Triple rear lamp",new Vector3(s*(.45f+k*.19f),.83f,-rear),new Vector3(.10f,.22f,.04f),brakeMaterial);
                Part("Hood racing stripe",new Vector3(s*.25f,1.062f,1.55f),new Vector3(.13f,.018f,1.57f),carbon);
                Part("Door handle",new Vector3(s*1.08f,.91f,-.65f),new Vector3(.035f,.035f,.22f),alloy);
            }
            Part("Ducktail spoiler",new Vector3(0,1.045f,-2.16f),new Vector3(2.10f,.12f,.27f),carbon,Quaternion.Euler(-12,0,0));
        }else{
            Loft("Teardrop canopy",new[]{-1.22f,-.64f,.14f,.98f},new[]{.73f,.66f,.66f,.78f},new[]{.87f,1.28f,1.28f,.84f},.78f,glass);
            Loft("Carbon canopy spine",new[]{-.80f,-.53f,.17f,.36f},new[]{.60f,.67f,.67f,.60f},new[]{1.23f,1.32f,1.32f,1.23f},1.21f,carbon);
            for(int s=-1;s<=1;s+=2){
                Beam("Hypercar pillar",new Vector3(s*.68f,1.26f,.20f),new Vector3(s*.82f,.84f,1.00f),.036f,paint);
                Part("Deep side intake",new Vector3(s*1.085f,.65f,-.60f),new Vector3(.035f,.33f,.70f),carbon,Quaternion.Euler(0,0,s*8));
                Beam("Intake flying blade",new Vector3(s*1.12f,.43f,-.95f),new Vector3(s*1.10f,.91f,-.28f),.065f,paint);
                Part("Blade headlight",new Vector3(s*.78f,.64f,2.35f),new Vector3(.51f,.044f,.05f),led,Quaternion.Euler(0,-s*12,0));
                Part("Carbon dive plane",new Vector3(s*1.02f,.37f,1.99f),new Vector3(.26f,.035f,.55f),carbon,Quaternion.Euler(9,s*10,0));
                Part("Swan neck wing support",new Vector3(s*.63f,1.12f,-1.91f),new Vector3(.046f,.56f,.12f),carbon,Quaternion.Euler(-20,0,0));
                Part("Wing end fence",new Vector3(s*1.16f,1.42f,-2.09f),new Vector3(.035f,.23f,.49f),paint);
            }
            Part("Full width rear light",new Vector3(0,.79f,-rear),new Vector3(1.90f,.037f,.035f),brakeMaterial);
            Part("Carbon race wing",new Vector3(0,1.40f,-2.10f),new Vector3(2.34f,.055f,.46f),carbon,Quaternion.Euler(-6,0,0));
            for(int k=0;k<8;k++)Part("Engine cooling slat",new Vector3(0,.929f,-1.22f-k*.105f),new Vector3(.99f,.024f,.04f),carbon);
        }
        if(!luxury){
            Part("Front carbon splitter",new Vector3(0,.27f,2.35f),new Vector3(2.18f,.065f,.32f),carbon);
            Part("Rear diffuser",new Vector3(0,.29f,-2.33f),new Vector3(1.93f,.10f,.30f),carbon);
            for(int i=-3;i<=3;i++)Part("Diffuser fin",new Vector3(i*.27f,.25f,-2.32f),new Vector3(.022f,.19f,.37f),carbon);
        }
        for(int s=-1;s<=1;s+=2){
            Part("Mirror stalk",new Vector3(s*.98f,luxury?1.28f:1.04f,.64f),new Vector3(.24f,.035f,.06f),alloy);
            Part("Sculpted mirror",new Vector3(s*1.13f,luxury?1.30f:1.07f,.64f),new Vector3(.26f,.13f,.27f),paint,Quaternion.identity,PrimitiveType.Sphere);
            for(int axle=-1;axle<=1;axle+=2)Wheel(s,axle,rubber,luxury?silver:alloy,disk,caliper,carbon);
        }
    }
    void CombineParts(Transform parent,bool body)
    {
        var groups=new Dictionary<Material,List<CombineInstance>>();var originals=new List<GameObject>();
        foreach(var filter in parent.GetComponentsInChildren<MeshFilter>(true)){
            if(body){bool moving=filter.transform.IsChildOf(glow);foreach(var wheel in wheels)if(filter.transform.IsChildOf(wheel.parent))moving=true;if(moving)continue;}
            var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)continue;var material=renderer.sharedMaterial;
            if(!groups.ContainsKey(material))groups[material]=new List<CombineInstance>();
            groups[material].Add(new CombineInstance{mesh=filter.sharedMesh,transform=parent.worldToLocalMatrix*filter.transform.localToWorldMatrix});originals.Add(filter.gameObject);
        }
        foreach(var pair in groups){var mesh=new Mesh{name=pair.Key.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());var go=new GameObject(pair.Key.name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;}
        foreach(var go in originals)UnityEngine.Object.Destroy(go);
    }
    void Body()
    {
        float[] zs={-2.39f,-2.22f,-1.65f,-1.1f,-.35f,.5f,1.42f,2.02f,2.40f};
        float[] widths={.96f,1.075f,1.10f,1.07f,1.025f,1.035f,1.105f,1.07f,.91f};
        float[] tops={.81f,.90f,.925f,.89f,.86f,.87f,.855f,.77f,.61f};
        if(body==CarBody.Luxury){zs=new[]{-2.84f,-2.64f,-1.85f,-1.25f,-.35f,.70f,1.85f,2.55f,2.84f};widths=new[]{1.00f,1.09f,1.11f,1.08f,1.08f,1.08f,1.11f,1.08f,1.01f};tops=new[]{1.09f,1.14f,1.17f,1.15f,1.15f,1.17f,1.20f,1.16f,1.11f};}
        else if(body==CarBody.Muscle){widths=new[]{1.02f,1.10f,1.14f,1.08f,1.065f,1.07f,1.14f,1.10f,1.01f};tops=new[]{.92f,1.00f,1.04f,1.00f,1.00f,1.02f,1.03f,1.02f,.93f};}
        else if(body==CarBody.Hyper){tops=new[]{.83f,.89f,.90f,.85f,.80f,.81f,.78f,.63f,.52f};}
        const int slices=100,ring=14;var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<=slices;i++){
            float zz=Mathf.Lerp(zs[0],zs[zs.Length-1],i/(float)slices),w=Sample(zs,widths,zz),h=Sample(zs,tops,zz);
            float dz=Mathf.Min(Mathf.Abs(zz-wheelbase),Mathf.Abs(zz+wheelbase));
            float arch=dz<.535f?.46f+Mathf.Sqrt(.535f*.535f-dz*dz):.32f;
            float shoulder=Mathf.Max(h-.025f,arch+.035f),edge=Mathf.Max(.35f,arch);
            Vector2[] p={new Vector2(-w*.80f,.27f),new Vector2(-w,edge),new Vector2(-w,shoulder),new Vector2(-w*.89f,Mathf.Max(h+.005f,shoulder+.035f)),new Vector2(-w*.69f,h+.022f),new Vector2(-w*.36f,h+.035f),new Vector2(0,h+.04f),new Vector2(w*.36f,h+.035f),new Vector2(w*.69f,h+.022f),new Vector2(w*.89f,Mathf.Max(h+.005f,shoulder+.035f)),new Vector2(w,shoulder),new Vector2(w,edge),new Vector2(w*.80f,.27f),new Vector2(0,.25f)};
            foreach(var point in p)v.Add(new Vector3(point.x,point.y,zz));
            if(i>0)for(int k=0;k<ring;k++){int a=(i-1)*ring+k,b=(i-1)*ring+(k+1)%ring,c=i*ring+k,d=i*ring+(k+1)%ring;t.AddRange(new[]{a,c,b,b,c,d});}
        }
        for(int k=1;k<ring-1;k++){t.AddRange(new[]{0,k,k+1});int a=slices*ring;t.AddRange(new[]{a,a+k+1,a+k});}
        MeshPart("GT-R formed aluminium body",v,t,paint,root);
    }
    static float Sample(float[] z,float[] values,float at){int i=0;while(i<z.Length-2&&z[i+1]<at)i++;return Mathf.Lerp(values[i],values[i+1],Mathf.SmoothStep(0,1,Mathf.InverseLerp(z[i],z[i+1],at)));}
    void Wheel(int side,int axle,Material rubber,Material alloy,Material disk,Material caliper,Material carbon)
    {
        var knuckle=new GameObject(axle>0?"Front steering knuckle":"Rear hub").transform;knuckle.SetParent(root,false);knuckle.localPosition=new Vector3(side*1.035f,.46f,axle*wheelbase);if(axle>0)steeringKnuckles.Add(knuckle);
        var spin=new GameObject("Wheel rotation").transform;spin.SetParent(knuckle,false);wheels.Add(spin);
        Torus("Rounded performance tire",spin,.365f,.080f,.155f,0,rubber);
        Torus("Machined rim barrel",spin,.303f,.022f,.024f,side*.143f,alloy);
        Torus("Sidewall bead",spin,.375f,.004f,.004f,side*.151f,carbon);
        Part("Brake rotor",new Vector3(side*.12f,0,0),new Vector3(.50f,.010f,.50f),disk,Quaternion.Euler(0,0,90),PrimitiveType.Cylinder,spin);
        for(int k=0;k<5;k++)for(int split=-1;split<=1;split+=2){float a=k*Mathf.PI*2/5+split*.065f;Beam("Split Y spoke",new Vector3(side*.174f,Mathf.Cos(a)*.058f,Mathf.Sin(a)*.058f),new Vector3(side*.174f,Mathf.Cos(a+.15f)*.287f,Mathf.Sin(a+.15f)*.287f),.025f,alloy,spin);}
        if(body==CarBody.Luxury)for(int k=0;k<20;k++){float a=k*Mathf.PI*2/20;Beam("Coach wheel polished spoke",new Vector3(side*.18f,Mathf.Cos(a)*.065f,Mathf.Sin(a)*.065f),new Vector3(side*.18f,Mathf.Cos(a)*.29f,Mathf.Sin(a)*.29f),.024f,alloy,spin);}
        Part("Center lock",new Vector3(side*.19f,0,0),new Vector3(.12f,.019f,.12f),alloy,Quaternion.Euler(0,0,90),PrimitiveType.Cylinder,spin);
        for(int k=0;k<5;k++){float a=k*Mathf.PI*2/5;Part("Wheel bolt",new Vector3(side*.213f,Mathf.Cos(a)*.036f,Mathf.Sin(a)*.036f),new Vector3(.012f,.012f,.012f),carbon,Quaternion.identity,PrimitiveType.Sphere,spin);}
        Part("Fixed brake caliper",new Vector3(side*.13f,.06f,-.20f),new Vector3(.08f,.24f,.075f),caliper,Quaternion.Euler(20,0,0),PrimitiveType.Cube,knuckle);
    }
    void Torus(string name,Transform parent,float radius,float radial,float axial,float center,Material material)
    {
        const int n=64,m=12;var v=new List<Vector3>();var t=new List<int>();
        for(int a=0;a<=n;a++)for(int b=0;b<=m;b++){float u=a*Mathf.PI*2/n,w=b*Mathf.PI*2/m,r=radius+radial*Mathf.Cos(w);v.Add(new Vector3(center+axial*Mathf.Sin(w),r*Mathf.Cos(u),r*Mathf.Sin(u)));if(a<n&&b<m){int i=a*(m+1)+b;t.AddRange(new[]{i,i+m+1,i+1,i+1,i+m+1,i+m+2});}}
        MeshPart(name,v,t,material,parent);
    }
    void Loft(string name,float[] zs,float[] widths,float[] heights,float bottom,Material material)
    {
        const int n=36,r=12;var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<=n;i++){float z=Mathf.Lerp(zs[0],zs[zs.Length-1],i/(float)n),w=Sample(zs,widths,z),h=Sample(zs,heights,z),rise=h-bottom;Vector2[] profile={new Vector2(-w*.85f,bottom),new Vector2(-w,bottom+rise*.17f),new Vector2(-w,h-rise*.06f),new Vector2(-w*.82f,h),new Vector2(-w*.38f,h+.006f),new Vector2(0,h+.01f),new Vector2(w*.38f,h+.006f),new Vector2(w*.82f,h),new Vector2(w,h-rise*.06f),new Vector2(w,bottom+rise*.17f),new Vector2(w*.85f,bottom),new Vector2(0,bottom)};foreach(var point in profile)v.Add(new Vector3(point.x,point.y,z));if(i>0)for(int k=0;k<r;k++){int a=(i-1)*r+k,b=(i-1)*r+(k+1)%r,c=i*r+k,d=i*r+(k+1)%r;t.AddRange(new[]{a,c,b,b,c,d});}}
        for(int k=1;k<r-1;k++){t.AddRange(new[]{0,k,k+1});int a=n*r;t.AddRange(new[]{a,a+k+1,a+k});}MeshPart(name,v,t,material,root);
    }
    void Panel(string name,Vector3[] vertices,Material material){MeshPart(name,new List<Vector3>(vertices),new List<int>{0,1,2,0,2,3,2,1,0,3,2,0},material,root);}
    void MeshPart(string name,List<Vector3> v,List<int> t,Material material,Transform parent){var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;}
    Transform Part(string name,Vector3 p,Vector3 s,Material material,Quaternion? r=null,PrimitiveType type=PrimitiveType.Cube,Transform parent=null){var go=GameObject.CreatePrimitive(type);UnityEngine.Object.Destroy(go.GetComponent<Collider>());go.name=name;go.transform.SetParent(parent??root,false);go.transform.localPosition=p;go.transform.localScale=s;go.transform.localRotation=r??Quaternion.identity;go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;}
    void Beam(string name,Vector3 a,Vector3 b,float radius,Material material,Transform parent=null){Part(name,(a+b)*.5f,new Vector3(radius,Vector3.Distance(a,b),radius),material,Quaternion.FromToRotation(Vector3.up,b-a),PrimitiveType.Cube,parent);}
    public void Animate(float speed,float steer,bool brake,bool nitro,float dt){wheelAngle=Mathf.Repeat(wheelAngle+speed*dt/.445f*Mathf.Rad2Deg,360);foreach(var wheel in wheels)wheel.localRotation=Quaternion.Euler(wheelAngle,0,0);foreach(var pivot in steeringKnuckles)pivot.localRotation=Quaternion.Euler(0,steer*22,0);brakeMaterial.SetColor("_EmissionColor",new Color(1,.025f,.008f)*(brake?3:1.35f));glow.gameObject.SetActive(nitro);}
}

public class CinematicEffect:MonoBehaviour
{
    Material material;
    void OnRenderImage(RenderTexture source,RenderTexture destination){if(material==null)material=new Material(Resources.Load<Shader>("Cinematic"));var bloom=RenderTexture.GetTemporary(Mathf.Max(1,source.width/4),Mathf.Max(1,source.height/4),0,RenderTextureFormat.DefaultHDR);Graphics.Blit(source,bloom,material,0);material.SetTexture("_Glow",bloom);Graphics.Blit(source,destination,material,1);RenderTexture.ReleaseTemporary(bloom);}
    void OnDestroy(){if(material!=null)Destroy(material);}
}
public class EngineAudio:MonoBehaviour
{
    public volatile float speed,throttle,gain=.15f,pitch=1;double phase;int sampleRate;
    void Awake(){sampleRate=AudioSettings.outputSampleRate;var source=gameObject.AddComponent<AudioSource>();source.loop=true;source.spatialBlend=0;source.clip=AudioClip.Create("Synthesized V8",sampleRate,1,sampleRate,false);source.Play();}
    void OnAudioFilterRead(float[] data,int channels){double hz=(42+(speed%18)*4.7+speed*.47)*pitch;float volume=gain*(.35f+throttle*.65f);for(int i=0;i<data.Length;i+=channels){phase+=hz/sampleRate;if(phase>1)phase-=1;double a=phase*Math.PI*2;float s=(float)(Math.Sin(a)*.4+Math.Sin(a*2)*.28+Math.Sin(a*4)*.12+Math.Sin(a*6)*.06)*volume;for(int c=0;c<channels;c++)data[i+c]=s;}}
}
}
