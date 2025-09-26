namespace TodoBackend.Domain.Interfaces;

// DB'de domain entity'e karsilik gelen bir dosya degildir
public interface ICurrentUser
{
    string UserName { get; }
    int? UserId { get; } // Giri? yapm?? kullan?c?n?n ID'si
}