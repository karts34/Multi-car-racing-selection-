using System.Collections.Generic;
using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    sealed class HeroTask
    {
        public string Name,Description;public Vector3[] Points;public float Limit;public bool Done;public int Reward;
        public HeroTask(string name,string description,Vector3[] points,int reward,float limit=0){Name=name;Description=description;Points=points;Reward=reward;Limit=limit;}
    }
    readonly List<HeroTask> heroTasks=new List<HeroTask>();
    readonly List<HumanVisual> rescueCitizens=new List<HumanVisual>();
    int activeTask=-1,taskStep,heroScore;
    float taskTime;Transform taskMarker;
    void SetupTasks()
    {
        var roofs=new List<Vector3>();
        foreach(var b in world.Buildings){
            if(b.size.y<17||b.center.x<-450||b.center.x>450||b.center.z<-350||b.center.z>350)continue;
            // Land on the edge of the flat roof, away from rooftop plant rooms.
            var p=new Vector3(b.min.x+3,b.max.y+.025f,b.min.z+3);
            if(roofs.Count==0||Vector3.Distance(p,roofs[roofs.Count-1])>65)roofs.Add(p);
            if(roofs.Count==5)break;
        }
        if(roofs.Count<3)throw new System.Exception("City needs three accessible mission rooftops");
        heroTasks.Add(new HeroTask("ROOFTOP RESCUE","Reach the stranded rooftop residents.\nStand beside each person and press E.",new[]{roofs[0],roofs[1]},250));
        heroTasks.Add(new HeroTask("SKYLINE PATROL","Reach three rooftop beacons in order.\nSwing, web zip and climb to reach the roofs.",new[]{roofs[2],roofs[0],roofs[1]},200));
        heroTasks.Add(new HeroTask("NEIGHBOURHOOD DELIVERY","Deliver supplies to three street markers.\nPress E at each stop within 150 seconds.",new[]{world.At(14,-300)+Vector3.up*.18f,world.At(14,-200)+Vector3.up*.18f,world.At(14,-100)+Vector3.up*.18f},300,150));
        heroScore=smoke?0:PlayerPrefs.GetInt("HeroScore",0);
        for(int i=0;i<heroTasks.Count;i++)heroTasks[i].Done=!smoke&&PlayerPrefs.GetInt("HeroTask_"+i,0)==1;
        foreach(var p in heroTasks[0].Points){var resident=new HumanVisual(world.Root,"Resident awaiting rooftop rescue",false,55+rescueCitizens.Count);resident.Root.position=p;rescueCitizens.Add(resident);}
        taskMarker=new GameObject("Active hero objective beacon").transform;taskMarker.SetParent(world.Root,false);
        var mat=TrackWorld.Mat("Hero objective gold",new Color(1,.65f,.06f),.2f,.5f,1);
        for(int i=0;i<4;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(g.GetComponent<Collider>());g.transform.SetParent(taskMarker,false);g.transform.localPosition=new Vector3(i%2==0?-1.5f:1.5f,1.5f,i<2?-1.5f:1.5f);g.transform.localScale=new Vector3(.075f,3,.075f);g.GetComponent<Renderer>().sharedMaterial=mat;}
        var label=new GameObject("Objective label").AddComponent<TextMesh>();label.transform.SetParent(taskMarker,false);label.transform.localPosition=Vector3.up*3.7f;label.text="HERO OBJECTIVE";label.characterSize=.06f;label.fontSize=40;label.anchor=TextAnchor.MiddleCenter;label.color=Color.yellow;
    }
    void StartHeroTask(int index)
    {
        activeTask=index;taskStep=0;taskTime=0;journal=false;fullMap=false;stage=Stage.Racing;Time.timeScale=1;hasWaypoint=false;
        for(int i=0;i<rescueCitizens.Count;i++)rescueCitizens[i].Root.gameObject.SetActive(true);
        UpdateTaskMarker();NotifyHero(heroTasks[index].Name+" / Follow the gold objective marker.");
    }
    void UpdateTaskMarker(){if(taskMarker==null)return;taskMarker.gameObject.SetActive(activeTask>=0);if(activeTask>=0)taskMarker.position=heroTasks[activeTask].Points[taskStep];}
    void InteractTask()
    {
        if(Driving){NotifyHero("Leave the car to interact with people and deliveries.");return;}
        if(activeTask<0){journal=true;beforePause=Stage.Racing;stage=Stage.Paused;Time.timeScale=0;return;}
        Vector3 target=heroTasks[activeTask].Points[taskStep];
        if(Vector3.Distance(worldPosition,target)>4){NotifyHero("Get closer to the gold marker. Rooftop tasks require reaching the roof.");return;}
        if(activeTask==0)rescueCitizens[taskStep].Root.gameObject.SetActive(false);
        AdvanceHeroTask();
    }
    void AdvanceHeroTask()
    {
        var task=heroTasks[activeTask];taskStep++;
        if(taskStep>=task.Points.Length){
            int finished=activeTask;bool first=!task.Done;task.Done=true;if(first)heroScore+=task.Reward;
            if(!smoke){PlayerPrefs.SetInt("HeroTask_"+finished,1);PlayerPrefs.SetInt("HeroScore",heroScore);PlayerPrefs.Save();}
            Debug.Log("HERO_TASK_COMPLETE "+task.Name+" time="+taskTime.ToString("F2")+" score="+heroScore);
            NotifyHero("TASK COMPLETE / "+task.Name+(first?" / +"+task.Reward+" HERO POINTS":" / Replay complete"));activeTask=-1;
        }else NotifyHero("OBJECTIVE COMPLETE / Next destination marked on your map.");
        UpdateTaskMarker();
    }
    void StepTasks(float dt)
    {
        if(activeTask<0)return;var task=heroTasks[activeTask];taskTime+=dt;
        if(task.Limit>0&&taskTime>=task.Limit){NotifyHero("Delivery time expired. Press J to try again.");activeTask=-1;UpdateTaskMarker();return;}
        if(activeTask==1&&Vector3.Distance(worldPosition,task.Points[taskStep])<3.5f)AdvanceHeroTask();
    }
    void DrawHeroMapImage(Rect rect)
    {
        GUI.color=Color.white;GUI.DrawTexture(rect,world.Map);
        foreach(var task in heroTasks)WorldDot(rect,task.Points[0],task.Done?new Color(.2f,.8f,.5f):Color.cyan,7);
        if(cityCars!=null)foreach(var car in cityCars)WorldDot(rect,car.root.position,Color.white,6);
        if(activeTask>=0)WorldDot(rect,heroTasks[activeTask].Points[taskStep],Color.yellow,12);
        if(hasWaypoint)WorldDot(rect,waypoint,Color.yellow,8);WorldDot(rect,worldPosition,accent,10);
    }
    void DrawHeroHUD()
    {
        Panel(35,30,465,110,ink);Panel(35,30,4,110,accent);Label(57,44,430,30,"WEB SENTINEL / SOLSTICE CITY",20,Color.white);Label(57,86,430,28,world.District(worldPosition)+" / "+heroScore+" HERO POINTS",13,faint);
        Panel(1195,30,370,110,ink);Label(1215,46,330,27,Driving?CarCatalog.All[drivenCar].Name:carriedCar>=0?"SUPER STRENGTH":climbing?"WALL WALK":swinging?"WEB SWING":!grounded?"AIRBORNE":"NEIGHBOURHOOD HERO",19,Color.white);
        if(activeTask>=0||hasWaypoint){
            Vector3 destination=activeTask>=0?heroTasks[activeTask].Points[taskStep]:waypoint,delta=destination-worldPosition;
            float angle=Mathf.DeltaAngle(cameraYaw,Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg);
            Panel(520,30,655,70,ink);Label(535,42,625,47,(angle>25?"RIGHT >":angle<-25?"< LEFT":"AHEAD")+" / "+(activeTask>=0?"OBJECTIVE":"WAYPOINT")+" "+Mathf.RoundToInt(Vector3.Distance(worldPosition,destination))+" M",21,Color.yellow,TextAnchor.MiddleCenter);
        }
        Label(1215,84,325,22,Driving?Mathf.RoundToInt(Mathf.Abs(speed)*3.6f)+" KM/H / "+(speed<-.1f?"REVERSE":"E / G EXIT WHEN STOPPED"):"WEB ENERGY  "+Mathf.RoundToInt(heroEnergy)+"%",12,Color.cyan);Panel(1215,119,325*heroEnergy/100,4,Color.cyan);
        Panel(35,567,260,280,ink);DrawHeroMapImage(new Rect(44,576,242,242));Label(51,825,235,20,"TAB / MAP     J / TASK JOURNAL",11,Color.white);
        if(activeTask>=0){var task=heroTasks[activeTask];Vector3 target=task.Points[taskStep];Panel(1060,571,505,276,ink);Label(1082,590,461,33,task.Name,23,Color.yellow);Label(1082,639,461,28,"OBJECTIVE "+(taskStep+1)+" / "+task.Points.Length+"  /  "+Mathf.RoundToInt(Vector3.Distance(worldPosition,target))+" M",17,Color.white);Label(1082,681,454,69,task.Description,15,faint);Label(1082,773,457,32,task.Limit>0?"TIME LEFT / "+Mathf.CeilToInt(task.Limit-taskTime)+" SECONDS":activeTask==1?"REACH THE GOLD ROOFTOP BEACON":"PRESS E BESIDE THE RESIDENT",16,Color.yellow);}
        if(messageTime>0){Panel(330,722,690,77,ink);Label(346,730,658,62,heroMessage,18,Color.white,TextAnchor.MiddleCenter,true);}
        int nearby=Driving?-1:NearbyCar();
        if(nearby>=0&&carriedCar<0){
            Panel(365,624,645,85,ink);Label(381,631,610,23,CarCatalog.All[nearby].Name+" / H LIFT",16,Color.white,TextAnchor.MiddleCenter);
            if(stage==Stage.Racing&&!fullMap&&!journal&&Button(new Rect(479,663,415,37),"ENTER CAR / E OR G",true))ToggleCityCar();
        }
        if(Driving&&Mathf.Abs(speed)<=2&&stage==Stage.Racing&&!fullMap&&!journal&&Button(new Rect(479,663,415,37),"EXIT CAR / E OR G",true))ToggleCityCar();
        if(!Application.isFocused){Panel(475,205,650,90,ink);Label(490,217,620,68,"CLICK THE GAME WINDOW TO PLAY\nWASD or arrows move / Hold Shift to run",21,Color.white,TextAnchor.MiddleCenter);}
        Label(305,809,737,73,Driving?"WASD / ARROWS Drive / S Brake and reverse / SPACE Handbrake\nE or G Exit when stopped / R Recover car / C Camera / TAB Map":carriedCar>=0?"SUPER STRENGTH / WASD or ARROWS Carry / H Set down / T Throw\nRMB or COMMA / PERIOD Aim throw / TAB Map":"WASD or ARROWS Walk / SHIFT Run / SPACE Jump / Hold LMB Swing\nQ Web zip / Hold F + movement Wall walk / SPACE Leap off wall\nE or G Enter car / H Lift / T Throw / E Task / RMB or , . Look",13,Color.white,TextAnchor.MiddleCenter);
        if(activeTask>=0){var p=cam.WorldToScreenPoint(heroTasks[activeTask].Points[taskStep]+Vector3.up*3);if(p.z>0){float x=(p.x-(Screen.width-1600*scale)*.5f)/scale,y=(Screen.height-p.y-(Screen.height-900*scale)*.5f)/scale;if(x>510&&x<1190&&y>150&&y<560)Label(x-120,y-14,240,28,"OBJECTIVE / "+Mathf.RoundToInt(Vector3.Distance(worldPosition,heroTasks[activeTask].Points[taskStep]))+" M",15,Color.yellow,TextAnchor.MiddleCenter);}}
    }
    void DrawHeroJournal(bool mapView)
    {
        Panel(0,0,1600,900,new Color(.025f,.035f,.055f,.97f));Label(55,40,1110,55,mapView?"SOLSTICE CITY / HERO MAP":"WEB SENTINEL / TASK JOURNAL",34,Color.white);
        Label(58,108,1130,30,"WALK THE STREETS. SWING THE SKYLINE. HELP THE CITY.    /    "+heroScore+" HERO POINTS",14,faint);
        DrawHeroMapImage(new Rect(60,180,640,640));
        if(mapView&&Event.current.type==EventType.MouseDown&&new Rect(60,180,640,640).Contains(Event.current.mousePosition)){var p=Event.current.mousePosition;waypoint=world.MapPosition(new Vector2((p.x-60)/640,(p.y-180)/640));hasWaypoint=true;Event.current.Use();}
        for(int i=0;i<heroTasks.Count;i++){
            var task=heroTasks[i];float y=180+i*200;Panel(755,y,790,184,ink);Label(775,y+13,748,32,(i+1)+" / "+task.Name+(task.Done?" / COMPLETED":""),22,task.Done?Color.green:Color.yellow);Label(775,y+52,747,53,task.Description,16,faint);
            Label(775,y+113,310,27,task.Reward+" POINTS / "+(task.Limit>0?task.Limit+" SECOND LIMIT":"NO TIME LIMIT"),13,Color.white);
            int id=i;if(Button(new Rect(1125,y+121,395,44),activeTask==i?"RESTART TASK":task.Done?"REPLAY TASK":"START TASK",true))StartHeroTask(id);
        }
        if(Button(new Rect(1245,43,300,48),mapView?"CLOSE / TAB":"CLOSE / J",false)){if(mapView)ToggleMap();else{journal=false;stage=beforePause;Time.timeScale=stage==Stage.Paused?0:1;}}
        if(activeTask>=0&&Button(new Rect(755,794,350,44),"CANCEL CURRENT TASK",false)){activeTask=-1;UpdateTaskMarker();}
        if(mapView&&Button(new Rect(1140,794,380,44),"CLEAR WAYPOINT",false))hasWaypoint=false;
        Label(60,856,1480,25,"Gold: objective   /   Cyan: task   /   White: usable car   /   Orange: you   /   Map and journal pause the game",14,faint);
    }
}
}
