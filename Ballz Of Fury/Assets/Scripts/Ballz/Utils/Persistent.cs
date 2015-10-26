using UnityEngine;


namespace Ballz.Utils
{
    public class Persistent : MonoBehaviour
    {
        void Start()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
            Application.LoadLevel("home");
        }
    }
}