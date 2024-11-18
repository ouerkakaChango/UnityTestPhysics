using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using XPhysic;
using static MathHelper.XMathFunc;

public class PhysicFS : MonoBehaviour
{
    public Vector3 balanceOffset = new Vector3(0,0.5f,0);
    public float sprintFK = 1.0f;
    public float dampFK = 0.1f;
    public float m = 0.1f;
    public PhysicGroup group;

    public Vector3 impulse = new Vector3(0, -10, 0);

    public Shader shader_AddFBySpring;
    public Shader shader_UpdateV1;
    public Shader shader_UpdateR1;

    //float4 RT，rgb分别放(F.x,F.y,F.z).默认每个元素的m都一样，所以就不用rt_F了
    public Texture2D tex_Fa,tex_Fb;
    public Texture2D tex_v0,tex_v1;
    public Texture2D tex_r0,tex_r1,tex_balancePos;

    SimulateState state = SimulateState.None;

    public float time_startSleep = 1.0f;
    float t_startSleep = 0.0f;
    public float time_interval = 0.02f;
    float t_interval;

    bool inited = false;

    RenderTexture rt;
    Material mat_AddForceBySpring;
    Material mat_UpdateV1;
    Material mat_UpdateR1;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if(!inited)
        {
            return;
        }

        if (state == SimulateState.Sleeping)
        {
            t_startSleep += Time.deltaTime;
            if (t_startSleep > time_startSleep)
            {
                state = SimulateState.Simulating;
            }
        }
        else if (state == SimulateState.Simulating)
        {
            t_interval += Time.deltaTime;
            if (t_interval >= time_interval)
            {
                float dt = t_interval;
                //Debug.Log("simulating");
                CalculateForce();
                Update_v1(dt);
                Update_r1(dt);
                Update_finish();

                t_interval -= time_interval;
            }

        }
    }

    void Init()
    {
        if (group==null || group.objs.Count==0)
        {
            Debug.LogError("group==null|| group.objs.Count==0||group.size!=size");
            return;
        }
        //@@@
        if(shader_AddFBySpring==null || shader_UpdateV1==null || shader_UpdateR1==null)
        {
            Debug.LogError("Shaders null");
            return;
        }
        CreateRT(ref rt, group.size);
        mat_AddForceBySpring = new Material(shader_AddFBySpring);
        mat_UpdateV1 = new Material(shader_UpdateV1);
        mat_UpdateR1 = new Material(shader_UpdateR1);
        //___
        var size = group.size;
        CreateTex(ref tex_Fa, size);
        ZeroTex(ref tex_Fa);
        CreateTex(ref tex_Fb, size);
        ZeroTex(ref tex_Fb);
        CreateTex(ref tex_v0, size);
        ZeroTex(ref tex_v0);
        CreateTex(ref tex_v1, size);
        ZeroTex(ref tex_v1);

        //注入当前位置
        CreateTex(ref tex_r0, size);
        Update_r0ByGroup();

        CreateTex(ref tex_r1, size);


        //注入平衡位置
        CreateTex(ref tex_balancePos, size);
        Init_balancePos();

        t_interval = 0;
        inited = true;
        if (time_startSleep > 0)
        {
            t_startSleep = 0;
            state = SimulateState.Sleeping;
        }
        else
        {
            state = SimulateState.Simulating;
        }
    }
    void Update_r0ByGroup()
    {
        List<Vector3> posList = new List<Vector3>();
        for (int i=0;i<group.objs.Count;i++)
        {
            posList.Add(group.objs[i].transform.position);
        }
        InitPosTex(ref tex_r0, posList.ToArray());
    }

    void Init_balancePos()
    {
        
        List<Vector3> posList = new List<Vector3>();
        for (int i = 0; i < group.objs.Count; i++)
        {
            posList.Add(group.objs[i].transform.position + balanceOffset);
        }
        //Debug.Log("balacnce"+posList[0]);
        InitPosTex(ref tex_balancePos, posList.ToArray());
    }

    static void InitPosTex(ref Texture2D tex,Vector3[] posArr)
    {
        Color[] colors = new Color[posArr.Length];
        for(int i=0;i<posArr.Length;i++)
        {
            colors[i] = new Color(posArr[i].x, posArr[i].y, posArr[i].z, 0);
        }
        tex.SetPixels(colors);
        tex.Apply();
    }

    void CalculateForce()
    {
        InitFByImpulse();
        AddFBySpring();
    }

    void Update_v1(float dt)
    {
        mat_UpdateV1.SetTexture( "_FbTex", tex_Fb);
        mat_UpdateV1.SetFloat("m", m);
        mat_UpdateV1.SetFloat("dt", dt);
        Graphics.Blit(tex_v0, rt, mat_UpdateV1);
        CopyTex(rt, ref tex_v1);
    }

    void Update_r1(float dt)
    {
        mat_UpdateR1.SetTexture("_v1Tex", tex_v1);
        mat_UpdateR1.SetFloat("dt", dt);
        Graphics.Blit(tex_r0, rt, mat_UpdateR1);
        CopyTex(rt, ref tex_r1);

        //2.将位置设置给group
        var colors = tex_r1.GetPixels();
        Vector3[] posArr = new Vector3[colors.Length];
        for(int i=0;i<colors.Length;i++)
        {
            posArr[i] = new Vector3(colors[i].r, colors[i].g, colors[i].b);
        }
        group.SetPositions(posArr);
    }

    void Update_finish()
    {
        CopyTex(tex_v1, ref tex_v0);
        CopyTex(tex_r1, ref tex_r0);
        ZeroTex(ref tex_Fa);
        ZeroTex(ref tex_Fb);
    }

    void InitFByImpulse()
    {
        var impulseCount = group.GetNowImpulseCount();
        Color[] colors = new Color[impulseCount.Count];
        for(int i=0;i<colors.Length;i++)
        {
            colors[i] = Vec3ToColor(impulse * impulseCount[i]);
        }
        tex_Fb.SetPixels(colors);
        group.ZeroNowImpulseCount();
    }

    void AddFBySpring()
    {
        CopyTex(tex_Fb, ref tex_Fa);
        mat_AddForceBySpring.SetTexture("_balancePosTex", tex_balancePos);
        mat_AddForceBySpring.SetTexture("_r0Tex", tex_r0);
        mat_AddForceBySpring.SetTexture("_v0Tex", tex_v0);
        mat_AddForceBySpring.SetFloat("springFK", sprintFK);
        mat_AddForceBySpring.SetFloat("dampFK", dampFK);
        Graphics.Blit(tex_Fa, rt, mat_AddForceBySpring);
        CopyTex(rt, ref tex_Fb);
    }

    static Color Vec3ToColor(Vector3 v)
    {
        return new Color(v.x, v.y, v.z, 0);
    }
    //###################################

    public static void CreateTex(ref Texture2D tex, Vector2Int size, int depth = 0)
    {
        tex = new Texture2D(size.x, size.y, TextureFormat.RGBAFloat, false,true);
        tex.filterMode = FilterMode.Point;
    }

    public static void CreateRT(ref RenderTexture rTex, Vector2Int size, int depth = 0)
    {
        rTex = new RenderTexture(size.x, size.y, depth, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rTex.enableRandomWrite = true;
        rTex.Create();
    }

    public static void ZeroTex(ref Texture2D tex)
    {
        Color[] colors = new Color[tex.width*tex.height];
        for(int i=0;i<colors.Length;i++)
        {
            colors[i] = new Color(0, 0, 0, 0);
        }
        tex.SetPixels(colors);
        tex.Apply();
    }

    public static void CopyTex(in Texture2D ra, ref Texture2D rb)
    {
        rb.SetPixels(ra.GetPixels());
        rb.Apply();
    }

    public static void CopyTex(in RenderTexture rTex, ref Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }
}
