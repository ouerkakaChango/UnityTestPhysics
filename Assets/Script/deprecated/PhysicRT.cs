using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using XPhysic;
using static MathHelper.XMathFunc;

public class PhysicRT : MonoBehaviour
{
    public Vector3 balanceOffset = new Vector3(0,0.5f,0);
    public float sprintFK = 1.0f;
    public float dampFK = 0.1f;
    public float m = 0.1f;
    public PhysicGroup group;
    //float4 RT，rgb分别放(F.x,F.y,F.z).默认每个元素的m都一样，所以就不用rt_F了
    public RenderTexture rt_Fa,rt_Fb;
    public RenderTexture rt_v0,rt_v1;
    public RenderTexture rt_r0,rt_r1,rt_balancePos;

    SimulateState state = SimulateState.None;

    public float time_startSleep = 1.0f;
    float t_startSleep = 0.0f;
    public float time_interval = 0.02f;
    float t_interval;

    bool inited = false;

    ComputeShader cs_springMover;

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
        cs_springMover = (ComputeShader)Resources.Load("ComputeShader/XPhysic/SpringMover");
        if (group==null || group.objs.Count==0|| cs_springMover==null)
        {
            Debug.LogError("group==null|| group.objs.Count==0||group.size!=size||cs_springMover==null");
            return;
        }
        var size = group.size;
        CreateRT(ref rt_Fa, size);
        ZeroRT(ref rt_Fa);
        CreateRT(ref rt_Fb, size);
        ZeroRT(ref rt_Fb);
        CreateRT(ref rt_v0, size);
        ZeroRT(ref rt_v0);
        CreateRT(ref rt_v1, size);
        ZeroRT(ref rt_v1);

        //注入当前位置
        CreateRT(ref rt_r0, size);
        Update_r0ByGroup();

        CreateRT(ref rt_r1, size);


        //注入平衡位置
        CreateRT(ref rt_balancePos, size);
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
        InitPosRT(ref rt_r0, posList.ToArray());
    }

    void Init_balancePos()
    {
        
        List<Vector3> posList = new List<Vector3>();
        for (int i = 0; i < group.objs.Count; i++)
        {
            posList.Add(group.objs[i].transform.position + balanceOffset);
        }

        InitPosRT(ref rt_balancePos, posList.ToArray());
    }

    static void InitPosRT(ref RenderTexture rTex,System.Array posArr)
    {
        ComputeBuffer buffer_pos = null;
        PreComputeBuffer(ref buffer_pos, sizeof(float) * 3, posArr);
        var cs = (ComputeShader)Resources.Load("ComputeShader/XPhysic/Init");
        //##################################
        //### compute
        int kInx = cs.FindKernel("InitR");

        cs.SetTexture(kInx, "Result4", rTex);
        cs.SetBuffer(kInx, "posBuffer", buffer_pos);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;
        SafeDispose(buffer_pos);
    }

    void CalculateForce()
    {
        InitFByImpulse();
        AddFBySpring();
    }

    void Update_v1(float dt)
    {
        var cs = cs_springMover;
        //##################################
        //### compute
        int kInx = cs.FindKernel("UpdateV1");

        cs.SetTexture(kInx, "Result4", rt_v1);

        cs.SetTexture(kInx, "FbRT", rt_Fb);
        cs.SetTexture(kInx, "v0RT", rt_v0);
        cs.SetFloat("m", m);
        cs.SetFloat("dt", dt);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;
    }

    void Update_r1(float dt)
    {
        ComputeBuffer buffer_pos = null;
        Vector3[] posArr=new Vector3[group.size.x * group.size.y];
        var cs = cs_springMover;
        PreComputeBuffer(ref buffer_pos, sizeof(float) * 3, posArr);
        //##################################
        //### compute
        int kInx = cs.FindKernel("UpdateR1");

        cs.SetTexture(kInx, "Result4", rt_r1);

        cs.SetTexture(kInx, "v1RT", rt_v1);
        cs.SetTexture(kInx, "r0RT", rt_r0);
        cs.SetFloat("dt", dt);
        //位置结果注入computeBuffer
        cs.SetBuffer(kInx, "posBuffer", buffer_pos);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;

        //2.将位置设置给group
        buffer_pos.GetData(posArr);
        //??? debug
        Debug.Log(posArr[3]);
        //___
        group.SetPositions(posArr);
        SafeDispose(buffer_pos);
    }

    void Update_finish()
    {
        CopyRT(rt_v1, ref rt_v0);
        ZeroRT(ref rt_Fa);
        ZeroRT(ref rt_Fb);
        //todo
        //ZeroRT(ref rt_impulse)
    }

    void InitFByImpulse()
    {
        //...
        //todo
    }

    void AddFBySpring()
    {
        CopyRT(rt_Fb, ref rt_Fa);

        var cs = cs_springMover;
        //##################################
        //### compute
        int kInx = cs.FindKernel("AddFBySpring");

        cs.SetTexture(kInx, "Result4", rt_Fb);

        cs.SetTexture(kInx, "balancePosRT", rt_balancePos);
        cs.SetTexture(kInx, "r0RT", rt_r0);
        cs.SetTexture(kInx, "FaRT", rt_Fa);
        cs.SetTexture(kInx, "v0RT", rt_v0);
        cs.SetFloat("springFK", sprintFK);
        cs.SetFloat("dampFK", dampFK);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;
    }
    //###################################

    public static void CreateRT(ref RenderTexture rTex, Vector2Int size, int depth = 0)
    {
        rTex = new RenderTexture(size.x, size.y, depth, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rTex.enableRandomWrite = true;
        rTex.Create();
    }

    static public void PreComputeBuffer(ref ComputeBuffer buffer, int stride, in System.Array dataArr)
    {
        if (buffer != null)
        {
            return;
        }
        buffer = new ComputeBuffer(dataArr.Length, stride);
        buffer.SetData(dataArr);
    }

    static public void SafeDispose(ComputeBuffer cb)
    {
        if (cb != null)
        {
            cb.Dispose();
        }
    }

    public static void ZeroRT(ref RenderTexture rTex)
    {
        var cs = (ComputeShader)Resources.Load("ComputeShader/XPhysic/Init");
        //##################################
        //### compute
        int kInx = cs.FindKernel("Zero4");

        cs.SetTexture(kInx, "Result4", rTex);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;
    }

    public static void CopyRT(in RenderTexture ra, ref RenderTexture rb)
    {
        var cs = (ComputeShader)Resources.Load("ComputeShader/XPhysic/Init");
        //##################################
        //### compute
        int kInx = cs.FindKernel("Copy4");

        cs.SetTexture(kInx, "Result4", rb);
        cs.SetTexture(kInx, "copyFromTex", ra);

        cs.Dispatch(kInx, 8, 8, 1);
        //### compute
        //#####################################;
    }
}
