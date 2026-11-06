using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDirectionalLight : MonoBehaviour
{
    [SerializeField] private float delayLight = 1.2f;  //reference to PlayerSpawner.cs
    private Camera mainCamera;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    void Start()
    {
        AssignCamera();
    }

    void Update()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(InputManager.PointerPosition);
        Vector2 direction = (Vector2)mousePosition - (Vector2)transform.position;
        transform.up = direction;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach(Transform child in transform)   child.gameObject.SetActive(false);
        AssignCamera();
        StartCoroutine(DelayLight());
    }

    private IEnumerator DelayLight()
    {
        yield return new WaitForSeconds(delayLight);
        foreach(Transform child in transform)   child.gameObject.SetActive(true);
    }

    private void AssignCamera()
    {
        mainCamera = Camera.main;
    }
}