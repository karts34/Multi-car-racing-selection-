using System.Collections;
using System.IO;
using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    IEnumerator HeroUpgradeChecks(string dir)
    {
        BeginOpenWorld();var wall=world.Buildings[0];foreach(var b in world.Buildings)if(b.size.y>25&&Mathf.Abs(b.center.x)<450&&Mathf.Abs(b.center.z)<350){wall=b;break;}
        worldPosition=new Vector3(wall.min.x-.6f,wall.min.y+7,wall.center.z);heroVelocity=Vector3.zero;grounded=false;cameraYaw=90;
        for(int i=0;i<50;i++)StepHero(.02f,Vector2.zero,false,false,false,false,true);
        float clingHeight=worldPosition.y;HeroCheck(climbing&&Mathf.Abs(clingHeight-(wall.min.y+7))<.1f,"stationary wall adhesion");
        Vector3 before=worldPosition;for(int i=0;i<50;i++)StepHero(.02f,Vector2.right,false,false,false,false,true);
        HeroCheck(climbing&&FlatDistance(before,worldPosition)>5&&Mathf.Abs(worldPosition.y-clingHeight)<.2f,"sideways wall walking");
        for(int i=0;i<30;i++)StepHero(.02f,Vector2.up,false,false,false,false,true);HeroCheck(worldPosition.y>clingHeight+3,"ascending wall walk");
        yield return HeroShot(dir,"hero-wall-walking");float high=worldPosition.y;
        for(int i=0;i<20;i++)StepHero(.02f,Vector2.down,false,false,false,false,true);HeroCheck(worldPosition.y<high-2,"descending wall walk");
        StepHero(.02f,Vector2.zero,false,true,false,false,true);HeroCheck(!climbing&&heroVelocity.y>10,"wall leap detaches and launches");
        for(int id=0;id<4;id++){
            BeginOpenWorld();var car=cityCars[id].root;car.SetPositionAndRotation(world.At(0,-800)+Vector3.up*.16f,Quaternion.identity);worldPosition=car.position+Vector3.right*3;grounded=true;
            HeroCheck(ToggleCityCar()&&drivenCar==id&&!hero.Root.gameObject.activeSelf,"enter city car "+id);Vector3 start=worldPosition;
            for(int i=0;i<100;i++)StepCityCar(.02f,0,true,false,false);
            HeroCheck(FlatDistance(start,worldPosition)>15&&speed>12,"city acceleration "+id);HeroCheck(!ToggleCityCar()&&Driving,"prevent exit at speed");
            for(int i=0;i<20;i++)StepCityCar(.02f,1,true,false,false);HeroCheck(cityCarYaw>15,"city car steering "+id);
            speed=0;HeroCheck(ToggleCityCar()&&!Driving&&hero.Root.gameObject.activeSelf&&!HeroObstructed(worldPosition),"safe exit "+id);
            HeroCheck(ToggleCityCar(),"reenter parked car "+id);cityCarYaw=0;start=worldPosition;
            for(int i=0;i<100;i++)StepCityCar(.02f,0,false,true,false);HeroCheck(speed< -5&&worldPosition.z<start.z-8,"reverse "+id);speed=0;ToggleCityCar();
        }
        BeginOpenWorld();cityCars[0].root.SetPositionAndRotation(world.At(wall.min.x-10,wall.center.z)+Vector3.up*.16f,Quaternion.Euler(0,90,0));worldPosition=cityCars[0].root.position+Vector3.back*3;grounded=true;HeroCheck(ToggleCityCar(),"enter wall collision test car");
        for(int i=0;i<160;i++)StepCityCar(.02f,0,true,false,false);HeroCheck(worldPosition.x<wall.min.x-2&&speed<1,"car building collision");
        RecoverCityCar();HeroCheck(speed==0&&!world.Blocked(worldPosition),"city car recovery");
        yield return HeroShot(dir,"hero-city-driving");speed=0;HeroCheck(ToggleCityCar(),"exit after recovery");
        BeginOpenWorld();worldPosition=cityCars[0].root.position+Vector3.right*3;heroYaw=0;cameraYaw=180;grounded=true;heroVelocity=Vector3.zero;
        HeroCheck(LiftCityCar()&&carriedCar==0&&hero.Carrying,"super strength lift");HeroCheck(!ToggleCityCar(),"cannot enter while holding car");
        StepHero(.02f,Vector2.zero,false,true,true,true,true);HeroCheck(grounded&&!swinging&&!climbing,"carrying keeps traversal safe");
        Vector3 carryStart=worldPosition;for(int i=0;i<40;i++)StepHero(.02f,Vector2.up,false,false,false,false,false);
        HeroCheck(FlatDistance(carryStart,worldPosition)>2&&Vector3.Distance(cityCars[0].root.position,worldPosition+Vector3.up*1.9f)<.1f,"carry follows moving hero");
        NotifyHero("SUPER STRENGTH / H set down / T throw the car");yield return HeroShot(dir,"hero-super-strength");Vector3 launchStart=cityCars[0].root.position;
        HeroCheck(ReleaseCityCar(true)&&carriedCar<0&&thrown[0]&&!hero.Carrying,"throw release");
        for(int i=0;i<35;i++)UpdateCityObjects(.02f);HeroCheck(cityCars[0].root.position.y>launchStart.y+2,"throw ballistic ascent");
        yield return HeroShot(dir,"hero-car-throw");
        for(int i=0;i<250;i++)UpdateCityObjects(.02f);
        HeroCheck(!thrown[0]&&FlatDistance(launchStart,cityCars[0].root.position)>25,"thrown car settles on ground");
        worldPosition=cityCars[0].root.position+Vector3.back*4;grounded=true;HeroCheck(ToggleCityCar(),"thrown car remains drivable");speed=0;ToggleCityCar();
        HeroCheck(LiftCityCar()&&ReleaseCityCar(false)&&carriedCar<0&&!thrown[0],"safe put down");
        BeginOpenWorld();worldPosition=cityCars[0].root.position+Vector3.right*3;grounded=true;ToggleCityCar();var parked=worldPosition;ToggleMap();yield return new WaitForSecondsRealtime(.3f);HeroCheck(stage==Stage.Paused&&Time.timeScale==0&&worldPosition==parked,"map pauses city driving");ToggleMap();
        BackToGarage();HeroCheck(!Driving&&carriedCar<0&&player.root.gameObject.activeSelf,"garage clears city vehicle state");BeginOpenWorld();
        File.WriteAllText(Path.Combine(dir,"hero-upgrade-tests.txt"),"PASS: stationary wall adhesion, sideways/ascent/descent wall walking, wall leap; all four city cars enter, accelerate, steer, reverse, reject moving exit, allow safe stopped exit and reentry; building collision, recovery, carry, traversal lock while carrying, ballistic throw and landing, thrown car remains drivable, safe put-down, paused driving, garage transition.\n");
        Debug.Log("HERO_DRIVING_STRENGTH_WALL_PASS");
    }
}
}
