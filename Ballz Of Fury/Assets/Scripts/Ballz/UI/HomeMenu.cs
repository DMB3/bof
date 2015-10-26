using UnityEngine;
using System.Collections;

namespace Ballz.Networking
{
    public class HomeMenu : MonoBehaviour
    {
        public void JoinOrCreateRoom()
        {
            Debug.Log("Play now clicked");
            Application.LoadLevel("lobby");
        }

        public void QuitApplication()
        {
            Debug.Log("Quit button clicked");
            Application.Quit();
        }
    }
}