using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMouseRay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit raycastHit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ray.origin = Camera.main.transform.position;
        bool hit = Physics.Raycast(ray, out raycastHit);
        if (hit)
        {
            Debug.DrawLine(ray.origin, raycastHit.point, Color.green);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin+ray.direction*5, Color.red);
        }
        
    }


}
