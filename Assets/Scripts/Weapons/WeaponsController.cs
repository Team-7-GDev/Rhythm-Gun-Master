using System;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class WeaponsController : MonoBehaviour
{
    public Action OnWeaponFired;
    public Action<bool, Vector3> OnBotHit;

    [SerializeField]
    private enum WeaponType
    {
        PISTOL = 0,
        ASSULT_RIFLE = 1
    }


    [SerializeField] private Camera m_weaponCamera;
    [SerializeField] private WeaponType m_WeaponType;
    [SerializeField] private AudioClip m_FireAudioClip;
    [SerializeField] private GameObject m_hitEffectPrefab;
    [SerializeField] private AudioSource m_WeaponAudiosource;
    [SerializeField] private Transform m_weaponMuzzleFlash;
    [SerializeField] private LineRenderer m_trailRenderer;

    [Range(0, 20)]
    [SerializeField] private float m_YForce;


    public static Action<float> WeaponYForce = null;

    private static readonly int RIFLE_FIRE_ANIM_HASH = Animator.StringToHash("Shot_Rifle");
    private static readonly int PISTOL_FIRE_ANIM_HASH = Animator.StringToHash("Shot_Pistol");


    private float m_FireVolume;
    private Camera m_MainCamera = null;
    private Animator m_WeaponAnimator = null;
    private Coroutine m_muzzleFlashRoutine = null;



    private void OnVolumeChangeHandler(float currVolume) => m_FireVolume = currVolume;


    void Awake()
    {
        m_MainCamera = Camera.main;
        m_trailRenderer.enabled = false;
        m_weaponMuzzleFlash.gameObject.SetActive(false);
        m_WeaponAnimator = GetComponentInChildren<Animator>();

        m_FireVolume = PlayerPrefs.GetInt("g_volume", 100) / 100.0f;

        SettingsOverlayScreen.OnVolumeChange += OnVolumeChangeHandler;
    }

    private void OnDestroy()
    {
        SettingsOverlayScreen.OnVolumeChange -= OnVolumeChangeHandler;
    }

    private void Update()
    {
        if (GameManager.IsGameOver || GameManager.IsGamePause)
            return;

        if (m_WeaponType == WeaponType.PISTOL)
            ControlPistol();
        else if (m_WeaponType == WeaponType.ASSULT_RIFLE)
            ControlRifle();
    }


    private void ControlPistol()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Fire.
            m_WeaponAnimator.Play(PISTOL_FIRE_ANIM_HASH, 0, 0.05f);

            PlaySFX();
            CalculateHit();

            WeaponYForce?.Invoke(m_YForce);
        }
    }


    float deltaTime = 0;
    int m_FireCount = 0;

    // Rate of fire of m14 is 700/sec
    private void ControlRifle()
    {
        if (Input.GetMouseButton(0) && m_FireCount > 0)
        {
            deltaTime += Time.deltaTime;
            if (deltaTime >= 0.1f)
            {
                // fire
                m_FireCount--;
                m_WeaponAnimator.Play(RIFLE_FIRE_ANIM_HASH, 0, 0.01f);
                deltaTime = Mathf.Repeat(deltaTime, 0.1f);

                PlaySFX();
                CalculateHit();

                WeaponYForce?.Invoke(m_YForce);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Reset the fire count;
            m_FireCount = 3;
        }
    }

    private void CalculateHit()
    {
        Vector3 toPos = Vector3.zero;
        Ray ray = m_MainCamera.ViewportPointToRay(Vector3.one * 0.5f);
        if (Physics.SphereCast(ray, 0.009f, out RaycastHit hitInfo, 100f, 1 << LayerMask.NameToLayer("Enemy")))
        {
            Vector3 hitPoint = hitInfo.point;
            Bot bot = hitInfo.transform.GetComponentInParent<Bot>();

            hitPoint.y = 0;
            toPos = hitInfo.point;

            switch (hitInfo.collider.tag)
            {
                case "Head":
                    bot.OnHit(hitPoint);
                    OnBotHit?.Invoke(true, hitPoint);
                    Debug.Log($"Head Hit: {hitInfo.collider.name} - {hitInfo.transform.root.name}...");
                    // hitInfo.transform.root.root.gameObject.SetActive(false);
                    break;

                case "Torso":
                    bot.OnHit(hitPoint);
                    OnBotHit?.Invoke(false, hitPoint);
                    Debug.Log($"Torso hit: {hitInfo.collider.name} - {hitInfo.transform.root.name}...");
                    // hitInfo.transform.root.gameObject.SetActive(false);
                    break;
            }

            GameObject spawnedHit = Instantiate(m_hitEffectPrefab);

            spawnedHit.transform.LookAt(Camera.main.transform);
            spawnedHit.transform.position = hitInfo.point;
        }
        else
        {
            OnWeaponFired?.Invoke();
            toPos = m_MainCamera.transform.position + m_MainCamera.transform.forward * 8.0f;
        }

        if (m_muzzleFlashRoutine == null)
            m_muzzleFlashRoutine = StartCoroutine(PlayMuzzleFlashAndTrail(toPos));
    }

    private void PlaySFX()
    {
        if (m_FireAudioClip != null && m_WeaponAudiosource != null)
            m_WeaponAudiosource.PlayOneShot(m_FireAudioClip, m_FireVolume);
    }

    private IEnumerator PlayMuzzleFlashAndTrail(Vector3 toPos)
    {
        Vector3 viewPoint = m_weaponCamera.WorldToViewportPoint(m_weaponMuzzleFlash.position);

        // viewPoint = m_MainCamera.ViewportPointToRay(viewPoint).GetPoint(viewPoint.z * 2);
        viewPoint = m_MainCamera.ViewportToWorldPoint(viewPoint);

        m_trailRenderer.enabled = true;
        // m_trailRenderer.SetPosition(0, Vector3.Lerp(viewPoint, toPos, UnityEngine.Random.Range(0.05f, 0.1f)));
        m_trailRenderer.SetPosition(0, viewPoint);
        m_trailRenderer.SetPosition(1, Vector3.Lerp(viewPoint, toPos, UnityEngine.Random.Range(0.75f, 0.9f)));

        m_weaponMuzzleFlash.gameObject.SetActive(true);
        m_weaponMuzzleFlash.localScale = Vector3.one * UnityEngine.Random.Range(0.7f, 1.2f);
        m_weaponMuzzleFlash.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.forward);

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.1f));

        m_trailRenderer.enabled = false;
        m_weaponMuzzleFlash.gameObject.SetActive(false);

        m_muzzleFlashRoutine = null;
    }
}
