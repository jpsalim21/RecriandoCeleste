using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    //Script que será ensinado durante o minicurso

    [Header("Movimentacao Padrao")]
    [SerializeField] LayerMask groundLayer;
    public float velocidade;
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

    [Header("Controle no ar da personagem")]
    public float wallJumpLerp;
    public float fallGravityMultiplier;
    public float midFallGravityMultiplier;


    float respostaTimeElapsed = 0, coyoteTimeElapsed = 0, climbTimeElapsed = 0, dashRespostaTimeElapsed = 0;
    Rigidbody2D rb;
    BoxCollider2D col;
    float direction, directionY;
    Vector2 directionVector;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Salva valores para usar mais vezes durante o frame.
        chao = IsGrounded(); 
        int paredeValor = isOnWall();
        wall = paredeValor != 0? true : false; 


        //Reseta o CoyoteTime, tempo de escalada e o dash
        if (chao)
        {
            coyoteTimeElapsed = coyoteTime; 
            climbTimeElapsed = climbTime; 
            if(!dashing)
                canDash = true;
        }

        //Input para o pulo
        if(Input.GetKeyDown(KeyCode.C)) 
        {
            respostaTimeElapsed = respostaTime; 
        }

        //Executa o pulo
        if(respostaTimeElapsed > 0) 
        {
            respostaTimeElapsed -= Time.deltaTime;

            Jump(paredeValor);
        }

        //Decresce o CoyoteTime
        if(coyoteTimeElapsed > 0)
        {
            coyoteTimeElapsed -= Time.deltaTime; 
        }

        //Input do dash
        if (Input.GetKeyDown(KeyCode.X))
        {
            dashRespostaTimeElapsed = dashRespostaTime;
        }

        //Executa o dash
        if (dashRespostaTimeElapsed > 0) 
        {
            dashRespostaTimeElapsed -= Time.deltaTime;
            Dash();
        }

        //Descresce a estamina de acordo com o tempo em parede
        if (wall)
        {
            if (climbTimeElapsed > 0)
                climbTimeElapsed -= Time.deltaTime;
            else
                wall = false;
        }

        //Input para escalar a parede
        if(Input.GetKeyDown(KeyCode.Z) && wall)
        {
            WallClimb(); 
        } else if((Input.GetKeyUp(KeyCode.Z) || !wall) && parede)
        {
            StopWallClimb();
        }


        //Aumenta a escala da gravidade ao cair, melhorando o pulo
        if (rb.velocity.y < -0.5f && mover && !(coyoteTimeElapsed > 0))
        {
            rb.gravityScale = fallGravityMultiplier;
            pulando = false;
        }

        //Aumenta a gravidade caso o jogador pare de apertar a tecla, limitando a altura do pulo
        if (pulando && !Input.GetKey(KeyCode.C) && mover)
        {
            rb.gravityScale = midFallGravityMultiplier;
            pulando = false;
        }



        //Inputs de movimentação
        direction = Input.GetAxisRaw("Horizontal");
        directionY = Input.GetAxisRaw("Vertical");  

    }

    //Aqui, a função é chamada em intervalos fixos. Assim, é mais confiável para executar os comandos do Rigidbody.
    private void FixedUpdate()
    {
        if (mover)
        {
            if (chao)
                rb.velocity = new Vector2(direction * velocidade, rb.velocity.y); //Movimentação padrão
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(direction * velocidade, rb.velocity.y), wallJumpLerp * Time.deltaTime);
        } else if(dashing)
        {
            rb.velocity = directionVector * dashForce; //Executa o dash
        } else if (parede)
        {
            rb.velocity = new Vector2(0, directionY * wallSpeed); //Executa a escalada
        }
    }

    void Jump(int paredeValor) //Executa o pulo
    {
        if (coyoteTimeElapsed > 0) //Primeiro pulo
        {
            coyoteTimeElapsed = 0;
            rb.gravityScale = 1;
            pulando = true;
            rb.velocity = new Vector2(rb.velocity.x, 0); //Reseta a velocidade no eixo Y, para que o pulo tenha sempre a mesma altura
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); 
            respostaTimeElapsed = 0; 
        }
        else if (wall) //Pulo na parede
        {
            mover = false; 
            rb.gravityScale = 1; 
            pulando = true;
            parede = false;
            coyoteTimeElapsed = 0;
            respostaTimeElapsed = 0;

            Vector2 direcaoPulo = new Vector2(paredeValor, 1).normalized;
            if (Input.GetKey(KeyCode.Z) && parede && direction != paredeValor)
            {
                direcaoPulo = Vector2.up;
            }
            rb.velocity = new Vector2(0, 0); //Reseta a velocidade no eixo Y, para que o pulo tenha sempre a mesma altura
            rb.AddForce(direcaoPulo * jumpForce, ForceMode2D.Impulse); //Função do pulo. 
            StartCoroutine(PosWallJump());
        }
    } 

    void Dash() //Inicia o dash
    {
        if (canDash && !dashing)
        {
            directionVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; //Recebe o vetor dire��o para direcionar o dash
            if (directionVector != Vector2.zero)
            {
                mover = false;
                dashing = true; 
                canDash = false; 
                parede = false; 
                rb.gravityScale = 0; 
                dashRespostaTimeElapsed = 0;
                StartCoroutine("PosDash");
            }
        }
    } 

    void WallClimb() //Setup da escalada
    {
        parede = true; 
        mover = false; 
        dashing = false; 
        rb.gravityScale = 0; 
        StopAllCoroutines(); 
    } 

    void StopWallClimb() //Para a escalada
    {
        parede = false; 
        mover = true; 
        rb.gravityScale = 1;  
    } 

    IEnumerator PosDash() //Retorna a movimentação normal após o dash
    {
        Vector2 alvo = new Vector2(directionVector.x * velocidade, directionVector.y * (dashForce / 2)); 

        yield return new WaitForSeconds(dashTime); 
        dashing = false;  
        rb.gravityScale = 1; 

        mover = true; 
        rb.velocity = alvo; 
    }

    IEnumerator PosWallJump() //Função após o pulo na parede, retornando para a movimentação normal e voltando para a parede se necessário
    {
        yield return new WaitForSeconds(wallJumpTime);
        if (Input.GetKey(KeyCode.Z))
        {
            parede = true;
            WallClimb();
        } else
        {
            mover = true;
        }
    }

    bool IsGrounded() //Retorna se a personagem está no chão
    {
        float extraHeight = 0.1f;
        bool retorno = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.down, extraHeight, groundLayer);
        return retorno;
    }

    int isOnWall() //retorna a direção contrária à parede encostada. 0 se não existir parede. 
    {
        float extraHeight = 0.1f; //Espessura do colisor além da colisão do jogador.
        bool retorno = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.left, extraHeight, groundLayer);
        bool retorno2 = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0, Vector2.right, extraHeight, groundLayer);
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
}