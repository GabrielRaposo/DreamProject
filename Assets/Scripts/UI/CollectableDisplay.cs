using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectableDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform tabAnchor;
    [SerializeField] private TextMeshProUGUI display;

    private const int HIDDEN_X  = -150;
    private const int HIGHLIGHT_X  = 40;

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

    private IEnumerator Show()
    {
        while (tabAnchor.anchoredPosition.x < HIGHLIGHT_X)
        {
            yield return new WaitForEndOfFrame();
            tabAnchor.anchoredPosition += Vector2.right * 20f;
        }
        tabAnchor.anchoredPosition = Vector2.right * HIGHLIGHT_X;
        yield return new WaitForSeconds(3);    

        StartCoroutine(Hide());
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

        StopAllCoroutines();
        StartCoroutine(Show());
    }

    public void AddScore(int quant)
    {
        Score += quant;
    }

    public void SaveScore()
    {
        savedScore = score;
        PlaytimeData.starsCount = score;
    }
}
