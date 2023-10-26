using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script feito durante o minicurso
public class PlayerScript : MonoBehaviour
{
    public float velocidade;
    public float forcaPulo;

    float direcao;
    Rigidbody2D rb;
    BoxCollider2D col;
    public LayerMask layerChao;
    bool noChao;

    bool movimentacaoPadrao = true;
    bool podeDarDash = true;
    bool dashing = false;
    public float forcaDash;
    public float tempoDash;
    Vector2 direcaoDash;

    float direcaoY;
    bool tocandoParede = false;
    bool escalando = false;
    public float velocidadeParede;

    public float tempoEscalada;
    float tempoEscaladaPassado = 0;

    public float gravidadeMultiplicador;
    bool pulando = false;

    int direcaoParede;
    public float tempoPuloParede;

    public float tempoCoyote;
    float tempoCoyotePassado = 0;

    public float tempoBufferingPulo;
    float tempoBufferingPuloPassado = 0;

    public float puloParedeLerp;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        noChao = IsGrounded();
        Debug.Log(noChao);
        direcao = Input.GetAxisRaw("Horizontal");
        direcaoY = Input.GetAxisRaw("Vertical");
        direcaoParede = ChecaTocandoParede();
        tocandoParede = direcaoParede != 0;

        if (noChao)
        {
            if (!podeDarDash)
            {
                podeDarDash = true;
            }
            if (!escalando)
            {
                tempoEscaladaPassado = tempoEscalada;
            }
            rb.gravityScale = 1;
            if (tempoCoyote != tempoCoyotePassado)
            {
                tempoCoyotePassado = tempoCoyote;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            tempoBufferingPuloPassado = tempoBufferingPulo;
        }
        if (tempoBufferingPuloPassado > 0)
        {
            tempoBufferingPuloPassado -= Time.deltaTime;
            Pulo();
        }


        if (Input.GetKeyDown(KeyCode.X))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.Z) && tocandoParede)
        {
            EscalarParede();
        }
        else if ((Input.GetKeyUp(KeyCode.Z) || !tocandoParede) && escalando)
        {
            PararEscalarParede();
        }
        if (escalando)
        {
            if (tempoEscaladaPassado < 0)
            {
                PararEscalarParede();
            }
            else
            {
                tempoEscaladaPassado -= Time.deltaTime;
            }
        }

        if (pulando && !Input.GetKey(KeyCode.C) && movimentacaoPadrao)
        {
            rb.gravityScale = gravidadeMultiplicador;
            pulando = false;
        }
        if (rb.velocity.y < -0.5f && movimentacaoPadrao && tempoCoyotePassado <= 0)
        {
            rb.gravityScale = gravidadeMultiplicador;
            pulando = false;
        }

        if (tempoCoyotePassado > 0)
        {
            tempoCoyotePassado -= Time.deltaTime;
        }

    }

    void FixedUpdate()
    {
        if (movimentacaoPadrao)
        {
            if (noChao)
                rb.velocity = new Vector2(direcao * velocidade, rb.velocity.y);
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(direcao * velocidade, rb.velocity.y), puloParedeLerp * Time.deltaTime);
        }
        else if (dashing)
        {
            rb.velocity = direcaoDash * forcaDash;
        }
        else if (escalando)
        {
            rb.velocity = new Vector2(0, direcaoY * velocidadeParede);
        }
    }

    void Pulo()
    {
        if (dashing)
            return;
        if (tempoCoyotePassado > 0)
        {
            pulando = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * forcaPulo, ForceMode2D.Impulse);
            tempoCoyotePassado = 0;
            tempoBufferingPuloPassado = 0;
        }
        else if (tocandoParede)
        {
            rb.gravityScale = 1;
            movimentacaoPadrao = false;
            pulando = true;
            Vector2 direcaoPulo = new Vector2(direcaoParede, 1).normalized;
            if (Input.GetKey(KeyCode.Z) && escalando && direcao != direcaoParede)
            {
                direcaoPulo = Vector2.up;
            }
            escalando = false;
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(direcaoPulo * forcaPulo, ForceMode2D.Impulse);
            StartCoroutine(PosWallJump());
            tempoCoyotePassado = 0;
            tempoBufferingPuloPassado = 0;
        }
    }
    IEnumerator PosWallJump()
    {
        yield return new WaitForSeconds(tempoPuloParede);
        if (Input.GetKey(KeyCode.Z) && tocandoParede)
        {
            EscalarParede();
        }
        else
        {
            movimentacaoPadrao = true;
        }
    }

    bool IsGrounded()
    {
        float extraHeight = 0.1f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, new Vector2(col.bounds.size.x - 0.005f, col.bounds.size.y), 0, Vector2.down, extraHeight, layerChao);
        return retorno;
    }

    void Dash()
    {
        if (dashing || !podeDarDash)
        {
            return;
        }
        direcaoDash = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (direcaoDash == Vector2.zero)
        {
            return;
        }
        dashing = true;
        movimentacaoPadrao = false;
        podeDarDash = false;
        rb.gravityScale = 0;
        StartCoroutine("PosDash");
    }
    IEnumerator PosDash()
    {
        Vector2 alvo = new Vector2(direcaoDash.x * velocidade, direcaoDash.y * (forcaDash / 2));

        yield return new WaitForSeconds(tempoDash);

        rb.gravityScale = 1;
        movimentacaoPadrao = true;
        rb.velocity = alvo;
        dashing = false;
    }

    int ChecaTocandoParede()
    {
        float extraHeight = 0.15f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.left, extraHeight, layerChao);
        bool retorno2 = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.right, extraHeight, layerChao);
        if (retorno)
        {
            return 1;
        }
        else if (retorno2)
        {
            return -1;
        }
        else
            return 0;
    }
    void EscalarParede()
    {
        escalando = true;
        movimentacaoPadrao = false;
        dashing = false;
        rb.gravityScale = 0;
        StopAllCoroutines();
    }
    void PararEscalarParede()
    {
        escalando = false;
        movimentacaoPadrao = true;
        rb.gravityScale = 1;
    }
}
