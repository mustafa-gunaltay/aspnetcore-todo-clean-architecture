namespace TodoBackend.Domain.Actors.Messages;

/// <summary>
/// Akka.NET Actor Model i�in email reminder mesaj tipleri
/// Clean Architecture: Domain katman?nda tan?mlan?r ��nk� core business logic'e aittir
/// </summary>

// ===== COMMAND MESSAGES - "?unu yap" komutlar? =====
/// <summary>
/// Hangfire Job'dan Supervisor Actor'a: "T�m kullan?c?lara reminder i?lemini ba?lat"
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
/// UserProcessor'dan EmailSender Actor'a: "Bu email'i g�nder"
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
/// EmailSender'dan UserProcessor'a: "Email g�nderildi"
/// </summary>
public record EmailSent(string Email, bool Success, string? ErrorMessage = null);

/// <summary>
/// Supervisor'a g�nderilecek: "T�m i?lemler tamamland?"
/// </summary>
public record AllUsersProcessingCompleted(int SuccessCount, int FailureCount);

// ===== QUERY MESSAGES - "?unu s�yle" sorgular? =====
/// <summary>
/// Monitoring i�in: "Aktif kullan?c? say?s?n? s�yle"
/// </summary>
public record GetActiveUserCount;

/// <summary>
/// Monitoring i�in: "??lem durumunu s�yle"
/// </summary>
public record GetProcessingStatus;