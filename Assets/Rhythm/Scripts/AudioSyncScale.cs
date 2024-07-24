using System.Collections;
using UnityEngine;

public class AudioSyncScale : AudioSyncer
{
    public Vector3 restScale;
    public Vector3 beatScale;
    public BeatPlayer beatPlayer;

    private Transform m_Transform;
    private Coroutine m_PreviousCoroutine;

    private void OnEnable()
    {
        beatPlayer.OnBeat += OnBeat;
    }

    private void OnDisable()
    {
        beatPlayer.OnBeat -= OnBeat;
    }

    private void Start()
    {
        m_Transform = transform;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (m_HasBeated)
            return;

        m_Transform.localScale = Vector3.Lerp(m_Transform.localScale, restScale, restSmoothTime * Time.deltaTime);
    }

    private void OnBeat(Beat[] beats)
    {
        OnBeat();
    }

    public override void OnBeat()
    {
        base.OnBeat();

        if (m_PreviousCoroutine != null)
            StopCoroutine(m_PreviousCoroutine);

        m_PreviousCoroutine = StartCoroutine(MoveToScale(beatScale));
    }

    private IEnumerator MoveToScale(Vector3 targetScale)
    {
        Vector3 currentScale = m_Transform.localScale;
        Vector3 initialScale = currentScale;
        float timer = 0.0f;

        while (currentScale != targetScale)
        {
            currentScale = Vector3.Lerp(initialScale, targetScale, timer / timeToBeat);
            timer += Time.deltaTime;

            m_Transform.localScale = currentScale;

            yield return null;
        }

        m_HasBeated = false;
    }
}
