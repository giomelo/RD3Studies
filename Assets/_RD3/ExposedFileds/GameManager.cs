using ToolsStudy;
using UnityEngine;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public Teste test;

    private void Awake()
    {
        test = new Teste();
        instance = this;
  
    }
}
