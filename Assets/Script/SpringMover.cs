using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XPhysic;

public class SpringMover : MonoBehaviour
{
    public Vector3 balanceOffset = new Vector3(0, 0.5f, 0);
    public float sprintFK = 1.0f;
    public float time_interval=0.02f;
    public float m = 0.1f;
    public float dampFK = 0.1f;
    public Vector3 debug_impulse = new Vector3(0, -10, 0);
    public float time_startSleep = 2.0f;
    float t_startSleep = 0.0f;
    float t_interval;
    Vector3 balancePos;
    Vector3 a;
    Vector3 v0, v1;
    Vector3 F,f_impluse;
    bool inited = false;

    SimulateState state = SimulateState.None;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == SimulateState.Sleeping)
        {
            t_startSleep += Time.deltaTime;
            if(t_startSleep>time_startSleep)
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
                CalculateForce();
                Update_a1();
                Update_v1(dt);
                Update_pos(dt);
                Update_finish();
                //Debug.Log("a:"+a1.y+" v:"+v1.y);

                t_interval -= time_interval;
            }
            
        }
    }

    private void OnDrawGizmos()
    {
        if(!inited)
        {
            Init();
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(balancePos, 0.1f);
    }

    //#####################################################
    public void AddImpulse()
    {
        f_impluse = debug_impulse;
    }
    //#####################################################
    void Init()
    {
        a = Vector3.zero;
        v0 = Vector3.zero;
        F = Vector3.zero;
        f_impluse = Vector3.zero;
        balancePos = transform.position + balanceOffset;

        t_interval = 0;
        inited = true;  
        if(time_startSleep>0)
        {
            t_startSleep = 0;
            state = SimulateState.Sleeping;
        }
        else
        {
            state= SimulateState.Simulating;
        }
    }

    void CalculateForce()
    {
        F = f_impluse;
        //计算平衡位置带来的弹簧力
        F+= (balancePos - transform.position) * sprintFK;
        //计算平衡位置带来的阻尼，模拟弹簧热损耗
        F += -v0 * dampFK;
    }

    void Update_a1()
    {
        a =  F/m;
    }

    void Update_v1(float dt)
    {
        //var dv = dt * a1;
        //Debug.Log(dv);
        v1 = v0 + dt * a;
        //Debug.Log(v1);
    }

    void Update_pos(float dt)
    {
        //var dr = dt * v1;
        //Debug.Log(dr);
        transform.position += dt * v1;
    }

    void Update_finish()
    {
        //a0 = a1;
        v0 = v1;
        F = Vector3.zero;
        f_impluse = Vector3.zero;
    }
}
