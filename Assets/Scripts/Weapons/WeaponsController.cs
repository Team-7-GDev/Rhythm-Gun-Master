using System;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class WeaponsController : MonoBehaviour
{

    [SerializeField]
    private enum WeaponType
    {
        PISTOL = 0,
        ASSULT_RIFLE = 1
    }

    [SerializeField] private WeaponType m_WeaponType;
    [SerializeField] private AudioClip m_FireAudioClip;
    [SerializeField] private GameObject m_hitEffectPrefab;
    [SerializeField] private AudioSource m_WeaponAudiosource;

    [Range(0, 1)]
    [SerializeField] private float m_FireVolume;

    [Range(0, 20)]
    [SerializeField] private float m_YForce;


    public static Action<float> WeaponYForce = null;


    private static readonly int RIFLE_FIRE_ANIM_HASH = Animator.StringToHash("Shot_Rifle");
    private static readonly int PISTOL_FIRE_ANIM_HASH = Animator.StringToHash("Shot_Pistol");


    private Camera m_MainCamera = null;
    private Animator m_WeaponAnimator = null;



    void Awake()
    {
        m_MainCamera = Camera.main;
        m_WeaponAnimator = GetComponent<Animator>();
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
            // fire.
            m_WeaponAudiosource.PlayOneShot(m_FireAudioClip);
            m_WeaponAnimator.Play(PISTOL_FIRE_ANIM_HASH, 0, 0.05f);

            PlaySFX();
            CalculateHit();

            WeaponYForce?.Invoke(m_YForce);
        }
    }


    float deltaTime = 0;
    int m_FireCount = 0;

    // rate of fire of m14 is 700/sec
    private void ControlRifle()
    {
        if (Input.GetMouseButton(0) && m_FireCount > 0)
        {
            deltaTime += Time.deltaTime;
            if (deltaTime >= 0.1f)
            {
                // fire
                m_FireCount--;
                m_WeaponAudiosource.PlayOneShot(m_FireAudioClip);
                m_WeaponAnimator.Play(RIFLE_FIRE_ANIM_HASH, 0, 0.01f);
                deltaTime = Mathf.Repeat(deltaTime, 0.1f);

                PlaySFX();
                CalculateHit();

                WeaponYForce?.Invoke(m_YForce);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // reset the fire count;
            m_FireCount = 3;
        }
    }

    private void CalculateHit()
    {
        Ray ray = m_MainCamera.ViewportPointToRay(Vector3.one * 0.5f);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, 1 << LayerMask.NameToLayer("Enemy")))
        {
            switch (hitInfo.collider.tag)
            {
                case "Torso":
                    Debug.Log($"Torso hit: {hitInfo.collider.name} - {hitInfo.transform.root.name}...");
                    hitInfo.transform.root.gameObject.SetActive(false);
                    break;

                case "Head":
                    Debug.Log($"Head Hit: {hitInfo.collider.name} - {hitInfo.transform.root.name}...");
                    hitInfo.transform.root.root.gameObject.SetActive(false);
                    break;
            }

            GameObject spawnedHit = Instantiate(m_hitEffectPrefab);

            spawnedHit.transform.LookAt(Camera.main.transform);
            spawnedHit.transform.position = hitInfo.point;
        }
    }

    private void PlaySFX()
    {
        if (m_FireAudioClip != null && m_WeaponAudiosource != null)
            m_WeaponAudiosource.PlayOneShot(m_FireAudioClip, m_FireVolume);
    }
}
