using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
    // Declare variables
    // Get a refernece to the GameManager
    private GameManager manager;

    // The speed the player will move at through the environment 
    [SerializeField] float movementSpeed = 1.0f;

    // The current position of the player
    public Cell position;

    // The target position for player movement
    public Cell target;

    // Boolan to determine whether the player has moved
    private bool playerMoved = false;

    // Boolean to determine if the player's movement animation is complete
    private bool movementComplete = true;

    // Boolean to determine if the player is initiating control input
    private bool controlInputDetected = false;

    void Start ()
    {
        // Get a reference to the game manager
        manager = Camera.main.GetComponent<GameManager>();

        // Set the colour of the player to black
        this.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
    }

    void Update()
    {
        // If the player is able to perform actions
        if (manager.GetPlayerState() && controlInputDetected)
        {
            // Detect input and move player towards a target position
            DetectMovement();

            // If the player hasn't moved
            if (!playerMoved)
            {
                // Detect if the player is placing a block
                DetectBlockPlacement();
            }
        } else
        {
            // Reset the player action booleans
            playerMoved = false;

            // Determine if an action is ready to be recorded
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                controlInputDetected = true;
            }
        }
    }

    // Return the contents of the current player's cell
    public CellContents CheckCurrentCell()
    { 
        return position.contains;
    }

    private void DetectMovement()
    {
        // Detect if any movement key has been pressed
        if (movementComplete && !Input.GetKey(KeyCode.Space)) { 
            if (Input.GetKeyUp(KeyCode.W))
            {
                if (position.yCoord != manager.newMaze.ySize - 1)
                {
                    target = manager.newMaze.GetCell(position.xCoord, position.yCoord + 1);
                    movementComplete = false;

                    // Determine that the player has moved
                    playerMoved = true;
                }
            } else if (Input.GetKeyUp(KeyCode.S))
            {
                if (position.yCoord != 0)
                {
                    target = manager.newMaze.GetCell(position.xCoord, position.yCoord - 1);
                    movementComplete = false;

                    // Determine that the player has moved
                    playerMoved = true;
                }
            } else if (Input.GetKeyUp(KeyCode.D))
            {
                if (position.xCoord != manager.newMaze.xSize - 1)
                {
                    target = manager.newMaze.GetCell(position.xCoord + 1, position.yCoord);
                    movementComplete = false;

                    // Determine that the player has moved
                    playerMoved = true;
                }
            } else if (Input.GetKeyUp(KeyCode.A))
            {
                if (position.xCoord != 0)
                {
                    target = manager.newMaze.GetCell(position.xCoord - 1, position.yCoord);
                    movementComplete = false;

                    // Determine that the player has moved
                    playerMoved = true;
                }
            }
        }

        // If the target cell is available to the player
        if (position.connectedCells.Contains(target) && target.contains != CellContents.Blocked) {
            // Move the player towards the new target location
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(target.xCoord, 0, target.yCoord), movementSpeed * Time.deltaTime);

            if (this.transform.position.x == target.xCoord && this.transform.position.z == target.yCoord)
            {
                movementComplete = true;

                // Determine that a control action has been made
                controlInputDetected = false;

                // Reset so that the player can move again
                playerMoved = false;

                // Determine that the player has taken a move
                manager.DisablePlayer();
                manager.EnableAI();

                // Get the closest cell to the player's position
                position = manager.newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);
            }
        } else
        {
            movementComplete = true;

            // Reset so that the player can move again
            playerMoved = false;


            // Get the closest cell to the player's position
            position = manager.newMaze.GetClosestCell(this.transform.position.x, this.transform.position.z);
        }
    }

    private void DetectBlockPlacement()
    {
        // Declare a cell that will be blocked
        Cell blockedCell = position;

        if (Input.GetKey(KeyCode.Space) && Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.W))
        {
            if (position.yCoord != manager.newMaze.ySize - 1)
            {
                blockedCell = manager.newMaze.GetCell(position.xCoord, position.yCoord + 1);
            }
        } else if (Input.GetKey(KeyCode.Space) && Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.S))
        {
            if (position.yCoord != 0)
            {
                blockedCell = manager.newMaze.GetCell(position.xCoord, position.yCoord - 1);
            }
        } else if (Input.GetKey(KeyCode.Space) && Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.D))
        {
            if (position.xCoord != manager.newMaze.xSize - 1)
            {
                blockedCell = manager.newMaze.GetCell(position.xCoord + 1, position.yCoord);
            }
        } else if (Input.GetKey(KeyCode.Space) && Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.Space) && Input.GetKey(KeyCode.A))
        {
            if (position.xCoord != 0)
            {
                blockedCell = manager.newMaze.GetCell(position.xCoord - 1, position.yCoord);
            }
        }

        // If the blocked cell is not it's default value, and the cell is able to be blocked
        if (blockedCell != position && position.connectedCells.Contains(blockedCell) && (blockedCell.contains != CellContents.Entrance && blockedCell.contains != CellContents.Exit))
        {
            // Block the cell
            blockedCell.contains = CellContents.Blocked;

            // Display that the cell is blocked
            blockedCell.cellFloor.GetComponent<Renderer>().material.color = Color.black;

            // Determine that a control action has been made
            controlInputDetected = false;

            // Determine that the player has taken a move
            manager.DisablePlayer();
            manager.EnableAI();
        } else if (blockedCell != position && !position.connectedCells.Contains(blockedCell))
        {
            // Allow the player to break through walls
            position.connectedCells.Add(blockedCell);
            blockedCell.connectedCells.Add(position);

            // Destroy the walls between these two cells
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    Destroy(GameObject.Find("Cell:(" + position.xCoord + ", " + position.yCoord + ") TO Cell:(" + blockedCell.xCoord + ", " + blockedCell.yCoord + ")"));
                } else
                {
                    Destroy(GameObject.Find("Cell:(" + blockedCell.xCoord + ", " + blockedCell.yCoord + ") TO Cell:(" + position.xCoord + ", " + position.yCoord + ")"));
                }
            }

            // Determine that a control action has been made
            controlInputDetected = false;

            // Determine that the player has taken a move
            manager.DisablePlayer();
            manager.EnableAI();
        }
    }
}
