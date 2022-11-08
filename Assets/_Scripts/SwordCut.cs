using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class SwordCut : MonoBehaviour
{
    [SerializeField] private Material crossMaterial;
    [SerializeField] private Transform cutPlane;

    public int score;
    public Scoreboard scoreboard; 

    public void Start() {

        scoreboard = GameObject.Find("Directional Light (PlayFab)").GetComponent<Scoreboard>();
    }

    public void Slice(GameObject sliceObject, GameObject parent)
    {
        SlicedHull hull = SliceObject(sliceObject, crossMaterial);

        GameObject bottom = hull.CreateLowerHull(sliceObject, crossMaterial);
        GameObject top = hull.CreateUpperHull(sliceObject, crossMaterial);
        AddHullComponents(bottom, parent);
        AddHullComponents(top, parent);
        Destroy(sliceObject);
        Destroy(parent);
        score++;
        scoreboard.checkCompletion(score);
    }

    public void AddHullComponents(GameObject go,GameObject parent)
    {
        go.transform.localPosition = parent.transform.localPosition;
        go.transform.localRotation = parent.transform.localRotation;
        //go.transform.localScale = go.transform.localScale;
        //go.layer = 6;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;

        //rb.AddExplosionForce(1, go.transform.position, 20);
    }

    public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
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
            Mesh bakedMesh = new Mesh();
            GameObject firstActiveGameObject = null;
            Material meshMaterial = null;
            for (int i = 0; i < collisionObject.transform.childCount; i++)
            {
                if (collisionObject.transform.GetChild(i).gameObject.activeSelf)
                {
                    firstActiveGameObject = collisionObject.transform.GetChild(i).gameObject;
                    Debug.Log(firstActiveGameObject.name);

                    firstActiveGameObject.GetComponent<SkinnedMeshRenderer>().BakeMesh(bakedMesh);
                    meshMaterial = firstActiveGameObject.GetComponent<SkinnedMeshRenderer>().material;

                    firstActiveGameObject.AddComponent<MeshFilter>();
                    firstActiveGameObject.AddComponent<MeshRenderer>();
                    firstActiveGameObject.GetComponent<MeshFilter>().mesh = bakedMesh;
                    firstActiveGameObject.GetComponent<MeshRenderer>().material = meshMaterial;

                    Destroy(firstActiveGameObject.GetComponent<SkinnedMeshRenderer>());
                    break;
                }
            }
            Slice(firstActiveGameObject,collisionObject);
            collisonOccured = true;
        }
    }
}
