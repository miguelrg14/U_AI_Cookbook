// Miguel Rodríguez Gallego
using System.Linq;
using NUnit.Framework;

namespace AICookbook.ReinforcementLearning.Tests
{
    /// <summary>
    ///     The grid world rules and what Q-Learning and SARSA end up learning on the demo maps.
    ///     Learning is random, so each check runs with several fixed seeds.
    /// </summary>
    public class LearningTests
    {
        static TabularLearner Create(bool sarsa, GridMap map, int seed, out GridWorld world)
        {
            world = map.CreateWorld();
            TabularLearner learner = sarsa ? new Sarsa(world, seed) : (TabularLearner)new QLearning(world, seed);
            learner.Alpha = map.Alpha;
            learner.Gamma = map.Gamma;
            learner.Epsilon = map.Epsilon;
            return learner;
        }

        static void Train(TabularLearner learner, int episodes)
        {
            for (int i = 0; i < episodes; i++)
                learner.RunEpisode();
        }

        // ---------------------------------------------------------------- environment

        [Test]
        public void MovingIntoAWall_KeepsTheAgentInPlace()
        {
            var world = GridWorld.Parse("###", "#S#", "#G#");

            StepResult result = world.Step(world.Start, 0); // up, into the wall

            Assert.AreEqual(world.Start, result.State);
            Assert.AreEqual(world.MoveReward, result.Reward);
            Assert.IsFalse(result.Terminal);
        }

        [Test]
        public void ReachingTheGoal_EndsTheEpisodeWithItsReward()
        {
            var world = GridWorld.Parse("###", "#S#", "#G#");

            StepResult result = world.Step(world.Start, 2); // down

            Assert.AreEqual(world.Goal, result.State);
            Assert.AreEqual(world.GoalReward, result.Reward);
            Assert.IsTrue(result.Terminal);
        }

        [Test]
        public void FallingOffTheCliff_IsPunishedAndSendsBackToStart()
        {
            var world = GridMaps.Cliff.CreateWorld();

            StepResult result = world.Step(world.Start, 1); // right, into the cliff

            Assert.IsTrue(result.FellOffCliff);
            Assert.AreEqual(world.Start, result.State);
            Assert.AreEqual(world.CliffReward, result.Reward);
            Assert.IsFalse(result.Terminal);
        }

        [Test]
        public void ShortestPath_IsFoundByBreadthFirstSearch()
        {
            Assert.AreEqual(22, GridMaps.Maze.CreateWorld().ShortestPathLength());
            Assert.AreEqual(13, GridMaps.Cliff.CreateWorld().ShortestPathLength(), "around the cliff");
            Assert.AreEqual(-1, GridWorld.Parse("S#G").ShortestPathLength(), "walled off");
        }

        // ---------------------------------------------------------------- update rule

        [Test]
        public void QLearningUpdate_FollowsTheFormula()
        {
            // One row: from S the only possible move is right
            var world = GridWorld.Parse("S.G");
            var learner = new QLearning(world) { Alpha = 0.5f, Gamma = 0.9f, Epsilon = 0f };

            learner.BeginEpisode();
            learner.Step();

            // Q(S,right) = 0 + 0.5 * (-1 + 0.9 * 0 - 0)
            Assert.AreEqual(-0.5f, learner.Q(world.Start, 1), 1e-5f);
        }

        [Test]
        public void GoalValue_PropagatesBackToTheStart([Values(false, true)] bool sarsa)
        {
            var world = GridWorld.Parse("S...G");
            TabularLearner learner = sarsa ? new Sarsa(world, 1) : (TabularLearner)new QLearning(world, 1);
            learner.Epsilon = 0.1f;

            Train(learner, 200);

            Assert.Greater(learner.MaxQ(world.Start), 0f, "the start knows the goal is worth reaching");
            Assert.IsTrue(learner.GreedyPathReachesGoal(out int length));
            Assert.AreEqual(4, length);
        }

        // ---------------------------------------------------------------- what each algorithm learns

        [Test]
        public void QLearning_FindsTheShortestWayOutOfTheMaze([Values(0, 1, 2, 3, 4)] int seed)
        {
            var learner = Create(sarsa: false, GridMaps.Maze, seed, out var world);

            Train(learner, 300);

            Assert.IsTrue(learner.GreedyPathReachesGoal(out int length), "reaches the goal");
            Assert.AreEqual(world.ShortestPathLength(), length, "with the shortest path");
        }

        [Test]
        public void Sarsa_FindsTheWayOutOfTheMaze([Values(0, 1, 2, 3, 4)] int seed)
        {
            var learner = Create(sarsa: true, GridMaps.Maze, seed, out _);

            Train(learner, 300);

            Assert.IsTrue(learner.GreedyPathReachesGoal(out _));
        }

        [Test]
        public void QLearning_WalksAlongTheCliffEdge([Values(0, 1, 2, 3, 4)] int seed)
        {
            var learner = Create(sarsa: false, GridMaps.Cliff, seed, out _);

            Train(learner, 500);

            Assert.IsTrue(learner.GreedyPathReachesGoal(out int length));
            Assert.AreEqual(13, length, "optimal path: right next to the cliff");
        }

        /// <summary>
        ///     The classic difference: while exploring, the edge path makes Q-Learning fall now and then,
        ///     so SARSA, which learns a safer path, collects more reward during training.
        /// </summary>
        [Test]
        public void Sarsa_CollectsMoreRewardThanQLearningWhileExploringTheCliff()
        {
            float RecentReward(bool sarsa)
            {
                float total = 0f;
                for (int seed = 0; seed < 10; seed++)
                {
                    var learner = Create(sarsa, GridMaps.Cliff, seed, out _);
                    Train(learner, 500);
                    total += learner.RewardPerEpisode.Skip(300).Average();
                }
                return total / 10f;
            }

            float qLearning = RecentReward(sarsa: false);
            float sarsaReward = RecentReward(sarsa: true);

            Assert.Greater(sarsaReward, qLearning, $"SARSA {sarsaReward:F1} vs Q-Learning {qLearning:F1} per episode");
        }

        [Test]
        public void Statistics_AreRecordedForEveryEpisode()
        {
            var learner = Create(sarsa: false, GridMaps.Practice, 0, out _);

            Train(learner, 20);

            Assert.AreEqual(20, learner.Episodes);
            Assert.AreEqual(20, learner.StepsPerEpisode.Count);
            Assert.AreEqual(learner.StepsPerEpisode.Sum(), learner.TotalSteps);
        }
    }
}
