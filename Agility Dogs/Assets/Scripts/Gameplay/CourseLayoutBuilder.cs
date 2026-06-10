using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Gameplay.Obstacles;

namespace AgilityDogs.Gameplay
{
    /// <summary>
    /// Builds the physical obstacles for a course at runtime from its
    /// CourseDefinition.obstacleSequence, laid out on a deterministic
    /// serpentine path. This lets every course asset be played in any
    /// gameplay scene instead of relying on hand-placed scene obstacles.
    /// </summary>
    public static class CourseLayoutBuilder
    {
        public const string GeneratedRootName = "CourseObstacles [Generated]";

        // Serpentine layout tuning. Mirrored by the web simulation tests —
        // keep in sync with tests/sim/layout.js.
        public const int ObstaclesPerRow = 4;
        public const float ColumnSpacing = 8f;
        public const float RowSpacing = 9f;
        public const float StartOffset = 6f;

        /// <summary>
        /// Deterministic position of the obstacle at the given sequence index.
        /// </summary>
        public static Vector3 GetObstaclePosition(int index, Vector3 origin)
        {
            int row = index / ObstaclesPerRow;
            int col = index % ObstaclesPerRow;

            // Reverse direction on odd rows (serpentine).
            if (row % 2 == 1)
            {
                col = ObstaclesPerRow - 1 - col;
            }

            float x = (col - (ObstaclesPerRow - 1) * 0.5f) * ColumnSpacing;
            float z = StartOffset + row * RowSpacing;
            return origin + new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Direction of travel through the obstacle at the given index.
        /// </summary>
        public static Vector3 GetObstacleDirection(int index)
        {
            int row = index / ObstaclesPerRow;
            int posInRow = index % ObstaclesPerRow;

            // Last obstacle in a row turns up toward the next row.
            if (posInRow == ObstaclesPerRow - 1)
            {
                return Vector3.forward;
            }

            return row % 2 == 0 ? Vector3.right : Vector3.left;
        }

        /// <summary>
        /// Build all obstacles for the course. Pre-existing scene obstacles are
        /// deactivated so they cannot trigger wrong-course faults against the
        /// generated layout. Returns the obstacles in course sequence order.
        /// </summary>
        public static ObstacleBase[] BuildCourse(CourseDefinition course)
        {
            if (course == null || course.obstacleSequence == null || course.obstacleSequence.Length == 0)
            {
                return new ObstacleBase[0];
            }

            // Remove a previously generated course.
            var previous = GameObject.Find(GeneratedRootName);
            if (previous != null)
            {
                Object.Destroy(previous);
            }

            // Deactivate hand-placed obstacles that are not part of this layout.
            foreach (var existing in Object.FindObjectsOfType<ObstacleBase>())
            {
                existing.gameObject.SetActive(false);
            }

            var root = new GameObject(GeneratedRootName);
            var result = new List<ObstacleBase>(course.obstacleSequence.Length);

            for (int i = 0; i < course.obstacleSequence.Length; i++)
            {
                ObstacleData data = course.obstacleSequence[i];
                if (data == null) continue;

                Vector3 position = GetObstaclePosition(i, course.startPosition);
                Vector3 direction = GetObstacleDirection(i);

                ObstacleBase obstacle = BuildObstacle(data, position, direction, root.transform, i);
                if (obstacle != null)
                {
                    result.Add(obstacle);
                }
            }

            return result.ToArray();
        }

        private static ObstacleBase BuildObstacle(ObstacleData data, Vector3 position, Vector3 direction, Transform parent, int index)
        {
            var go = new GameObject($"{index + 1:00}_{data.obstacleType}");
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            // Entry and exit points along the direction of travel. ObstacleBase
            // auto-finds these children by name in Awake, so they must exist
            // before the component is added.
            float halfLength = Mathf.Max(0.5f, data.length * 0.5f);
            CreatePoint(go.transform, "EntryPoint", new Vector3(0f, 0f, -halfLength - 0.5f));
            CreatePoint(go.transform, "CommitPoint", new Vector3(0f, 0f, -halfLength - 1.5f));
            CreatePoint(go.transform, "ExitPoint", new Vector3(0f, 0f, halfLength + 0.5f));

            BuildVisuals(go.transform, data);

            ObstacleBase obstacle = AddObstacleComponent(go, data.obstacleType);
            if (obstacle == null)
            {
                Object.Destroy(go);
                return null;
            }

            obstacle.AssignObstacleData(data);
            return obstacle;
        }

        private static void CreatePoint(Transform parent, string name, Vector3 localPosition)
        {
            var point = new GameObject(name);
            point.transform.SetParent(parent);
            point.transform.localPosition = localPosition;
            point.transform.localRotation = Quaternion.identity;
        }

        private static ObstacleBase AddObstacleComponent(GameObject go, ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.BarJump: return go.AddComponent<BarJumpObstacle>();
                case ObstacleType.TireJump: return go.AddComponent<TireJumpObstacle>();
                case ObstacleType.BroadJump: return go.AddComponent<BroadJumpObstacle>();
                case ObstacleType.WallJump: return go.AddComponent<WallJumpObstacle>();
                case ObstacleType.DoubleJump: return go.AddComponent<DoubleJumpObstacle>();
                case ObstacleType.TripleJump: return go.AddComponent<TripleJumpObstacle>();
                case ObstacleType.PanelJump: return go.AddComponent<PanelJumpObstacle>();
                case ObstacleType.LongJump: return go.AddComponent<LongJumpObstacle>();
                case ObstacleType.SpreadJump: return go.AddComponent<SpreadJumpObstacle>();
                case ObstacleType.Tunnel: return go.AddComponent<TunnelObstacle>();
                case ObstacleType.WeavePoles: return go.AddComponent<WeavePolesObstacle>();
                case ObstacleType.AFrame: return go.AddComponent<AFrameObstacle>();
                case ObstacleType.DogWalk: return go.AddComponent<DogWalkObstacle>();
                case ObstacleType.Teeter: return go.AddComponent<TeeterObstacle>();
                case ObstacleType.PauseTable: return go.AddComponent<PauseTableObstacle>();
                default:
                    Debug.LogWarning($"[CourseLayoutBuilder] Unsupported obstacle type: {type}");
                    return null;
            }
        }

        private static void BuildVisuals(Transform parent, ObstacleData data)
        {
            switch (data.obstacleType)
            {
                case ObstacleType.WeavePoles:
                    BuildWeavePoleVisuals(parent);
                    break;

                case ObstacleType.Tunnel:
                    AddPrimitive(parent, PrimitiveType.Cylinder, "TunnelBody",
                        new Vector3(0f, 0.6f, 0f),
                        Quaternion.Euler(90f, 0f, 0f),
                        new Vector3(1.2f, Mathf.Max(1f, data.length * 0.5f), 1.2f),
                        new Color(0.9f, 0.7f, 0.2f));
                    break;

                case ObstacleType.PauseTable:
                    AddPrimitive(parent, PrimitiveType.Cube, "TableTop",
                        new Vector3(0f, 0.3f, 0f),
                        Quaternion.identity,
                        new Vector3(1.2f, 0.6f, 1.2f),
                        new Color(0.3f, 0.5f, 0.9f));
                    break;

                case ObstacleType.AFrame:
                case ObstacleType.DogWalk:
                case ObstacleType.Teeter:
                    AddPrimitive(parent, PrimitiveType.Cube, "Ramp",
                        new Vector3(0f, 0.4f, 0f),
                        Quaternion.identity,
                        new Vector3(Mathf.Max(0.3f, data.width), 0.8f, Mathf.Max(1f, data.length)),
                        new Color(0.8f, 0.4f, 0.2f));
                    break;

                default: // Jumps
                    BuildJumpVisuals(parent, data);
                    break;
            }
        }

        private static void BuildWeavePoleVisuals(Transform parent)
        {
            const int poleCount = 12;
            const float poleSpacing = 0.6f;
            float startZ = -(poleCount - 1) * poleSpacing * 0.5f;

            for (int i = 0; i < poleCount; i++)
            {
                // WeavePolesObstacle.AutoFindPolePositions discovers children
                // named "Pole<N>" sorted numerically.
                var pole = AddPrimitive(parent, PrimitiveType.Cylinder, $"Pole{i + 1}",
                    new Vector3(0f, 0.5f, startZ + i * poleSpacing),
                    Quaternion.identity,
                    new Vector3(0.08f, 0.5f, 0.08f),
                    i % 2 == 0 ? new Color(0.2f, 0.4f, 0.9f) : Color.white);
            }
        }

        private static void BuildJumpVisuals(Transform parent, ObstacleData data)
        {
            float width = Mathf.Max(1f, data.width);
            float barHeight = Mathf.Clamp(data.height, 0.3f, 0.7f);

            AddPrimitive(parent, PrimitiveType.Cylinder, "PoleLeft",
                new Vector3(-width * 0.5f, 0.5f, 0f), Quaternion.identity,
                new Vector3(0.1f, 0.5f, 0.1f), Color.white);
            AddPrimitive(parent, PrimitiveType.Cylinder, "PoleRight",
                new Vector3(width * 0.5f, 0.5f, 0f), Quaternion.identity,
                new Vector3(0.1f, 0.5f, 0.1f), Color.white);
            AddPrimitive(parent, PrimitiveType.Cube, "Bar",
                new Vector3(0f, barHeight, 0f), Quaternion.identity,
                new Vector3(width, 0.08f, 0.08f), new Color(0.9f, 0.2f, 0.2f));
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;

            // Visual only: colliders would block the NavMesh agent's body and
            // are not needed for completion (the dog navigates entry/exit points).
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            return go;
        }
    }
}
