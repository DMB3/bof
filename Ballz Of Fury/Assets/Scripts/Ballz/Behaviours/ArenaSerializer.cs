using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Ballz.Serialization;

namespace Ballz.Behaviours {

    /// <summary>
    /// Behaviour for saving and loading an Arena to and from a file path.
    /// 
    /// The ParentGameObject property stores a GameObject that will be used as the parent of the loaded/saved arena's
    /// contents.
    /// </summary>
    public class ArenaSerializer : MonoBehaviour {

        public GameObject ParentGameObject;
        public GameObject GoalPrefab;
        public GameObject SpawnPointPrefab;
        public String TargetFilePath;

        /// <summary>
        /// Save the current Arena to the specified target file path.
        /// </summary>
        public void SaveToTargetFilePath() {
            if (this.TargetFilePath == null) {
                throw new Exception("Target file path is not specified.");
            }

            Arena arena = new Arena();
            arena.Goals = new List<Goal>();
            arena.Obstacles = new List<Obstacle>();
            arena.SpawnPoints = new List<SpawnPoint>();
            arena.Lights = new List<Ballz.Serialization.Light>();
            arena.Name = this.ParentGameObject.name;
            this.AddToArena(this.ParentGameObject.transform, arena);

            arena.MaximumPlayers = 0;
            foreach (Goal goal in arena.Goals) {
                arena.MaximumPlayers = Math.Max(arena.MaximumPlayers, goal.PlayerID + 1);
            }

            XmlSerializer serializer = new XmlSerializer(arena.GetType());            
            using (StreamWriter file = new StreamWriter(this.TargetFilePath)) {
                serializer.Serialize(file, arena);
            }
            MonoBehaviour.print("Wrote file " + Path.GetFullPath(this.TargetFilePath));
        }

        private void AddToArena(Transform parent, Arena arena) {
            for (int i = 0; i < parent.childCount; i++) {
                Transform childTransform = parent.GetChild(i);
                GameObject child = childTransform.gameObject;
                Collider collider = child.GetComponent<Collider>();

                if (child.tag.Equals("SpawnPoint")) {
                    SpawnPointBehaviour spawn = child.GetComponent<SpawnPointBehaviour>();
                    SpawnPoint newSpawn = new SpawnPoint();
                    newSpawn.Ball = spawn.Ball;
                    newSpawn.PlayerID = spawn.PlayerID;
                    newSpawn.Position = childTransform.position;
                    newSpawn.Rotation = childTransform.rotation;
                    newSpawn.Name = childTransform.name;
                    arena.SpawnPoints.Add(newSpawn);
                } else if (child.tag.Equals("Goal")) {
                    GoalBehaviour goal = child.GetComponent<GoalBehaviour>();
                    Goal newGoal = new Goal();
                    newGoal.PlayerID = goal.PlayerID;
                    newGoal.Position = childTransform.position;
                    newGoal.Rotation = childTransform.rotation;
                    newGoal.Scale = childTransform.localScale;
                    arena.Goals.Add(newGoal);
                } else if (child.tag.Equals("Floor")) {
                    Floor newFloor = new Floor();
                    newFloor.Position = childTransform.position;
                    newFloor.Scale = childTransform.localScale;
                    arena.Floor = newFloor;
                } else if (child.tag.Equals("Light")) {
                    Ballz.Serialization.Light newLight = new Ballz.Serialization.Light();
                    newLight.Name = child.name;
                    newLight.Position = childTransform.position;
                    newLight.Rotation = childTransform.rotation;
                    UnityEngine.Light light = child.GetComponent<UnityEngine.Light>();
                    newLight.LightType = light.type;
                    newLight.Range = light.range;
                    newLight.SpotAngle = light.spotAngle;
                    newLight.Intensity = light.intensity;
                    newLight.BounceIntensity = light.bounceIntensity;
                    newLight.Colour = light.color;
                    arena.Lights.Add(newLight);
                } else if (collider != null) {
                    Obstacle newObstacle = new Obstacle();
                    newObstacle.PhysicsMaterial = collider.material;
                    newObstacle.RendererMaterialName = child.GetComponent<MeshRenderer>().material.name.Replace("(Instance)", "").Trim();
                    newObstacle.Position = childTransform.position;
                    newObstacle.Rotation = childTransform.rotation;
                    newObstacle.Scale = childTransform.localScale;
                    if (collider.GetType().Equals(typeof(BoxCollider))) {
                        newObstacle.Shape = Obstacle.ObstacleShape.Cube;
                    } else if (collider.GetType().Equals(typeof(SphereCollider))) {
                        newObstacle.Shape = Obstacle.ObstacleShape.Sphere;
                    } else {
                        throw new Exception("Unknown collider shape for " + collider);
                    }
                    newObstacle.Layer = child.layer;
                    newObstacle.Name = child.name;
                    arena.Obstacles.Add(newObstacle);
                    this.AddToArena(childTransform, arena);
                } else {
                    this.AddToArena(childTransform, arena);
                }
            }
        }

        /// <summary>
        /// Load the Arena present in specified target file path.
        /// This method removes all objects present in the ParentGameObject first.
        /// </summary>
        public void LoadFromTargetFilePath() {
            if (this.TargetFilePath == null) {
                throw new Exception("Target file path is not specified.");
            }
            if (!File.Exists(this.TargetFilePath)) {
                throw new Exception("Specified file path does not exist.");
            }

            Arena arena = new Arena();
            XmlSerializer serializer = new XmlSerializer(arena.GetType());
            using (StreamReader file = new StreamReader(this.TargetFilePath)) {
                arena = (Arena) serializer.Deserialize(file);
            }
            MonoBehaviour.print("Read file " + Path.GetFullPath(this.TargetFilePath));

            for (int i = 0; i < this.ParentGameObject.transform.childCount; i++) {
                GameObject.Destroy(this.ParentGameObject.transform.GetChild(i).gameObject);
            }
            this.ParentGameObject.name = arena.Name;

            GameObject floorObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorObject.transform.parent = this.ParentGameObject.transform;
            floorObject.transform.position = arena.Floor.Position;
            floorObject.transform.localScale = arena.Floor.Scale;
            floorObject.name = "FloorPlane";
            floorObject.layer = LayerMask.NameToLayer("Floor");
            floorObject.GetComponent<MeshRenderer>().enabled = false;

            foreach (Obstacle obstacle in arena.Obstacles) {
                GameObject obstacleObject;
                if (obstacle.Shape == Obstacle.ObstacleShape.Cube) {
                    obstacleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                } else if (obstacle.Shape == Obstacle.ObstacleShape.Sphere) {
                    obstacleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                } else {
                    throw new Exception("Unhandled obstacle shape " + obstacle.Shape);
                }
                obstacleObject.transform.parent = this.ParentGameObject.transform;
                obstacleObject.transform.position = obstacle.Position;
                obstacleObject.transform.rotation = obstacle.Rotation;
                obstacleObject.transform.localScale = obstacle.Scale;
                obstacleObject.layer = obstacle.Layer;
                obstacleObject.name = obstacle.Name;
                obstacleObject.GetComponent<MeshRenderer>().material = Resources.Load(obstacle.RendererMaterialName, typeof(Material)) as Material;
            }

            foreach (Goal goal in arena.Goals) {
                GameObject goalObject = GameObject.Instantiate(this.GoalPrefab);
                goalObject.transform.parent = this.ParentGameObject.transform;
                goalObject.transform.position = goal.Position;
                goalObject.transform.rotation = goal.Rotation;
                goalObject.transform.localScale = goal.Scale;
                goalObject.GetComponent<GoalBehaviour>().PlayerID = goal.PlayerID;
            }

            foreach (SpawnPoint spawn in arena.SpawnPoints) {
                GameObject spawnObject = GameObject.Instantiate(this.SpawnPointPrefab);
                spawnObject.transform.parent = this.ParentGameObject.transform;
                spawnObject.transform.position = spawn.Position;
                spawnObject.transform.rotation = spawn.Rotation;
                spawnObject.name = spawn.Name;
                spawnObject.GetComponent<SpawnPointBehaviour>().PlayerID = spawn.PlayerID;
                spawnObject.GetComponent<SpawnPointBehaviour>().Ball = spawn.Ball;
                spawnObject.GetComponent<SpawnPointBehaviour>().Spawn();
            }

            foreach (Ballz.Serialization.Light light in arena.Lights) {
                GameObject lightObject = new GameObject();
                lightObject.transform.parent = this.ParentGameObject.transform;
                UnityEngine.Light gameLight = lightObject.AddComponent<UnityEngine.Light>();
                lightObject.name = light.Name;
                lightObject.transform.position = light.Position;
                lightObject.transform.rotation = light.Rotation;
                gameLight.type = light.LightType;
                gameLight.range = light.Range;
                gameLight.spotAngle = light.SpotAngle;
                gameLight.intensity = light.Intensity;
                gameLight.bounceIntensity = light.BounceIntensity;
                gameLight.color = light.Colour;
            }
        }

    }

}
