using UnityEngine;
using System;

[Serializable]
public class UserProfile
{
    public string nickname;
    public string email;
    public long createdAt;

    public UserProfile()
    {

    }

    public UserProfile(string nickname, string email)
    {
        this.nickname = nickname;
        this.email = email;
        this.createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserProfile FromJson(string json)
    {
        return JsonUtility.FromJson<UserProfile>(json);
    }
    
    
}
