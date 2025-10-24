using UnityEngine;
using Firebase;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

public class FirebaseInitializer : MonoBehaviour
{
    // 싱글톤 instance
    private static FirebaseInitializer instance;
    public static FirebaseInitializer Instance => instance;

    // Firebase 초기화 상태
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    // FirebaseApp 인스턴스
    private FirebaseApp firebaseApp;
    public FirebaseApp FirebaseApp => firebaseApp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirebaseAsync().Forget();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private async UniTaskVoid InitializeFirebaseAsync()
    {
        Debug.Log("[Firebase] 초기화 시작");

        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            if (status == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                isInitialized = true;
                
                Debug.Log($"[Firebase] 초기화 성공! {firebaseApp.Name}");
            }
            else
            {
                Debug.LogError($"[Firebase] 초기화 오류: {status}");
                isInitialized = false;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] 초기화 오류: {ex.Message}");
            isInitialized = false;
        }
    }

    public async UniTask WaitForInitializationAsync()
    {
        await UniTask.WaitUntil(() => isInitialized);
    }
}