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

        // Kill active tweens and clean up spawned fish
        foreach (Transform child in transform)
        {
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

        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(startPoint.position.x, randomY, 0);

        GameObject fish = Instantiate(wave.fishPrefab, spawnPos, Quaternion.identity, transform);

        bool isSwimmingRight = endPoint.position.x > startPoint.position.x;
        float localScaleX = Mathf.Abs(fish.transform.localScale.x);

        if (isSwimmingRight != spriteFacesRightInitially) localScaleX *= -1f;
        fish.transform.localScale = new Vector3(localScaleX, fish.transform.localScale.y, 1);

        float distance = Vector3.Distance(startPoint.position, new Vector3(endPoint.position.x, randomY, 0));
        float duration = distance / wave.swimSpeed;

        // Cá tịnh tiến ra đích và tự hủy khi khuất màn hình
        fish.transform.DOMoveX(endPoint.position.x, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(fish));
    }
}