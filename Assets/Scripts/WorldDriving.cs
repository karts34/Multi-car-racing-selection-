using UnityEngine;
namespace Afterglow
{
public partial class RaceGame
{
    OpenWorld world;
    bool inWorld,worldTest,worldTestingSteps,hasWaypoint;
    Vector3 worldPosition,worldVelocity,waypoint;
    float worldTravel;
    void BeginOpenWorld()
    {
        choosingMap=false;selectedMap=0;
        if(world==null)world=new OpenWorld();
        inWorld=true;fullMap=false;help=false;stage=Stage.Racing;Time.timeScale=1;
        world.Root.gameObject.SetActive(true);track.root.gameObject.SetActive(false);
        RenderSettings.fogDensity=.00065f;cam.farClipPlane=2800;
        speed=0;steer=0;nitro=100;boosting=false;elapsed=0;worldTravel=0;hasWaypoint=false;
        SetupHero();SaveSettings();Debug.Log("HERO_WORLD_READY citizens="+citizens.Count+" buildings="+world.BuildingCount);
    }
    void LeaveWorld()
    {
        if(!inWorld)return;inWorld=false;hasWaypoint=false;journal=false;drivenCar=carriedCar=-1;hero.Carrying=false;
        world.Root.gameObject.SetActive(false);web.enabled=false;
        player.root.gameObject.SetActive(true);track.root.gameObject.SetActive(true);
        foreach(var rival in rivals)rival.root.gameObject.SetActive(true);
        RenderSettings.fogDensity=.00115f;cam.farClipPlane=1500;Time.timeScale=1;
    }
    static float FlatDistance(Vector3 a,Vector3 b)=>new Vector2(a.x-b.x,a.z-b.z).magnitude;
    void WorldDot(Rect rect,Vector3 point,Color color,float size){var p=world.MapPoint(point);Panel(rect.x+p.x*rect.width-size/2,rect.y+p.y*rect.height-size/2,size,size,color);}
}
}
