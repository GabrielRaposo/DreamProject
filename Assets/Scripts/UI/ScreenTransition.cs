using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
    [SerializeField] private Image screenfade;

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
    }

    public static void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Call(string scene)
    {
        Time.timeScale = 1;
        StopAllCoroutines();

        StartCoroutine(TransitionToScene(scene));
    }

    private IEnumerator TransitionToScene(string scene)
    {
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
        float time = .2f;
        yield return new WaitForSecondsRealtime(time);
        while (screenfade.color.a < 1)
        {
            yield return new WaitForEndOfFrame();
            Color color = screenfade.color;
            color.a += .05f;
            screenfade.color = color;
        }
        yield return new WaitForSeconds(time);
    }

    private IEnumerator TransitionOut()
    {
        float time = .2f;
        yield return new WaitForSecondsRealtime(time);
        while (screenfade.color.a > 0)
        {
            yield return new WaitForEndOfFrame();
            Color color = screenfade.color;
            color.a -= .05f;
            screenfade.color = color;
        }
        yield return new WaitForSeconds(time);
    }
}
