using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationHandler : MonoBehaviour
{

    public float range;
    public float hitRange;
    public Transform player;
    public Animator animator; 
    public float yOffset;
    float rValue;
    float timer = 0;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("XR Origin").transform;


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) <= range && Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) >= hitRange){
             animator.SetBool("Spotted", true);
             agent.SetDestination(player.position);
        

         } else if (Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) <= hitRange){
            animator.SetBool("Spotted", false);
            rValue = Random.value;
            Debug.Log(rValue);
              if (rValue >= 0.5)
                {
                     animator.SetBool("HitRangeL", true);
                    timer += Time.deltaTime;
                    if (timer > 1.5) {
                         animator.SetBool("HitRangeL", false);
                         timer = 0;
                    }
                    
                }
                    animator.SetBool("HitRangeR", true);
                    if (timer > 1.5) {
                         animator.SetBool("HitRangeR", false);
                         timer = 0;
                    }
                    
                }
        
    }

      void OnDrawGizmos()
    {   
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z), range);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z), hitRange);
    }

}
