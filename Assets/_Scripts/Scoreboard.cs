using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsaneScatterbrain.ScriptGraph;

public class Scoreboard : MonoBehaviour
{
    public int enemyCount;
    public int score;
    PlayFabManager playfab;
    public ScriptGraphRunner graphRunner;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] totalEnemies;
        playfab = GetComponent<PlayFabManager>();
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = totalEnemies.Length;
    }

    public void checkCompletion() {

        if (score == enemyCount){
            endLevel();
        }

    }

    void endLevel(){
        playfab.SendLeaderboard(score);
        //graphRunner.RunAsync();

    }
}
