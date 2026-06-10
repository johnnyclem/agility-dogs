using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Services;

namespace AgilityDogs.Tests.Editor
{
    /// <summary>
    /// Tests for the release-candidate fixes: scoring semantics, course layout
    /// determinism, and career data normalization.
    /// </summary>
    public class GameLogicTests
    {
        #region Scoring

        private AgilityScoringService MakeScoring(float standardTime, float maximumTime,
            float runTimer, int faults)
        {
            var go = new GameObject("Scoring");
            var scoring = go.AddComponent<AgilityScoringService>();

            var course = ScriptableObject.CreateInstance<CourseDefinition>();
            course.standardTime = standardTime;
            course.maximumTime = maximumTime;
            scoring.SetCourse(course);

            typeof(AgilityScoringService)
                .GetField("runTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(scoring, runTimer);
            typeof(AgilityScoringService)
                .GetField("faultCount", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(scoring, faults);

            return scoring;
        }

        [Test]
        public void CleanRunUnderStandardTime_Qualifies()
        {
            var scoring = MakeScoring(45f, 60f, runTimer: 38f, faults: 0);
            Assert.AreEqual(RunResult.Qualified, scoring.EvaluateRunResult());
            Object.DestroyImmediate(scoring.gameObject);
        }

        [Test]
        public void RunOverStandardTime_IsTimeFaultOnly()
        {
            var scoring = MakeScoring(45f, 60f, runTimer: 50f, faults: 0);
            Assert.AreEqual(RunResult.TimeFaultOnly, scoring.EvaluateRunResult());
            Object.DestroyImmediate(scoring.gameObject);
        }

        [Test]
        public void RunOverMaximumTime_DoesNotQualify()
        {
            var scoring = MakeScoring(45f, 60f, runTimer: 61f, faults: 0);
            Assert.AreEqual(RunResult.NonQualified, scoring.EvaluateRunResult());
            Object.DestroyImmediate(scoring.gameObject);
        }

        [Test]
        public void FaultPenaltiesPushingScoreOverMax_DoesNotQualify()
        {
            // 55s + 2 faults * 5s = 65s > 60s max
            var scoring = MakeScoring(45f, 60f, runTimer: 55f, faults: 2);
            Assert.AreEqual(RunResult.NonQualified, scoring.EvaluateRunResult());
            Object.DestroyImmediate(scoring.gameObject);
        }

        [Test]
        public void CleanRunWithFaultsUnderMax_IsTimeFaultOnly()
        {
            // 40s + 1 fault * 5s = 45s <= 60s max, but faulted
            var scoring = MakeScoring(45f, 60f, runTimer: 40f, faults: 1);
            Assert.AreEqual(RunResult.TimeFaultOnly, scoring.EvaluateRunResult());
            Object.DestroyImmediate(scoring.gameObject);
        }

        [Test]
        public void FinalScore_AddsFaultPenaltySeconds()
        {
            var scoring = MakeScoring(45f, 60f, runTimer: 30f, faults: 2);
            Assert.AreEqual(40f, scoring.GetFinalScore(), 0.001f);
            Object.DestroyImmediate(scoring.gameObject);
        }

        #endregion

        #region Course layout

        [Test]
        public void ObstaclePositions_AreDeterministicSerpentine()
        {
            // First row runs left to right
            var p0 = CourseLayoutBuilder.GetObstaclePosition(0, Vector3.zero);
            var p3 = CourseLayoutBuilder.GetObstaclePosition(3, Vector3.zero);
            Assert.Less(p0.x, p3.x);
            Assert.AreEqual(p0.z, p3.z, 0.001f);

            // Second row is further down course and runs right to left
            var p4 = CourseLayoutBuilder.GetObstaclePosition(4, Vector3.zero);
            Assert.Greater(p4.z, p0.z);
            Assert.AreEqual(p3.x, p4.x, 0.001f);
        }

        [Test]
        public void ObstacleSpacing_IsAtLeastFiveMeters()
        {
            for (int i = 1; i < 15; i++)
            {
                float gap = Vector3.Distance(
                    CourseLayoutBuilder.GetObstaclePosition(i, Vector3.zero),
                    CourseLayoutBuilder.GetObstaclePosition(i - 1, Vector3.zero));
                Assert.GreaterOrEqual(gap, 5f, $"obstacles {i - 1}->{i} too close");
            }
        }

        [Test]
        public void AllCourseAssets_HaveNonEmptySequences()
        {
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
            Assert.GreaterOrEqual(courses.Length, 11, "course assets missing from Resources");

            foreach (var course in courses)
            {
                Assert.IsNotNull(course.obstacleSequence, course.name);
                Assert.GreaterOrEqual(course.obstacleSequence.Length, 8,
                    $"{course.name} has too few obstacles");
                foreach (var obstacle in course.obstacleSequence)
                {
                    Assert.IsNotNull(obstacle, $"{course.name} has a broken obstacle reference");
                }
                Assert.Greater(course.maximumTime, course.standardTime, course.name);
            }
        }

        [Test]
        public void AllBreedAssets_LoadFromResources()
        {
            var breeds = Resources.LoadAll<BreedData>("Data/Breeds");
            Assert.GreaterOrEqual(breeds.Length, 19, "breed assets missing from Resources");
        }

        #endregion

        #region Career

        [Test]
        public void PuppyStats_ClampTo01_BoundsAllStats()
        {
            var stats = new PuppyStats
            {
                speed = 5.4f, acceleration = 4.8f, agility = 0.33f, jumpPower = 1.2f,
                stamina = 0.5f, intelligence = 0.5f, focus = -0.2f, confidence = 0.5f
            };
            stats.ClampTo01();

            Assert.LessOrEqual(stats.speed, 1f);
            Assert.LessOrEqual(stats.acceleration, 1f);
            Assert.GreaterOrEqual(stats.focus, 0f);
            Assert.LessOrEqual(stats.GetOverallRating(), 1f);
        }

        [Test]
        public void EffectiveSkill_IncludesTrainingBonus()
        {
            var puppy = new PuppyData
            {
                baseStats = new PuppyStats
                {
                    speed = 0.5f, acceleration = 0.5f, agility = 0.5f, jumpPower = 0.5f,
                    stamina = 0.5f, intelligence = 0.5f, focus = 0.5f, confidence = 0.5f
                },
                trainingProgress = new System.Collections.Generic.Dictionary<TrainingSkill, int>()
            };
            foreach (TrainingSkill skill in System.Enum.GetValues(typeof(TrainingSkill)))
            {
                puppy.trainingProgress[skill] = 100; // fully trained
            }

            Assert.AreEqual(0.8f, puppy.GetEffectiveSkill(), 0.001f);
        }

        #endregion
    }
}
