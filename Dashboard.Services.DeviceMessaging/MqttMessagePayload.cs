// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

// TODO: Share this with the controller, so changes don't have to be made
// in two places

namespace BrewHub.Devices.Platform.Mqtt;

public record MessagePayload
{
    private static int NextSeq = 1;
    public long Timestamp { get; init; }
    public int Seq { get; init; } = NextSeq++;
    public string? Model { get; init; }
    public Dictionary<string, object>? Metrics { get; init; }
}
