using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class MazeGenerator : MonoBehaviourPunCallbacks
    {
        private int _width = 15;
        private int _height = 15;
        public Dropdown size;
        public GameObject plane;

        public Maze GenerateMaze()
        {
            size = GameObject.Find("Size").GetComponent<Dropdown>();
            _width = size.value + 10;
            _height = _width;
            plane = PhotonNetwork.Instantiate("Floor", new Vector3((_width * 10) - 5, 0, (_height * 10) - 5),
                Quaternion.identity);
            plane.transform.localScale = new Vector3(_width * 2, 1, _height * 2);
            var cells = new MazeGeneratorCell[_width, _height];

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y] = new MazeGeneratorCell {X = x, Y = y};
                }
            }

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                cells[x, _height - 1].WallLeft = false;
                cells[x, 0].WallBottom = false;
                cells[x, 0].WallCenter = false;
            }

            for (var y = 0; y < cells.GetLength(1); y++)
            {
                cells[_width - 1, y].WallBottom = false;
                cells[0, y].WallLeft = false;
                cells[0, y].WallCenter = false;
            }

            cells[_width - 1, _height - 1].WallCenter = false;
            cells[_width - 1, _height - 2].WallLeft = false;
            cells[_width - 2, _height - 1].WallBottom = false;
            cells[0, _height - 1].WallBottom = false;
            cells[1, _height - 1].WallCenter = false;
            cells[1, _height - 2].WallLeft = false;
            cells[_width - 1, 0].WallLeft = false;
            cells[_width - 2, 1].WallBottom = false;
            cells[_width - 1, 1].WallCenter = false;
            cells[0, 1].WallBottom = false;
            cells[1, 0].WallLeft = false;
            cells[1, 1].WallCenter = false;

            RemoveWallsWithBacktracker(cells);

            var maze = new Maze {Cells = cells};
            return maze;

        }

        private void RemoveWallsWithBacktracker(MazeGeneratorCell[,] maze)
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
                if (x < _width - 1 && !maze[x + 1, y].Visited) unvisitedNeighbours.Add(maze[x + 1, y]);
                if (y < _height - 1 && !maze[x, y + 1].Visited) unvisitedNeighbours.Add(maze[x, y + 1]);

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