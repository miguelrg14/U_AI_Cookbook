// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Movement helpers shared by the agents: move on the ground plane with a rigidbody
    ///     and turn to face where they go.
    /// </summary>
    public static class AgentMotor
    {
        /// <summary>
        ///     Moves towards <paramref name="targetPosition"/>
        /// </summary>
        public static void MoveTowards(Rigidbody rb, Vector3 targetPosition, float speed, float maxSpeed, float rotationSpeed)
            => Move(rb, targetPosition - rb.position, speed, maxSpeed, rotationSpeed);

        /// <summary>
        ///     Moves in the opposite direction of <paramref name="threatPosition"/>
        /// </summary>
        public static void MoveAwayFrom(Rigidbody rb, Vector3 threatPosition, float speed, float maxSpeed, float rotationSpeed)
            => Move(rb, rb.position - threatPosition, speed, maxSpeed, rotationSpeed);

        /// <summary>
        ///     Moves along <paramref name="direction"/> (ignoring height) and faces it
        /// </summary>
        public static void Move(Rigidbody rb, Vector3 direction, float speed, float maxSpeed, float rotationSpeed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }
            direction.Normalize();

            Face(rb.transform, direction, rotationSpeed);
            rb.linearVelocity = Vector3.ClampMagnitude(direction * speed * Time.fixedDeltaTime, maxSpeed);
        }

        /// <summary>
        ///     Turns on the Y axis towards <paramref name="direction"/> at <paramref name="rotationSpeed"/> degrees per second
        /// </summary>
        public static void Face(Transform transform, Vector3 direction, float rotationSpeed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        ///     Turns towards a world position
        /// </summary>
        public static void FacePosition(Transform transform, Vector3 position, float rotationSpeed)
            => Face(transform, position - transform.position, rotationSpeed);
    }
}
