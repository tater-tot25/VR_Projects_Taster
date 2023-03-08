using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class generateGrid : MonoBehaviour
{
    public int pathTileVariations;
    public int emptyTileVariations;

    public string[][] descriptorList;
    public Transform mazeGenerator;
    generateMaze maze;
    private int size;
    public float spawnOffset;
    public Vector3 middle = Vector3.zero;
    public GameObject[] emptys;
    public GameObject[] dead_ends;
    public GameObject[] straights;
    public GameObject[] l_turns;
    public GameObject[] three_ways;
    public GameObject[] cross_sects;
    public GameObject[] starts;
    public GameObject[] ends;
    public GameObject[] structures;
    int structNum = 0;

    // spawn a tile based of the spawnNum referencing the spawn list
    void decideAndSpawn(int spawnNum, Vector3 spawnPos)
    {
        int pick;
        if (descriptorList[spawnNum][0] == "EMPTY") // decide the randomness of the tile picker
        {
            pick = Random.Range(0, emptyTileVariations);
        }
        else
        {
            pick = Random.Range(0, pathTileVariations);
        }

        GameObject tile; 
        if (descriptorList[spawnNum][0] == "START")
        {
            tile = ends[0];
        }
        else if (descriptorList[spawnNum][0] == "END")
        {
            tile = starts[0];
        }
        else if (descriptorList[spawnNum][0] == "STRUCTURE")
        {
            tile = structures[structNum];
            if (structNum == 2)
            {
                structNum = 0;
            }
            else
            {
                structNum++;
            }
        }
        else if (descriptorList[spawnNum][0] == "DEAD_END") // decide which tile to use
        {
            tile = dead_ends[pick];
        }
        else if (descriptorList[spawnNum][0] == "STRAIGHT")
        {
            tile = straights[pick];
        }
        else if (descriptorList[spawnNum][0] == "L_TURN")
        {
            tile = l_turns[pick];
        }
        else if (descriptorList[spawnNum][0] == "THREE_WAY")
        {
            tile = three_ways[pick];
        }
        else if (descriptorList[spawnNum][0] == "CROSS_SECT")
        {
            tile = cross_sects[pick];
        }
        else
        {
            tile = emptys[pick];
        }
        float rotation = float.Parse(descriptorList[spawnNum][1]); //Decide tile rotation
        rotation += 180;
        int[] angles = { 0, 90, 180, 270 };
        if (descriptorList[spawnNum][0] == "EMPTY" || descriptorList[spawnNum][0] == "CROSS_SECT")
        {
            int randAngle = Random.Range(0, 4);
            rotation += angles[randAngle];
        }
        else if (descriptorList[spawnNum][0] == "STRAIGHT")
        {
            angles[1] = 0;
            angles[3] = 180;
            int randAngle = Random.Range(0, 4);
            rotation += angles[randAngle];
        }
        GameObject generatedTile = Instantiate(tile, spawnPos, Quaternion.Euler(0f, rotation, 0f)); //spawn the tile
    }
    private void startGeneration()
    {
        int count = 0; // count keeps track of which tile is being spawned
        for (int x = 0; x < size; x ++)
        {
            for (int z = 0; z < size; z ++)
            {
                Vector3 spawnSpot = new Vector3(x * spawnOffset, 0, z * spawnOffset) + middle;
                decideAndSpawn(count, spawnSpot);
                count ++; 
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        maze = mazeGenerator.GetComponent<generateMaze>();
        descriptorList = maze.getMazeDescription();
        size = maze.getSize();
        startGeneration();
    }
}
