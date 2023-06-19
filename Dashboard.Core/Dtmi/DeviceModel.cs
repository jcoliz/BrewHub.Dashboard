// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace BrewHub.Dashboard.Core.Dtmi;

/// <summary>
/// Model-driven schema describing a single device model
/// </summary>
public class DeviceModel
{
    public string Name { get; set; }

    public Dictionary<string, DeviceModelMetric> Metrics { get; set; } = new();

    public DeviceModel(string name)
    {
        Name = name;
    }
}