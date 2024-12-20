using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


public class TorsoTarget : Bot
{
    private void Start()
    {
        transform.rotation = Quaternion.AngleAxis(-90f, Vector3.right) * transform.rotation;
        for (int i = 0; i < m_colliders.Length; i++)
            m_colliders[i].enabled = false;
    }

    public override void Initialize(float delayTime, Action<Bot> callback)
    {
        base.Initialize(delayTime, callback);
        transform.DOLocalRotateQuaternion(Quaternion.Euler(0, 180, 0), delayTime).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            for (int i = 0; i < m_colliders.Length; i++)
                m_colliders[i].enabled = true;
        });
    }

    public override void Hide()
    {
        transform.DOLocalRotateQuaternion(Quaternion.Euler(-90, 180, 0), 0.125f).SetEase(Ease.Linear).OnComplete(() =>
        {
            base.Hide();
            for (int i = 0; i < m_colliders.Length; i++)
                m_colliders[i].enabled = false;
        });
    }
}
