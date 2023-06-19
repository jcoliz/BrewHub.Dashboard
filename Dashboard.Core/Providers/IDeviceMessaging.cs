// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace BrewHub.Dashboard.Core.Providers;

/// <summary>
/// Provides a means to communicate information to devices
/// </summary>
public interface IDeviceMessaging
{
    Task SendDesiredPropertyAsync(string deviceid, string? componentid, string metric, string value);

    Task SendCommandAsync(string deviceid, string? componentid, string metric, string value);
}