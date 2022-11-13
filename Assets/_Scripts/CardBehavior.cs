using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject FirePoint;
    void Start()
    {
        //TODO: Generate Random number and asign material to FrontFace Mesh. Use same number to save spell type and  mechanics (Eg. 1 - Healing, 2 Fire, etc)
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
        
    }

    public void Fire(GameObject projectile)
    {
        Instantiate(projectile, FirePoint.transform.position, FirePoint.transform.rotation);
    }
}
