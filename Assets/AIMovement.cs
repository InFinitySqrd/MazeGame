using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIMovement : MonoBehaviour {
    // Declare variables
    // List of cells in the path that this AI will be following
    private Cell newTargetCell;

    // Reference to the pathfinding script
    Pathfinding pathScript;

    // Reference to the game manager script
    GameManager manager;

    // Current target point in space
    private Vector3 targetPoint;

    // Number value assigned to the AI
    public int AINumber;

    // Store the distance to the target, based off of the length of the calculated path
    public int distance = 0;

    // Movement speed of the AI unit
    [SerializeField] float movementSpeed = 1.0f;

	// Use this for initialization
	void Start () {
        // Get a reference to the pathfinding script
        pathScript = Camera.main.GetComponent<Pathfinding>();

        // Get a reference to the maze generating script
        manager = Camera.main.GetComponent<GameManager>();

        // Initialize the movement path of the unit
        newTargetCell = manager.newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);

        // Colour the AI
        Color AIColour = new Color(Random.value, Random.value, Random.value);
        this.GetComponent<Renderer>().material.color = AIColour;
	}

    public bool AtTarget ()
    {
        if ((this.transform.position.x == newTargetCell.xCoord && this.transform.position.z == newTargetCell.yCoord) || newTargetCell.occupantNumber != AINumber)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Move along the path towards the end goal
    public void FollowPath ()
    {
        if (newTargetCell.occupantNumber == AINumber)
        {
            // Move towards the next point every frame
            targetPoint = new Vector3(newTargetCell.xCoord, 0, newTargetCell.yCoord);
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPoint, movementSpeed * Time.deltaTime);
        }
    }

    // Request a new point from the pathfinding script
    public void GetNewPath(Cell target)
    {
        // Get a new path
        List<Cell> newPath = pathScript.ReturnPath(manager.newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z), target);

        // Store the distance of the discovered path
        distance = newPath.Count;

        // If there is some distance to travel to the target, then travel
        if (newPath.Count > 1)
        {
            // Set the target cell to be on the next path
            newTargetCell = newPath[1];

            // If the target cell is blocked, change it to empty
            if (newTargetCell.contains == CellContents.Blocked)
            {
                newTargetCell.contains = CellContents.Empty;
                newTargetCell.cellFloor.GetComponent<Renderer>().material.color = Color.white;
            }

            // Set the next cell to be occupied by this AI
            if (newTargetCell.contains != CellContents.Entrance && newTargetCell.contains != CellContents.Exit)
            {
                newTargetCell.contains = CellContents.Occupied;
            }
            newTargetCell.occupantNumber = AINumber;

            // If the current cell is occupied, by the current AI, set it to empty
            Cell closest = manager.newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);
            if (closest.contains != CellContents.Exit && closest.contains != CellContents.Entrance)
            {
                closest.contains = CellContents.Empty;
            }
            closest.occupantNumber = -1;
        } else
        {
            // If there is no distance to the player, damage can be dealt
        }
    }
}
