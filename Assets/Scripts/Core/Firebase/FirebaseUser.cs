using System;
using System.Collections.Generic;

namespace YuankunHuang.Unity.FirebaseCore
{
    [Serializable]
    public class FirebaseUser : IFirebaseUser
    {
        public string UUID { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastSignInTime { get; set; }

        public FirebaseUser()
        {
        }

        public FirebaseUser(string userId, string email, string displayName = null, string avatar = null, bool isEmailVerified = false)
        {
            UUID = userId;
            Email = email;
            DisplayName = displayName;
            Avatar = avatar;
            IsEmailVerified = isEmailVerified;
            CreationTime = DateTime.UtcNow;
            LastSignInTime = DateTime.UtcNow;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["userId"] = UUID,
                ["email"] = Email,
                ["displayName"] = DisplayName,
                ["photoUrl"] = Avatar,
                ["isEmailVerified"] = IsEmailVerified,
                ["creationTime"] = CreationTime.ToBinary(),
                ["lastSignInTime"] = LastSignInTime.ToBinary()
            };
        }

        public static FirebaseUser FromDictionary(Dictionary<string, object> data)
        {
            if (data == null) return null;

            var user = new FirebaseUser();

            if (data.TryGetValue("userId", out var userId)) user.UUID = userId.ToString();
            if (data.TryGetValue("email", out var email)) user.Email = email.ToString();
            if (data.TryGetValue("displayName", out var displayName)) user.DisplayName = displayName?.ToString();
            if (data.TryGetValue("photoUrl", out var photoUrl)) user.Avatar = photoUrl?.ToString();
            if (data.TryGetValue("isEmailVerified", out var isEmailVerified)) user.IsEmailVerified = Convert.ToBoolean(isEmailVerified);

            if (data.TryGetValue("creationTime", out var creationTime))
            {
                try { user.CreationTime = DateTime.FromBinary(Convert.ToInt64(creationTime)); }
                catch { user.CreationTime = DateTime.UtcNow; }
            }

            if (data.TryGetValue("lastSignInTime", out var lastSignInTime))
            {
                try { user.LastSignInTime = DateTime.FromBinary(Convert.ToInt64(lastSignInTime)); }
                catch { user.LastSignInTime = DateTime.UtcNow; }
            }

            return user;
        }

        public override string ToString()
        {
            return $"FirebaseUser[{UUID}] Email:{Email} DisplayName:{DisplayName} Verified:{IsEmailVerified}";
        }
    }
}