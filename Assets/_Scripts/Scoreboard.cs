using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsaneScatterbrain.ScriptGraph;

public class Scoreboard : MonoBehaviour
{


    public GameObject[] totalEnemies;
    public int enemyCount;
    public int score;
    PlayFabManager playfab;
    public ScriptGraphRunner graphRunner;


    // Start is called before the first frame update
    void Start()
    {
        playfab = GetComponent<PlayFabManager>();
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = totalEnemies.Length;
    }

    public void checkCompletion(int current) {

        if (current == enemyCount){
            endLevel(current);
        }

    }

    void endLevel(int score){
        playfab.SendLeaderboard(score);
        graphRunner.Run();

    }
}
