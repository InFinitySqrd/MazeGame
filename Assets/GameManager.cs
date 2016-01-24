using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Class for storing information about previous mazes
public class MazeInfo
{
    // Store the seed number
    private int seedNum;

    // Store the heirarchy as a string
    private string treePos;

    // Constructor for the info storage class
    public MazeInfo(int seed, string tree)
    {
        seedNum = seed;
        treePos = tree;
    }

    // Get the seed number
    public int GetSeed ()
    {
        return seedNum;
    }

    // Get the tree string
    public string GetTree()
    {
        return treePos;
    }
} 

public class Maze
{
    // Declare variables
    // Variable to store the current state of the grid
    public Cell[,] mazeArray;

    // Variable to store the entrance and exit(s) for the maze
    public Cell entrance;
    public List<Cell> exit;

    // Variable to determine the size of the maze
    public int xSize = 0, ySize = 0;

    // Constructor for the maze class
    public Maze ()
    {
        // Initialize the maze array
        mazeArray = new Cell[xSize, ySize];

        // Initialize the list of exit cells
        exit = new List<Cell>();
    }

    // Return a random cell in the maze
    public Cell GetRandomCell ()
    {
        return mazeArray[Random.Range(0, xSize - 1), Random.Range(0, ySize - 1)];
    }

    // Return a cell based off of coordinates
    public Cell GetCell (int x, int y)
    {
        return mazeArray[x, y];
    }

    // Get the closest cell to a set of coordinates
    public Cell GetClosestCell(float xPos, float yPos)
    {
        int x = (int)Mathf.Round(xPos);
        int y = (int)Mathf.Round(yPos);

        return GetCell(x, y);
    }
}

public class GameManager : MonoBehaviour {
    // Declare variables
    // Variable to store the maze
    public Maze newMaze;

    // Variable to store the current level of the maze
    public int level = 0;

    // Variable to store the AI game object that needs to be spawned
    [SerializeField] GameObject AIUnit;

    // Variable to determine how many AI should be created
    [SerializeField] int maxAI = 3;

    // Variable to determine how many exits should be created
    private int numExits;

    // Array of all AI units in the game
    private AIMovement[] AIUnitArray;

    // Variable to track the player's position
    private GameObject player;

    // Variable to store the exit that the player left in
    private int exitNumber;

    // List of information about the mazes
    private List<MazeInfo> infoList;

    // The indexer for the list, updated every time a new maze is created
    private int treeIndex = 0;

    // Variable to access the player's script
    private PlayerMovement playerScript;

    // Variables to determine the X and Y sizes of the maze
    [SerializeField] int xLength = 1, yHeight = 1;

    // Determines the mininum distance AI can be spawned at
    [SerializeField] float spawnDistance = 2.0f;

    // Reference to the Generate Maze script
    private GenerateMaze createMaze;

    // Boolean to determine whether AI movement can be started
    private bool AIMovePhase = false;

    // Boolean to determine whether player movement is allowed
    private bool playerMovePhase = true;

    // Determine if the player has moved yet this level
    private Cell startingCell;
    private bool playerMoved = false;

    // Use this for initialization
    void Start () {
        // Get a reference to the GenerateMaze script
        createMaze = this.GetComponent<GenerateMaze>();

        // Get a reference to the player
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerMovement>();

        // Initialize the maze info list
        infoList = new List<MazeInfo>();

        // Build a new maze
        BuildMaze(true);

        // Set the position of the player
        SetPlayerPosition(true);

        // Spawn all the required AI and store them
        SpawnAI();
    }

    // Publicly accessible method to create new mazes and dispose of existing ones
    public void CreateNewMaze (bool isNewMaze)
    {
        // Remove all components from the current maze
        ClearCurrentMaze();

        // Build a new maze
        BuildMaze(isNewMaze);

        // Set the position of the player
        SetPlayerPosition(isNewMaze);

        // Spawn all the required AI and store them
        SpawnAI();

        // Reset the move counter
        playerMoved = false;

        // Set the game to the player's turn
        AIMovePhase = false;
        playerMovePhase = true;
    }

    private void ClearCurrentMaze ()
    {
        // Destroy the floor of the maze
        Destroy(GameObject.Find("Wall Parent"));

        // Destroy the walls of the maze
        Destroy(GameObject.Find("Floor Parent"));

        // Destroy all AI in the maze
        GameObject[] ai = GameObject.FindGameObjectsWithTag("AIUnit");

        foreach (GameObject obj in ai)
        {
            Destroy(obj);
        }
    }

    // Method to calculate a function, based off of parameters & the level count
    private int ExponentialFunction (float stretch, float minValue)
    {
        // Stretch the sqrt formula by a factor, add the square of the min value
        return Mathf.FloorToInt(Mathf.Sqrt((stretch * level) + (minValue * minValue)));
    }

    // Method to update the tree string, based off of the exit that was previously taken
    private string UpdateTreeString(string tree)
    {
        switch (exitNumber)
        {
            case 0:
                tree += "A";
                break;
            case 1:
                tree += "B";
                break;
            case 2:
                tree += "C";
                break;
            case 3:
                tree += "D";
                break;
            case 4:
                tree += "E";
                break;
            default:
                tree += "F";
                Debug.LogError("TOO MANY EXITS");
                break;
        }

        return tree;
    }

    private int UpdateExitNumber (char lastTreeLetter)
    {
        switch (lastTreeLetter)
        {
            case 'A':
                return 0;
            case 'B':
                return 1;
            case 'C':
                return 2;
            case 'D':
                return 3;
            case 'E':
                return 4;
            default:
                Debug.LogError("TOO MANY EXITS");
                return 5;
        }
    }

    // Find the seed value of a maze, given it's tree string
    private int FindExistingSeed(List<MazeInfo> list, string target)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetTree() == target)
            {
                // Set the tree index for the correct list item
                treeIndex = i;

                return list[i].GetSeed();             
            }
        }

        // If no appropriate item was found, the index will be the length of the list
        treeIndex = list.Count;

        return -1;
    }

    // Method to create a maze and set its values
    private void BuildMaze(bool isNewMaze)
    {
        // The current position in the level tree
        string tree = "";

        // Variable to store the seed being used for this level
        int seed = 0;

        if (isNewMaze)
        {
            // If this is the first maze
            if (level == 0)
            {
                // For the first level, set the tree to be A
                tree = "A";

                // If this tree value does not exist, generate a random seed
                seed = FindExistingSeed(infoList, tree);

                if (seed == -1)
                {
                    seed = Random.Range(0, 999999);

                    // Add this info to the info list
                    infoList.Add(new MazeInfo(seed, tree));
                }

                // Set the tree index to be 0
                treeIndex = 0;
            } else
            {
                // If this is a new maze further down
                // Determine the new value for tree
                tree = infoList[treeIndex].GetTree();

                // Add a letter to the end of tree, based off of the exit that was taken
                tree = UpdateTreeString(tree);

                // If this tree value does not exist, generate a random seed
                seed = FindExistingSeed(infoList, tree);

                if (seed == -1)
                {
                    seed = Random.Range(0, 999999) + exitNumber;

                    // Add this info to the info list
                    infoList.Add(new MazeInfo(seed, tree));
                }
            }
        } else
        {
            // If this is an old maze
            // Determine the new value for tree
            tree = infoList[treeIndex].GetTree();

            // Update the exit number with the value taken from the tree string
            exitNumber = UpdateExitNumber(tree[tree.Length - 1]);

            // Remove the last character to find the parent
            tree = tree.Remove(tree.Length - 1);

            // If this tree value does not exist, generate a random seed
            seed = FindExistingSeed(infoList, tree);

            if (seed == -1)
            {
                seed = Random.Range(0, 999999);

                // Add this info to the info list
                infoList.Add(new MazeInfo(seed, tree));
            }
        }

        // Set a new seed for the random number generator
        Random.seed = seed;

        // Create a new maze
        newMaze = new Maze();

        // Calculate the x and y values for the maze
        newMaze.xSize = Random.Range(ExponentialFunction(4, 8), ExponentialFunction(20, 8));
        newMaze.ySize = Random.Range(ExponentialFunction(4, 8), ExponentialFunction(20, 8));

        // Calculate the number of AI units for the maze
        maxAI = ExponentialFunction(3, 1);

        // Calculate the number of exits for the maze
        numExits = ExponentialFunction(0.5f, 2); 

        // Generate a new maze and store it
        newMaze.mazeArray = createMaze.CreateMaze(newMaze.xSize, newMaze.ySize);

        // Set the entrance and exit for the maze
        newMaze.entrance = newMaze.GetRandomCell();

        // If this is a new maze, set the new parent to be the previous maze and allow entrances
        if (level != 0)
        {
            newMaze.entrance.contains = CellContents.Entrance;
            newMaze.entrance.cellFloor.GetComponent<Renderer>().material.color = Color.blue;
        }

        // Set the exits
        for (int i = 0; i < numExits; i++)
        {
            Cell newExit = new Cell(0, 0);

            do
            {
                newExit = newMaze.GetRandomCell();
            } while (newExit.contains == CellContents.Entrance);

            // Add the new exit to the list
            newMaze.exit.Add(newExit);

            // Determine that the cell is an exit
            newMaze.exit[i].contains = CellContents.Exit;
            newMaze.exit[i].cellFloor.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    // Set a random position for the player to start at
    private void SetPlayerPosition (bool isNewMaze)
    {
        // Set the starting position for the player
        if (isNewMaze)
        {
            startingCell = newMaze.entrance;
        } else
        {
            startingCell = newMaze.exit[exitNumber];
        }

        player.transform.position = new Vector3(startingCell.xCoord, 0, startingCell.yCoord);

        // Intiate the first movement target
        playerScript.target = newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);

        // Intiate the first movement starting position
        playerScript.position = newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);
    }

    // Method to create AI objects
    private void SpawnAI()
    {
        // Initialize the array of AI Units
        AIUnitArray = new AIMovement[maxAI];

        int xDist, yDist;

        // Check that the spawn dead zone still allows for AI to be spawned
        if (player.transform.position.x > xLength - (int)player.transform.position.x)
        {
            xDist = (int)player.transform.position.x;
        } else
        {
            xDist = xLength - (int)player.transform.position.x;
        }

        if (player.transform.position.z > yHeight - (int)player.transform.position.z)
        {
            yDist = (int)player.transform.position.z;
        }
        else
        {
            yDist = yHeight - (int)player.transform.position.z;
        }

        // Create spawn points until the point is far enough from the player
        if (!(spawnDistance > xDist && spawnDistance > yDist))
        {
            // Spawn all the required AI and store them
            for (int i = 0; i < maxAI; i++)
            {
                Vector3 spawnPoint = new Vector3(0, 0, 0);

                do
                {
                    spawnPoint = new Vector3(Random.Range(0, newMaze.xSize - 1), 0, Random.Range(0, newMaze.ySize - 1));
                }
                while (Vector3.Distance(spawnPoint, player.transform.position) < spawnDistance 
                       && newMaze.GetClosestCell(spawnPoint.x, spawnPoint.z).contains != CellContents.Occupied 
                       && newMaze.GetClosestCell(spawnPoint.x, spawnPoint.z).contains != CellContents.Exit);

                // Set the spawned point to be an occupied cell
                newMaze.GetClosestCell(spawnPoint.x, spawnPoint.z).contains = CellContents.Occupied;

                // Spawn the AI
                GameObject AI = (GameObject)Instantiate(AIUnit, spawnPoint, Quaternion.identity);
                AIUnitArray[i] = AI.GetComponent<AIMovement>();
                AIUnitArray[i].AINumber = i + 1;
            }
        }
        else
        {
            Debug.LogError("SpawnDistance too large");
        }
    }

    // Runs once per frame
    void Update ()
    {

        // Check to see if the player has moved
        if (!playerMoved && (player.transform.position.x != startingCell.xCoord || player.transform.position.z != startingCell.yCoord))
        {
            playerMoved = true;
        }

        if (AIMovePhase)
        {
            if (playerScript.CheckCurrentCell() == CellContents.Exit && playerMoved)
            {
                // Increase the level number
                level++;

                // Determine the exit number
                Cell closest = newMaze.GetClosestCell(player.transform.position.x, player.transform.position.z);
                exitNumber = newMaze.exit.FindIndex(x => x == closest);

                // If the player has landed on the exit, move to the next maze
                CreateNewMaze(true);
            } else if (playerScript.CheckCurrentCell() == CellContents.Entrance && playerMoved)
            {
                // Decrease the level
                level--;

                // If the player has returned to the entrance, return to the previous maze
                CreateNewMaze(false);
            }
            else 
            {
                // Set a target for each unit, and create a new path
                for (int i = 0; i < maxAI; i++)
                {
                    AIUnitArray[i].GetNewPath(newMaze.GetClosestCell(player.transform.position.x, player.transform.position.z));
                }

                // Prevent the AI from running more than one movement phase
                AIMovePhase = false;

                // Run the coroutine to animate movement of all units
                StartCoroutine(MoveAIUnits());
            }

        }
    }

    public void EnableAI ()
    {
        AIMovePhase = true;
    }

    public void DisablePlayer ()
    {
        playerMovePhase = false;
    }

    public bool GetPlayerState ()
    {
        return playerMovePhase;
    }

    // Sort the AI array so it works from the closest target to the furthest
    private void SortAIArray ()
    {
        AIMovement temp;

        for (int i = 0; i < maxAI - 1; i++)
        {
            if (AIUnitArray[i].distance > AIUnitArray[i + 1].distance)
            {
                temp = AIUnitArray[i];
                AIUnitArray[i] = AIUnitArray[i + 1];
                AIUnitArray[i + 1] = temp;
            }
        }
    }

    // Move each AI unit, one at a time
    IEnumerator MoveAIUnits()
    {
        // Sort the array of AI to make sure that the closest target is calculated first
        SortAIArray();

        // Loop through all units, and make one move per frame
        for (int i = 0; i < maxAI; i++)
        {
            while (!AIUnitArray[i].AtTarget())
            {
                AIUnitArray[i].FollowPath();
                
                yield return null;
            }
        }
        
        playerMovePhase = true;
    }
}
