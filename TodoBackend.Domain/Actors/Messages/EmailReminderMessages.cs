namespace TodoBackend.Domain.Actors.Messages;

/// <summary>
/// Akka.NET Actor Model için email reminder mesaj tipleri
/// Clean Architecture: Domain katman?nda tan?mlan?r çünkü core business logic'e aittir
/// </summary>

// ===== COMMAND MESSAGES - "?unu yap" komutlar? =====
/// <summary>
/// Hangfire Job'dan Supervisor Actor'a: "Tüm kullan?c?lara reminder i?lemini ba?lat"
/// </summary>
public record ProcessAllUsersReminders;

/// <summary>
/// Supervisor'dan UserProcessor Actor'a: "Bu kullan?c?y? i?le"
/// </summary>
public record ProcessUserReminder(int UserId, string Email);

/// <summary>
/// UserProcessor'dan TaskFetcher Actor'a: "Bu kullan?c?n?n task'lar?n? getir"
/// </summary>
public record FetchUserTasks(int UserId);

/// <summary>
/// UserProcessor'dan EmailSender Actor'a: "Bu email'i gönder"
/// </summary>
public record SendEmailReminder(string Email, List<string> TaskTitles);

// ===== EVENT MESSAGES - "?u oldu" bildirimleri =====
/// <summary>
/// Child Actor'dan Parent'a: "Kullan?c? i?lemi tamamland?"
/// </summary>
public record UserReminderCompleted(int UserId, bool Success, string? ErrorMessage = null);

/// <summary>
/// TaskFetcher'dan UserProcessor'a: "Task'lar getirildi"
/// </summary>
public record TasksRetrieved(int UserId, List<string> TaskTitles);

/// <summary>
/// EmailSender'dan UserProcessor'a: "Email gönderildi"
/// </summary>
public record EmailSent(string Email, bool Success, string? ErrorMessage = null);

/// <summary>
/// Supervisor'a gönderilecek: "Tüm i?lemler tamamland?"
/// </summary>
public record AllUsersProcessingCompleted(int SuccessCount, int FailureCount);

// ===== QUERY MESSAGES - "?unu söyle" sorgular? =====
/// <summary>
/// Monitoring için: "Aktif kullan?c? say?s?n? söyle"
/// </summary>
public record GetActiveUserCount;

/// <summary>
/// Monitoring için: "??lem durumunu söyle"
/// </summary>
public record GetProcessingStatus;