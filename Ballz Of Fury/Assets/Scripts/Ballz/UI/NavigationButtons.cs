using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace Ballz.UI
{

    public class NavigationButtons : MonoBehaviour
    {

        public string BackText = "Back";
        public string NextText = "Next";

        // Change button visibility/text according to defined values.
        void Start()
        {
            GameObject back = this.gameObject.transform.Find("Back/Text").gameObject;
            GameObject next = this.gameObject.transform.Find("Next/Text").gameObject;

            if ("".Equals(this.BackText))
            {
                back.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                back.GetComponent<Text>().text = this.BackText;
            }

            if ("".Equals(this.NextText))
            {
                next.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                next.GetComponent<Text>().text = this.NextText;
            }
        }
    }
}