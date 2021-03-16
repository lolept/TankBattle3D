using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Cell : MonoBehaviour
    {
        [FormerlySerializedAs("WallLeft")] public GameObject wallLeft;
        [FormerlySerializedAs("WallBottom")] public GameObject wallBottom;
        [FormerlySerializedAs("WallCenter")] public GameObject wallCenter;
    }
}