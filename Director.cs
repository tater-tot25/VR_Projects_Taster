using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Director : MonoBehaviour
{
    public GameObject patrolOne;
    public GameObject patrolTwo;
    public GameObject patrolThree;
    public GameObject patrolFour;
    int len;
    public GameObject[] patrols;
    int numPatrols = 4;
    public bool ready = true;
    public bool docile = false; // whether or not the enemy will chase the player
    public bool first = true; // generally used to run a function once on first frame
    bool paused;
    private bool stopwatch = false;
    public string mode; // the behaviour mode of the AI
    public bool directVision = false; // whether or not the enemy has direct vision of the player
    public Vector3 actualPlayerCoords; // the actual location of the player
    public Vector3 playerCoords; // last seen location of the player
    public bool PlayerIsHiding = false; // is the player hiding
    public LayerMask IgnoreMe; // which layers to ignore on vision cone collision
    public Transform player; // transform of player
    public Transform head; // transform of enemy's head
    public Transform enemy; // transform of enemy
    Vector3 coords; // coords of next patrol point?
    Vector3 dCoords; // coords of the distraction point for the AI;
    enemyAI force;
    int step = 0; // the patrol num the AI is at?
    public float time;
    private int interval = 25; // how many frames before we run the update script
    public float timePassed;
    public float AIawareness; //The lower the number, the faster the AI notices the Player
    public float EnemyHearingRange; // How close a noise object has to be for the AI to hear it;
    public bool getHide()
    {
        return PlayerIsHiding;
    }
    public void setHide(bool set)
    {
        PlayerIsHiding = set;
    }
    public IEnumerator wait(float time)
    {
        paused = true;
        yield return new WaitForSeconds(time);
        stopwatch = true;
        paused = false;
    }
    public bool getVision()
    {
        return directVision;
    }
    public void noDirectVision()
    {
        directVision = false;
    }
    public Vector3 getActualPlayerCoords() // returns the true location of the player
    {
        return actualPlayerCoords;
    }
    public Vector3 getPlayerCoords() // returns the last seen location of the player based on enemy vision
    {
        return playerCoords;
    }
    public void GiveVisionCollision()
    {
        if (!docile)
        {
            if (!(PlayerIsHiding))
            {
                RaycastHit hit;
                if (Physics.Linecast(head.position, player.position, out hit, ~IgnoreMe))
                {
                    playerCoords = player.position;
                    if (hit.transform.tag == "Player" | hit.transform.tag == "body")
                    {
                        int stopwatchMultiplier = 1;
                        if (Vector3.Distance(enemy.position, player.position) > 8f)
                        {
                            stopwatchMultiplier = 6;
                        }
                        directVision = true;
                        if (stopwatch)
                        {
                            Debug.Log("pursuing");
                            setBehaviour("pursue");
                        }
                        else
                        {
                            if ((!paused) && (!stopwatch))
                            {
                                float waitTime = AIawareness * stopwatchMultiplier;
                                Debug.Log(waitTime);
                                StartCoroutine(wait(waitTime));
                            }
                        }
                    }
                    else
                    {
                        noDirectVision();
                        stopwatch = false;
                    }
                }
            }
        }
    }
    public void updateStep()
    {
        step++;
    }
    public Vector3 GetPosition()
    {
        return coords;
    }
    public void setBehaviour(string behaviour)
    {
        mode = behaviour;
    }
    public void setDistractionCoords(Vector3 location, bool door = false)
    {
        if (mode != "pursue" && (Vector3.Distance(location, enemy.position) <= EnemyHearingRange)) // adjust diretor's scale to adjust the hearing range of the Enemy
        {         
            dCoords = location;
            mode = "investigate";
        }
        else if ((mode != "pursue") && door)
        {
            dCoords = location;
            mode = "investigate";
        }
    }
    public string getMode()
    {
        return mode;
    }

    public void pickPatrol()
    {
        PatrolPosition closestObj;
        Vector3 currentClosest = patrols[0].transform.position;
        for (int i = 0; i < numPatrols; i ++)
        {
            if (Vector3.Distance(patrols[i].transform.position, enemy.position) < Vector3.Distance(currentClosest, enemy.position))
            {
                currentClosest = patrols[i].transform.position;
                closestObj = patrols[i].transform.GetComponent<PatrolPosition>();
            }
        }
        step = 0;
        setBehaviour("patrol");
        time = 0;
        stopwatch = false;
    }
    void LateUpdate()
    {
        if ((Time.frameCount % interval == 0) && first)
        {
            EnemyHearingRange = this.gameObject.transform.localScale.x;
            patrolOne = GameObject.Find("PatrolPointOne");
            patrolTwo = GameObject.Find("PatrolPointTwo");
            patrolThree = GameObject.Find("PatrolPointThree");
            patrolFour = GameObject.Find("PatrolPointFour");
            player = GameObject.Find("body").transform;
            patrols = new GameObject[] { patrolOne, patrolTwo, patrolThree, patrolFour };
            force = enemy.GetComponent<enemyAI>();
            enemy.gameObject.GetComponent<NavMeshAgent>().enabled = true;
            force.StartProccess(); // fix later
            mode = "patrol";
            ready = true;
            first = false;
        }
        if ((Time.frameCount % interval == 0) && !first)
        {
            actualPlayerCoords = player.position;
            if (ready)
            {
                if (mode == "patrol")    // Define the patrols
                {
  
                    len = patrols.Length;
                    if (step > len)
                    {
                        coords = patrols[0].transform.position;
                        step = 0;
                    }
                    else if (step < len)
                    {
                        coords = patrols[step].transform.position;
                    }
                    
                }
                if (mode == "investigate")     // Send the stalker to the location of the thrown object
                {
                    coords = dCoords;
                }
                if (mode == "pursue")         //if the player is out of sight, start a cool down, 10 seconds if the player is not in a hiding spot, or 3 if the player is
                {
                    float hideTime;
                    if (PlayerIsHiding)
                    {
                        hideTime = 3f; // sets how long the player must hide before the AI forgets
                    }
                    else
                    {
                        hideTime = 12f; // sets how long the player must evade eyesight before the AI forgets
                    }
                    playerCoords = player.position;
                    if (!directVision)
                    {
                        timePassed = Time.time - time;
                        if (timePassed > hideTime)
                        {
                            Vector3 currentClosest = patrols[0].transform.position;
                            for (int i = 0; i < numPatrols; i++)
                            {
                                if (Vector3.Distance(patrols[i].transform.position, enemy.position) < Vector3.Distance(currentClosest, enemy.position))
                                {
                                    currentClosest = patrols[i].transform.position;
                                }
                            }
                            step = 0;
                            setBehaviour("patrol");
                            time = 0;
                            stopwatch = false;
                        }
                    }
                    else
                    {
                        time = Time.time;
                        timePassed = 0;
                    }
                }
                
            }
        }
    }
}
