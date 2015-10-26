using UnityEngine;
using System.Collections;

namespace Ballz.UI
{
    public class Lobby : MonoBehaviour
    {
        public void LoadHomeMenu()
        {
            Application.LoadLevel("home");
        }
    }
}