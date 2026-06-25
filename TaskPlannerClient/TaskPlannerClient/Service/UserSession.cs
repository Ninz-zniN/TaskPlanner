using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Models;

namespace TaskPlannerClient.Service
{
    public class UserSession
    {
        private static readonly Lazy<UserSession> _instance =
            new Lazy<UserSession>(() => new UserSession());

        public static UserSession Instance => _instance.Value;

        public ApiService Api { get; private set; }
        public User CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null && Api != null;

        private UserSession() { }

        public void Initialize(User user, string token)
        {
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
            Api = new ApiService();
            Api.SetToken(token);
        }
    }
}
