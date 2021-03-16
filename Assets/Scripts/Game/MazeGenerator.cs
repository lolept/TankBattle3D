using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game
{
    public class MazeGenerator : MonoBehaviourPunCallbacks
    {
        private const int Width = 15;
        private const int Height = 15;

        public Maze GenerateMaze()
        {
            var cells = new MazeGeneratorCell[Width, Height];

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y] = new MazeGeneratorCell {X = x, Y = y};
                }
            }

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                cells[x, Height - 1].WallLeft = false;
                cells[x, 0].WallBottom = false;
                cells[x, 0].WallCenter = false;
            }

            for (var y = 0; y < cells.GetLength(1); y++)
            {
                cells[Width - 1, y].WallBottom = false;
                cells[0, y].WallLeft = false;
                cells[0, y].WallCenter = false;
            }

            cells[Width - 1, Height - 1].WallCenter = false;
            cells[0, Height - 1].WallBottom = false;
            cells[Width - 1, 0].WallLeft = false;

            RemoveWallsWithBacktracker(cells);

            var maze = new Maze {Cells = cells};

            var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            var sendOptions = new SendOptions {Reliability = true};
            //PhotonNetwork.RaiseEvent(69, cells, options, sendOptions);
            return maze;

        }

        private static void RemoveWallsWithBacktracker(MazeGeneratorCell[,] maze)
        {
            var current = maze[0, 0];
            current.Visited = true;
            current.DistanceFromStart = 0;

            var stack = new Stack<MazeGeneratorCell>();
            do
            {
                var unvisitedNeighbours = new List<MazeGeneratorCell>();

                var x = current.X;
                var y = current.Y;

                if (x > 0 && !maze[x - 1, y].Visited) unvisitedNeighbours.Add(maze[x - 1, y]);
                if (y > 0 && !maze[x, y - 1].Visited) unvisitedNeighbours.Add(maze[x, y - 1]);
                if (x < Width - 1 && !maze[x + 1, y].Visited) unvisitedNeighbours.Add(maze[x + 1, y]);
                if (y < Height - 1 && !maze[x, y + 1].Visited) unvisitedNeighbours.Add(maze[x, y + 1]);

                if (unvisitedNeighbours.Count > 0)
                {
                    var chosen = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                    RemoveWall(current, chosen);

                    chosen.Visited = true;
                    stack.Push(chosen);
                    chosen.DistanceFromStart = current.DistanceFromStart + 1;
                    current = chosen;
                }
                else
                {
                    current = stack.Pop();
                }
            } while (stack.Count > 0);
        }

        private static void RemoveWall(MazeGeneratorCell a, MazeGeneratorCell b)
        {
            if (a.X == b.X)
            {
                if (a.Y > b.Y) a.WallBottom = false;
                else b.WallBottom = false;
            }
            else
            {
                if (a.X > b.X) a.WallLeft = false;
                else b.WallLeft = false;
            }
        }
    }
}