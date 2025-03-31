using UnityEngine;

public enum TestMode
{
    Physics,
    Lerp,
    MoveTowards,
    SmoothDamp,
    Rigidbody
}

public class PhysicsMoveTowardsExample : MonoBehaviour
{
    public TestMode mode;

    public float acceleration = 5;
    public float lerpSpeed = 2f;
    public float moveTowardsSpeed = 5f;
    public float smoothDampSpeed = 2f;

    private float target;
    private float velocity;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                target = hit.point.x;
            }
        }

        if (mode == TestMode.Rigidbody)
            return;

        var position = transform.position;
        switch (mode)
        {
            case TestMode.Physics:
                position.x = PhysicsMoveTowards.MoveTowards(position.x, target, ref velocity, acceleration);
                break;
            case TestMode.MoveTowards:
                position.x = Mathf.MoveTowards(position.x, target, Time.deltaTime * moveTowardsSpeed);
                break;
            case TestMode.Lerp:
                position.x = Mathf.Lerp(position.x, target, Time.deltaTime * lerpSpeed);
                break;
            case TestMode.SmoothDamp:
                position.x = Mathf.SmoothDamp(position.x, target, ref velocity, smoothDampSpeed);
                break;
        }

        transform.position = position;
    }

    private void FixedUpdate()
    {
        if (mode != TestMode.Rigidbody)
            return;

        var rb = GetComponent<Rigidbody>();
        var force = PhysicsMoveTowards.MoveTowardsForce(rb.position.x, target, rb.velocity.x, acceleration, Time.fixedDeltaTime);
        rb.AddForce(force, 0, 0, ForceMode.Acceleration);
    }
}
