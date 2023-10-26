using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Esse Script é a versão melhorada daquele produzido durante o minicurso. Nele, você encontra as animações e algumas partículas que
 * fazem esse protótipo se parecer mais com Celeste. 
 */ 
public class PlayerScript2 : MonoBehaviour
{
    //O comando Header apenas cria um cabeçalho na Unity, servindo apenas para facilitar a compreensão
    [Header("Movimentação Padrão")] //Variáveis para a movimentação do jogador
    [SerializeField] LayerMask groundLayer;
    public float speed;
    public float jumpForce;
    public float dashForce;
    public float dashTime;
    public float wallSpeed;
    public float climbTime;
    public float wallJumpTime;

    [Header("Booleanas de controle")] //Variáveis que servem para o controle de cada movimentação e para informar o Animator
    public bool dashing = false;
    public bool canDash = true;
    public bool wall = false;
    public bool pulando = false;
    public bool mover = true;
    public bool chao = true;
    public bool parede = false;

    [Header("Melhorar Respostas do jogo")] //Variáveis para CoyoteTime, Jump Buffering e etc...
    public float respostaTime;
    public float coyoteTime;
    public float dashRespostaTime;
    public float wallJumpRespostaTime;
    float respostaTimeElapsed = 0, coyoteTimeElapsed = 0, climbTimeElapsed = 0, dashRespostaTimeElapsed = 0;

    [Header("Controle no ar da personagem")] //Melhora no controle no ar
    public float wallJumpLerp;
    public float fallGravityMultiplier;
    public float midFallGravityMultiplier;

    //Variáveis que não precisam ser mostradas
    Rigidbody2D rb;
    BoxCollider2D col;
    float direction, directionY;
    Vector2 directionVector;
    int paredeValor;
    Vector2 posInicial;

    Animator anim; //Essa variável faz referência ao Animator, que é responsável por rodar e alternar as animações
    SpriteRenderer sp; //Essa variável faz referência ao sprite. Aqui, usamos ela apenas para inverter no Eixo X (virar para direita ou esquerda)

    [Header("Finalização")] //Variáveis que servem para adicionar detalhes
    [SerializeField] Particula particulaPular, particulaDash;
    [SerializeField] Color corPosDash;
    [SerializeField] Material flashMaterial, normalMaterial;
    void Start()
    {
        //Pega os componentes para que n�o precisarmos colocar dentro do editor. 
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
        sp = GetComponentInChildren<SpriteRenderer>();
        normalMaterial = sp.material;
        posInicial = transform.position;
    }
    void Update()
    {
        chao = IsGrounded(); //Salva em uma variável, para que n�o seja preciso chamar a fun��o mais de uma vez por frame.
        paredeValor = isOnWall();
        wall = paredeValor != 0; //Salva o valor da função em uma vari�vel, para n�o chamar mais de uma vez por frame.
        if (chao)
        {
            rb.gravityScale = 1;
            if(coyoteTimeElapsed != coyoteTime)
                coyoteTimeElapsed = coyoteTime; //Caso esteja no ch�o, reseta o CoyoteTime

            if(climbTimeElapsed != climbTime)
                climbTimeElapsed = climbTime; //Reseta o tempo de parede

            if (!dashing && !canDash)
            {
                ResetDash();
            }
        }
        if (Input.GetKeyDown(KeyCode.C)) //Input para o pulo
        {
            respostaTimeElapsed = respostaTime;
        }
        if (respostaTimeElapsed > 0) //Executa o pulo 
        {
            respostaTimeElapsed -= Time.deltaTime; 
            Jump(paredeValor);
        }
        if (coyoteTimeElapsed > 0)
        {
            coyoteTimeElapsed -= Time.deltaTime; //Decresce no CoyoteTime.
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dashRespostaTimeElapsed = dashRespostaTime;
        }
        if (dashRespostaTimeElapsed > 0) //Input para o dash
        {
            dashRespostaTimeElapsed -= Time.deltaTime;
            Dash();
        }
        if (wall)
        {
            if (climbTimeElapsed > 0)
            {
                climbTimeElapsed -= Time.deltaTime;
            }
            else
                StopWallClimb();

            if(rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.98f);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z) && wall)
        {
            WallClimb();
        }
        else if ((Input.GetKeyUp(KeyCode.Z) || !wall) && parede)
        {
            StopWallClimb();
        }
        if(rb.velocity.y < -0.5f && mover && !(coyoteTimeElapsed > 0))
        {
            rb.gravityScale = fallGravityMultiplier;
            pulando = false;
        }
        if(pulando && !Input.GetKey(KeyCode.C) && mover)
        {
            rb.gravityScale = midFallGravityMultiplier;
            pulando = false;
        }
        direction = Input.GetAxisRaw("Horizontal"); //Input do eixo X. 
        directionY = Input.GetAxisRaw("Vertical"); //Input do eixo Y. 
        AtualizaAnimator();
    }
    void AtualizaAnimator() //Atualiza os valores do animator para as anima��es funcionarem corretamente
    {
        anim.SetFloat("Velocidade", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Dashing", dashing);
        anim.SetBool("NoAr", !chao);
        anim.SetBool("WallSlide", wall);
        anim.SetBool("WallClimb", rb.velocity.y > 0.5);
        if (rb.velocity.x > 0 || paredeValor == -1) //Inverte o sprite dependendo das circunstâncias. 
        {
            sp.flipX = false;
        } else if(rb.velocity.x < 0 || paredeValor == 1)
        {
            sp.flipX = true;
        }
    }
    //Aqui, a fun��o � chamada em tempos fixos. Assim, � mais confi�vel para aplicar comandos envolvendo a f�sica. 
    private void FixedUpdate()
    {
        if (mover)
        {
            if (chao)
                rb.velocity = new Vector2(direction * speed, rb.velocity.y); //Movimenta��o padr�o
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(direction * speed, rb.velocity.y), wallJumpLerp * Time.deltaTime);
        }
        else if (dashing)
        {
            rb.velocity = directionVector * dashForce; //Fun��o do dash
        }
        else if (parede)
        {
            rb.velocity = new Vector2(0, directionY * wallSpeed); //Fun��o de subir a parede
        }
    }
    void Jump(int paredeValor)
    {
        if (dashing)
            return;
        if (coyoteTimeElapsed > 0) //Caso o tempo de respota e o CoyoteTime esteja maior que 0, realiza o pulo. 
        {
            pulando = true;
            coyoteTimeElapsed = 0;
            respostaTimeElapsed = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0); //Reseta a velocidade no eixo Y, para que o pulo tenha sempre a mesma altura
            particulaPular.PlayParticula(transform.position);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); //Fun��o do pulo. 
        }
        else if (wall) //Pulo na parede
        {
            Debug.Log("Chamou pulo na parede");
            mover = false; //Informa que a personagem pode se movimentar de maneira convencional agora.
            rb.gravityScale = 1; //Retorna a gravidade.
            coyoteTimeElapsed = 0;
            pulando = true;
            Vector2 direcaoPulo = new Vector2(paredeValor, 1).normalized;
            if (Input.GetKey(KeyCode.Z) && parede && direction != paredeValor)
            {
                direcaoPulo = Vector2.up;
            }
            Debug.Log("WallJump " + direcaoPulo);
            parede = false;
            rb.velocity = new Vector2(0, 0); //Reseta a velocidade no eixo Y, para que o pulo tenha sempre a mesma altura
            rb.AddForce(direcaoPulo * jumpForce, ForceMode2D.Impulse); //Fun��o do pulo. 
            StartCoroutine(PosWallJump());
            respostaTimeElapsed = 0; //Impede que essa parte do c�digo seja chamada novamente. 
        }
    } //Pulo da personagem
    void Dash()
    {
        if (canDash && !dashing)
        {
            directionVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; //Recebe o vetor dire��o para direcionar o dash
            if (directionVector != Vector2.zero)
            {
                mover = false; //Impede que a personagem se mova durante o dash
                dashing = true; //Informa que a personagem est� se movendo atrav�s da vari�vel
                canDash = false; //Impede que dashes sejam cont�nuos
                parede = false; //Caso esteja na parede, sai dela
                rb.gravityScale = 0; //Evita que a gravidade esteja errada
                dashRespostaTimeElapsed = 0;
                StartCoroutine("PosDash"); //Inicia coroutine que vai fazer a contagem do tempo
                sp.color = corPosDash;
                
                particulaDash.RotateParticula(directionVector, transform.position);
            }
        }
    } //Dash da personagem
    void WallClimb()
    {
        parede = true; //Informa que a personagem est� na parede
        mover = false; //Informa que a personagem n�o pode se mover de maneira convencional
        dashing = false; //Informa que a personagem n�o est� mais no dash
        rb.gravityScale = 0; //Como a gravidade est� em -30, colocar rb.velocity.y = 0 n�o funcionar� para parar a personagem.
        StopAllCoroutines(); //Caso ela estivesse no dash e apertar Z, parando o dash. 
        sp.material = normalMaterial;
        Debug.Log("Parou as coroutines");
    } //Faz a personagem come�ar a andar na parede
    void StopWallClimb()
    {
        parede = false; //Informa que a personagem saiu da parede
        mover = true; //Informa que a personagem pode se movimentar de maneira convencional agora.
        rb.gravityScale = 1; //Retorna a gravidade. 
    } //Para de fazer a personagem andar na parede. 
    IEnumerator PosDash() //Fun��o chamada ap�s o dash, fazendo uma transi��o para a movimenta��o normal. 
    {
        Vector2 alvo = new Vector2(directionVector.x * speed, directionVector.y * (dashForce / 2)); //Salva o vetor da velocidade que dever� ser o final da transi��o
        //O eixo X retornar� para a movimenta��o padr�o desejada durante o dash. 
        //O exio Y retornar� para metade da velocidade do dash, fazendo com que ainda tenha for�a para seguir um tempo mas que n�o seja pouca a ponto de parar o momento. 

        yield return new WaitForSeconds(dashTime); //Espera o final do dash.
        dashing = false; //Diz que a personagem n�o est� mais dando dash. 
        rb.gravityScale = 1; //Retorna a gravidade

        mover = true; //Reabilita a movimenta��o novamente
        rb.velocity = alvo; //Retorna a velocidade para o um valor de transi��o. 
    }
    IEnumerator PosWallJump() //Fun��o ap�s o pulo na parede, retornando para a movimenta��o normal e voltando para a parede se necess�rio
    {
        yield return new WaitForSeconds(wallJumpTime);
        if (Input.GetKey(KeyCode.Z))
        {
            parede = true;
            WallClimb();
        }
        else
        {
            mover = true;
        }
    }
    public void ResetDash() //Essa função é chamada quando o jogador retorna ao chão ou pega um cristal
    {
        canDash = true;
        StartCoroutine(FlashColor());
    }
    IEnumerator FlashColor() //Essa função é responsável por colocar um frame todo branco no personagem, quando ele recupera o dash.
    {
        sp.material = flashMaterial;
        sp.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sp.material = normalMaterial;
    }
    bool IsGrounded() //A fun��o gera um quadrado no p� da personagem do tamanho do colisor, e verifica se est� encostando no ch�o.
    {
        float extraHeight = 0.1f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, new Vector2(col.bounds.size.x-0.005f, col.bounds.size.y), 0, Vector2.down, extraHeight, groundLayer);
        return retorno;
    }
    int isOnWall() //A fun��o gera dois quadrados aos lados da personagem e verifica se est�o enconstsando nas paredes. 
    {
        float extraHeight = 0.15f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.left, extraHeight, groundLayer); //Quadrado da esquerda
        bool retorno2 = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.right, extraHeight, groundLayer); //Quadrado da direita
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
    private void OnCollisionEnter2D(Collision2D collision) //Caso o jogador faça colisão com um inimigo / buraco, ele retorna aqui. 
    {
        if(collision.gameObject.layer == 7)
        {
            transform.position = posInicial;
        }
    }
}