using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Declare objects
// Enumerated type to determine whether a cell contains some object
public enum CellContents
{
    Empty,
    Occupied,
    Blocked,
    Entrance,
    Exit
};

// Class to hold information about each cell in the maze
public class Cell
{
    // Lists to contain relevant information about adjacent cells in the maze
    public List<Cell> adjacentCells;
    public List<Cell> connectedCells;
    
    // Coordinates of the cell
    public int xCoord, yCoord;

    // The contents held in the current cell
    public CellContents contains = CellContents.Empty;

    public int occupantNumber = -1;     // 0 = Player, 1+ = AI

    // The floor of the cell in the maze
    public GameObject cellFloor;

    // Parent cell to track the order of the path
    public Cell parent = null;

    // Constructor for the cell class
    public Cell (int x, int y)
    {
        adjacentCells = new List<Cell>();
        connectedCells = new List<Cell>();

        xCoord = x;
        yCoord = y;
    }

    // Build a visualisation of the floor of the maze
    public GameObject CreateFloor (GameObject floor, GameObject parent)
    {
        cellFloor = (GameObject)Object.Instantiate(floor, new Vector3(xCoord, 0, yCoord), Quaternion.Euler(new Vector3(90, 0, 0)));
        cellFloor.transform.parent = parent.transform;
        cellFloor.name = ("Cell:(" + xCoord + ", " + yCoord + ")");
        return cellFloor;
    }

    public int GetNumParents ()
    {
        int parentCount = 0;
        Cell checkCell = this;

        while (checkCell.parent != null)
        {
            parentCount++;
            checkCell = checkCell.parent;
        }

        return parentCount;
    }
}

public class GenerateMaze : MonoBehaviour
{
    // Declare variables
    // Variable to determine the percentage chance of a wall being randomly broken
    [SerializeField] float wallBreakChance = 0.1f;

    // Variable to store the current state of the grid
    private Cell[,] maze;

    // Variable to determine the size of the maze
    private int gridSizeX = 0, gridSizeY = 0;

    // Variable to determine whether the maze has finished generating or not
    public bool mazeFinished = false;

    // List of cells that are already a part of the maze
    private List<Cell> inMaze;

    // List of cells that are currently part of the frontier
    private List<Cell> inFrontier;

    // Variables to store 3D objects used for maze generation
    private GameObject xWall, yWall, floor;

    // Variables to act as parents of all created floors and walls
    private GameObject wallParent, floorParent;

    // Begin the construction of a maze
    public Cell[,] CreateMaze (int xSize, int ySize)
    {
        // Initialize the x and y coords for this script
        gridSizeX = xSize;
        gridSizeY = ySize;

        // Initialize the maze
        maze = new Cell[gridSizeX, gridSizeY];

        // Create parents for the maze visuals
        wallParent = new GameObject();
        wallParent.name = "Wall Parent";
        floorParent = new GameObject();
        floorParent.name = "Floor Parent";

        // Generate objects for use as reference
        xWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // Set proper dimensions for walls
        xWall.transform.localScale = new Vector3(1.0f, 2.0f, 0.1f);
        yWall.transform.localScale = new Vector3(0.1f, 2.0f, 1.0f);


        // Set the position of the camera
        Camera.main.transform.position = new Vector3((gridSizeX / 2.0f) - 0.5f, gridSizeY + 1, (gridSizeY / 2.0f) - 0.5f);

        // Initialize the in and frontier lists
        inMaze = new List<Cell>();
        inFrontier = new List<Cell>();

        // Begin making the maze
        InitializeMaze();
        Generate();
        BuildMaze();

        return maze;
    }

    // Return a random cell in the maze
    public Cell GetRandomCell ()
    {
        return maze[Random.Range(0, gridSizeX - 1), Random.Range(0, gridSizeY - 1)];
    }

    // Return a cell based off of coordinates
    public Cell GetCell(int x, int y)
    {
        return maze[x, y];
    }

    // Determine which cells are adjacent to the current cells
    private List<Cell> GetAdjacentCells (int xCoord, int yCoord)
    {
        // Declare private variable
        List<Cell> cellList = new List<Cell>();

        // Determine the list of adjacent cells
        if (xCoord > 0)
        {
            cellList.Add(maze[xCoord - 1, yCoord]);
        }

        if (xCoord < gridSizeX - 1)
        {
            cellList.Add(maze[xCoord + 1, yCoord]);
        }

        if (yCoord > 0)
        {
            cellList.Add(maze[xCoord, yCoord - 1]);
        }

        if (yCoord < gridSizeY - 1)
        {
            cellList.Add(maze[xCoord, yCoord + 1]);
        }

        return cellList;
    }

    // Expand the frontier when a new cell is added to the maze
    private void ExpandFrontier (Cell cell)
    {
        foreach (Cell adjacents in cell.adjacentCells)
        {
            if (!inFrontier.Contains(adjacents) && !inMaze.Contains(adjacents))
            {
                inFrontier.Add(adjacents);
            }
        }
    }

    // Find an adjacent cell that is already in the maze
    private List<Cell> GetInMaze (Cell cell)
    {
        List<Cell> returnCell = new List<Cell>();

        foreach (Cell adjacents in cell.adjacentCells)
        {
            if (inMaze.Contains(adjacents))
            {
                returnCell.Add(adjacents);
            }
        }

        return returnCell;
    }

    // Determine which walls will be randomly broken to allow for additional paths through the maze
    private void BreakRandomWalls ()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if ((maze[x,y].connectedCells.Count < maze[x,y].adjacentCells.Count / 2) && (Random.value <= wallBreakChance))
                {
                    Cell newConnection = maze[x, y].adjacentCells[Random.Range(0, maze[x, y].adjacentCells.Count - 1)];

                    while (maze[x,y].connectedCells.Contains(newConnection))
                    {
                        newConnection = maze[x, y].adjacentCells[Random.Range(0, maze[x, y].adjacentCells.Count - 1)];
                    }

                    maze[x, y].connectedCells.Add(newConnection);
                    newConnection.connectedCells.Add(maze[x, y]);
                }
            }
        }
    }

    // Cleanup method when the maze is finished
    private void MazeComplete ()
    {
        // Remove the original objects used to create the laze
        Destroy(xWall);
        Destroy(yWall);
        Destroy(floor);

        mazeFinished = true;
    }

    // Loop through the maze array and initialize all cells within it
    private void InitializeMaze ()
    {
        // Add all cells to the maze
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Create new instance of a cell for each point in the maze
                maze[x, y] = new Cell(x, y);
                maze[x, y].CreateFloor(floor, floorParent);
            }
        }

        // Update cells with their adjacencies
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Add adjacent cells to the current cell
                maze[x, y].adjacentCells = GetAdjacentCells(x, y);
            }
        }
    }

    // Method to check all cells until the maze is complete
    private void Generate ()
    {
        // Variable to store the current cell being worked on
        Cell currentCell = new Cell(0, 0);

        // Variable to store the next cell to be checked
        Cell nextCell = new Cell(0, 0);

        // Select a random cell in the maze
        int randomCellX = Random.Range(0, gridSizeX);
        int randomCellY = Random.Range(0, gridSizeY);

        // Pick an adjacent cell to create a path to
        currentCell = maze[randomCellX, randomCellY];
        inMaze.Add(currentCell);
        ExpandFrontier(currentCell);

        while (inFrontier.Count > 0)
        {
            // Determine which of the adjacent cells will be the next cell
            nextCell = inFrontier[Random.Range(0, inFrontier.Count - 1)];

            // Record which adjacent cell is already in the maze
            List<Cell> allIn = GetInMaze(nextCell);
            currentCell = allIn[Random.Range(0, allIn.Count)];

            // Connect these two adjacent cells
            currentCell.connectedCells.Add(nextCell);
            nextCell.connectedCells.Add(currentCell);

            // Add the next cell to the maze
            inMaze.Add(nextCell);

            // Expand the frontier to include all adjacent cells not already in the maze
            ExpandFrontier(nextCell);
            inFrontier.Remove(nextCell);
        }

        // Prevent a perfect maze by breaking walls
        BreakRandomWalls();
    }

    // Generate the maze in 3D
    private void BuildMaze ()
    {
        // Array of GameObjects to store all walls for a certain grid
        bool[] buildWall = new bool[4];

        // Loop through all cells and generate a visual output
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Initially assume that all walls are required
                for (int i = 0; i < buildWall.Length; i++)
                {
                    buildWall[i] = true;
                }

                // Temporary variable to store the walls when they are created
                GameObject tempWall;

                // Loop through all cells and determine which walls need to be built
                foreach (Cell cell in maze[x, y].connectedCells)
                {
                    // Determine difference between the coordinates of current cell and connected cell
                    int xDifference, yDifference;
                    xDifference = x - cell.xCoord;
                    yDifference = y - cell.yCoord;

                    // Destroy walls depending on the position of the connection
                    if (xDifference > 0)
                    {
                        buildWall[0] = false;
                    }
                    else if (xDifference < 0)
                    {
                        buildWall[1] = false;
                    }

                    if (yDifference > 0)
                    {
                        buildWall[2] = false;
                    }
                    else if (yDifference < 0)
                    {
                        buildWall[3] = false;
                    }
                }

                // Build walls that are required for this cell
                if (buildWall[0])
                {
                    tempWall = (GameObject)Instantiate(yWall, new Vector3(x - 0.5f, 0, y), Quaternion.identity);
                    tempWall.transform.parent = wallParent.transform;
                    tempWall.name = ("Cell:(" + x + ", " + y + ") TO Cell:(" + (x - 1) + ", " + y + ")");
                }

                if (buildWall[1])
                {
                    tempWall = (GameObject)Instantiate(yWall, new Vector3(x + 0.5f, 0, y), Quaternion.identity);
                    tempWall.transform.parent = wallParent.transform;
                    tempWall.name = ("Cell:(" + x + ", " + y + ") TO Cell:(" + (x + 1) + ", " + y +")");
                }
                
                if (buildWall[2])
                {
                    tempWall = (GameObject)Instantiate(xWall, new Vector3(x, 0, y - 0.5f), Quaternion.identity);
                    tempWall.transform.parent = wallParent.transform;
                    tempWall.name = ("Cell:(" + x + ", " + y + ") TO Cell:(" + x + ", " + (y - 1) + ")");
                }

                if (buildWall[3])
                {
                    tempWall = (GameObject)Instantiate(xWall, new Vector3(x, 0, y + 0.5f), Quaternion.identity);
                    tempWall.transform.parent = wallParent.transform;
                    tempWall.name = ("Cell:(" + x + ", " + y + ") TO Cell:(" + x + ", " + (y + 1) + ")");
                }
            }
        }

        // Construction of the maze is complete
        MazeComplete();
    }

}