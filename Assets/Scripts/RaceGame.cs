using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Afterglow
{
public partial class RaceGame:MonoBehaviour
{
    enum Stage { Menu, Countdown, Racing, Paused, Finished }
    Stage stage=Stage.Menu, beforePause;
    TrackWorld track;CarVisual player;CarVisual[] rivals;
    readonly string[] names={"YOU","M. SILVA","R. KAI","J. MOREAU","A. REYES"};
    float[] aiDistance=new float[4],aiLane=new float[4],aiSpeed=new float[4];
    float distance,lane,speed,heading,steer,nitro=100,elapsed,lapStart,lastLap,bestLap,countdown=3.5f,penaltyFlash;
    float menuClock,frameSmooth=.016f;int lap=1,position=5,cameraMode,quality=2,finishPosition;
    const int TotalLaps=3;
    bool boosting,muted,smoke;Vector3 cameraAnchor;
    Camera cam;EngineAudio engine;CinematicEffect fx;Texture2D white,map;GUIStyle style;Font font;
    Color accent=new Color(1,.31f,.13f),ink=new Color(.035f,.052f,.07f,.92f),faint=new Color(.68f,.73f,.77f);
    float scale,mapScale;Vector2 mapCenter;float bestRace;bool help,fullMap,previewCapture;Stage mapReturn;int previewAngle=-1;
    CarVisual[] garage;int selectedCar,paintIndex,difficulty=1;
    CarSpec Selected=>CarCatalog.All[selectedCar];
    string RecordKey=>"GarageTour_"+Selected.Id+"_"+difficulty;
    string DifficultyName=>difficulty==0?"ROOKIE":difficulty==1?"CLUB":"PRO";

    void Start()
    {
        foreach(string arg in Environment.GetCommandLineArgs()){if(arg=="--smoke-test")smoke=true;if(arg=="--keyboard-test"){keyboardTest=true;smoke=true;}if(arg=="--world-test"||arg=="--hero-test"){worldTest=true;smoke=true;}}
        selectedCar=smoke?0:Mathf.Clamp(PlayerPrefs.GetInt("SelectedCar",0),0,CarCatalog.All.Length-1);
        selectedMap=smoke?0:Mathf.Clamp(PlayerPrefs.GetInt("SelectedMap",0),0,1);
        paintIndex=smoke?0:Mathf.Clamp(PlayerPrefs.GetInt("Paint",0),0,CarCatalog.Paints.Length-1);
        difficulty=smoke?1:Mathf.Clamp(PlayerPrefs.GetInt("Difficulty",1),0,2);
        muted=!smoke&&PlayerPrefs.GetInt("Muted",0)==1;
        Application.targetFrameRate=120;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=4;
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=150;
        QualitySettings.shadowCascades=2;QualitySettings.pixelLightCount=4;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.59f,.67f,.76f);RenderSettings.ambientEquatorColor=new Color(.52f,.51f,.46f);RenderSettings.ambientGroundColor=new Color(.24f,.27f,.27f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00115f;RenderSettings.fogColor=new Color(.49f,.55f,.58f);
        RenderSettings.skybox=new Material(Resources.Load<Shader>("Sunset"));
        var sun=new GameObject("Golden hour sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.85f,.67f);sun.intensity=1.35f;sun.shadows=LightShadows.Soft;sun.shadowBias=.025f;sun.transform.rotation=Quaternion.LookRotation(-new Vector3(-.5f,.32f,.8f));RenderSettings.sun=sun;
        track=new TrackWorld();
        garage=new CarVisual[CarCatalog.All.Length];
        for(int i=0;i<garage.Length;i++){var spec=CarCatalog.All[i];garage[i]=new CarVisual(spec.Name+" / Player",CarCatalog.Paints[paintIndex],spec.Body);garage[i].root.gameObject.SetActive(i==selectedCar);}
        player=garage[selectedCar];
        var colors=new[]{new Color(.13f,.43f,.55f),new Color(.72f,.76f,.70f),new Color(.82f,.60f,.15f),new Color(.24f,.15f,.38f)};
        rivals=new CarVisual[4];for(int i=0;i<4;i++)rivals[i]=new CarVisual(names[i+1],colors[i],CarCatalog.All[i].Body);
        cam=new GameObject("Race camera").AddComponent<Camera>();cam.tag="MainCamera";cam.nearClipPlane=.12f;cam.farClipPlane=1500;cam.fieldOfView=58;cam.allowHDR=true;cam.allowMSAA=true;cam.gameObject.AddComponent<AudioListener>();fx=cam.gameObject.AddComponent<CinematicEffect>();
        engine=gameObject.AddComponent<EngineAudio>();
        var probe=new GameObject("Sky and circuit reflections").AddComponent<ReflectionProbe>();probe.transform.position=track.Position(0)+Vector3.up*4;probe.size=new Vector3(2000,400,2000);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;probe.resolution=128;probe.cullingMask=1;probe.clearFlags=ReflectionProbeClearFlags.Skybox;probe.RenderProbe();
        white=Texture2D.whiteTexture;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        LoadRecords();SetQuality(smoke?2:Mathf.Clamp(PlayerPrefs.GetInt("Quality",2),0,2));
        MakeMap();ResetRace();stage=Stage.Menu;
        Debug.Log("AFTERGLOW_READY GPU="+SystemInfo.graphicsDeviceName+" TRACK="+track.Length.ToString("F1")+"m");
        if(keyboardTest)StartCoroutine(KeyboardTest());else if(worldTest)StartCoroutine(HeroTest());else if(smoke)StartCoroutine(SmokeTest());
    }
    void ResetRace()
    {
        fullMap=false;steer=0;boosting=false;finishPosition=0;
        distance=-26;lane=-2.6f;speed=0;heading=0;nitro=100;elapsed=0;lapStart=0;lastLap=0;lap=1;position=5;countdown=3.5f;penaltyFlash=0;
        for(int i=0;i<4;i++){aiDistance[i]=-8-(i/2)*8;aiLane[i]=(i%2==0?-2.6f:2.6f);aiSpeed[i]=0;Place(rivals[i],aiDistance[i],aiLane[i],0);}
        Place(player,distance,lane,0);cameraAnchor=player.root.position;cam.transform.position=player.root.position-player.root.forward*9+Vector3.up*4;cam.transform.LookAt(player.root.position+Vector3.up);
        Time.timeScale=1;
    }
    void BeginRace(){choosingMap=false;selectedMap=1;LeaveWorld();ResetRace();stage=Stage.Countdown;help=false;SaveSettings();}
    void BackToGarage(){choosingMap=false;LeaveWorld();ResetRace();stage=Stage.Menu;LoadRecords();}
    void LoadRecords(){bestRace=smoke?0:PlayerPrefs.GetFloat(RecordKey+"Race",0);bestLap=smoke?0:PlayerPrefs.GetFloat(RecordKey+"Lap",0);}
    void SaveSettings(){if(smoke)return;PlayerPrefs.SetInt("SelectedMap",selectedMap);PlayerPrefs.SetInt("SelectedCar",selectedCar);PlayerPrefs.SetInt("Paint",paintIndex);PlayerPrefs.SetInt("Difficulty",difficulty);PlayerPrefs.SetInt("Quality",quality);PlayerPrefs.SetInt("Muted",muted?1:0);PlayerPrefs.Save();}
    void SelectCar(int index)
    {
        if(stage!=Stage.Menu||fullMap)return;
        player.root.gameObject.SetActive(false);selectedCar=(index+garage.Length)%garage.Length;player=garage[selectedCar];player.root.gameObject.SetActive(true);player.paint.color=CarCatalog.Paints[paintIndex];
        LoadRecords();ResetRace();SaveSettings();
    }
    void SelectPaint(int index){paintIndex=(index+CarCatalog.Paints.Length)%CarCatalog.Paints.Length;player.paint.color=CarCatalog.Paints[paintIndex];SaveSettings();}
    void CycleDifficulty(){difficulty=(difficulty+1)%3;LoadRecords();SaveSettings();}
    void Update()
    {
        float dt=Mathf.Min(Time.deltaTime,.05f);frameSmooth=Mathf.Lerp(frameSmooth,Time.unscaledDeltaTime,.035f);menuClock+=Time.unscaledDeltaTime;
        if(keyboardTest&&Input.anyKey)lastKeyboardState="focus="+Application.isFocused+" stage="+stage+" Oheld="+Input.GetKey(KeyCode.O)+" Odown="+Input.GetKeyDown(KeyCode.O)+" any="+Input.anyKey;
        if(Input.GetKeyDown(KeyCode.F11)){Screen.fullScreen=!Screen.fullScreen;}
        if(Input.GetKeyDown(KeyCode.M)){muted=!muted;SaveSettings();}
        if(stage==Stage.Menu&&!fullMap&&!choosingMap){
            if(Input.GetKeyDown(KeyCode.O))BeginOpenWorld();
            if(Input.GetKeyDown(KeyCode.LeftArrow))SelectCar(selectedCar-1);
            if(Input.GetKeyDown(KeyCode.RightArrow))SelectCar(selectedCar+1);
            for(int i=0;i<garage.Length;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))SelectCar(i);
            if(Input.GetKeyDown(KeyCode.P))SelectPaint(paintIndex+1);
        }
        else if(stage==Stage.Menu&&choosingMap){if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))SelectMap(1-selectedMap);}
        if(Input.GetKeyDown(KeyCode.C))cameraMode=(cameraMode+1)%3;
        if(inWorld&&!fullMap&&Input.GetKeyDown(KeyCode.J)&&(stage==Stage.Racing||journal)){journal=!journal;beforePause=Stage.Racing;stage=journal?Stage.Paused:Stage.Racing;Time.timeScale=journal?0:1;}
        if(Input.GetKeyDown(KeyCode.Tab)&&!choosingMap&&!journal)ToggleMap();
        if(Input.GetKeyDown(KeyCode.Escape)&&fullMap)ToggleMap();
        else if(Input.GetKeyDown(KeyCode.Escape)) {
            if(journal)journal=false;
            if(choosingMap){choosingMap=false;}
            if(stage==Stage.Racing||stage==Stage.Countdown){beforePause=stage;stage=Stage.Paused;Time.timeScale=0;}
            else if(stage==Stage.Paused){journal=false;stage=beforePause;Time.timeScale=1;}
        }
        if(Input.GetKeyDown(KeyCode.Return)&&!fullMap){if(stage==Stage.Menu){if(choosingMap)BeginSelectedMap();else ShowMapSelection();}else if(stage==Stage.Finished){BeginRace();}else if(stage==Stage.Paused){journal=false;stage=beforePause;Time.timeScale=1;}}
        if(Input.GetKeyDown(KeyCode.R)&&(stage==Stage.Racing||stage==Stage.Countdown))Recover();
        if(stage==Stage.Countdown){countdown-=dt;if(countdown<=0)stage=Stage.Racing;}
        if(stage==Stage.Racing){if(inWorld){if(!worldTestingSteps)HeroInput(dt);}else Simulate(dt,smoke);}
        if(stage==Stage.Menu) {Place(player,-26,-2.6f,0);}
        boosting=boosting&&stage==Stage.Racing;
        player.Animate(speed,steer,Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow),boosting,dt);
        var soundCar=inWorld&&Driving?CarCatalog.All[drivenCar]:Selected;engine.speed=speed;engine.pitch=soundCar.Body==CarBody.Luxury?.78f:soundCar.Body==CarBody.Muscle?.68f:soundCar.Body==CarBody.Hyper?1.35f:1;engine.throttle=boosting?1:Mathf.Clamp01(speed/50);engine.gain=muted||(inWorld&&!Driving)||stage==Stage.Paused?0:(stage==Stage.Racing?.19f:.045f);
        penaltyFlash=Mathf.Max(0,penaltyFlash-dt);
    }
    void Recover(){if(inWorld){RecoverHero();return;}lane=0;heading=0;speed=Mathf.Min(speed,15);penaltyFlash=.6f;}
    void Simulate(float dt,bool auto)
    {
        elapsed+=dt;
        float input=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
        bool throttle=Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow),brake=Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow),drift=Input.GetKey(KeyCode.Space);
        if(auto){throttle=true;input=Mathf.Clamp(-lane*.18f-heading*3.5f,-1,1);}
        steer=Mathf.MoveTowards(steer,input,dt*4);
        boosting=(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)||auto)&&throttle&&nitro>1&&speed>12;
        if(boosting)nitro=Mathf.Max(0,nitro-dt*21);else nitro=Mathf.Min(100,nitro+dt*8);
        float acceleration=throttle?Selected.Acceleration+(boosting?10:0): -3.6f;
        if(brake)acceleration-=Selected.Braking;
        acceleration-=speed*speed*.0017f;if(drift)acceleration-=6;
        speed=Mathf.Clamp(speed+acceleration*dt,0,boosting?Selected.BoostSpeed:Selected.TopSpeed);
        float curve=Mathf.DeltaAngle(track.Rotation(distance).eulerAngles.y,track.Rotation(distance+speed*dt).eulerAngles.y)*Mathf.Deg2Rad;
        // Steering assistance follows most of the road curvature while preserving player control.
        heading+=steer*Selected.Handling*Mathf.Lerp(.95f,.48f,speed/95)*dt*(drift?1.65f:1)-curve*.24f;
        heading=Mathf.Lerp(heading,0,dt*(drift?.5f:Selected.Grip));heading=Mathf.Clamp(heading,-.9f,.9f);
        lane+=Mathf.Sin(heading)*speed*dt;
        if(Mathf.Abs(lane)>6.45f){lane=Mathf.Clamp(lane,-6.45f,6.45f);speed*=.87f;heading*=-.35f;penaltyFlash=.35f;}
        distance+=Mathf.Cos(heading)*speed*dt;
        int nextLap=Mathf.Max(1,Mathf.FloorToInt(distance/track.Length)+1);
        if(nextLap>lap){lastLap=elapsed-lapStart;lapStart=elapsed;if(bestLap<=0||lastLap<bestLap){bestLap=lastLap;if(!smoke)PlayerPrefs.SetFloat(RecordKey+"Lap",bestLap);}lap=nextLap;}
        for(int i=0;i<4;i++){
            float bend=Mathf.Abs(Mathf.DeltaAngle(track.Rotation(aiDistance[i]).eulerAngles.y,track.Rotation(aiDistance[i]+35).eulerAngles.y));
            float target=52+difficulty*5+i*1.5f-Mathf.Clamp(bend*.30f,0,14);
            aiSpeed[i]=Mathf.MoveTowards(aiSpeed[i],target,dt*(10+i));
            aiDistance[i]+=aiSpeed[i]*dt;
            float desired=(i%2==0?-2.8f:2.8f)+Mathf.Sin(aiDistance[i]*.009f+i)*.65f;
            aiLane[i]=Mathf.Lerp(aiLane[i],desired,dt);
            float gap=Mathf.DeltaAngle(Mathf.Repeat(distance,track.Length)/track.Length*360,Mathf.Repeat(aiDistance[i],track.Length)/track.Length*360)/360*track.Length;
            float contactLength=(Selected.Body==CarBody.Luxury||i==0)?5.15f:4.5f;
            if(Mathf.Abs(gap)<contactLength&&Mathf.Abs(lane-aiLane[i])<2.15f){float side=lane>=aiLane[i]?1:-1;lane=Mathf.Clamp(lane+side*dt*5,-6.45f,6.45f);speed=Mathf.MoveTowards(speed,aiSpeed[i]*.87f,dt*35);penaltyFlash=.18f;}
            Place(rivals[i],aiDistance[i],aiLane[i],0);rivals[i].Animate(aiSpeed[i],0,false,false,dt);
        }
        position=1;foreach(float d in aiDistance)if(d>distance)position++;
        Place(player,distance,lane,heading);
        if(distance>=track.Length*TotalLaps)Finish();
    }
    void Finish()
    {
        finishPosition=position;stage=Stage.Finished;boosting=false;
        if(bestRace<=0||elapsed<bestRace){bestRace=elapsed;if(!smoke)PlayerPrefs.SetFloat(RecordKey+"Race",bestRace);}if(!smoke)PlayerPrefs.Save();
        Debug.Log("AFTERGLOW_FINISH position="+position+" time="+elapsed.ToString("F2"));
    }
    void Place(CarVisual car,float d,float offset,float yaw){var q=track.Rotation(d);car.root.SetPositionAndRotation(track.Position(d)+q*new Vector3(offset,.10f,0),q*Quaternion.Euler(0,yaw*Mathf.Rad2Deg,0));}
    void LateUpdate()
    {
        if(track==null)return;
        if(inWorld){HeroCamera();return;}
        Vector3 target,look;float fov;
        if(previewAngle>=0){var r=player.root.rotation;target=player.root.position+r*(previewAngle==0?new Vector3(5.7f,2.05f,6.7f):new Vector3(6.5f,1.65f,-4.7f));look=player.root.position+Vector3.up*.7f;fov=43;}
        else if(stage==Stage.Menu){float orbit=Mathf.Sin(menuClock*.1f)*.3f;var r=player.root.rotation;target=player.root.position+r*new Vector3(6.8f+orbit,2.8f,8.0f);look=player.root.position+r*new Vector3(1.65f,.72f,-.75f);fov=49;}
        else if(cameraMode==2){target=player.root.position+player.root.forward*(Selected.Body==CarBody.Luxury?1.08f:1.22f)+Vector3.up*(Selected.Body==CarBody.Luxury?1.73f:1.40f);look=target+player.root.forward*30;fov=78;}
        else {var r=track.Rotation(distance);float behind=cameraMode==0?8.2f:14;target=player.root.position-r*Vector3.forward*(behind+speed*.009f)+Vector3.up*(cameraMode==0?3.35f:6);look=player.root.position+r*Vector3.forward*(3+speed*.035f)+Vector3.up*.9f;fov=58+speed*.065f+(boosting?4:0);}
        float dt=Time.unscaledDeltaTime;
        if(stage!=Stage.Menu)cam.transform.position+=player.root.position-cameraAnchor;
        cameraAnchor=player.root.position;
        cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-dt*(cameraMode==2?30:6)));
        cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,Quaternion.LookRotation(look-cam.transform.position),1-Mathf.Exp(-dt*8));cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,fov,dt*3);
    }
    void MakeMap()
    {
        map=new Texture2D(512,400,TextureFormat.RGBA32,false);var pixels=new Color[512*400];mapCenter=new Vector2(track.MapBounds.center.x,track.MapBounds.center.z);mapScale=Mathf.Min(218/track.MapBounds.size.x,166/track.MapBounds.size.z);
        for(int y=0;y<400;y++)for(int x=0;x<512;x++){float xx=(x*.5f-128)/mapScale+mapCenter.x,zz=(y*.5f-100)/mapScale+mapCenter.y;float n=Mathf.PerlinNoise(xx*.006f+15,zz*.006f+15);Color c=Color.Lerp(new Color(.095f,.15f,.125f),new Color(.20f,.21f,.15f),n);if(xx>280)c=Color.Lerp(c,new Color(.31f,.21f,.14f),.65f);if(track.LakeMask(xx,zz)<1)c=new Color(.065f,.28f,.37f);float contour=Mathf.Repeat(n*24,1);if(contour<.065f)c*=.73f;pixels[y*512+x]=c;}
        for(int i=0;i<TrackWorld.Samples;i++){var v=MapPoint(track.points[i])*2;for(int x=-3;x<=3;x++)for(int y=-3;y<=3;y++){int px=Mathf.RoundToInt(v.x)+x,py=Mathf.RoundToInt(v.y)+y;if(px>=0&&px<512&&py>=0&&py<400)pixels[py*512+px]=i<45?accent:new Color(.84f,.86f,.82f);}}
        map.SetPixels(pixels);map.Apply();
    }
    Vector2 MapPoint(Vector3 p)=>new Vector2(128+(p.x-mapCenter.x)*mapScale,100+(p.z-mapCenter.y)*mapScale);
    void ToggleMap(){if(fullMap){fullMap=false;stage=mapReturn;Time.timeScale=stage==Stage.Paused?0:1;}else{mapReturn=stage;fullMap=true;stage=Stage.Paused;Time.timeScale=0;}}
    void SetQuality(int q)
    {
        quality=q;QualitySettings.antiAliasing=q==0?0:q==1?2:4;QualitySettings.shadowDistance=q==0?55:q==1?95:150;QualitySettings.shadowResolution=q==2?ShadowResolution.High:ShadowResolution.Medium;fx.enabled=q>0;
        SaveSettings();
    }
    void OnGUI()
    {
        if(track==null||white==null)return;scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        if(style==null)style=new GUIStyle(GUI.skin.label){font=font};
        if(stage==Stage.Menu){if(choosingMap)DrawMapSelection();else DrawMenu();}else if(inWorld)DrawHeroHUD();else DrawHUD();
        if(stage==Stage.Countdown){Panel(699,320,202,174,new Color(.03f,.045f,.06f,.82f));Label(699,326,202,125,countdown>.5f?Mathf.CeilToInt(countdown-.5f).ToString():"GO",84,Color.white,TextAnchor.MiddleCenter);Label(510,520,580,35,"GET READY  /  SOLSTICE CIRCUIT",16,Color.white,TextAnchor.MiddleCenter);}
        if(journal)DrawHeroJournal(false);
        else if(stage==Stage.Paused&&!fullMap&&!previewCapture)DrawPause();
        if(stage==Stage.Finished)DrawFinish();
        if(fullMap){if(inWorld)DrawHeroJournal(true);else DrawMap();}
        if(penaltyFlash>0){Panel(0,0,1600,7,new Color(1,.22f,.1f,penaltyFlash*2));Label(600,720,400,30,"CONTACT  /  HOLD YOUR LINE",15,accent,TextAnchor.MiddleCenter);}
        GUI.matrix=Matrix4x4.identity;
    }
    void DrawMenu()
    {
        Panel(0,0,510,900,new Color(.022f,.038f,.052f,.95f));Panel(0,0,1600,4,accent);
        Label(44,35,430,26,"A F T E R G L O W   /   M O T O R S P O R T",13,faint);
        Label(38,73,420,104,"APEX",86,Color.white);
        Label(44,182,410,28,"YOUR CAR. YOUR RACING LINE.",20,accent);
        Panel(44,232,420,1,new Color(1,1,1,.16f));
        Label(44,253,420,24,"GARAGE  /  "+(selectedCar+1).ToString("00")+" OF 04",12,accent);
        Label(42,285,440,44,Selected.Name,32,Color.white);
        Label(44,336,430,23,Selected.Class+"  /  "+Selected.Power+" HP",13,faint);
        Label(44,379,440,65,Selected.Description,15,new Color(.84f,.86f,.87f));
        GarageStat(456,"TOP SPEED",Mathf.RoundToInt(Selected.TopSpeed*3.6f)+" KM/H",Selected.TopSpeed/100);
        GarageStat(490,"ACCELERATION",Selected.Acceleration>=18?"EXTREME":Selected.Acceleration>=16?"STRONG":"REFINED",Selected.Acceleration/22);
        GarageStat(524,"HANDLING",Selected.Handling>=1.1f?"RAZOR SHARP":Selected.Handling>=1?"BALANCED":"PROGRESSIVE",Selected.Handling/1.4f);
        Label(44,572,420,24,"PAINT  /  "+CarCatalog.PaintNames[paintIndex],12,faint);
        for(int i=0;i<CarCatalog.Paints.Length;i++){
            float x=44+i*70;Panel(x,607,56,36,i==paintIndex?accent:new Color(1,1,1,.2f));Panel(x+3,610,50,30,CarCatalog.Paints[i]);
            if(GUI.Button(new Rect(x,607,56,36),GUIContent.none,GUIStyle.none))SelectPaint(i);
        }
        if(Button(new Rect(44,673,420,58),"CHOOSE MAP & PLAY     >",true))ShowMapSelection();
        if(Button(new Rect(44,746,203,43),"RIVALS / "+DifficultyName,false))CycleDifficulty();
        if(Button(new Rect(261,746,203,43),"GRAPHICS / "+QualityName(),false))SetQuality((quality+1)%3);
        if(Button(new Rect(44,802,203,40),"CONTROLS",false))help=!help;
        if(Button(new Rect(261,802,203,40),"QUIT GAME",false))Application.Quit();
        Label(44,862,425,22,"ENTER  Choose map    /    ARROWS  Car    /    P  Paint",12,faint);
        Panel(550,34,1006,80,new Color(.025f,.04f,.05f,.78f));
        Label(576,48,590,28,"TWO MAPS. TWO WAYS TO PLAY.",22,Color.white);
        Label(577,83,625,20,"SOLSTICE CITY    /    GRAND TOUR CIRCUIT    /    "+DifficultyName,12,faint);
        if(Button(new Rect(1306,53,228,42),"CHOOSE MAP",false))ShowMapSelection();
        if(Button(new Rect(550,600,490,49),"CHOOSE YOUR DESTINATION     >",true))ShowMapSelection();
        Label(1070,606,475,45,"Play the city as a web-powered hero.\nRace the circuit in your selected car.",13,Color.white);
        Label(576,665,870,25,"CHOOSE YOUR MACHINE     /     1 - 4 OR LEFT / RIGHT",13,accent);
        for(int i=0;i<CarCatalog.All.Length;i++){
            var spec=CarCatalog.All[i];float x=550+i*254;
            Panel(x,709,244,132,i==selectedCar?new Color(.14f,.22f,.26f,.98f):ink);Panel(x,709,244,3,i==selectedCar?accent:new Color(.4f,.45f,.47f,.6f));
            Label(x+18,726,210,22,"0"+(i+1)+"  /  "+(i==selectedCar?"SELECTED":"AVAILABLE"),11,i==selectedCar?accent:faint);
            Label(x+18,761,218,29,spec.Name,19,Color.white);Label(x+18,804,218,20,spec.Class,10,faint);
            if(GUI.Button(new Rect(x,709,244,132),GUIContent.none,GUIStyle.none))SelectCar(i);
        }
        Label(576,862,970,22,"F11  Fullscreen     /     M  Audio     /     Original cars, built for the last light",12,faint);
        if(help)Controls(655,280);
    }
    void GarageStat(float y,string label,string value,float amount){Label(44,y,180,22,label,11,faint);Label(225,y,239,22,value,12,Color.white,TextAnchor.UpperRight);Panel(44,y+25,420,3,new Color(1,1,1,.12f));Panel(44,y+25,420*Mathf.Clamp01(amount),3,accent);}
    string QualityName()=>quality==0?"LOW":quality==1?"MEDIUM":"HIGH";
    void DrawHUD()
    {
        Panel(40,34,396,103,ink);Panel(40,34,4,103,accent);
        Label(63,46,130,23,"POSITION",12,faint);Label(63,69,142,59,position.ToString("00"),43,Color.white);Label(130,87,66,28,"/ 05",19,faint);
        Label(237,46,160,23,"LAP",12,faint);Label(237,73,156,49,Mathf.Min(lap,3)+" / 3",34,Color.white);
        Label(563,39,470,32,"A P E X   /   A F T E R G L O W",20,Color.white,TextAnchor.MiddleCenter);
        Label(570,78,460,24,track.Sector(distance),12,new Color(.86f,.85f,.79f),TextAnchor.MiddleCenter);
        Panel(1262,34,298,130,ink);Label(1285,47,110,23,"RACE TIME",12,faint);Label(1285,77,257,42,Clock(elapsed),32,Color.white,TextAnchor.MiddleRight);Label(1285,127,258,21,"BEST LAP   "+(bestLap>0?Clock(bestLap):"--:--.---"),12,faint,TextAnchor.MiddleRight);
        Panel(40,162,237,192,new Color(.035f,.052f,.07f,.75f));Label(58,171,202,23,"RACE ORDER",11,faint);
        int[] order={0,1,2,3,4};Array.Sort(order,(a,b)=>Progress(b).CompareTo(Progress(a)));
        for(int i=0;i<5;i++){bool you=order[i]==0;Label(58,207+i*27,201,24,(i+1).ToString("00")+"   "+names[order[i]],14,you?accent:new Color(.86f,.88f,.9f));}
        Panel(40,643,272,214,new Color(.025f,.04f,.05f,.79f));GUI.color=Color.white;GUI.DrawTexture(new Rect(48,650,256,200),map);
        for(int i=0;i<4;i++)MapDot(aiDistance[i],new Color(.75f,.78f,.79f),5);
        MapDot(distance,accent,9);Label(59,827,210,23,"SOLSTICE  /  "+(track.Length/1000).ToString("F2")+" KM",11,faint);
        Panel(1230,640,330,217,ink);Label(1254,652,280,24,boosting?"NITRO ACTIVE":Selected.Name,12,boosting?new Color(.25f,.8f,1):faint);
        Label(1251,678,222,97,Mathf.RoundToInt(speed*3.6f).ToString("000"),76,Color.white,TextAnchor.MiddleRight);Label(1481,731,65,27,"KM/H",13,faint);
        Label(1255,757,146,26,"GEAR  "+Mathf.Clamp(1+(int)(speed/14),1,6),14,Color.white);
        for(int i=0;i<26;i++)Panel(1256+i*10.3f,794,7,6,i<speed/Selected.BoostSpeed*26?accent:new Color(1,1,1,.15f));
        Panel(1256,817,267,5,new Color(1,1,1,.14f));Panel(1256,817,267*nitro/100,5,new Color(.18f,.71f,.95f));
        Label(1256,830,267,18,"SHIFT  /  NITRO                         "+Mathf.RoundToInt(nitro)+"%",10,faint);
        Label(394,840,814,28,"W A S D  Drive     SPACE  Drift     SHIFT  Nitro     C  Camera     R  Recover     ESC  Pause",13,new Color(.89f,.9f,.91f),TextAnchor.MiddleCenter);
        Label(1401,869,154,18,QualityName()+"  /  "+Mathf.RoundToInt(1/Mathf.Max(.001f,frameSmooth))+" FPS",10,new Color(.78f,.80f,.82f),TextAnchor.MiddleRight);
    }
    float Progress(int id)=>id==0?distance:aiDistance[id-1];
    void MapDot(float d,Color color,float size){var p=MapPoint(track.Position(d));Panel(48+p.x-size/2,650+200-p.y-size/2,size,size,color);}
    void DrawMap()
    {
        Panel(0,0,1600,900,new Color(.025f,.043f,.056f,.98f));Label(65,45,1100,54,"SOLSTICE  /  GRAND TOUR",35,Color.white);Label(68,108,1100,28,(track.Length/1000).ToString("F2")+" KM    /    3 LAPS    /    FIVE DISTINCT SECTORS",15,faint);
        GUI.color=Color.white;GUI.DrawTexture(new Rect(65,168,880,687.5f),map);
        var at=MapPoint(track.Position(distance));Panel(65+at.x*3.4375f-6,168+(200-at.y)*3.4375f-6,12,12,accent);
        string[] descriptions={"Pit garages, hospitality and covered grandstands","Cliffside bends through layered red sandstone","Rolling hills and dense evergreen woodland","Elevated steel bridge above the turquoise lake","Mountain views on the climb back to the start"};
        for(int i=0;i<5;i++){Label(1002,210+i*109,540,27,"0"+(i+1)+"   "+track.SectorNames[i],18,i==0?accent:Color.white);Label(1002,248+i*109,530,44,descriptions[i],13,faint);Panel(1002,291+i*109,523,1,new Color(1,1,1,.10f));}
        if(Button(new Rect(1255,48,277,48),"CLOSE  /  TAB",false))ToggleMap();
        Label(1002,806,530,38,"Race pauses while the circuit map is open.",14,faint);
    }
    void DrawPause()
    {
        Panel(0,0,1600,900,new Color(.01f,.02f,.035f,.80f));Panel(533,160,534,579,ink);
        Label(579,195,440,38,"TAKE A BREATHER",29,Color.white);Label(579,250,440,28,inWorld?"EXPLORATION PAUSED":"YOUR RACE IS PAUSED",13,faint);
        if(Button(new Rect(579,307,442,57),inWorld?"RESUME HERO ADVENTURE":"RESUME RACE",true)){stage=beforePause;Time.timeScale=1;}
        if(Button(new Rect(579,380,442,49),inWorld?"RETURN TO CITY SPAWN":"RESTART RACE",false)){if(inWorld)BeginOpenWorld();else BeginRace();}
        if(Button(new Rect(579,445,442,49),"GRAPHICS  /  "+QualityName(),false))SetQuality((quality+1)%3);
        if(Button(new Rect(579,510,442,49),muted?"AUDIO  /  OFF":"AUDIO  /  ON",false)){muted=!muted;SaveSettings();}
        if(Button(new Rect(579,575,442,49),"BACK TO CAR SELECTION",false))BackToGarage();
        if(Button(new Rect(579,640,442,49),"QUIT TO DESKTOP",false))Application.Quit();
    }
    void DrawFinish()
    {
        Panel(0,0,1600,900,new Color(.01f,.02f,.035f,.75f));Panel(486,156,628,584,ink);Panel(486,156,628,4,accent);
        Label(531,194,538,32,"S O L S T I C E   /   R A C E   C O M P L E T E",16,accent,TextAnchor.MiddleCenter);
        Label(531,250,538,87,finishPosition==1?"VICTORY":"FINISH  /  P"+finishPosition,62,Color.white,TextAnchor.MiddleCenter);
        Label(548,369,500,33,"RACE TIME",14,faint);Label(548,399,500,58,Clock(elapsed),41,Color.white);
        Label(548,483,500,32,"PERSONAL BEST     "+Clock(bestRace),17,faint);
        if(Button(new Rect(548,556,504,60),"RACE AGAIN     >",true))BeginRace();
        if(Button(new Rect(548,636,504,46),"BACK TO GARAGE",false))BackToGarage();
    }
    void Controls(float x,float y)
    {
        Panel(x,y,630,335,ink);Label(x+30,y+23,560,38,"MAKE EVERY CORNER COUNT",24,Color.white);
        string[] rows={"W / UP                 Accelerate","S / DOWN           Brake","A D / ARROWS    Steer","SPACE                   Drift / handbrake","SHIFT                     Nitro boost (recharges)","C   Camera     R   Recover     ESC   Pause","F11   Fullscreen     M   Mute"};
        for(int i=0;i<rows.Length;i++)Label(x+30,y+85+i*31,565,26,rows[i],16,i<5?Color.white:faint);
    }
    void Panel(float x,float y,float w,float h,Color c){GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),white);GUI.color=Color.white;}
    void Label(float x,float y,float w,float h,string text,int size,Color color,TextAnchor align=TextAnchor.UpperLeft,bool wrap=false)
    {
        style.fontSize=size;style.fontStyle=FontStyle.Normal;style.alignment=align;style.normal.textColor=color;style.wordWrap=wrap;GUI.Label(new Rect(x,y,w,h),text,style);
    }
    bool Button(Rect rect,string text,bool primary)
    {
        bool hover=rect.Contains(Event.current.mousePosition);Panel(rect.x,rect.y,rect.width,rect.height,primary?(hover?new Color(1,.43f,.24f):accent):hover?new Color(.19f,.24f,.28f,.97f):new Color(.13f,.17f,.20f,.95f));
        Label(rect.x+18,rect.y,rect.width-36,rect.height,text,primary?18:14,Color.white,TextAnchor.MiddleLeft);
        return GUI.Button(rect,GUIContent.none,GUIStyle.none);
    }
    static string Clock(float t){int m=(int)t/60;return m.ToString("00")+":"+(t%60).ToString("00.000");}

    IEnumerator SmokeTest()
    {
        string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Logs"));Directory.CreateDirectory(dir);
        yield return new WaitForSecondsRealtime(3);
        for(int car=0;car<garage.Length;car++){
            SelectCar(car);SelectPaint(car);
            int active=0;foreach(var item in garage)if(item.root.gameObject.activeSelf)active++;
            if(active!=1||player!=garage[car]||player.wheels.Count!=4)throw new Exception("Garage selection or wheels failed");
            if(player.paint.color!=CarCatalog.Paints[car])throw new Exception("Paint selection failed");
            yield return new WaitForSecondsRealtime(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"garage-"+Selected.Id+".png"));
            yield return new WaitForSecondsRealtime(.25f);
        }
        SelectCar(0);SelectPaint(0);yield return new WaitForSecondsRealtime(2);
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"01-garage.png"));
        yield return new WaitForSecondsRealtime(1);BeginRace();
        SelectCar(2);if(selectedCar!=0)throw new Exception("Car selection allowed during countdown");
        yield return new WaitForSecondsRealtime(12);
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"02-racing.png"));
        Debug.Log("SMOKE_DRIVING speed="+speed+" distance="+distance+" position="+position+" nitro="+nitro);
        yield return new WaitForSecondsRealtime(1);
        if(speed<10||distance<30)throw new Exception("Smoke test: car did not accelerate or progress");
        if(nitro>=99||Array.Exists(aiDistance,d=>d<=0))throw new Exception("Smoke test: nitro or AI did not run");
        float saved=distance;beforePause=stage;stage=Stage.Paused;Time.timeScale=0;
        yield return new WaitForSecondsRealtime(.3f);
        if(distance!=saved)throw new Exception("Smoke test: pause changed race distance");
        ToggleMap();ToggleMap();if(stage!=Stage.Paused||Time.timeScale!=0)throw new Exception("Map failed to preserve pause");
        stage=beforePause;Time.timeScale=1;lane=8;speed=40;Simulate(.02f,true);
        if(Mathf.Abs(lane)>6.5f||speed>=40)throw new Exception("Smoke test: barrier contact did not constrain car");
        Recover();if(lane!=0||heading!=0)throw new Exception("Smoke test: recovery failed");
        // Exercise the finish transition by crossing the actual finish line in the simulation.
        distance=track.Length*3-.2f;lap=3;speed=60;elapsed=220;Simulate(.02f,true);
        if(stage!=Stage.Finished||lap!=4)throw new Exception("Smoke test: race finish failed");
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"03-finish.png"));
        yield return new WaitForSecondsRealtime(1);
        BeginRace();if(lap!=1||elapsed!=0||nitro!=100||speed!=0)throw new Exception("Smoke test: restart failed");
        SetQuality(0);SetQuality(1);SetQuality(2);
        previewCapture=true;stage=Stage.Paused;
        float[] views={.29f,.52f,.74f,.90f};
        for(int i=0;i<views.Length;i++){
            distance=track.Length*views[i];lane=0;speed=0;heading=0;Place(player,distance,lane,0);cam.transform.position=player.root.position-player.root.forward*9+Vector3.up*4;cameraAnchor=player.root.position;
            yield return new WaitForSecondsRealtime(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"sector-"+(i+1)+".png"));
            yield return new WaitForSecondsRealtime(.25f);
        }
        distance=-26;Place(player,distance,-2.6f,0);cameraAnchor=player.root.position;
        for(int i=0;i<2;i++){previewAngle=i;yield return new WaitForSecondsRealtime(2);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"car-detail-"+i+".png"));yield return new WaitForSecondsRealtime(.25f);}
        previewAngle=-1;previewCapture=false;ToggleMap();yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"full-map.png"));yield return new WaitForSecondsRealtime(.3f);ToggleMap();
        // Drive the entire redesigned route, rather than only teleporting to a finish state.
        BeginRace();stage=Stage.Racing;int steps=0;
        while(stage==Stage.Racing&&steps<18000){for(int j=0;j<200&&stage==Stage.Racing;j++){Simulate(.02f,true);steps++;}yield return null;}
        if(stage!=Stage.Finished||lap!=4)throw new Exception("Full route test did not finish all three laps");
        Debug.Log("GRAND_TOUR_FULL_RACE_PASS steps="+steps+" simulatedSeconds="+elapsed+" distance="+distance);
        var raceResults=new System.Text.StringBuilder();
        for(int car=0;car<garage.Length;car++)for(int level=0;level<3;level++){
            stage=Stage.Menu;SelectCar(car);difficulty=level;LoadRecords();BeginRace();stage=Stage.Racing;steps=0;
            while(stage==Stage.Racing&&steps<18000){for(int j=0;j<200&&stage==Stage.Racing;j++){Simulate(.02f,true);steps++;}yield return null;}
            if(stage!=Stage.Finished||lap!=4||float.IsNaN(distance)||nitro<0||nitro>100)throw new Exception("Full race failed: "+Selected.Id+" / "+DifficultyName);
            raceResults.AppendLine(Selected.Name+" / "+DifficultyName+": "+elapsed.ToString("F2")+" seconds, P"+finishPosition);
        }
        File.WriteAllText(Path.Combine(dir,"garage-race-tests.txt"),raceResults.ToString());
        Debug.Log("GARAGE_ALL_12_RACES_PASS");
        for(int i=0;i<TrackWorld.Samples;i++){var p=MapPoint(track.points[i]);if(float.IsNaN(p.x)||float.IsNaN(p.y)||p.x<10||p.x>246||p.y<10||p.y>190)throw new Exception("Circuit does not fit minimap");if(float.IsNaN(track.points[i].y))throw new Exception("Invalid track elevation");}
        File.WriteAllText(Path.Combine(dir,"smoke-test.txt"),"PASS: four selectable cars, paint, selection lock, startup, acceleration, AI, nitro, pause/map, barriers, recovery, finish, restart, graphics presets, 12 complete three-lap races (all cars/difficulties), minimap bounds.\nGPU: "+SystemInfo.graphicsDeviceName+"\nCircuit: "+track.Length.ToString("F1")+" m\nSee garage-race-tests.txt for all race results.\nScreenshots: all four garage cars, racing, finish, four sectors, luxury front/rear, full map.\n");
        Debug.Log("AFTERGLOW_SMOKE_PASS");Application.Quit();
    }
}
}
