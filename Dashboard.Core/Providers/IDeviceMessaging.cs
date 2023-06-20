// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using BrewHub.Dashboard.Core.Models;

namespace BrewHub.Dashboard.Core.Providers;

/// <summary>
/// Provides a means to communicate information to devices
/// </summary>
public interface IDeviceMessaging
{
    Task SendDesiredPropertyAsync(Datapoint point);

    Task SendCommandAsync(string deviceid, string? componentid, string metric, object value);
}