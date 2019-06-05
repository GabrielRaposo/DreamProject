using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthDisplay : MonoBehaviour
{
    [SerializeField] private Image heart;
    [SerializeField] private Image delayedHeart;

    RectTransform _rect;

	void Awake ()
    {
        _rect = GetComponent<RectTransform>();
        InstantRefill();
    }
    
    public void InstantRefill()
    {
        heart.fillAmount = 1;
        delayedHeart.fillAmount = 1;
    }

    public void ChangeFill(float amount, bool instant = false)
    {
        StopAllCoroutines();

        if(!instant)
        {
            if(heart.fillAmount > amount)
            {
                delayedHeart.fillAmount = heart.fillAmount;
                StartCoroutine(HorizontalTremble());
                StartCoroutine(DelayedDecrease(amount));
            }
            else if (heart.fillAmount < amount)
            {
                delayedHeart.fillAmount = amount;
                StartCoroutine(PumpScale());
            }
        }
        else
        {
            heart.fillAmount = delayedHeart.fillAmount = amount;
        }
    }

    IEnumerator PumpScale()
    {
        _rect.localScale = Vector3.one * 1.3f;
        float steps = 10;
        float fillStep = (delayedHeart.fillAmount - heart.fillAmount) / steps;
        while(_rect.localScale.x > 1f)
        {
            yield return new WaitForEndOfFrame();
            _rect.localScale -= Vector3.one * (.3f / steps);
            heart.fillAmount += fillStep;
        }
        _rect.localScale = Vector3.one * 1f;
    }

    IEnumerator HorizontalTremble()
    {
        _rect = GetComponent<RectTransform>();
        float
            force = 10f,
            originalX = _rect.anchoredPosition.x;

        _rect.anchoredPosition += Vector2.left * force / 2;
        for(int i = 0; i < 10; i++)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _rect.anchoredPosition += Vector2.right * force * (i % 2 == 0 ? 1 : -1);
            force -= .05f;
        }
        _rect.anchoredPosition = new Vector2(originalX, _rect.anchoredPosition.y);
    }

    IEnumerator DelayedDecrease(float amount)
    {
        yield return new WaitForSeconds(.1f);

        while (heart.fillAmount > amount)
        {
            yield return new WaitForFixedUpdate();
            heart.fillAmount -= .1f;
        }
        heart.fillAmount = amount;

        yield return new WaitForSeconds(.5f);
        while(delayedHeart.fillAmount > amount)
        {
            yield return new WaitForFixedUpdate();
            delayedHeart.fillAmount -= .01f;
        }
        delayedHeart.fillAmount = amount;
    }
}
