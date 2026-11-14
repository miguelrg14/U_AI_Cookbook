// Miguel Rodríguez Gallego
using NUnit.Framework;
using UnityEngine;

namespace AICookbook.Shared.Tests
{
    /// <summary>
    ///     Detection ranges and hysteresis of <see cref="TwoRangeTargetDetector"/>, using real colliders
    ///     and physics queries (they work in Edit Mode after Physics.SyncTransforms).
    /// </summary>
    public class TargetDetectorTests
    {
        const int TargetLayer = 31;

        /// <summary> Exposes the serialized settings so each test can configure the detector </summary>
        class TestDetector : TwoRangeTargetDetector
        {
            public bool ignoreSelf;
            protected override bool IgnoreSelf => ignoreSelf;

            public void Configure(int seeRange, int closeRange, float exitMargin)
            {
                targetLayerMask = 1 << TargetLayer;
                seeTargetRange = seeRange;
                closeTargetRange = closeRange;
                closeRangeExitMargin = exitMargin;
            }
        }

        TestDetector detector;
        GameObject target;

        [SetUp]
        public void SetUp()
        {
            detector = new GameObject("Detector").AddComponent<TestDetector>();
            detector.Configure(seeRange: 15, closeRange: 10, exitMargin: 3f);
            target = CreateTarget("Target");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(detector.gameObject);
            Object.DestroyImmediate(target);
        }

        static GameObject CreateTarget(string name)
        {
            var go = new GameObject(name) { layer = TargetLayer };
            go.AddComponent<SphereCollider>().radius = 0.1f;
            return go;
        }

        void PlaceTargetAt(float distance)
        {
            target.transform.position = new Vector3(distance, 0f, 0f);
            Physics.SyncTransforms();
            detector.FindTarget();
        }

        [Test]
        public void TargetOutsideSeeRange_IsNotDetected()
        {
            PlaceTargetAt(20f);

            Assert.IsNull(detector.target);
            Assert.IsNull(detector.CloseTarget);
        }

        [Test]
        public void TargetInsideSeeRange_IsSeenButNotClose()
        {
            PlaceTargetAt(12f);

            Assert.AreSame(target, detector.target);
            Assert.IsNull(detector.CloseTarget);
        }

        [Test]
        public void TargetInsideCloseRange_IsSeenAndClose()
        {
            PlaceTargetAt(9f);

            Assert.AreSame(target, detector.target);
            Assert.AreSame(target, detector.CloseTarget);
        }

        [Test]
        public void ClosestOfSeveralTargets_IsChosen()
        {
            var nearer = CreateTarget("Nearer");
            nearer.transform.position = new Vector3(0f, 0f, 5f);
            try
            {
                PlaceTargetAt(8f);
                Assert.AreSame(nearer, detector.target);
                Assert.AreSame(nearer, detector.CloseTarget);
            }
            finally
            {
                Object.DestroyImmediate(nearer);
            }
        }

        [Test]
        public void CloseTarget_IsKeptInsideExitMargin_AndLostBeyondIt()
        {
            PlaceTargetAt(9f);
            Assert.AreSame(target, detector.CloseTarget, "enters the close range");

            PlaceTargetAt(12f);
            Assert.AreSame(target, detector.CloseTarget, "between 10 and 13 it is still close (hysteresis)");

            PlaceTargetAt(14f);
            Assert.IsNull(detector.CloseTarget, "beyond 13 it is no longer close");

            PlaceTargetAt(12f);
            Assert.IsNull(detector.CloseTarget, "coming back it must enter below 10 again");
        }

        [Test]
        public void DisabledTarget_IsDroppedImmediately()
        {
            PlaceTargetAt(9f);
            target.SetActive(false);
            Physics.SyncTransforms();
            detector.FindTarget();

            Assert.IsNull(detector.CloseTarget);
            Assert.IsNull(detector.target);
        }

        [Test]
        public void DetectorOnTargetLayer_IgnoresItselfOnlyWhenAsked()
        {
            Object.DestroyImmediate(target);
            target = new GameObject("Unused");
            detector.gameObject.layer = TargetLayer;
            detector.gameObject.AddComponent<SphereCollider>().radius = 0.1f;
            Physics.SyncTransforms();

            detector.ignoreSelf = false;
            detector.FindTarget();
            Assert.AreSame(detector.gameObject, detector.target, "without IgnoreSelf it detects itself");

            detector.ignoreSelf = true;
            detector.FindTarget();
            Assert.IsNull(detector.target, "with IgnoreSelf it skips itself");
        }
    }
}
