
using System.Collections.Generic; // necessary to define list
using UnityEngine;

public class Snake_2 : MonoBehaviour
{
    private Vector2 _direction = Vector2.up;
    // a Transform is like an object
    private List<Transform> _segments = new List<Transform>();
    public Transform segmentPrefab;
    public int initialSize = 4;

    private Vector3 _startPosition;

    private void Start()
    // initialise snake with head
    {
        // initialise start position with inspector value
        // ensure rounding to int
        _startPosition = new Vector3(
            Mathf.Round(this.transform.position.x),
            Mathf.Round(this.transform.position.y),
            Mathf.Round(this.transform.position.z)
        );
        ResetState();
    }

    private void Update()
    // Update() is called every frame
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            _direction = Vector2.up;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            _direction = Vector2.down;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            _direction = Vector2.left;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            _direction = Vector2.right;
        }
    }

    private void FixedUpdate()
    // FixedUpdate() is called at fixed time intervals
    // frame-rate independent
    // important for physics (don't use Update())
    {
        // move each segment from tail to head
        for (int i = _segments.Count - 1; i > 0; i--){
            _segments[i].position = _segments[i - 1].position;
        }

        
        // 3D vector for position, even in 2D game
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + _direction.x,
            Mathf.Round(this.transform.position.y) + _direction.y,
            0.0f
        );
    }   

    private void Grow()
    {
        // clone prefab asset
        Transform segment = Instantiate(this.segmentPrefab);
        // set position of new segment to the current snake tail
        segment.position = _segments[_segments.Count - 1].position;
        _segments.Add(segment);
    }

    private void ResetState()
    {
        // destroy snake body elements
        for (int i = 1; i < _segments.Count; i++) {
            Destroy(_segments[i].gameObject);
        }
        // need to remove body elements from list as well
        _segments.Clear();
        // add the head element back after clearing the full list
        _segments.Add(this.transform);

        for (int i = 1; i < this.initialSize; i++) {
            _segments.Add(Instantiate(this.segmentPrefab));
        }
        // reset location to initial position
        this.transform.position = _startPosition;
    }

    // actions when collision occurs
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food") {
            Grow();
        }

        if (other.tag == "Obstacle") {
            ResetState();
        }
    }

}
 