using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Esse script é chamado pelo botão do menu inicial, mudando a cena. 
public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void MudaCena(int i)
    {
        SceneManager.LoadScene(i);
    }
}
