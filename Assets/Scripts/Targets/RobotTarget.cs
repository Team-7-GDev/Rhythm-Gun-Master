using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotTarget : Bot
{
    [SerializeField] private Material m_origMat;
    [SerializeField] private Material m_ghostMat;
    [SerializeField] private SkinnedMeshRenderer m_skinMeshRenderer;


    private void Start()
    {
        gameObject.SetActive(false);
    }

    public override void Initialize(float delayTime, Action<Bot> callback)
    {
        base.Initialize(delayTime, callback);

        gameObject.SetActive(true);
        if (delayTime >= 0)
        {
            m_skinMeshRenderer.material = m_ghostMat;
            for (int i = 0; i < m_colliders.Length; i++)
                m_colliders[i].enabled = false;

            StartCoroutine(ChangeMaterial(delayTime));
        }
        else
        {
            m_skinMeshRenderer.material = m_origMat;
            for (int i = 0; i < m_colliders.Length; i++)
                m_colliders[i].enabled = true;
        }
    }

    public override void Hide()
    {
        base.Hide();
        gameObject.SetActive(false);
    }

    private IEnumerator ChangeMaterial(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        m_skinMeshRenderer.material = m_origMat;
        for (int i = 0; i < m_colliders.Length; i++)
            m_colliders[i].enabled = true;
    }
}
