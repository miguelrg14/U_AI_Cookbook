// Miguel Rodríguez Gallego
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace AICookbook.GeneticAlgorithms.Tests
{
    /// <summary>
    ///     Tests of the genetic algorithm without the scene: fitness is computed here as the distance
    ///     from each individual's genes to a known ideal shot, instead of actually shooting.
    /// </summary>
    public class GeneticAlgorithmTests
    {
        const int PopulationSize = 10;

        // Valid gene ranges of a random individual
        const float MinX = 0f, MaxX = 90f;
        const float MinY = -45f, MaxY = 45f;
        const float MinStrength = 0f, MaxStrength = 12f;

        [SetUp]
        public void SetUp() => Random.InitState(1234); // reproducible runs

        static GeneticAlgorithm Create(int generations = 5,
                                       Selections selection = Selections.ranking,
                                       Crossovers crossover = Crossovers.real_uniform,
                                       Mutations mutation = Mutations.uniform)
            => new GeneticAlgorithm(generations, PopulationSize, selection, crossover, mutation);

        /// <summary> Lower is better, like the distance to the target in the demo </summary>
        static float DistanceToIdealShot(Individual individual)
            => Mathf.Abs(individual.xDegree - 45f) + Mathf.Abs(individual.yDegree - 0f) + Mathf.Abs(individual.strength - 6f) * 5f;

        static void AssertGenesInRange(Individual individual)
        {
            Assert.That(individual.xDegree, Is.InRange(MinX, MaxX), "xDegree");
            Assert.That(individual.yDegree, Is.InRange(MinY, MaxY), "yDegree");
            Assert.That(individual.strength, Is.InRange(MinStrength, MaxStrength), "strength");
        }

        // ---------------------------------------------------------------- population

        [Test]
        public void RandomPopulation_HasRequestedSize_AndValidGenes()
        {
            var ga = Create();

            Assert.AreEqual(PopulationSize, ga.Population.Count);
            foreach (var individual in ga.Population)
                AssertGenesInRange(individual);
        }

        [Test]
        public void GetFittest_ReturnsLowestFitness()
        {
            var ga = Create();
            for (int i = 0; i < ga.Population.Count; i++)
                ga.Population[i].fitness = 100 - i * 7; // last one is the best
            Individual best = ga.Population.Last();

            Assert.AreSame(best, ga.GetFittest());
        }

        // ---------------------------------------------------------------- selection

        [Test]
        public void RankingSelection_PicksTheTwoBest()
        {
            var ga = Create();
            for (int i = 0; i < ga.Population.Count; i++)
                ga.Population[i].fitness = Random.Range(0f, 100f);
            var expected = ga.Population.OrderBy(individual => individual.fitness).Take(2).ToList();

            ga.Population.Sort(); // the algorithm sorts at the end of every generation
            ga.Selection_Ranking();

            Assert.AreSame(expected[0], ga.ind1);
            Assert.AreSame(expected[1], ga.ind2);
        }

        [Test]
        public void TournamentSelection_PicksParentsFromThePopulation()
        {
            var ga = Create();

            for (int run = 0; run < 50; run++)
            {
                ga.Selection_Tournament();
                CollectionAssert.Contains(ga.Population, ga.ind1);
                CollectionAssert.Contains(ga.Population, ga.ind2);
            }
        }

        // ---------------------------------------------------------------- crossover

        [Test]
        public void UniformCrossover_SwapsGenes_ReplacingTheWorstTwo()
        {
            var ga = Create();
            ga.ind1 = new Individual(10f, 20f, 3f);
            ga.ind2 = new Individual(80f, -30f, 9f);

            ga.Crossover_Real_Uniform();

            Assert.AreEqual(PopulationSize, ga.Population.Count);
            Individual child1 = ga.Population[PopulationSize - 2];
            Individual child2 = ga.Population[PopulationSize - 1];
            Assert.AreEqual((10f, -30f, 9f), (child1.xDegree, child1.yDegree, child1.strength));
            Assert.AreEqual((80f, 20f, 3f), (child2.xDegree, child2.yDegree, child2.strength));
        }

        [Test]
        public void BlendCrossover_ChildGenesStayInsideTheBlendInterval()
        {
            var ga = Create(crossover: Crossovers.real_combined);
            float a = 20f, b = 60f;
            float d = b - a;

            for (int run = 0; run < 500; run++)
            {
                ga.ind1 = new Individual(a, -10f, 2f);
                ga.ind2 = new Individual(b, 30f, 10f);
                ga.Crossover_Real_Combined();

                Individual child = ga.Population.Last();
                Assert.That(child.xDegree, Is.InRange(a - GeneticAlgorithm.BlendAlpha * d, b + GeneticAlgorithm.BlendAlpha * d));
                Assert.AreEqual(PopulationSize, ga.Population.Count);
            }
        }

        /// <summary>
        ///     Regression test: the interval used to be [min - alpha*d, max - alpha*d], so a child could
        ///     never get past the higher parent and the search was biased towards low values.
        /// </summary>
        [Test]
        public void BlendCrossover_ExploresBeyondBothParents()
        {
            var ga = Create(crossover: Crossovers.real_combined);
            float a = 20f, b = 60f;
            int belowBoth = 0, aboveBoth = 0;

            for (int run = 0; run < 500; run++)
            {
                ga.ind1 = new Individual(a, 0f, 6f);
                ga.ind2 = new Individual(b, 0f, 6f);
                ga.Crossover_Real_Combined();

                float x = ga.Population.Last().xDegree;
                if (x < a) belowBoth++;
                if (x > b) aboveBoth++;
            }

            Assert.Greater(belowBoth, 0, "No child below the lower parent");
            Assert.Greater(aboveBoth, 0, "No child above the higher parent");
        }

        // ---------------------------------------------------------------- mutation

        [Test]
        public void Mutation_KeepsGenesInValidRanges([Values] Mutations mutation)
        {
            var ga = Create(mutation: mutation);

            for (int run = 0; run < 1000; run++)
            {
                if (mutation == Mutations.uniform) ga.Mutation_Uniform();
                else ga.Mutation_Swap();
            }

            foreach (var individual in ga.Population)
                AssertGenesInRange(individual);
        }

        // ---------------------------------------------------------------- whole algorithm

        [Test]
        public void GetNext_ReturnsEveryIndividualOfEveryGeneration_ThenNull()
        {
            const int generations = 3;
            var ga = Create(generations);
            int evaluated = 0;

            Individual individual;
            while ((individual = ga.GetNext()) != null)
            {
                individual.fitness = DistanceToIdealShot(individual);
                evaluated++;
                Assert.Less(evaluated, 10000, "GetNext never finished");
            }

            Assert.AreEqual(PopulationSize * generations, evaluated);
            Assert.AreEqual(PopulationSize, ga.Population.Count);
        }

        [Test]
        public void Evolution_ImprovesTheAveragePopulation()
        {
            const int generations = 40;
            var ga = Create(generations, Selections.ranking, Crossovers.real_uniform, Mutations.uniform);
            var fitnessByGeneration = new Dictionary<int, List<float>>();

            Individual individual;
            while ((individual = ga.GetNext()) != null)
            {
                individual.fitness = DistanceToIdealShot(individual);
                if (!fitnessByGeneration.TryGetValue(ga.Generation, out var list))
                    fitnessByGeneration[ga.Generation] = list = new List<float>();
                list.Add(individual.fitness);
            }

            float first = fitnessByGeneration[1].Average();
            float last = fitnessByGeneration[generations].Average();
            Assert.Less(last, first, $"Average fitness went from {first:F1} to {last:F1}");
        }
    }
}
