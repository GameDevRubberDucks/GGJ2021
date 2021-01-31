using UnityEngine;
using System.Collections;

public class DontDestroyOnLoad : MonoBehaviour
{   
    private void Awake()
    {
        DontDestroyOnLoad[] others = GameObject.FindObjectsOfType<DontDestroyOnLoad>();

        if (others.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
