using UnityEngine;

namespace Game
{
    public class SelfDestroy : MonoBehaviour
    {
        [SerializeField] private bool wall;
        [SerializeField] private float time;

        private void Update()
        {
            if (wall)
            {
                if (gameObject.transform.childCount == 0)
                    Destroy(gameObject);
            }
            else
            {
                time -= Time.deltaTime;
                if (time <= 0)
                    Destroy(gameObject);
            }
        }
    }
}
