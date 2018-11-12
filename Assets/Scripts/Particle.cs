using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {

    public Color myColor;
    public bool showParticle;
    MeshRenderer _renderer;
    SphereCollider _sphere;

    public State state;


    // Use this for initialization 
    void Start()
    {
        _renderer = this.GetComponent<MeshRenderer>();
        _sphere = this.GetComponent<SphereCollider>();
        
        // setup color
        var material = _renderer.material;


        if (state == State.active)
        {
            _sphere.enabled = false;

            if (showParticle) material.color = Color.blue;
            else _renderer.enabled = false;
        }
        else if (state == State.dla)
        {
           material.color = myColor;
        }

    }
 }

public enum State { active, dla };
