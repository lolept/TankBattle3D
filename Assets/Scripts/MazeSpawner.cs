using UnityEngine;
using UnityEngine.Networking;

public class MazeSpawner : MonoBehaviour
{
    public Cell SampleWall;
    public Cell LuckyWall;
    public Cell IronWall;
    private int type;
    public GameObject FrameBlock;
    public Vector3 CellSize = new Vector3(5, 0, 5);

    public Maze maze;

    private void Start()
    {
        MazeGenerator generator = new MazeGenerator();
        maze = generator.GenerateMaze();

        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
            for (int y = 0; y < maze.cells.GetLength(1); y++)
            {
                Cell c;
                type = Random.Range(0, 15);
                if (type == 0)
                {
                    c = Instantiate(LuckyWall, new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                        Quaternion.identity);
                }
                else if (type > 0 && type < 4) {
                    c = Instantiate(IronWall, new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                        Quaternion.identity);
                }
                else
                {
                    c = Instantiate(SampleWall, new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                        Quaternion.identity);
                }

                c.WallLeft.SetActive(maze.cells[x, y].WallLeft);
                c.WallBottom.SetActive(maze.cells[x, y].WallBottom);
                c.WallCenter.SetActive(maze.cells[x, y].WallCenter);
            }
        }

        for (int x = 0; x <= maze.cells.GetLength(0) * 2 - 1; x++)
        {
            for (int y = 0; y <= maze.cells.GetLength(1) * 2 - 1; y++)
            {
                if (x == 0 || y == 0 || x == maze.cells.GetLength(0) * 2 - 1 || y == maze.cells.GetLength(1) * 2 - 1)
                {
                    Instantiate(FrameBlock, new Vector3(x * 10, 0.125f, y * 10), Quaternion.identity);
                }
            }
        }
    }
}