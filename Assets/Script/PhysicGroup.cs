using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static XUtility.ObjManagerUtility;

public class PhysicGroup : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(2,2);
    public float cellSize = 1.0f;
    public GameObject prefab;
    public List<GameObject> objs = new List<GameObject>();
    public bool bRandY = false;
    public Vector2 randYRange = new Vector2(-1,1);
    List<int> nowImpulseCount = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        InitCollisionPlane();
        InitImpulseCount();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckAddImpulseCount();
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 hGridSize = new Vector2((size.x - 1) * 0.5f * cellSize, (size.y - 1) * 0.5f * cellSize);
        Vector3 gridCenter = transform.position+new Vector3(hGridSize.x, 0, hGridSize.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gridCenter, new Vector3(hGridSize.x*2+cellSize,0.01f, hGridSize.y*2+cellSize));
    }



    //##########################################

    public void CreateGroup()
    {
        objs.Clear();
        ClearChildren(gameObject);
        Vector3 p0 = transform.position;
        for(int j=0;j<size.y;j++)
        {
            for(int i=0;i<size.x;i++)
            {
                var go = Instantiate(prefab, 
                    p0 + new Vector3(i * cellSize, 0, j * cellSize)+ (bRandY ?new Vector3(0,Random.Range(randYRange.x,randYRange.y),0):Vector3.zero),
                    prefab.transform.rotation);
                go.transform.parent = transform;
                objs.Add(go);
            }
        }
    }

    public Vector2Int GetIxyByID(int id)
    {
        int ix = id%size.x;
        int iy = id/size.x;
        return new Vector2Int(ix, iy);
    }

    public int GetIDByIxy(Vector2Int ixy)
    {
        return GetIDByIxy(ixy.x, ixy.y);
    }

    public int GetIDByIxy(int ix,int iy)
    {
        return ix + iy * size.x;
    }

    public void SetPositions(Vector3[] posArr)
    {
        for(int i=0;i<objs.Count;i++)
        {
            objs[i].transform.position = posArr[i];
        }
    }

    public List<int> GetNowImpulseCount()
    { 
        return nowImpulseCount; 
    }

    public void ZeroNowImpulseCount()
    {
        for(int i=0;i<nowImpulseCount.Count;i++)
        {
            nowImpulseCount[i] = 0; 
        }
    }
    //#########################################################
    void InitCollisionPlane()
    {
        Vector2 hGridSize = new Vector2((size.x - 1) * 0.5f * cellSize, (size.y - 1) * 0.5f * cellSize);
        var boxCollider = GetComponent<BoxCollider>();
        boxCollider.center = new Vector3(hGridSize.x, 0, hGridSize.y);
        boxCollider.size = new Vector3(hGridSize.x * 2+cellSize, 0.01f, hGridSize.y * 2+cellSize);
    }

    void InitImpulseCount()
    {
        nowImpulseCount.Clear();
        int num = size.x*size.y;
        for (int i = 0; i < num; i++)
        {
            nowImpulseCount.Add(0);
        }
    }

    void CheckAddImpulseCount()
    {
        RaycastHit raycastHit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ray.origin = Camera.main.transform.position;
        bool hit = Physics.Raycast(ray, out raycastHit);
        if (hit)
        {
            Debug.DrawLine(ray.origin, raycastHit.point, Color.green,2.0f);
            var result = raycastHit.point;
            int ix = (int)((result.x - transform.position.x) / cellSize);
            int iy = (int)((result.z - transform.position.z) / cellSize);
            //Debug.Log("add impulse at ixy: " + ix + " " + iy);
            nowImpulseCount[GetIDByIxy(ix, iy)] += 1;
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 5, Color.red,2.0f);
        }
    }
}
