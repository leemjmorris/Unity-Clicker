using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks.CompilerServices;
using Cysharp.Threading.Tasks.Triggers;

public class LoginUI : MonoBehaviour
{
    public GameObject loginPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button anonymousLoginButton;
    [SerializeField] private Button profileButton;

    [Header("Feedback UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI profileText;

    private bool isProcessing = false;

    //LMJ: Common authentication handling method
    private async UniTask HandleAuthOperation(
        string operationName,
        System.Func<UniTask<(bool success, string error)>> authOperation,
        string processingMessage,
        string successMessage)
    {
        if (isProcessing) return;

        isProcessing = true;
        SetButtonsInteractable(false);
        ShowMessage(processingMessage, false);

        var (success, error) = await authOperation();

        SetButtonsInteractable(true);
        isProcessing = false;

        if (success)
        {
            ShowMessage(successMessage, false);
            await UniTask.Delay(500);
            OnLoginSuccess();
        }
        else
        {
            ShowMessage($"{operationName} 실패: {error}", true);
        }
    }

    private void Start()
    {
        if (loginButton != null) loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        if (registerButton != null) registerButton.onClick.AddListener(() => OnRegisterButtonClicked().Forget());
        if (anonymousLoginButton != null) anonymousLoginButton.onClick.AddListener(() => OnAnonymousLoginButtonClicked().Forget());
        if (profileButton != null) profileButton.onClick.AddListener(OnLogoutButtonClicked);

        DefaultMessage();

        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            OnLoginSuccess();
        }
    }

    private void OnLogoutButtonClicked()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.SignOut();
            gameObject.SetActive(true);
            DefaultMessage();
            if (profileText != null)
            {
                profileText.text = string.Empty;
            }
        }
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("아이디 비번 좀요.", true);
            return;
        }

        await HandleAuthOperation(
            "로그인",
            () => AuthManager.Instance.SignInWithEmailAsync(email, password),
            "로그인 중...",
            "로그인 성공!"
        );
    }

    private async UniTaskVoid OnRegisterButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("아이디랑 비번 입력해.", true);
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage("비밀번호는 최소 6자 이상이어야 합니다.", true);
            return;
        }

        await HandleAuthOperation(
            "회원가입",
            () => AuthManager.Instance.CreateUserWithEmailAsync(email, password),
            "회원가입 중...",
            "회원가입 성공!"
        );
    }

    private async UniTaskVoid OnAnonymousLoginButtonClicked()
    {
        await HandleAuthOperation(
            "익명 로그인",
            () => AuthManager.Instance.SignInAnonymouslyAsync(),
            "익명 로그인 중...",
            "익명 로그인 성공!"
        );
    }

    private void OnLoginSuccess()
    {
        Debug.Log("[LoginUI] Login successful!");

        //LMJ: Display UID in profile text
        if (profileText != null && AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            string userId = AuthManager.Instance.UserId;
            profileText.text = $"UID: {userId}";
            Debug.Log($"[LoginUI] 프로필 텍스트 업데이트: {userId}");
        }

        gameObject.SetActive(false);
    }

    private void ShowMessage(string message, bool isError)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = isError ? Color.red : Color.white;
        }

        if (isError)
        {
            Debug.LogWarning($"[LoginUI] {message}");
        }
    }

    private void DefaultMessage()
    {
        if (messageText != null)
        {
            ShowMessage("이메일과 비밀번호로 가입을 하던가 익명으로 하던가 하세요.", false);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (loginButton != null) loginButton.interactable = interactable;
        if (registerButton != null) registerButton.interactable = interactable;
        if (anonymousLoginButton != null) anonymousLoginButton.interactable = interactable;
    }

    public void UpdateUI()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsInitialized)
        {
            bool isLoggedIn = AuthManager.Instance.IsLoggedIn;
            loginPanel.SetActive(!isLoggedIn);

            if (isLoggedIn)
            {
                string userId = AuthManager.Instance.UserId;
                profileText.text = userId;
            }
            else
            {
                profileText.text = string.Empty;
            }
        }
    }
}
