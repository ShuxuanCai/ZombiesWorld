﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    float destroyHeight;

    // Start is called before the first frame update
    void Start()
    {
        if(this.gameObject.tag == "Ragdoll")
        {
            Invoke("StartSink", 5);
        }
    }

    public void StartSink()
    {
        destroyHeight = this.transform.position.y - 5;
        Collider[] colList = this.transform.GetComponentsInChildren<Collider>();
        foreach(Collider c in colList)
        {
            Destroy(c);
        }

        InvokeRepeating("SinkIntoGround", 3, 0.1f);
    }

    void SinkIntoGround()
    {
        this.transform.Translate(0, -0.001f, 0);
        if(this.transform.position.y < destroyHeight)
        {
            Destroy(this.gameObject);
        }
    }
}
