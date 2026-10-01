using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    CarVisual[] cityCars;
    Vector3[] thrownVelocity;
    bool[] thrown;
    int drivenCar=-1,carriedCar=-1;
    float cityCarYaw;
    bool Driving=>drivenCar>=0;
    void SetupCityCars()
    {
        if(cityCars==null){cityCars=new CarVisual[4];thrownVelocity=new Vector3[4];thrown=new bool[4];for(int i=0;i<4;i++){cityCars[i]=new CarVisual(CarCatalog.All[i].Name+" / City vehicle",CarCatalog.Paints[i],CarCatalog.All[i].Body);cityCars[i].root.SetParent(world.Root,false);}}
        drivenCar=carriedCar=-1;hero.Carrying=false;hero.Root.gameObject.SetActive(true);
        for(int i=0;i<4;i++){cityCars[i].root.SetPositionAndRotation(world.At(7,-335+i*16)+Vector3.up*.16f,Quaternion.identity);thrownVelocity[i]=Vector3.zero;thrown[i]=false;}
    }
    int NearbyCar(float radius=5)
    {
        if(cityCars==null)return -1;int found=-1;float best=radius;
        for(int i=0;i<cityCars.Length;i++){if(thrown[i]||i==carriedCar)continue;float d=Vector3.Distance(worldPosition,cityCars[i].root.position);if(d<best){best=d;found=i;}}
        return found;
    }
    bool HeroObstructed(Vector3 point)
    {
        if(world.HeroBlocked(point))return true;
        if(carriedCar>=0){var rotation=Quaternion.Euler(0,heroYaw+90,0);Vector3 load=point+Vector3.up*1.9f;foreach(var offset in new[]{new Vector3(-1.1f,0,-2.8f),new Vector3(1.1f,0,-2.8f),new Vector3(-1.1f,0,2.8f),new Vector3(1.1f,0,2.8f)})if(world.HeroBlocked(load+rotation*offset))return true;}
        if(cityCars!=null)for(int i=0;i<4;i++){
            if(i==carriedCar)continue;var car=cityCars[i].root;var local=car.InverseTransformPoint(point);
            if(local.y<1.9f&&local.y> -1.7f&&Mathf.Abs(local.x)<1.5f&&Mathf.Abs(local.z)<3.1f)return true;
        }
        return false;
    }
    bool ToggleCityCar()
    {
        if(Driving){
            if(Mathf.Abs(speed)>2){NotifyHero("Brake to a stop before leaving the car.");return false;}
            var car=cityCars[drivenCar].root;
            foreach(var offset in new[]{Vector3.left*3,Vector3.right*3,Vector3.back*4,Vector3.forward*4}){
                Vector3 p=car.position+car.rotation*offset;p.y=world.HeroFloor(p+Vector3.up*.5f);
                if(HeroObstructed(p))continue;
                drivenCar=-1;worldPosition=p;heroVelocity=Vector3.zero;speed=0;grounded=true;hero.Root.gameObject.SetActive(true);PlaceHero();ResetHeroCamera();NotifyHero("ON FOOT / E or G enter car / H lift car / T throw");return true;
            }
            NotifyHero("No space beside the car. Move to an open area first.");return false;
        }
        if(carriedCar>=0||!grounded||climbing){NotifyHero("Stand on the ground with empty hands to enter a car.");return false;}
        int nearby=NearbyCar();if(nearby<0){NotifyHero("Move beside a parked car and press E or G.");return false;}
        drivenCar=nearby;worldPosition=cityCars[nearby].root.position;cityCarYaw=cityCars[nearby].root.eulerAngles.y;speed=0;steer=0;swinging=false;web.enabled=false;heroVelocity=Vector3.zero;hero.Root.gameObject.SetActive(false);cameraYaw=cityCarYaw;
        ResetHeroCamera();NotifyHero("DRIVING / WASD or arrows drive / S brake or reverse / E or G exit when stopped");return true;
    }
    bool CarSpace(Vector3 p,Quaternion rotation,int ignore)
    {
        Vector3 forward=rotation*Vector3.forward;
        if(world.Blocked(p)||world.Blocked(p+forward*2)||world.Blocked(p-forward*2))return false;
        for(int i=0;i<4;i++)if(i!=ignore&&i!=carriedCar&&Mathf.Abs(cityCars[i].root.position.y-p.y)<2&&FlatDistance(p,cityCars[i].root.position)<4.6f)return false;
        return true;
    }
    void StepCityCar(float dt,float turn,bool throttle,bool brake,bool handbrake)
    {
        var spec=CarCatalog.All[drivenCar];steer=Mathf.MoveTowards(steer,turn,dt*4);
        float acceleration=throttle?(speed<0?spec.Braking:spec.Acceleration):brake?(speed>0?-spec.Braking:-7):0;
        if(!throttle&&!brake)speed=Mathf.MoveTowards(speed,0,dt*4);
        speed=Mathf.Clamp(speed+acceleration*dt-Mathf.Sign(speed)*speed*speed*.0017f*dt,-12,spec.TopSpeed);
        if(handbrake)speed=Mathf.MoveTowards(speed,0,dt*18);
        world.NearestRoad(worldPosition,out float roadDistance,out _);if(roadDistance>12&&Mathf.Abs(speed)>25)speed=Mathf.MoveTowards(speed,Mathf.Sign(speed)*25,dt*20);
        cityCarYaw+=steer*Mathf.Lerp(100,48,Mathf.Abs(speed)/90)*spec.Handling*dt*Mathf.Clamp01(Mathf.Abs(speed)/4)*Mathf.Sign(speed);
        Quaternion rotation=Quaternion.Euler(0,cityCarYaw,0);Vector3 move=rotation*Vector3.forward*speed*dt;
        int steps=Mathf.Max(1,Mathf.CeilToInt(move.magnitude/.4f));
        for(int i=0;i<steps;i++){Vector3 next=worldPosition+move/steps;next.y=world.Height(next.x,next.z)+.16f;if(!CarSpace(next,rotation,drivenCar)){speed=0;break;}worldTravel+=FlatDistance(worldPosition,next);worldPosition=next;}
        cityCars[drivenCar].root.SetPositionAndRotation(worldPosition,rotation);cityCars[drivenCar].Animate(speed,steer,brake,false,dt);cameraYaw=cityCarYaw;
        heroClock+=dt;messageTime=Mathf.Max(0,messageTime-dt);UpdateCitizens(dt);UpdateCityObjects(dt);StepTasks(dt);
    }
    void CityCarCamera()
    {
        var car=cityCars[drivenCar].root;Vector3 look=car.position+Vector3.up*1.2f+car.forward*3,target=car.position-car.forward*(cameraMode==1?15:10)+Vector3.up*(cameraMode==1?6:3.6f);
        target.y=Mathf.Max(target.y,world.Height(target.x,target.z)+1.2f);cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-Time.unscaledDeltaTime*7));cam.transform.LookAt(look);cam.fieldOfView=62+Mathf.Abs(speed)*.08f;cameraAnchor=worldPosition;
    }
    bool LiftCityCar()
    {
        if(Driving)return false;
        if(carriedCar>=0)return ReleaseCityCar(false);
        if(!grounded||climbing||swinging){NotifyHero("Stand beside a car on the ground to lift it.");return false;}
        int nearby=NearbyCar(5.5f);if(nearby<0){NotifyHero("SUPER STRENGTH / Move beside a parked car, then press H.");return false;}
        carriedCar=nearby;hero.Carrying=true;PositionCarriedCar();NotifyHero("SUPER STRENGTH / Carry with WASD / H put down / T throw toward the camera direction");return true;
    }
    void PositionCarriedCar(){if(carriedCar<0)return;cityCars[carriedCar].root.SetPositionAndRotation(worldPosition+Vector3.up*1.9f,Quaternion.Euler(0,heroYaw+90,0));}
    bool ReleaseCityCar(bool launch)
    {
        if(carriedCar<0)return false;int id=carriedCar;var car=cityCars[id].root;
        if(launch){thrown[id]=true;thrownVelocity[id]=Quaternion.Euler(0,cameraYaw,0)*Vector3.forward*30+Vector3.up*12;NotifyHero("CAR THROW / Super strength released!");}
        else{
            bool placed=false;foreach(var offset in new[]{Vector3.forward*5,Vector3.back*5,Vector3.right*5,Vector3.left*5}){
                Vector3 p=worldPosition+Quaternion.Euler(0,heroYaw,0)*offset;p.y=world.Height(p.x,p.z)+.16f;
                if(!CarSpace(p,car.rotation,id))continue;car.position=p;placed=true;break;
            }
            if(!placed){NotifyHero("No clear space to put the car down. Walk to an open street.");return false;}
            NotifyHero("CAR SET DOWN / G to drive it.");
        }
        carriedCar=-1;hero.Carrying=false;return true;
    }
    void UpdateCityObjects(float dt)
    {
        PositionCarriedCar();if(cityCars==null)return;
        for(int i=0;i<4;i++)if(thrown[i]){
            var car=cityCars[i].root;thrownVelocity[i]+=Vector3.down*20*dt;Vector3 move=thrownVelocity[i]*dt;int count=Mathf.Max(1,Mathf.CeilToInt(move.magnitude/.3f));
            for(int s=0;s<count;s++){
                Vector3 p=car.position+move/count,forward=car.forward;
                if(world.HeroBlocked(p)||world.HeroBlocked(p+forward*2)||world.HeroBlocked(p-forward*2)){
                    thrownVelocity[i]=new Vector3(0,Mathf.Min(0,thrownVelocity[i].y),0);p=car.position+Vector3.up*move.y/count;
                    if(world.HeroBlocked(p))p=car.position;
                }
                float floor=world.HeroFloor(new Vector3(p.x,Mathf.Max(p.y,car.position.y),p.z));
                if(p.y<=floor&&thrownVelocity[i].y<=0){p.y=floor;thrown[i]=false;thrownVelocity[i]=Vector3.zero;car.position=p;break;}
                car.position=p;
            }
        }
    }
    void RecoverCityCar()
    {
        worldPosition=world.NearestRoad(worldPosition,out _,out var direction)+Vector3.up*.16f;cityCarYaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;speed=0;steer=0;cityCars[drivenCar].root.SetPositionAndRotation(worldPosition,Quaternion.Euler(0,cityCarYaw,0));
    }
}
}
