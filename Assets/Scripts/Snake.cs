using System.Collections;
using System.Collections.Generic; // necessary to define list
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

// Snake requires the GameObject to have a BoxCollider2D component
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Snake : MonoBehaviour
{
    // snake position and movement variables
    public float speed = 8f;
    public float speedMultiplier = 1f;
    public Vector2Int direction = Vector2Int.up;
    protected Vector2Int input = Vector2Int.zero; 
    public Vector2 startPosition; // starting snake position

    // snake segment variables
    public Transform segmentPrefab; // snake body segments
    public int initialSize = 4; // snake length at start
    protected Color myColor;
    private Transform mySegment; // create a copy of the base segment prefab
    protected List<Transform> segments = new List<Transform>();

    // manage calculation update rate
    private float deltaTime;
    private float nextUpdate;

    // track score
    public int pointCounter = 0;
    public int pointPenalty = 0;
    public int lifeCounter = 3; // max number of lives
    public string screenName = "Green";
    public bool gameActive = true; // set to false if game is over

     // manage invincibility frames and GameController
    private bool isInvulnerable = false;
    private GameController GameController;
    private GridArea gridArea;

    protected virtual void Awake()
    {
        GameController = FindObjectOfType<GameController>();
        gridArea = FindObjectOfType<GridArea>();
    }

    private void Start()
    // initialise snake with head
    {
        // initialise start position with inspector value
        // ensure rounding to int
        startPosition = new Vector2(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        
        SetDeltaTime();

        myColor = GetComponent<SpriteRenderer>().color;

        ResetState();
    }

    private void Update()
    // Update() is called every frame
    {
        input = GetInput();
    }

    protected void SetDeltaTime() {
        deltaTime = 1f / (speed * speedMultiplier);
        nextUpdate = Time.time + deltaTime;
    }
    
    protected virtual Vector2Int GetInput() 
    // Enable different derived classes to get input differently
    {
        // only allow y movement if snake is pointing in x direction
        if (direction.x != 0) 
        {
            if (Input.GetKeyDown(KeyCode.W)) {
                input = Vector2Int.up;
            } else if (Input.GetKeyDown(KeyCode.S)) {
                input = Vector2Int.down;
            }
        } 
        // only allow x movement if snake is pointing in y direction
        else if (direction.y != 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) {
                input = Vector2Int.left;
            } else if (Input.GetKeyDown(KeyCode.D)) {
                input = Vector2Int.right;
            }
        }

        return input;
    }

    private void FixedUpdate()
    // FixedUpdate() is called at fixed time intervals
    // frame-rate independent
    // important for physics (don't use Update())
    {
  
         // Wait until the next update before proceeding
        if (Time.time < nextUpdate) {
            return;
        }

        // Set the new direction based on the input
        if (input != Vector2Int.zero) {
            direction = input;
        }

        Move(direction);

         // Set the next update time based on the speed
        nextUpdate = Time.time + deltaTime;       
    }   

    private void Move(Vector2Int direction)
    {
        // add tail location to gridArea.openPositions list, unless snake is growing
        if (segments[^1].position != segments[^2].position) {
            gridArea.AddOpenPosition(segments[^1].position);
        }

        // move each segment from tail to head
        for (int i = segments.Count - 1; i > 0; i--){
            segments[i].position = segments[i - 1].position;
        }

        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y = Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = new Vector2(x, y);

        // remove new head location from gridArea.openPositions list
        gridArea.RemoveOpenPosition(transform.position);
    }


    protected virtual void ResetState()
    {
        for (int i = 0; i < segments.Count; i++) {
            // make occupied positions available
            gridArea.AddOpenPosition(segments[i].position);
            if (i >0) {
                // destroy segments apart from head
                Destroy(segments[i].gameObject);
            }
        }

        segments.Clear();
        segments.Add(transform); // add snake head

        // make invulnerable on reset
        StartCoroutine(OnInvulnerable());

        for (int i = 1; i < initialSize; i++) {
            mySegment = Instantiate(segmentPrefab, startPosition, Quaternion.identity);
            mySegment.gameObject.GetComponent<SpriteRenderer>().color = myColor;
            segments.Add(mySegment);
        }

        transform.position = startPosition;

        gridArea.RemoveOpenPosition(startPosition);
    }

    public bool Occupies(int x, int y)
    // check if a position is occupied by a snake segment
    {
        foreach (Transform segment in segments)
        {
            if (Mathf.RoundToInt(segment.position.x) == x &&
                Mathf.RoundToInt(segment.position.y) == y) {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    // actions when collision occurs
    {
        if (gameActive) {
            if (other.CompareTag("Food")) {
                Grow(other);
                pointCounter += other.gameObject.GetComponent<Food>().points;
            }

            if (other.CompareTag("Obstacle") && isInvulnerable == false) {
                ResetState();
                pointCounter -= pointPenalty;
                lifeCounter -= 1;

                if (lifeCounter == 0) {
                    Destroy(this.GameObject());
                    GameController.GameOver();
                }
                else {
                    StartCoroutine(OnInvulnerable());
                }
            }
        }
    }

    private IEnumerator OnInvulnerable()
    // coroutine to set period of invulnerability after collision
    {
        int penalty = pointPenalty; 
        isInvulnerable = true;
        pointPenalty = 0;

        // set invulnerability duration
        yield return new WaitForSeconds(0.5f); 

        pointPenalty = penalty;
        isInvulnerable = false;
    }

    protected virtual void Grow(Collider2D food)
    // food is an argument for derived classes
    {
        // Instantiate new segment at snake tail position
        Transform segment = Instantiate(segmentPrefab, segments[^1].position, Quaternion.identity);
        segments.Add(segment);
    }

}
 