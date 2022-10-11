using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class SwordCut : MonoBehaviour
{
    [SerializeField] private Material crossMaterial;
    [SerializeField] private Transform cutPlane;
    public void Slice(GameObject sliceObject)
    {
        SlicedHull hull = SliceObject(sliceObject, crossMaterial);

        GameObject bottom = hull.CreateLowerHull(sliceObject, crossMaterial);
        GameObject top = hull.CreateUpperHull(sliceObject, crossMaterial);
        AddHullComponents(bottom);
        AddHullComponents(top);
        Destroy(sliceObject);
    }

    public void AddHullComponents(GameObject go)
    {
        //go.layer = 6;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;

        rb.AddExplosionForce(150, go.transform.position, 20);
    }

    public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        if (obj.GetComponent<MeshFilter>() == null)
            if(obj.GetComponent<SkinnedMeshRenderer>() == null)
                return null;

        return obj.Slice(cutPlane.position, cutPlane.up, crossSectionMaterial);
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool collisonOccured = false;
        GameObject collisionObject = collision.gameObject;
        Debug.Log("Interact");
        if (collisonOccured)
        {
            return;
        }   
        if (collisionObject.layer == 6)
        {
            Slice(collisionObject);
            collisonOccured = true;
        }
    }
}
