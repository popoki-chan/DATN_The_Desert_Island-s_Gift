using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FishWave
    {
        public string waveName;
        public GameObject fishPrefab;
        public int fishCount = 5;
        public float spawnDelay = 0.8f;
        public float swimSpeed = 4f;
    }

    [Header("Start Point & End Point")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Y-axis limits")]
    public float minY = -3f;
    public float maxY = -1f;

    [Header("Fish Wave")]
    public List<FishWave> waves;
    public float delayBetweenWaves = 3f;

    [Header("Toxic Fish Configurations")]
    public float toxicFishChance = 0.25f;
    public Color toxicColor = new Color(0.7f, 0.2f, 0.9f, 1f);

    [Header("Sine Wave Parameters")]
    public float minAmplitude = 0.2f;
    public float maxAmplitude = 0.6f;
    public float minFrequency = 1.5f;
    public float maxFrequency = 3.5f;

    [Header("Sprite-oriented Configuration")]
    public bool spriteFacesRightInitially = false;

    private int currentWaveIndex = 0;
    private bool isMinigameActive = false;

    void OnEnable()
    {
        StartMinigame();
    }

    void OnDisable()
    {
        isMinigameActive = false;
        StopAllCoroutines();

        // Kill active tweens and clean up spawned fish (tránh hủy startPoint và endPoint)
        foreach (Transform child in transform)
        {
            if (child == startPoint || child == endPoint) continue;
            child.DOKill();
            Destroy(child.gameObject);
        }
    }

    public void StartMinigame()
    {
        isMinigameActive = true;
        currentWaveIndex = 0;
        StartCoroutine(SpawnWaveRoutine());
    }

    public void StopFutureWaves()
    {
        isMinigameActive = false;
        StopAllCoroutines();
        //Debug.Log("<color=yellow>[Spawner]</color> Đã hủy lịch trình spawn các đợt cá tiếp theo.");
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (isMinigameActive && currentWaveIndex < waves.Count)
        {
            FishWave currentWave = waves[currentWaveIndex];

            for (int i = 0; i < currentWave.fishCount; i++)
            {
                if (!isMinigameActive) yield break;

                SpawnFish(currentWave);
                yield return new WaitForSeconds(currentWave.spawnDelay);
            }

            yield return new WaitForSeconds(delayBetweenWaves);
            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count) currentWaveIndex = 0;
        }
    }

    private void SpawnFish(FishWave wave)
    {
        if (wave.fishPrefab == null) return;

        // Chọn hướng bơi ngẫu nhiên: 50% bơi từ Trái sang Phải, 50% bơi từ Phải sang Trái
        bool isSwimmingRight = Random.value > 0.5f;
        
        // startPoint.x = 10 (bên phải), endPoint.x = -10 (bên trái)
        // Bơi sang phải (isSwimmingRight == true) nghĩa là đi từ endPoint (trái) sang startPoint (phải)
        Vector3 actualStart = isSwimmingRight ? endPoint.position : startPoint.position;
        Vector3 actualEnd = isSwimmingRight ? startPoint.position : endPoint.position;

        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(actualStart.x, randomY, 0);

        GameObject fish = Instantiate(wave.fishPrefab, spawnPos, Quaternion.identity, transform);

        // Chỉnh scale của Spine/Sprite theo hướng bơi
        float localScaleX = Mathf.Abs(fish.transform.localScale.x);
        if (isSwimmingRight != spriteFacesRightInitially) localScaleX *= -1f;
        fish.transform.localScale = new Vector3(localScaleX, fish.transform.localScale.y, 1);

        // Quyết định ngẫu nhiên xem có phải cá độc không
        bool isToxic = Random.value < toxicFishChance;
        if (isToxic)
        {
            var skeletonAnim = fish.GetComponent<Spine.Unity.SkeletonAnimation>();
            if (skeletonAnim != null)
            {
                skeletonAnim.Initialize(false);
                if (skeletonAnim.skeleton != null)
                {
                    skeletonAnim.skeleton.R = toxicColor.r;
                    skeletonAnim.skeleton.G = toxicColor.g;
                    skeletonAnim.skeleton.B = toxicColor.b;
                }
            }
            var mr = fish.GetComponent<MeshRenderer>();
            if (mr != null && mr.material != null)
            {
                mr.material.color = toxicColor;
            }
        }

        // Khởi tạo component điều khiển chuyển động hình Sin
        var behavior = fish.AddComponent<FishBehavior>();
        float randomAmp = Random.Range(minAmplitude, maxAmplitude);
        float randomFreq = Random.Range(minFrequency, maxFrequency);
        behavior.Initialize(
            new Vector3(actualStart.x, randomY, actualStart.z),
            new Vector3(actualEnd.x, randomY, actualEnd.z),
            wave.swimSpeed,
            randomAmp,
            randomFreq,
            isToxic
        );
    }
}