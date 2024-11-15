using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XUtility
{
    public enum ModelAxisImportMode
    {
        SameInUnity,
        YZInverse,
    }

    public enum ModelCenterImportMode
    {
        Normal,
        OnTheGround,
    }
    public static class ObjManagerUtility
    {
        public static void ClearChildren(GameObject root)
        {
            if(root == null)
            {
                return;
            }
            for(int i= root.transform.childCount - 1; i>=0;i--)
            {
                GameObject.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }
        }

        public static GameObject FindChildGo(GameObject root, string name)
        {
            Transform[] transArr = root.GetComponentsInChildren<Transform>(true);
            foreach (var trans in transArr)
            {
                //Debug.Log(trans.name);
                if (trans.gameObject.name == name)
                {
                    return trans.gameObject;
                }
            }
            return null;
        }

        public static Bounds GetBoundsOf(GameObject obj)
        {
            Bounds re = new Bounds(obj.transform.position,Vector3.zero);
            var mr = obj.GetComponent<MeshRenderer>();
            if(mr)
            {
                re.Encapsulate(mr.bounds);
            }
            for(int i=0;i<obj.transform.childCount;i++)
            {
                mr = obj.transform.GetChild(i).GetComponent<MeshRenderer>();
                if (mr)
                {
                    re.Encapsulate(mr.bounds);
                }
            }
            return re;
        }

        public static void SetChildrenLayerExcludeSelf(GameObject root,int targetLayer)
        {
            if (root == null)
            {
                return;
            }
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                SetGameLayerRecursive(root.transform.GetChild(i).gameObject, targetLayer);
            }
        }

        public static void SetGameLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetGameLayerRecursive(child.gameObject, layer);
            }
        }
    }
}