using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    private Vector2 moveInput;

    private void Start()
    {
        // Give each player a random color
        GetComponent<SpriteRenderer>().color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
    }

    // Called by the new Input System when Move is performed
    public void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    void Update()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
