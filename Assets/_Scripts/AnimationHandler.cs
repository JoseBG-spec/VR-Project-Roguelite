using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{

    public float range;
    public float hitRange;
    public Transform player;
    public Animator animator; 
    public float yOffset;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) <= range && Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) >= hitRange){
             animator.SetBool("Spotted", true);
        

         } else if (Vector3.Distance(player.position, new Vector3 (transform.position.x, transform.position.y + yOffset, transform.position.z)) <= hitRange){
            animator.SetBool("Spotted", false);
  
              if (Random.value >= 0.5)
                {
                     animator.SetBool("HitRangeL", true);
                     animator.SetBool("HitRangeR", false);
                }
                    animator.SetBool("HitRangeL", false);
                    animator.SetBool("HitRangeR", true);
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
