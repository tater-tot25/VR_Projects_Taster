using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Interactions.SectorInteraction;

public class enemyAI : MonoBehaviour
{
    Director directions;
    public NavMeshAgent agent;
    public string mode;
    bool paused = false;
    bool running = false;
    Vector3 prevPos;
    string scene;
    int currentScene;
    public bool ready = false;
    float seconds = 0;
    private int interval = 6;
    Vector3 coords;
    public Transform player;
    public bool caught = false;
    private bool first = true;
    public GameObject monitor;
    TestFace face;


    // Update is called once per frame
    void Update()
    {
        if (first)
        {
            player = GameObject.Find("body").transform;
            face = monitor.GetComponent<TestFace>();
            first = false;
        }
    }
    public void setMode(string setting)
    {
        mode = setting;
    }
    public IEnumerator wait(float time)
    {
        paused = true;
        running = true;
        yield return new WaitForSeconds(time);
        paused = false;
    }
    public void clockMath()
    {
        if ((mode == "investigate") | (mode == "goto"))
        {
            if (this.transform.position == prevPos)
            {
                directions.setBehaviour("patrol");
            }
        }
        prevPos = this.transform.position;
    }
    public void StartProccess()
    { 
        seconds = 0;
        directions = GameObject.Find("TheDirector").GetComponent<Director>();
        scene = SceneManager.GetActiveScene().name;
        currentScene = SceneManager.GetActiveScene().buildIndex;
        ready = true;
        directions.setBehaviour("patrol");
        mode = "patrol";
    }
    public IEnumerator restart()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(currentScene);
    }
    void LateUpdate()
    {
        if ((Time.frameCount % interval == 0) && !first)
        {          
            if (Time.time > seconds)
            {
                seconds += 2.5f;
                clockMath();
            }
            if (ready)
            {
                mode = directions.getMode();
                coords = directions.GetPosition();
                Vector3 selfCoords = transform.position;
                if (!(paused))
                {
                    if (mode != "pursue")
                    {
                        agent.SetDestination(coords);
                    }
                }
                if (mode == "patrol")
                {
                    face.setpassive();
                    if (Vector3.Distance(coords, selfCoords) < 0.75f)
                    {
                        if (!(running))
                        {
                            StartCoroutine(wait(0.1f));
                        }
                        if (paused)
                        {
                            agent.SetDestination(selfCoords);
                        }
                        else
                        {
                            directions.updateStep();
                            running = false;
                        }
                    }
                }
                if (mode == "investigate")
                {
                    face.setinvestigate();
                    if (Vector3.Distance(coords, selfCoords) < 1.25f)
                    {
                        if (!(running))
                        {
                            StartCoroutine(wait(2f));
                        }
                        if (paused)
                        {
                            agent.SetDestination(selfCoords);
                        }
                        else
                        {
                            running = false;                          
                            directions.pickPatrol();
                        }
                    }
                }
                if (mode == "pursue")
                {
                    face.setpursue();
                    agent.SetDestination(player.position);
                    if (Vector3.Distance(player.position, selfCoords) < 1.8f)
                    {
                        if (!(directions.getHide()))
                        {
                            StaticVar.qeuedLevel = currentScene;
                            StaticVar.levelComplete = true;
                            caught = true;
                            StartCoroutine(restart());
                        }
                    }
                }                             
                if (mode == "goto")
                {
                    agent.SetDestination(directions.GetPosition());
                    if (Vector3.Distance(directions.GetPosition(), selfCoords) < 1.25f)
                    {         
                        directions.setBehaviour("patrol");
                        mode = "patrol";
                        face.setpassive();
                    }
                }              
            }
        }
    }
}
