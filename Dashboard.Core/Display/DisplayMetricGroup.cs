// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace BrewHub.Dashboard.Core.Display;

/// <summary>
/// A group of display metrics, organized in the way they will be displayed
/// </summary>
public record DisplayMetricGroup
{
    /// <summary>
    /// Human-readable name for this group
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// How we are known in the database
    /// </summary>
    /// <remarks>
    /// The frontend needs this to be set. And it must be a valid HTML
    /// identifier.
    /// </remarks>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Device model for the component or device shown here
    /// </summary>
    public string Model { get; init; } = string.Empty;

    public DisplayMetricGroupKind Kind { get; init; } = DisplayMetricGroupKind.Empty;

    public DisplayMetric[] Telemetry { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] ReadOnlyProperties { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] WritableProperties { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] Commands { get; init; } = Array.Empty<DisplayMetric>();
}

public enum DisplayMetricGroupKind { Empty = 0, Device, Component, Grouping };
