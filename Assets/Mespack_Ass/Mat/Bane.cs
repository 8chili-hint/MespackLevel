using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bane : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        InvokeRepeating("myfuck",1f,2f);

    }
    void myfuck()
    {
        Debug.Log("Fuck");
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        Debug.Log("Awake");
    }

    private void OnEnable()
    {
        Debug.Log("Enable");
    }
}

