using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    private string SceneName;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.SetColor("_FaceColor", Color.black);
    }

    public void OnMouseEnter()
    {
        GetComponent<Renderer>().material.SetColor("_FaceColor", Color.cyan);
    }
    public void OnMouseExit()
    {
        GetComponent<Renderer>().material.SetColor("_FaceColor", Color.black);
    }
    public void OnMouseDown()
    {
        GetComponent<Renderer>().material.SetColor("_FaceColor", Color.clear);
        SceneManager.LoadScene(sceneName: SceneName);
    }
}
