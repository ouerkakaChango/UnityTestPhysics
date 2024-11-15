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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public void SetPositions(Vector3[] posArr)
    {
        for(int i=0;i<objs.Count;i++)
        {
            objs[i].transform.position = posArr[i];
        }
    }
}
