using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamante : MonoBehaviour
{
    bool ativo = true;
    [SerializeField] GameObject sprite;
    [SerializeField] float time, timeWaitDash;
    PlayerScript2 pScript;
    private void Start()
    {
        pScript = FindObjectOfType<PlayerScript2>();
    }
    private void OnTriggerEnter2D(Collider2D collision) //Quando entra em colis�o com o jogador, reseta o dash. 
    {
        if (collision.tag == "Player" && ativo)
        {
            if (pScript.canDash)
            {
                StartCoroutine("EsperaDashTerminar");
                return;
            }
            pScript.ResetDash();
            ativo = false;
            sprite.SetActive(ativo);
            Invoke("ResetaDiamante", time); //O m�todo Invoke chama uma fun��o ap�s um tempo (segundo par�metro)
        }
    }
    private void ResetaDiamante() //Ap�s um tempo, essa fun��o � chamada, reaparecendo o cristal
    {
        ativo = true;
        sprite.SetActive(ativo);
    }

    IEnumerator EsperaDashTerminar() //Caso o jogador d� o dash em cima do cristal, ele espera o jogador perder o dash para repor. 
    {
        yield return new WaitForSeconds(timeWaitDash);
        if (!pScript.canDash)
        {
            pScript.ResetDash();
            ativo = false;
            sprite.SetActive(ativo);
            Invoke("ResetaDiamante", time);
        }

    }

}
