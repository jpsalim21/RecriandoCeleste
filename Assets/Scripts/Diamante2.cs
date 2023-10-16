using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamante2 : MonoBehaviour
{
    bool ativo = true;
    [SerializeField] GameObject sprite;
    [SerializeField] float time, timeWaitDash;
    PlayerScript pScript;
    private void Start()
    {
        pScript = FindObjectOfType<PlayerScript>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && ativo)
        {
            if (pScript.canDash)
            {
                StartCoroutine("EsperaDashTerminar");
                return;
            }
            pScript.canDash = true;
            ativo = false;
            sprite.SetActive(ativo);
            Invoke("ResetaDiamante", time);
        }
    }
    private void ResetaDiamante()
    {
        ativo = true;
        sprite.SetActive(ativo);
    }

    IEnumerator EsperaDashTerminar()
    {
        yield return new WaitForSeconds(timeWaitDash);
        if (!pScript.canDash)
        {
            pScript.canDash = true;
            ativo = false;
            sprite.SetActive(ativo);
            Invoke("ResetaDiamante", time);
        }

    }
}
