using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;


public class PointText : MonoBehaviour
{
    [SerializeField] private TMP_Text m_pointText;


    /* private IEnumerator Start()
    {
        yield return new WaitForSeconds(2.0f);
        ShowPoint(3, transform.position);
    } */

    public void ShowPoint(int point, Vector3 worldPos)
    {
        Color color = Color.white;

        gameObject.SetActive(true);
        transform.position = worldPos;
        m_pointText.text = $"+{point}";

        transform.DOScale(2.0f, 2.0f).SetEase(Ease.OutQuint);
        transform.DOMoveY(worldPos.y + 0.5f, 2.0f).SetEase(Ease.OutQuint);
        DOTween.To((float x) =>
        {
            color.a = x;
            m_pointText.color = color;

        }, 1.0f, 0.0f, 2.0f).SetEase(Ease.OutQuint)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
