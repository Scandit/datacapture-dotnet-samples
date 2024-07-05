/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2024- Scandit AG. All rights reserved.
 */

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SearchAndFindSample.Models;

/// <summary>
/// Represents a message that is used to handle application lifecycle events.
/// </summary>
public class ApplicationMessage : ValueChangedMessage<string>
{
    public ApplicationMessage(string value) : base(value)
    {}
}
