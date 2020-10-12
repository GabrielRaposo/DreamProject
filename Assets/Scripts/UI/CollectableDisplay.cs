using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectableDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform tabAnchor;
    [SerializeField] private TextMeshProUGUI display;

    [Header("Effects")]
    [SerializeField] private AudioSource coinGetSFX;
    [SerializeField] private AudioSource coinLoseSFX;
 
    private const int HIDDEN_X  = -200;
    private const int HIGHLIGHT_X  = 40;

    private Coroutine showAndHideCoroutine;
    private bool onDecreaseAnimation;

    public static int savedScore = 0;

    private int score;
    public int Score
    {
        get
        {
            return score;
        }

        private set
        {
            score = value;
            UpdateDisplay();
        }
    }

    public static CollectableDisplay instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        Score = savedScore;
    }

    private void Start() 
    {
        tabAnchor.anchoredPosition = Vector2.right * HIDDEN_X;
    }

    private IEnumerator ShowAndHide()
    {
        yield return Show();
        yield return new WaitForSeconds(3);
        yield return Hide();
    }

    public void ToggleVisible(bool value)
    {
        if (showAndHideCoroutine != null) StopCoroutine(showAndHideCoroutine);

        if(value)
            StartCoroutine(Show());
        else 
            StartCoroutine(Hide());
    }

    private IEnumerator Show()
    {
        while (tabAnchor.anchoredPosition.x < HIGHLIGHT_X)
        {
            yield return new WaitForEndOfFrame();
            tabAnchor.anchoredPosition += Vector2.right * 20f;
        }
        tabAnchor.anchoredPosition = Vector2.right * HIGHLIGHT_X;
    }

    private IEnumerator Hide()
    {
        while (tabAnchor.anchoredPosition.x > HIDDEN_X)
        {
            yield return new WaitForEndOfFrame();
            tabAnchor.anchoredPosition += Vector2.left * 15f;
        }
        tabAnchor.anchoredPosition = Vector2.right * HIDDEN_X;
    }

    private void UpdateDisplay()
    {
        if (display)
        {
            display.text = Score.ToString();
        }

        if (showAndHideCoroutine != null) StopCoroutine(showAndHideCoroutine);
        if (!onDecreaseAnimation) showAndHideCoroutine = StartCoroutine(ShowAndHide());
    }

    public void AddScore(int quant)
    {
        Score += quant;
        coinGetSFX.Play();
    }

    public void SaveScore()
    {
        savedScore = score;
        PlaytimeData.starsCount = score;
    }

    public bool BuyAt (int price)
    {
        if (price > score) 
            return false;
        else 
        {
            StopAllCoroutines();
            StartCoroutine(DecreaseValue(price));
            return true;
        }
    }

    private IEnumerator DecreaseValue(int price)
    {
        yield return Show();    

        coinLoseSFX.Play();
        onDecreaseAnimation = true;
        for (int i = 0; i < price; i++)
        {
            if (display)
            {
                display.text = (Score - i).ToString();
            }
            yield return new WaitForEndOfFrame();
        }
        Score -= price;
        onDecreaseAnimation = false;
        coinLoseSFX.Stop();

        yield return new WaitForSecondsRealtime(2);
        yield return Hide();
    }
}
