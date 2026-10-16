namespace RaceDayAPI.Services
{
    public static class SessionExtensions
    {
        public static void SetUserSession(this ISession session, int userId, string role, string fullName)
        {
            session.SetInt32("UserId", userId);
            session.SetString("Role", role);
            session.SetString("FullName", fullName);
        }

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32("UserId");
        }

        public static string? GetRole(this ISession session)
        {
            return session.GetString("Role");
        }

        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetInt32("UserId").HasValue;
        }
    }
}