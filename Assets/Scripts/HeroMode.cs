using System.Collections.Generic;
using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    HumanVisual hero;
    readonly List<HumanVisual> citizens=new List<HumanVisual>();
    readonly List<Vector3> walkA=new List<Vector3>(),walkB=new List<Vector3>();
    readonly List<float> walkPhase=new List<float>();
    Vector3 heroVelocity,webAnchor,wallNormal=Vector3.forward;
    float heroYaw,cameraYaw,heroClock,ropeLength,heroEnergy=100,messageTime,wallVisualInset;
    bool grounded,swinging,climbing,journal;
    string heroMessage="Welcome, Web Sentinel. Press J to find people who need your help.";
    LineRenderer web;
    void SetupHero()
    {
        if(hero==null){
            hero=new HumanVisual(world.Root,"WEB SENTINEL / Human player",true);
            for(int i=0;i<40;i++){
                float x=(i%5-2)*200+(i%2==0?14:-14),z=-365+(i/5)*95;
                var a=world.At(x,z)+Vector3.up*.18f;var b=world.At(x,z+70)+Vector3.up*.18f;
                if(world.HeroBlocked(a)||world.HeroBlocked(b))continue;
                var human=new HumanVisual(world.Root,"Walking citizen "+i,false,i);citizens.Add(human);walkA.Add(a);walkB.Add(b);walkPhase.Add(i*.71f);
            }
            var g=new GameObject("Web tether");g.transform.SetParent(world.Root,false);web=g.AddComponent<LineRenderer>();web.positionCount=2;web.startWidth=.035f;web.endWidth=.016f;web.material=new Material(Shader.Find("Sprites/Default"));web.startColor=web.endColor=Color.white;
            SetupTasks();
        }
        foreach(var car in garage)car.root.gameObject.SetActive(false);foreach(var car in rivals)car.root.gameObject.SetActive(false);
        worldPosition=world.At(0,-345)+Vector3.up*.18f;heroVelocity=Vector3.zero;heroYaw=0;cameraYaw=0;grounded=true;swinging=false;climbing=false;journal=false;heroEnergy=100;web.enabled=false;activeTask=-1;messageTime=8;
        SetupCityCars();PlaceHero();UpdateTaskMarker();ResetHeroCamera();NotifyHero("Explore on foot or by car. G enter / H lift / T throw / J hero tasks.");
    }
    void PlaceHero(){hero.Root.SetPositionAndRotation(climbing?worldPosition-wallNormal*wallVisualInset:worldPosition,climbing?Quaternion.LookRotation(-wallNormal,Vector3.up):Quaternion.Euler(0,heroYaw,0));hero.Animate(climbing?heroVelocity.magnitude:new Vector2(heroVelocity.x,heroVelocity.z).magnitude,heroClock,!grounded&&!climbing,swinging,climbing);}
    void RecoverHero(){if(Driving){RecoverCityCar();return;}if(carriedCar>=0&&!ReleaseCityCar(false))return;worldPosition=world.NearestRoad(worldPosition,out _,out _)+Vector3.up*.18f;heroVelocity=Vector3.zero;grounded=true;swinging=false;climbing=false;web.enabled=false;PlaceHero();ResetHeroCamera();}
    void HeroInput(float dt)
    {
        if(Input.GetKeyDown(KeyCode.G)||Driving&&Input.GetKeyDown(KeyCode.E)){ToggleCityCar();return;}
        if(Driving){StepCityCar(dt,(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow),Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow),Input.GetKey(KeyCode.Space));return;}
        if(Input.GetKeyDown(KeyCode.H))LiftCityCar();
        if(Input.GetKeyDown(KeyCode.T))ReleaseCityCar(true);
        if(Input.GetMouseButton(1))cameraYaw+=Input.GetAxis("Mouse X")*3;
        cameraYaw+=((Input.GetKey(KeyCode.Period)?1:0)-(Input.GetKey(KeyCode.Comma)?1:0))*80*dt;
        Vector2 move=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
        if(keyboardTest){lastHeroInput=move;if(move.sqrMagnitude>0)keyboardFrames++;}
        StepHero(dt,move,Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),Input.GetKeyDown(KeyCode.Space),Input.GetMouseButton(0),Input.GetKeyDown(KeyCode.Q),Input.GetKey(KeyCode.F));
        if(Input.GetKeyDown(KeyCode.E)){
            bool atObjective=activeTask>=0&&Vector3.Distance(worldPosition,heroTasks[activeTask].Points[taskStep])<=4;
            if(!atObjective&&NearbyCar()>=0&&carriedCar<0)ToggleCityCar();
            else if(activeTask>=0)InteractTask();
            else NotifyHero("Move beside a car and press E or G to enter. Press J to choose a task.");
        }
    }
    bool AttachWeb()
    {
        Vector3 forward=Quaternion.Euler(0,cameraYaw,0)*Vector3.forward;
        if(!world.WebAnchor(worldPosition+Vector3.up,forward,out webAnchor)){NotifyHero("No rooftop in reach. Move closer to the city buildings.");return false;}
        swinging=true;grounded=false;ropeLength=Vector3.Distance(worldPosition+Vector3.up,webAnchor);
        heroVelocity+=forward*12+Vector3.up*22;return true;
    }
    bool ZipWeb()
    {
        if(heroEnergy<30){NotifyHero("Web energy is recharging.");return false;}
        if(!world.WebAnchor(worldPosition+Vector3.up,Quaternion.Euler(0,cameraYaw,0)*Vector3.forward,out var anchor)){NotifyHero("Aim toward a nearby rooftop to web zip.");return false;}
        heroEnergy-=30;heroVelocity=(anchor-worldPosition).normalized*38+Vector3.up*7;grounded=false;swinging=false;NotifyHero("WEB ZIP / Hold F near a wall to climb to the roof.");return true;
    }
    void StepHero(float dt,Vector2 input,bool run,bool jump,bool holdWeb,bool zip,bool wall)
    {
        if(carriedCar>=0){jump=holdWeb=zip=wall=false;run=false;}
        heroClock+=dt;messageTime=Mathf.Max(0,messageTime-dt);heroEnergy=Mathf.Min(100,heroEnergy+dt*12);
        Vector3 movement=Quaternion.Euler(0,cameraYaw,0)*new Vector3(input.x,0,input.y).normalized;
        climbing=wall&&world.FindWall(worldPosition,out _,out _);
        if(!holdWeb)swinging=false;else if(!swinging)AttachWeb();
        if(zip)ZipWeb();
        if(jump&&grounded){heroVelocity.y=16;grounded=false;}
        if(climbing&&world.FindWall(worldPosition,out var face,out var normal)){
            wallNormal=normal;wallVisualInset=world.WallSurfaceInset(face)-.18f;swinging=false;heroVelocity=-normal*.8f+(Vector3.up*input.y+Vector3.Cross(Vector3.up,-normal)*input.x)*(run?10:7);heroYaw=Mathf.Atan2(-normal.x,-normal.z)*Mathf.Rad2Deg;grounded=false;
            if(jump){heroVelocity=normal*12+Vector3.up*14;climbing=false;}
            else if(worldPosition.y>=face.max.y-.35f&&input.y>0){worldPosition-=normal*2.4f;worldPosition.y=face.max.y+.025f;heroVelocity=Vector3.zero;climbing=false;grounded=true;}
        }else if(grounded){
            Vector3 target=movement*(run?9:3.5f);heroVelocity.x=Mathf.MoveTowards(heroVelocity.x,target.x,dt*35);heroVelocity.z=Mathf.MoveTowards(heroVelocity.z,target.z,dt*35);heroVelocity.y=-1;
        }else {heroVelocity+=movement*dt*(swinging?18:10);heroVelocity.y-=22*dt;heroVelocity=Vector3.ClampMagnitude(heroVelocity,55);}
        if(swinging){
            ropeLength=Mathf.Max(12,ropeLength-dt*8);Vector3 radial=worldPosition+Vector3.up-webAnchor;
            if(radial.magnitude>=ropeLength){Vector3 n=radial.normalized;float outward=Vector3.Dot(heroVelocity,n);if(outward>0)heroVelocity-=n*outward;heroVelocity-=n*12*dt;}
        }
        Vector3 previous=worldPosition,delta=heroVelocity*dt;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.2f));bool landed=false;
        for(int i=0;i<steps;i++){
            Vector3 next=worldPosition+delta/steps;float floor=world.HeroFloor(new Vector3(next.x,Mathf.Max(next.y,worldPosition.y),next.z));
            if(next.y<=floor&&heroVelocity.y<=0){next.y=floor;heroVelocity.y=0;landed=true;}
            if(!HeroObstructed(next))worldPosition=next;
            else{
                var slide=new Vector3(worldPosition.x,next.y,next.z);if(!HeroObstructed(slide))worldPosition=slide;
                slide=new Vector3(next.x,worldPosition.y,worldPosition.z);if(!HeroObstructed(slide))worldPosition=slide;
                heroVelocity.x*=.7f;heroVelocity.z*=.7f;
            }
        }
        if(swinging){
            Vector3 tether=worldPosition+Vector3.up-webAnchor;
            if(tether.magnitude>ropeLength){Vector3 constrained=webAnchor+tether.normalized*ropeLength-Vector3.up;if(!HeroObstructed(constrained)){worldPosition=constrained;landed=false;}}
        }
        grounded=landed&&!climbing;
        if(grounded&&swinging)swinging=false;
        if(movement.sqrMagnitude>.01f&&!climbing)heroYaw=Mathf.LerpAngle(heroYaw,Mathf.Atan2(movement.x,movement.z)*Mathf.Rad2Deg,dt*10);
        worldTravel+=FlatDistance(previous,worldPosition);worldVelocity=heroVelocity;speed=0;boosting=false;
        web.enabled=swinging;web.SetPosition(0,worldPosition+Vector3.up*1.35f);web.SetPosition(1,webAnchor);
        UpdateCitizens(dt);UpdateCityObjects(dt);StepTasks(dt);PlaceHero();
    }
    void UpdateCitizens(float dt)
    {
        for(int i=0;i<citizens.Count;i++){
            walkPhase[i]+=dt*.024f;float phase=Mathf.PingPong(walkPhase[i],1);Vector3 p=Vector3.Lerp(walkA[i],walkB[i],phase);p.y=world.Height(p.x,p.z)+.18f;
            citizens[i].Root.SetPositionAndRotation(p,Quaternion.LookRotation((Mathf.Repeat(walkPhase[i],2)<1?1:-1)*(walkB[i]-walkA[i])));citizens[i].Animate(1.7f,heroClock+i,false,false,false);
        }
    }
    void NotifyHero(string message){heroMessage=message;messageTime=6;}
    void ResetHeroCamera(){cameraAnchor=worldPosition;cam.transform.position=worldPosition-Quaternion.Euler(0,cameraYaw,0)*Vector3.forward*7+Vector3.up*3.4f;cam.transform.LookAt(worldPosition+Vector3.up*1.2f);}
    void HeroCamera()
    {
        if(Driving){CityCarCamera();return;}
        Vector3 forward=Quaternion.Euler(0,cameraYaw,0)*Vector3.forward;
        Vector3 target=worldPosition-forward*(cameraMode==1?11:carriedCar>=0?9:5.5f)+Vector3.up*(cameraMode==1?5:3),look=worldPosition+Vector3.up*1.15f;
        // Pull the camera toward the hero when a building would hide the character.
        for(int i=1;i<=24;i++){Vector3 p=Vector3.Lerp(look,target,i/24f);if(world.HeroBlocked(p)){target=Vector3.Lerp(look,target,(i-1)/24f);break;}}
        float dt=Time.unscaledDeltaTime;cam.transform.position+=worldPosition-cameraAnchor;cameraAnchor=worldPosition;
        cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-dt*9));
        if((look-cam.transform.position).sqrMagnitude>.01f)cam.transform.rotation=Quaternion.LookRotation(look-cam.transform.position);
        cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,swinging?72:60,dt*4);
    }
}
}
