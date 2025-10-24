using System.Linq.Expressions;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Firebase.Database;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    private static ProfileManager instance;
    public static ProfileManager Instance => instance;

    private DatabaseReference databaseRef;
    private DatabaseReference usersRef;

    private UserProfile cachedProfile;
    public UserProfile CachedProfile => cachedProfile;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {

        }
    }

    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitializationAsync();

        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        usersRef = databaseRef.Child("users");

        Debug.Log("[Profile] ProfileManager 초기화 완료");
    }

    public async UniTask<(bool success, string error)> SaveProfileAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "[Profile] 로그인 X");
        }

        string userId = AuthManager.Instance.UserId;
        string email = AuthManager.Instance.CurrentUser.Email ?? "익명";

        try
        {
            Debug.Log($"[Profile] 프로필 저장 시도 {nickname}");

            UserProfile profile = new UserProfile(nickname, email);
            string json = profile.ToJson();

            await usersRef.Child(userId).SetRawJsonValueAsync(json).AsUniTask();

            cachedProfile = profile;

            Debug.Log($"[Profile] 프로필 성공 {nickname}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 실패 {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<(UserProfile profile, string error)> LoadProfileAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (null, "로그인 X");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[Profile] 프로필 로드 시도 {nickname}");
            DataSnapshot snapshot = await usersRef.Child(userId).GetValueAsync().AsUniTask();

            if (!snapshot.Exists)
            {
                Debug.Log($"[Profile] 프로필 없음");
                return (null, "프로필 없음");
            }

            string json = snapshot.GetRawJsonValue();
            cachedProfile = UserProfile.FromJson(json);

            Debug.Log($"[Profile] 프로필 성공 {nickname}");
            return (cachedProfile, null);
        }

        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 실패 {ex.Message}");
            return (null, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> UpdateNicknameAsync(string newNickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 X");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[Profile] 닉네임 변경 시도 {newNickname}");

            await usersRef.Child(userId).Child("nickname").SetValueAsync(newNickname).AsUniTask();

            cachedProfile.nickname = newNickname;

            Debug.Log($"[Profile] 닉네임 변경 성공 {cachedProfile.nickname}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 닉네임 변경 실패 {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<bool> ProfileExistAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return false;
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await usersRef.Child(userId).GetValueAsync().AsUniTask();
            return snapshot.Exists;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 확인 실패: {ex.Message}");
            return false;
        }
    }
}
