using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particula : MonoBehaviour
{
    ParticleSystem particula;
    [SerializeField] Vector2 offset;

    void Start()
    {
        particula = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayParticula(Vector2 pos)
    {
        pos += offset;
        transform.position = pos;
        particula.Play();
    }
    public void RotateParticula(Vector2 dir, Vector2 pos)
    {
        pos += offset;
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, dir.y, 0), Vector3.up);
        particula.Play();
    }


}
