using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript2 : MonoBehaviour
{
    //Script do projeto melhorado, para ajudar os alunos a continuar

    [Header("Movimentação Padrão")]
    [SerializeField] LayerMask groundLayer;
    public float speed;
    public float jumpForce;
    public float dashForce;
    public float dashTime;
    public float wallSpeed;
    public float climbTime;
    public float wallJumpTime;

    [Header("Booleanas de controle")]
    public bool dashing = false;
    public bool canDash = true;
    public bool wall = false;
    public bool pulando = false;
    public bool mover = true;
    public bool chao = true;
    public bool parede = false;

    [Header("Melhorar Respostas do jogo")]
    public float respostaTime;
    public float coyoteTime;
    public float dashRespostaTime;
    public float wallJumpRespostaTime;

    float respostaTimeElapsed = 0, coyoteTimeElapsed = 0, climbTimeElapsed = 0, dashRespostaTimeElapsed = 0;

    [Header("Controle no ar da personagem")]
    public float wallJumpLerp;
    public float fallGravityMultiplier;
    public float midFallGravityMultiplier;


    Rigidbody2D rb;
    BoxCollider2D col;
    float direction, directionY;
    Vector2 directionVector;
    int paredeValor;
    Vector2 posInicial;

    Animator anim;
    SpriteRenderer sp;

    [Header("Finalização")]
    [SerializeField] Particula particulaPular, particulaDash;

    void Start()
    {
        //Pega os componentes para que não precisarmos colocar dentro do editor. 
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
        sp = GetComponentInChildren<SpriteRenderer>();
        posInicial = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        chao = IsGrounded(); //Salva em uma variável, para que não seja preciso chamar a função mais de uma vez por frame.
        paredeValor = isOnWall();
        wall = paredeValor != 0; //Salva o valor da função em uma variável, para não chamar mais de uma vez por frame.

        if (chao)
        {
            rb.gravityScale = 1;
            if(coyoteTimeElapsed != coyoteTime)
                coyoteTimeElapsed = coyoteTime; //Caso esteja no chão, reseta o CoyoteTime

            if(climbTimeElapsed != climbTime)
                climbTimeElapsed = climbTime; //Reseta o tempo de parede

            if (!dashing)
                canDash = true; //Caso não esteja dando Dash, reabilita o dash. 
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
                wall = false;

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

    void AtualizaAnimator() //Atualiza os valores do animator para as animações funcionarem corretamente
    {
        anim.SetFloat("Velocidade", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Dashing", dashing);
        anim.SetBool("NoAr", !chao);
        anim.SetBool("WallSlide", wall);
        anim.SetBool("WallClimb", rb.velocity.y > 0.5);
        if (rb.velocity.x > 0 || paredeValor == -1)
        {
            sp.flipX = false;
        } else if(rb.velocity.x < 0 || paredeValor == 1)
        {
            sp.flipX = true;
        }
    }

    //Aqui, a função é chamada em tempos fixos. Assim, é mais confiável para aplicar comandos envolvendo a física. 
    private void FixedUpdate()
    {
        if (mover)
        {
            if (chao)
                rb.velocity = new Vector2(direction * speed, rb.velocity.y); //Movimentação padrão
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(direction * speed, rb.velocity.y), wallJumpLerp * Time.deltaTime);
        }
        else if (dashing)
        {
            rb.velocity = directionVector * dashForce; //Função do dash
        }
        else if (parede)
        {
            rb.velocity = new Vector2(0, directionY * wallSpeed); //Função de subir a parede
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
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); //Função do pulo. 
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
            rb.AddForce(direcaoPulo * jumpForce, ForceMode2D.Impulse); //Função do pulo. 
            StartCoroutine(PosWallJump());
            respostaTimeElapsed = 0; //Impede que essa parte do código seja chamada novamente. 
        }
    } //Pulo da personagem

    void Dash()
    {
        if (canDash && !dashing)
        {
            directionVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; //Recebe o vetor direção para direcionar o dash
            if (directionVector != Vector2.zero)
            {
                mover = false; //Impede que a personagem se mova durante o dash
                dashing = true; //Informa que a personagem está se movendo através da variável
                canDash = false; //Impede que dashes sejam contínuos
                parede = false; //Caso esteja na parede, sai dela
                rb.gravityScale = 0; //Evita que a gravidade esteja errada
                dashRespostaTimeElapsed = 0;
                StartCoroutine("PosDash"); //Inicia coroutine que vai fazer a contagem do tempo
                
                particulaDash.RotateParticula(directionVector, transform.position);
            }
        }
    } //Dash da personagem

    void WallClimb()
    {
        parede = true; //Informa que a personagem está na parede
        mover = false; //Informa que a personagem não pode se mover de maneira convencional
        dashing = false; //Informa que a personagem não está mais no dash
        rb.gravityScale = 0; //Como a gravidade está em -30, colocar rb.velocity.y = 0 não funcionará para parar a personagem.
        StopAllCoroutines(); //Caso ela estivesse no dash e apertar Z, para o dash. 
        Debug.Log("Parou as coroutines");
    } //Faz a personagem começar a andar na parede

    void StopWallClimb()
    {
        parede = false; //Informa que a personagem saiu da parede
        mover = true; //Informa que a personagem pode se movimentar de maneira convencional agora.
        rb.gravityScale = 1; //Retorna a gravidade. 
    } //Para de fazer a personagem andar na parede. 

    IEnumerator PosDash() //Função chamada após o dash, fazendo uma transição para a movimentação normal. 
    {
        Vector2 alvo = new Vector2(directionVector.x * speed, directionVector.y * (dashForce / 2)); //Salva o vetor da velocidade que deverá ser o final da transição
        //O eixo X retornará para a movimentação padrão desejada durante o dash. 
        //O exio Y retornará para metade da velocidade do dash, fazendo com que ainda tenha força para seguir um tempo mas que não seja pouca a ponto de parar o momento. 

        yield return new WaitForSeconds(dashTime); //Espera o final do dash.
        dashing = false; //Diz que a personagem não está mais dando dash. 
        rb.gravityScale = 1; //Retorna a gravidade

        mover = true; //Reabilita a movimentação novamente
        rb.velocity = alvo; //Retorna a velocidade para o um valor de transição. 
    }

    IEnumerator PosWallJump() //Função após o pulo na parede, retornando para a movimentação normal e voltando para a parede se necessário
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


    bool IsGrounded() //A função gera um quadrado no pé da personagem do tamanho do colisor, e verifica se está encostando no chão.
    {
        float extraHeight = 0.1f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, new Vector2(col.bounds.size.x-0.005f, col.bounds.size.y), 0, Vector2.down, extraHeight, groundLayer);
        return retorno;
    }

    int isOnWall() //A função gera dois quadrados aos lados da personagem e verifica se estão enconstsando nas paredes. 
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 7)
        {
            transform.position = posInicial;
        }
    }
}