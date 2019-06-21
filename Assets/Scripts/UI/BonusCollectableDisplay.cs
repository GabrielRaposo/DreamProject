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

    public int score { get; private set; }

    public static BonusCollectableDisplay instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start() 
    {
        tabAnchor.anchoredPosition = Vector2.right * HIDDEN_X;
        score = BonusCollectableList.GetWorldCollectedCount(GameplayData.currentWorld);
        UpdateDisplay();
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

    public void AddValue(int value)
    {
        score += value;
        UpdateDisplay(value == 0);
    }

    public void UpdateDisplay(bool toneDownAnimation = false)
    {
        if (display)
        {
            int maxScore = GameplayData.currentWorld == 1 ? GameplayData.world1Data.moonMax : GameplayData.world2Data.moonMax;

            display.text = score.ToString() + "/" + maxScore.ToString();
            if (score > maxScore - 1) display.color = completeColor;
        }

        collectSFX.PlayDelayed(.1f);
        if(toneDownAnimation) StartCoroutine(BGMVolumeDelay());
        StartCoroutine(ShowAndHide());
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
}
