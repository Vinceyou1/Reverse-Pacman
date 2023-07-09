using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    public AnimatedSprite deathSequence;
    public SpriteRenderer spriteRenderer { get; private set; }
    public new Collider2D collider { get; private set; }
    public Movement movement { get; private set; }
    public string difficulty;
    public Ghost[] ghosts;
    public Transform pellets;
    public Tilemap pelletTiles;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        movement = GetComponent<Movement>();
        difficulty = PlayerPrefs.GetString("difficulty");
    }

    private void Update()
    {
        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        if(difficulty.Equals("Hard"))
        {
            foreach(Ghost g in ghosts)
            {
                if (g.isFrightened) continue;
                Vector3 ghost_position = g.transform.position;
                Vector3 pacman_position = transform.position;
                if ((ghost_position - pacman_position).magnitude >= 8) continue;
                if(ghost_position.x == pacman_position.x && movement.direction.x == 0)
                {
                    Vector3 forward = pacman_position + new Vector3(0, movement.direction.y, 0);
                    Vector3 backward = pacman_position + new Vector3(0, -movement.direction.y, 0);
                    if((backward - ghost_position).magnitude > (forward - ghost_position).magnitude) {
                        movement.SetDirection(new Vector2(0, -movement.direction.y));
                    }
                    return;
                }
                if (ghost_position.y == pacman_position.y && movement.direction.y == 0)
                {
                    Vector3 forward = pacman_position + new Vector3(movement.direction.x, 0, 0);
                    Vector3 backward = pacman_position + new Vector3(-movement.direction.x, 0, 0);
                    if ((backward - ghost_position).magnitude > (forward - ghost_position).magnitude)
                    {
                        movement.SetDirection(new Vector2(-movement.direction.x, 0));
                    }
                    return;
                }
            }
        }
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        deathSequence.spriteRenderer.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.spriteRenderer.enabled = true;
        deathSequence.Restart();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();
        if (node == null) return;
        if (difficulty == "Easy")
        {
            moveRandom(node);
        }
        else if (difficulty == "Medium")
        {
            Ghost closestGhost = null;
            float mindistance = float.MaxValue;
            foreach (Ghost g in ghosts)
            {
                if (g.atHome) continue;
                float distance = (g.transform.position - transform.position).magnitude;
                if (distance < mindistance)
                {
                    closestGhost = g;
                    mindistance = distance;
                }
            }

            if (closestGhost == null || mindistance >= 8) { moveRandom(node); }
            else { 
                Vector2 direction = Vector2.zero;
                float maxDistance = 0;
                // Find the available direction that moves away from ghosts
                foreach (Vector2 availableDirection in node.availableDirections)
                {
                    // If the distance in this direction is less than the current
                    // min distance then this direction becomes the new closest
                    Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                    float distance = (closestGhost.transform.position - newPosition).sqrMagnitude;

                    if (distance > maxDistance)
                    {
                        direction = availableDirection;
                        maxDistance = distance;
                    }
                }
                movement.SetDirection(direction);
            }
        }
        else {
            Ghost closestFrightenedGhost = null;
            float mindistance = float.MaxValue;
            Ghost closestGhost = null;
            float nonmindistance = float.MaxValue;
            foreach (Ghost g in ghosts)
            {
                if (g.atHome) continue;
                float distance = (g.transform.position - transform.position).magnitude;
                if (g.isFrightened && distance < mindistance)
                {
                    closestFrightenedGhost = g;
                    mindistance = distance;
                }
                if (!g.isFrightened && distance < nonmindistance)
                {
                    closestGhost = g;
                    nonmindistance = distance;
                }
            }
            if (closestFrightenedGhost != null)
            {
                if (closestGhost == null ||
                    (closestFrightenedGhost.transform.position - transform.position).magnitude <
                    (closestGhost.transform.position - transform.position).magnitude)
                {
                    moveTowards(closestFrightenedGhost.transform.position, node);
                    return;
                }
            }

            if(closestGhost != null)
            {
                if (nonmindistance <= 8) {
                    Vector2 direction = Vector2.zero;
                    float maxDistance = 0;
                    // Find the available direction that moves away from ghosts
                    foreach (Vector2 availableDirection in node.availableDirections)
                    {
                        // If the distance in this direction is less than the current
                        // min distance then this direction becomes the new closest
                        Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                        float distance = (closestGhost.transform.position - newPosition).sqrMagnitude;

                        if (distance > maxDistance)
                        {
                            direction = availableDirection;
                            maxDistance = distance;
                        }
                    }
                    movement.SetDirection(direction);
                    return;
                }
                // I tried to do power pellet stuff, couldn't figure out how to use GameObject to determine type
                //if(mindistance <= 10)
                //{
                //    Pellet power = null;
                //    mindistance = float.MaxValue;
                //    for(int x = pelletTiles.cellBounds.min.x; x < pelletTiles.cellBounds.max.x; x++)
                //    {
                //        for(int y = pelletTiles.cellBounds.min.y; y < pelletTiles.cellBounds.max.y; y++)
                //        {
                //            Object p = pelletTiles.GetInstantiatedObject(new Vector3Int(x, y, 0));

                //        }
                //    }
                //    foreach(Pellet location in pellets)
                //    {
                //        pelletTiles.
                        
                //        if(p.points == 50 && p.gameObject.activeSelf)
                //        {
                //            float distance = (transform.position - p.transform.position).magnitude;
                //            if(distance < mindistance)
                //            {
                //                mindistance= distance;
                //                power = p;
                //            }
                //        }
                //    }
                //    if(power != null)
                //    {
                //        moveTowards(power.transform.position, node);
                //        return;
                //    }
                //}
            }
            
            // Collect Pellets
            foreach(Vector2 availableDirection in node.availableDirections)
            {
                Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                foreach (Transform p in pellets)
                {
                    if(p.gameObject.activeSelf && p.gameObject.transform.position.Equals(newPosition))
                    {
                        movement.SetDirection(availableDirection); return;
                    }
                }
            }
            // if all else fails, move randomly
            moveRandom(node);
        }
    }

    private void moveRandom(Node node) {
        // Pick a random available direction
        int index = Random.Range(0, node.availableDirections.Count);

        // Prefer not to go back the same direction so increment the index to
        // the next available direction
        if (node.availableDirections.Count > 1 && node.availableDirections[index] == -movement.direction)
        {
            index++;

            // Wrap the index back around if overflowed
            if (index >= node.availableDirections.Count)
            {
                index = 0;
            }
        }

        movement.SetDirection(node.availableDirections[index]);
    }

    private void moveTowards(Vector3 position, Node node)
    {
        Vector2 direction = Vector2.zero;
        float mindistance = float.MaxValue;
        // Find the available direction that moves away from ghosts
        foreach (Vector2 availableDirection in node.availableDirections)
        {
            // If the distance in this direction is less than the current
            // min distance then this direction becomes the new closest
            Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
            float distance = (position - newPosition).sqrMagnitude;

            if (distance < mindistance)
            {
                direction = availableDirection;
                mindistance = distance;
            }
        }
        movement.SetDirection(direction);
    }
}
