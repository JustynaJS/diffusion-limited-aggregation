using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid3D : MonoBehaviour
{
    public int width, height, depth;
    float unitSize = 1.0f;
    float density = 0.005f;

    // array of boxes in grid
    GameObject[,,] grid;

    // introduce one unit in grid at Unity
    public GameObject gridUnit;

    private float update;
    float color;

    // Use this for initialization
    void Start()
    {
        grid = new GameObject[width, height, depth];
        SeedDLA(grid);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // Randomly colour 10% of boxes for blue
                    if (Random.Range(0.0f, 1.0f) < density)
                    {
                        InstantiateParticle(x, y, z, grid);
                        ActivateParticle(x, y, z, grid);
                    }
                }
            }
        }
    }

    void Update()
    {
        update += Time.deltaTime;
        color += Time.deltaTime/100;

        if (color > 1.0f) color = 0.0f;

        if (update > 0.15f)
        {
            update = 0.0f;
            MoveParticles();
        }
    }

    void MoveParticles()
    {
        GameObject[,,] _tempGrid = new GameObject[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                
                    if (grid[x, y, z] == null)
                    {
                        continue;
                    }
                    
                    Particle boxScript = grid[x, y, z].GetComponent<Particle>();

                    // If particle is already part of DLA, do not run any function for it, it is already perfect
                    if(boxScript.state == State.dla)
                    {
                        _tempGrid[x, y, z] = grid[x, y, z];
                    }
                    // for every active particle, find next random step
                    if (boxScript.state == State.active)
                    {
                        // return -1, 0 or 1
                        int xPos = GetMovementWithProbabity(0.5f, 0.75f);
                        int yPos = GetMovementWithProbabity(0.5f, 0.75f);
                        int zPos = GetRandomMovement();

                        //check boundary conditions
                        if (x == 0 && xPos == -1) xPos = width - 1;
                        else if (x == width - 1 && xPos == 1) xPos = -width + 1;

                        if (y == 0 && yPos == -1) yPos = height - 1;
                        else if (y == height - 1 && yPos == 1) yPos = -height + 1;

                        if (z == 0 && zPos == -1) zPos = depth - 1;
                        else if (z == depth - 1 && zPos == 1) zPos = -depth + 1;

                        int newXPos = x + xPos;
                        int newYPos = y + yPos;
                        int newZPos = z + zPos;

                        InstantiateParticle(newXPos, newYPos, newZPos, _tempGrid);
                        ActivateParticle(newXPos, newYPos, newZPos, _tempGrid);

                        // We check if particle is on a border of area, if yes: it jumps to the other side of area
                        if ((newXPos < width - 1) && (newXPos > 0) && (newYPos < height - 1) && (newYPos > 0) && (newZPos < depth - 1) && (newZPos > 0))
                        {
                            // Check if any particle is next to DLA particle, if yes : add it to DLA Structure
                            if (ParticleHasDLANeighbour(newXPos, newYPos, newZPos, _tempGrid))
                            {
                                AddParticleToDLA(newXPos, newYPos, newZPos, _tempGrid);
                                SetColor(newXPos, newYPos, newZPos, _tempGrid);
                            }
                        }
                        Destroy(grid[x, y, z]);
                    }
;
                }
            }
        }
        grid = _tempGrid;
    }

    void SeedDLA(GameObject[,,] gridToAddElement)
    {
        var oneParticle = Instantiate(gridUnit, this.transform);
        Vector3 initialPosition = new Vector3((int)(width / 2 * unitSize), (int)(height / 2 * unitSize), (int)(depth / 2 * unitSize));
        oneParticle.transform.position = initialPosition;

        //set particlet state to DLA
        Particle seedScript = oneParticle.GetComponent<Particle>();
        seedScript.state = State.dla;
        seedScript.name = "DLA Particle - SEED";

        gridToAddElement[(int)(width / 2), (int)(height / 2), (int)(depth / 2)] = oneParticle;

    }

    void InstantiateParticle(int x, int y, int z, GameObject[,,] gridToAddElement)
    {
        var oneParticle = Instantiate(gridUnit, this.transform);
        Vector3 initialPosition = new Vector3(x * unitSize, y * unitSize, z * unitSize);
        oneParticle.transform.position = initialPosition;
        gridToAddElement[x, y, z] = oneParticle;
    }

    void ActivateParticle(int x, int y, int z, GameObject[,,] currentGrid)
    {
        Particle boxScript = currentGrid[x, y, z].GetComponent<Particle>();
        boxScript.state = State.active;
        boxScript.name = "Active Particle";
    }

    void SetColor(int x, int y, int z, GameObject[,,] currentGrid)
    {
        //set particlet state to DLA
        Particle boxScript = currentGrid[x, y, z].GetComponent<Particle>();
        boxScript.myColor = new Color(color/5, color, color);
        
    }

    void AddParticleToDLA(int x, int y, int z, GameObject[,,] currentGrid)
    {

        // destroy existing particle
        Destroy(currentGrid[x, y, z]);

        // new particle
        var oneParticle = Instantiate(gridUnit, this.transform);
        Vector3 initialPosition = new Vector3(x, y, z);
        oneParticle.transform.position = initialPosition;
        currentGrid[x, y, z] = oneParticle;
        
        
        //set particlet state to DLA
        Particle boxScript = currentGrid[x, y, z].GetComponent<Particle>();
        boxScript.state = State.dla;
        boxScript.name = "DLA Particle";
    }



    bool ParticleHasDLANeighbour(int x, int y, int z, GameObject[,,] currentGrid)
    {
        bool IfHasDLANeighbour = false;
        for (int i = x - 1; i < x + 2; i++)
        {
            for (int j = y -1; j < y + 2; j++)
            {
                for (int k = z - 1; k < z + 2; k++)
                {
                    if (currentGrid[i,j,k] == null)
                    {
                        continue;
                    }

                    Particle neighbourScript = currentGrid[i,j,k].GetComponent<Particle>();
                    if (neighbourScript.state == State.dla)
                    {
                        IfHasDLANeighbour = true;
                    }
                    
                }
            }
        }
        return IfHasDLANeighbour;
    }

    int GetRandomMovement()
    {
        float tempRandom = Random.Range(0.0f, 1.0f);

        if (tempRandom < 0.33f) return -1;
        else if (tempRandom >= 0.33f && tempRandom <= 0.66f) return 0;
        else return 1;
    }

    int GetMovementWithProbabity(float borderBetwenBackandStay, float borderBetweenStayandForward)
    {
        float tempRandom = Random.Range(0.0f, 1.0f);

        if (tempRandom < borderBetwenBackandStay) return -1;
        else if (tempRandom >= borderBetwenBackandStay && tempRandom <= borderBetweenStayandForward) return 0;
        else return 1;
    }

    GameObject[,,] Copy3DList(GameObject[,,] listToCopy)
    {
        GameObject[,,] copy3DList = new GameObject[width, height, depth];

        for (int x = 0; x < listToCopy.GetLength(0); x++)
        {
            for (int y = 0; y < listToCopy.GetLength(1); y++)
            {
                for (int z = 0; z < listToCopy.GetLength(2); z++)
                {
                    copy3DList[x, y, z] = listToCopy[x, y, z];
                }
            }
        }
        return copy3DList;
    }

}
