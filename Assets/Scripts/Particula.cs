using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particula : MonoBehaviour
{
    ParticleSystem particula;
    [SerializeField] Vector2 offset; //É aumentado na posição final. Assim, temos um controle melhor de onde vai aparecer. 

    void Start()
    {
        particula = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayParticula(Vector2 pos) //Coloca a partícula em uma posição e faz ela iniciar. 
    {
        pos += offset;
        transform.position = pos;
        particula.Play();
    }
    public void RotateParticula(Vector2 dir, Vector2 pos) //Coloca a partícula em uma posição, rotaciona ela e faz ela iniciar. 
    {
        pos += offset;
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, dir.y, 0), Vector3.up);
        particula.Play();
    }


}
