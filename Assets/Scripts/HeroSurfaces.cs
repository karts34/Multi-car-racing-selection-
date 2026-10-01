using System.Collections.Generic;
using UnityEngine;

namespace Afterglow
{
public sealed partial class OpenWorld
{
    public IReadOnlyList<Bounds> Buildings=>urbanFootprints;
    readonly Dictionary<Bounds,float> wallSurfaceInsets=new Dictionary<Bounds,float>();
    public float WallSurfaceInset(Bounds wall)=>wallSurfaceInsets.TryGetValue(wall,out float inset)?inset:0;
    public float HeroFloor(Vector3 p)
    {
        float floor=Height(p.x,p.z)+.18f;
        foreach(var b in urbanFootprints)if(p.x>b.min.x-.1f&&p.x<b.max.x+.1f&&p.z>b.min.z-.1f&&p.z<b.max.z+.1f&&p.y>=b.max.y-.5f)floor=Mathf.Max(floor,b.max.y+.015f);
        return floor;
    }
    public bool HeroBlocked(Vector3 p)
    {
        if(Mathf.Abs(p.x)>Extent-12||Mathf.Abs(p.z)>Extent-12||Lake(p.x,p.z)<1.035f&&p.y<Height(p.x,p.z)+2)return true;
        foreach(var b in urbanFootprints){
            if(p.y>=b.max.y-.05f||p.y+1.8f<b.min.y)continue;
            float dx=Mathf.Max(b.min.x-p.x,0,p.x-b.max.x),dz=Mathf.Max(b.min.z-p.z,0,p.z-b.max.z);
            if(dx*dx+dz*dz<.42f*.42f)return true;
        }
        return false;
    }
    public bool FindWall(Vector3 p,out Bounds wall,out Vector3 normal)
    {
        wall=new Bounds();normal=Vector3.forward;float nearest=2.0f;bool found=false;
        foreach(var b in urbanFootprints){
            if(p.y>b.max.y+1||p.y+1.8f<b.min.y)continue;var q=b.ClosestPoint(p+Vector3.up);q.y=p.y;
            Vector3 d=p-q;float distance=d.magnitude;if(distance>=nearest||distance<.01f)continue;
            nearest=distance;wall=b;normal=d.normalized;found=true;
        }
        return found;
    }
    public bool WebAnchor(Vector3 p,Vector3 forward,out Vector3 anchor)
    {
        anchor=Vector3.zero;float best=float.MaxValue;
        foreach(var b in urbanFootprints){
            Vector3 target=new Vector3(Mathf.Clamp(p.x+forward.x*25,b.min.x+1,b.max.x-1),b.max.y+.3f,Mathf.Clamp(p.z+forward.z*25,b.min.z+1,b.max.z-1));
            Vector3 delta=target-p;float distance=delta.magnitude;Vector3 flat=delta;flat.y=0;
            float facing=Vector3.Dot(forward,flat.normalized);
            if(target.y<p.y+5||distance>145||distance<10||facing<-.15f)continue;
            float score=distance+(1-facing)*65;if(score<best){best=score;anchor=target;}
        }
        return best<float.MaxValue;
    }
}
}
