using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    // Declare variables
    // List of cells on the frontier of the search
    private List<Cell> open;

    // List of cells that have already been visited
    private List<Cell> visited;

    // List of cells that are part of the path
    private List<Cell> path;

    // Get a reference to the game manager
    private GameManager manager;

    // Start and end points for the search
    private Cell start, end;

    // Boolean to determine if there is no way to reach the player
    private bool noPath = false;

    // Use this for initialization
    void Start ()
    {
        // Store a reference to the maze script
        manager = this.GetComponent<GameManager>();
    }

    // Public method that allows for the creation and return of a path
    public List<Cell> ReturnPath(Cell startingCell, Cell targetCell)
    {
        // Reset or initialize all variables for a new path
        ResetPathfinding();

        // Start searching for a new path
        StartSearch(startingCell, targetCell);

        return path;
    }

    // Reset the functions for pathfinding, so another path can be searched for
    private void ResetPathfinding ()
    {
        // Initialize lists
        open = new List<Cell>();
        visited = new List<Cell>();
        path = new List<Cell>();

        // Initialize search points
        start = new Cell(0, 0);
        end = new Cell(0, 0);

        // Loop through all cells in the maze, and remove all parent references
        for (int x = 0; x < manager.newMaze.xSize; x++)
        {
            for (int y = 0; y < manager.newMaze.ySize; y++)
            {
                manager.newMaze.GetCell(x, y).parent = null;
            }
        }
    }

    // Calculate the distance between two cells
    private float CalculateDistance (Cell cell1, Cell cell2)
    {
        float xd = cell1.xCoord - cell2.xCoord;
        float yd = cell1.yCoord - cell2.yCoord;

        float result = (Mathf.Sqrt((xd * xd) + (yd * yd)));
        return result;
    }

    // Calculate the value of the heuristic function for a cell
    private float CalculateFValue (Cell target)
    {
        // Set a variable that can be used to store the best connected and visited cell
        Cell checkCell = target;

        int parentCount = 0;
        // Determine the best cell which is both visited and connected
        foreach (Cell cell in target.connectedCells)
        {
            if (visited.Contains(cell))
            {
                int numParents = cell.GetNumParents();

                if (parentCount > numParents || parentCount == 0)
                {
                    parentCount = numParents;
                    checkCell = cell;
                }
            }
        }

        // Add the distance from the current cell to the target, to get the full value for G
        float travelDist = CalculateDistance(target, checkCell);

        while (checkCell.parent != null)
        {
            travelDist += CalculateDistance(checkCell, checkCell.parent);
            checkCell = checkCell.parent;
        }

        float f = travelDist + CalculateDistance(target, end);
        return f;
    }

    // Find the best cell in a list, based off of it's f value
    private Cell GetBestCell (List<Cell> chosenList, Cell current)
    {
        Cell resultCell = new Cell(0, 0);
        float functionValue = 0.0f;

        foreach (Cell cell in chosenList)
        {
            if (current != cell)
            {
                float fVal = CalculateFValue(cell);

                if (functionValue > fVal || functionValue == 0.0f)
                {
                    resultCell = cell;
                    functionValue = fVal;
                }
            }
        }

        return resultCell;
    }

    public void StartSearch (Cell startingCell, Cell targetCell)
    {
        // Pick a starting point in the maze
        start = startingCell;

        // Pick a goal point in the maze
        end = targetCell;

        SearchFunction();
    }

    // Search through the maze to find an optimum path
    private void SearchFunction ()
    {
        // Declare a variable to store the current cell, and assign it the value of the starting cell
        Cell currentCell = start;
        open.Add(start);

        // Declare a variable to store the best cell
        Cell bestCell = null;

        // Declare a boolean to track if the end has been found
        bool pathFound = false;

        // Loop through the open list to determine the best path
        while (open.Count > 0 && !pathFound)
        {
            // Check if we have reached our goal
            if (currentCell == end)
            {
                pathFound = true;
            }
            else
            {
                // Add all connected cells to the open list
                foreach (Cell cell in currentCell.connectedCells)
                {
                    if (!open.Contains(cell) && !visited.Contains(cell))
                    {
                        // If no path was found, add any adjacent cells to open
                        if (noPath)
                        {
                            open.Add(cell);
                        } else
                        {
                            // If this is the first search, only add unblocked cells to open
                            if (cell.contains != CellContents.Blocked)
                            {
                                open.Add(cell);
                            }
                        }
                    }
                }

                // Remove current cell from open, and add it to visited
                open.Remove(currentCell);
                visited.Add(currentCell);

                bestCell = GetBestCell(open, currentCell);

                // Set the parent cell to be the previous cell in the path
                int parentCount = 0;

                // Determine the best cell which is both visited and connected
                foreach (Cell cell in bestCell.connectedCells)
                {
                    if (visited.Contains(cell))
                    {
                        int numParents = cell.GetNumParents();

                        if (parentCount > numParents || parentCount == 0)
                        {
                            parentCount = numParents;
                            bestCell.parent = cell;
                        }
                    }
                }

                // If the parent of the new cell is still null, assign it to the previous cell
                if (bestCell.parent == null)
                {
                    bestCell.parent = currentCell;
                }

                // Set the current cell to best cell
                currentCell = bestCell;
            }
        }

        if (!pathFound)
        {
            noPath = true;
        } else
        {
            noPath = false;
        }

        // When a path has been found, update the path list
        CreatePath();
    }

    private void CreatePath ()
    { 
        // Add the final cell to the path
        path.Add(end);

        // Loop through from the end to the start, checking the assigned parent of each cell
        Cell checkCell = end;

        while (checkCell.parent != null)
        {
            path.Add(checkCell.parent);
            checkCell = checkCell.parent;
        }

        // When all cells have been added, reverse the path, so that start is at index 0
        path.Reverse();
    }
}