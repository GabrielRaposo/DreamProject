using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BonusCollectableDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform tabAnchor;
    [SerializeField] private TextMeshProUGUI display;

    [Header("Effects")]
    [SerializeField] private Color completeColor;
    [SerializeField] private AudioSource collectSFX;
 
    private const int HIDDEN_X  = -200;
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

    public static BonusCollectableDisplay instance;

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
        StopAllCoroutines();

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
            int maxScore = GameplayData.currentWorld == 1 ? GameplayData.world1MoonMax : GameplayData.world2MoonMax;
            display.text = Score.ToString() + "/" + maxScore.ToString();
            if (Score > maxScore - 1) display.color = completeColor;
        }

        StartCoroutine(ShowAndHide());
    }

    public void AddScore(int quant)
    {
        Score += quant;

        collectSFX.PlayDelayed(.1f);
        StartCoroutine(BGMVolumeDelay());
    }

    private IEnumerator BGMVolumeDelay()
    {
        BGMPlayer bgmPlayer = BGMPlayer.instance;
        if(bgmPlayer)
        {
            BGMPlayer.instance.LowerVolume();
            yield return new WaitForSeconds(2);
            BGMPlayer.instance.RiseVolume();
        }
    }

    public void SaveScore()
    {
        savedScore = score;
        PlaytimeData.starsCount = score;
    }
}
