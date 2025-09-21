﻿using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.Authentication.Login;

/// <summary>
/// Kullan?c?n?n giri? yapmak için gönderdi?i email ve password bilgileri
/// Bu command MediatR pattern'i kullanarak login i?lemini ba?lat?r
/// </summary>
public record LoginCommand(
    string Email,      // Kullanıcının email adresi
    string Password    // Kullanıcının şifresi
) : IRequest<Result<string>>;  // String = JWT Token döner