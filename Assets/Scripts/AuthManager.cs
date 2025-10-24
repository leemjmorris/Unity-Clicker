using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance => instance;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isInitialized = false;

    public FirebaseUser CurrentUser => currentUser;
    public bool IsLoggedIn => currentUser != null;

    public string UserId => currentUser?.UserId ?? string.Empty;
    public bool IsInitialized => isInitialized;

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
    }

    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitializationAsync();

        if (!FirebaseInitializer.Instance.IsInitialized)
        {
            Debug.LogError("[Auth] Firebase 초기화 실패로 인증 불가");
            return;
        }

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;

        currentUser = auth.CurrentUser;

        isInitialized = true;

        Debug.Log("[Auth] AuthManager 초기화 완료");
    }

    public async UniTask<(bool success, string error)> SignInAnonymouslyAsync()
    {
        try
        {
            Debug.Log("[Auth] 익명 로그인 시도...");
            AuthResult result = await auth.SignInAnonymouslyAsync().AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 익명 로그인 성공! UserID: {currentUser.UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            string friendlyError = ParseFirebaseError(ex.Message);
            Debug.LogError($"[Auth] 익명 로그인 오류: {friendlyError}");
            return (false, friendlyError);
        }
    }

    public async UniTask<(bool success, string error)> CreateUserWithEmailAsync(string email, string password)
    {
        try
        {
            Debug.Log("[Auth] 회원 가입 시도...");
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 회원 가입 성공! UserID: {currentUser.UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            string friendlyError = ParseFirebaseError(ex.Message);
            Debug.LogError($"[Auth] 회원 가입 오류: {friendlyError}");
            return (false, friendlyError);
        }
    }

    public async UniTask<(bool success, string error)> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            Debug.Log("[Auth] 로그인 시도...");
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 로그인 성공! UserID: {currentUser.UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            string friendlyError = ParseFirebaseError(ex.Message);
            Debug.LogError($"[Auth] 로그인 오류: {friendlyError}");
            return (false, friendlyError);
        }
    }

    public void SignOut()
    {
        if (auth != null && currentUser != null)
        {
            Debug.Log("[Auth] 로그아웃");
            auth.SignOut();
            currentUser = null;
        }
    }

    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            bool signedIn = auth.CurrentUser != null && auth.CurrentUser != currentUser;
            if (!signedIn && currentUser != null)
            {
                Debug.Log($"[Auth] 로그 아웃됨요: {currentUser.UserId}");
            }

            currentUser = auth.CurrentUser;

            if (currentUser == null)
            {
                Debug.Log("[Auth] 로그인 필요함요");
                return;
            }

            Debug.Log($"[Auth] 이미 로그인됨요: {currentUser.UserId}");
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private string ParseFirebaseError(string error)
    {
        if (string.IsNullOrEmpty(error)) return "알 수 없는 오류";

        // Firebase 에러 코드를 한글로 변환
        if (error.Contains("EMAIL_NOT_FOUND")) return "이메일을 찾을 수 없습니다";
        if (error.Contains("INVALID_PASSWORD")) return "비밀번호가 틀렸습니다";
        if (error.Contains("EMAIL_EXISTS")) return "이미 존재하는 이메일입니다";
        if (error.Contains("WEAK_PASSWORD")) return "비밀번호가 너무 약합니다 (최소 6자 이상)";
        if (error.Contains("INVALID_EMAIL")) return "유효하지 않은 이메일 형식입니다";
        if (error.Contains("USER_DISABLED")) return "비활성화된 계정입니다";
        if (error.Contains("TOO_MANY_REQUESTS")) return "너무 많은 시도. 잠시 후 다시 시도하세요";
        if (error.Contains("NETWORK_ERROR")) return "네트워크 연결을 확인하세요";

        return error; // 기본 에러 메시지
    }
}
