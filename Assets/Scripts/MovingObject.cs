using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2d;
    private float inverseMoveTime;

    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position; // implicit conversion
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (!hit)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }


    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrDist = (transform.position - end).sqrMagnitude;

        while (sqrDist > float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(rb2d.position, end, inverseMoveTime * Time.deltaTime);
            rb2d.MovePosition(newPos);
            sqrDist = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (!hit)
        {
            return;
        }

        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent)
            OnCantMove(hitComponent);
    }


    protected abstract void OnCantMove<T>(T component) where T : Component;

    // Use this for initialization
    void Start()
    {

    }
}
