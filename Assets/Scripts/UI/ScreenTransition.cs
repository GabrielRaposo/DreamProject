using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ScreenTransition : MonoBehaviour
{
    private const int _X = 8, _Y = 5;

    [SerializeField] private Transform tilesAnchor; 

    private RectTransform[,] tiles; 

    public static ScreenTransition instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent);
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }
    
        tiles = new RectTransform[_X, _Y];
        int x, y;
        for(int i = x = y = 0; i < tilesAnchor.childCount; i++)
        {
            x = i % _X;
            y = (i / _X) % _Y;
            //Debug.Log("x: " + x + ", y: " + y);
            tiles[x,y] = tilesAnchor.GetChild(i).gameObject.GetComponent<RectTransform>();
        }
    }

    public static void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Call(string scene)
    {
        StopAllCoroutines();
        StartCoroutine(TransitionToScene(scene));
    }

    private IEnumerator TransitionToScene(string scene)
    {
        yield return new WaitForEndOfFrame(); // temporário para impedir que player pule quando volta pro menu pela pausa 
        Time.timeScale = 1;

        yield return TransitionIn();

        yield return new WaitForEndOfFrame();
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;
        while (async.progress < .9f)
        {
            yield return null;
        }
        async.allowSceneActivation = true;
        //EventSystem.current.enabled = false;

        yield return new WaitForSeconds(.2f);
        yield return TransitionOut();
        //EventSystem.current.enabled = true;
    }

    private IEnumerator TransitionIn()
    {
        yield return new WaitForSecondsRealtime(.2f);
        float time = .04f;

        int diagonal = 0;
        while(true)
        {
            int x = diagonal, y = 0;
            while(true)
            {  
                if(x < _X) tiles[x,y].DORotate(Vector3.zero, time);
                x--; 
                y++;
                
                if(x < 0 || y + 1  > _Y)
                {
                    yield return new WaitForSecondsRealtime(time);
                    break;
                }
                
            }
            diagonal++;
            if(diagonal + 1 > _X + _Y) break;
        }
        yield return new WaitForSecondsRealtime(.2f);
    }

    private IEnumerator TransitionOut()
    {
        yield return new WaitForSecondsRealtime(.2f);
        float time = .04f;

        int diagonal = 0;
        while(true)
        {
            int x = diagonal, y = 0;
            while(true)
            {  
                if(x < _X) tiles[x,y].DORotate(new Vector3(90,90), time);
                x--; 
                y++;
                
                if(x < 0 || y + 1  > _Y)
                {
                    yield return new WaitForSecondsRealtime(time);
                    break;
                }
                
            }
            diagonal++;
            if(diagonal + 1 > _X + _Y) break;
        }
        yield return new WaitForSecondsRealtime(.2f);
    }
}

