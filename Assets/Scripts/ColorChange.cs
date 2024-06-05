using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    [SerializeField] Material normalColor;

    public void Restart()
    {
        GetComponent<MeshRenderer>().material = normalColor;
        gameObject.tag = "Tile";
    }

}
