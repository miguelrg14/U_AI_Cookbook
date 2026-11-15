// Miguel Rodríguez Gallego
using NUnit.Framework;
using UnityEngine;

namespace AICookbook.Shared.Tests
{
    /// <summary>
    ///     Velocity produced by <see cref="AgentMotor"/>: direction on the ground plane and speed limit.
    /// </summary>
    public class AgentMotorTests
    {
        Rigidbody body;

        [SetUp]
        public void SetUp()
        {
            body = new GameObject("Agent").AddComponent<Rigidbody>();
            body.useGravity = false;
            body.position = body.transform.position = Vector3.zero;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(body.gameObject);

        [Test]
        public void MoveTowards_GoesToTheTarget_OnTheGroundPlane()
        {
            AgentMotor.MoveTowards(body, new Vector3(10f, 5f, 0f), speed: 100f, maxSpeed: 50f, rotationSpeed: 360f);

            Vector3 velocity = body.linearVelocity;
            Assert.AreEqual(0f, velocity.y, 1e-4f, "never moves vertically");
            Assert.Greater(velocity.x, 0f);
            Assert.AreEqual(0f, velocity.z, 1e-4f);
        }

        [Test]
        public void MoveAwayFrom_GoesInTheOppositeDirection()
        {
            AgentMotor.MoveAwayFrom(body, new Vector3(0f, 0f, 10f), speed: 100f, maxSpeed: 50f, rotationSpeed: 360f);

            Assert.Less(body.linearVelocity.z, 0f);
            Assert.AreEqual(0f, body.linearVelocity.x, 1e-4f);
        }

        [Test]
        public void Speed_IsClampedToMaxSpeed()
        {
            AgentMotor.MoveTowards(body, new Vector3(10f, 0f, 0f), speed: 100000f, maxSpeed: 10f, rotationSpeed: 360f);

            Assert.AreEqual(10f, body.linearVelocity.magnitude, 1e-3f);
        }

        [Test]
        public void TargetOnTopOfTheAgent_StopsIt()
        {
            body.linearVelocity = Vector3.one;

            AgentMotor.MoveTowards(body, Vector3.zero, speed: 100f, maxSpeed: 10f, rotationSpeed: 360f);

            Assert.AreEqual(Vector3.zero, body.linearVelocity);
        }
    }
}
