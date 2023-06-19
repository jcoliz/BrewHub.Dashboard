// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace BrewHub.Dashboard.Core.Models;

public record User
{
    public string Name { get; init; } = string.Empty;
    public string Sms { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}