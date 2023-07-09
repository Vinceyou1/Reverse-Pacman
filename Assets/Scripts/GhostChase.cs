using UnityEngine;

public class GhostChase : GhostBehavior
{
    private void OnEnable()
    {
        if (PlayerPrefs.GetString("difficulty") == "Hard") {
            Disable();
        }
    }

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (ghost.selected) return;
        Node node = other.GetComponent<Node>();

        if(node != null && node.Equals(previousNode))
        {
            return;
        }
        previousNode = node;
        // Do nothing while the ghost is frightened
        if (node != null && enabled && !ghost.frightened.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;

            // Find the available direction that moves closet to pacman
            foreach (Vector2 availableDirection in node.availableDirections)
            {
                // If the distance in this direction is less than the current
                // min distance then this direction becomes the new closest
                Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                float distance = (ghost.target.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }

            ghost.movement.SetDirection(direction);
        }
    }

}
