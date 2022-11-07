using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject[] cardBases;

    // Start is called before the first frame update
    void Start()
    {

        cardBases = GameObject.FindGameObjectsWithTag("CardBase");

        foreach (GameObject cardBase in cardBases) {
            Instantiate(cardPrefab, cardBase.transform.position + (cardBase.transform.up * 3), cardBase.transform.rotation);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
