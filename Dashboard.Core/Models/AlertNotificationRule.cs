// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace BrewHub.Dashboard.Core.Models;

public record AlertNotificationRule
{
    public string Name { get; init; } = string.Empty;
    public string DevicesRule { get; init; } = "*";
    public string StatusRule { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public IEnumerable<User> Recipients { get; init; } = Enumerable.Empty<User>();
}