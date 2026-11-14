// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Idle routine used by every agent when it has nothing better to do:
    ///     wait, walk forward, wait, turn, and start again with new random times.
    ///     Call <see cref="Tick"/> every frame while the agent is idle.
    /// </summary>
    public class WanderRoutine
    {
        /// <summary>
        ///     Time spent in the current routine; controls which part of it is played
        /// </summary>
        float timer;

        // Randomly generated at the start of each routine (seconds from its start)
        int walkWait;
        int walkTime;
        int rotateWait;
        int rotationTime;
        float rotationQuantity;

        public void Tick(Rigidbody rb, float movementSpeed)
        {
            Transform transform = rb.transform;

            if (timer <= 0)
                StartNewRoutine(rb);

            timer += Time.deltaTime;

            if (timer <= walkWait)
                return;

            if (timer <= walkTime)
            {
                Vector3 velocity = transform.forward * movementSpeed * Time.fixedDeltaTime;
                velocity.y = 0;
                rb.linearVelocity = velocity;
                return;
            }

            if (timer <= rotateWait)
                return;

            if (timer <= rotationTime)
            {
                transform.Rotate(transform.up * Time.deltaTime * rotationQuantity);
                return;
            }

            timer = 0;
        }

        /// <summary>
        ///     Next <see cref="Tick"/> starts a new routine (e.g. after fleeing)
        /// </summary>
        public void Restart() => timer = 0;

        void StartNewRoutine(Rigidbody rb)
        {
            walkWait = Random.Range(1, 3);
            walkTime = walkWait + Random.Range(0, 3);
            rotateWait = walkTime + Random.Range(0, 3);
            rotationTime = rotateWait + Random.Range(0, 3);
            rotationQuantity = Random.Range(-180, 180);

            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
