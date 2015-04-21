using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Ballz.Arenas;
using System.IO;

namespace Ballz.Testing {

    /// <summary>
    /// Creates a serializable arena and tests its output and deserialization.
    /// </summary>
    public class ArenaSerializationTest : MonoBehaviour {

        public void Start() {
            // build a serializable arena
            // note that this arena, as is right now, is not playable
            Arena arena = new Arena();
            arena.Name = "Open Space";
            arena.MaximumPlayers = 2;

            arena.SpawnPoints = new List<SpawnPoint>();
            // spawn points for player 1's ballz
            SpawnPoint spawn = new SpawnPoint();
            spawn.Position = new Vector3(-3.0f, 0.0f, -0.5f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Defender;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(-3.0f, 0.0f, 0.5f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Defender;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(-2.5f, 0.0f, 0.0f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Midfielder;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(-2.0f, 0.0f, 0.0f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Attacker;
            arena.SpawnPoints.Add(spawn);
            // spawn points for player 2's ballz
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(3.0f, 0.0f, -0.5f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Defender;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(3.0f, 0.0f, 0.5f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Defender;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(2.5f, 0.0f, 0.0f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Midfielder;
            arena.SpawnPoints.Add(spawn);
            spawn = new SpawnPoint();
            spawn.Position = new Vector3(2.0f, 0.0f, 0.0f);
            spawn.Rotation = Quaternion.identity;
            spawn.PlayerID = 0;
            spawn.Ball = SpawnPoint.BallType.Attacker;
            arena.SpawnPoints.Add(spawn);

            arena.Goals = new List<Goal>();
            Goal left = new Goal();
            left.PlayerID = 0;
            left.Position = new Vector3(-5.3f, 0.0f, 0.0f);
            left.Rotation = Quaternion.identity;
            left.Scale = new Vector3(0.6f, 1.0f, 1.0f);
            arena.Goals.Add(left);
            Goal right = new Goal();
            right.PlayerID = 1;
            right.Position = new Vector3(5.3f, 0.0f, 0.0f);
            right.Rotation = new Quaternion(0.0f, 180.0f, 0.0f, 0.0f);
            right.Scale = new Vector3(0.6f, 1.0f, 1.0f);
            arena.Goals.Add(right);

            arena.Obstacles = new List<Obstacle>();
            Obstacle wall = new Obstacle();
            wall.Position = new Vector3(5.0f, 0.0f, 1.75f);
            wall.Scale = new Vector3(0.1f, 1.0f, 2.5f);
            wall.Shape = Obstacle.ObstacleShape.Cube;
            wall.PhysicsMaterial = "Wood";
            arena.Obstacles.Add(wall);

            string xmlArena;
            XmlSerializer serializer = new XmlSerializer(arena.GetType());
            using (StringWriter writer = new StringWriter()) {
                serializer.Serialize(writer, arena);
                xmlArena = writer.ToString();
            }
            MonoBehaviour.print("Testing Arena XML Serialization:");
            MonoBehaviour.print(xmlArena);

            MonoBehaviour.print("Testing Arena XML Deserialization:");
            using (StringReader reader = new StringReader(xmlArena)) {
                Arena deserialized = (Arena) serializer.Deserialize(reader);
                MonoBehaviour.print(deserialized);
            }
        }

    }

}
