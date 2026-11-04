using UnityEngine;

/// <summary>
/// Attached to ball objects in the scene. Inherits all functionality from Ball
/// to maintain backward compatibility with existing scene objects.
/// </summary>
public class Player : Ball
{
    // All score, click, lifetime, and physics logic is implemented in base Ball class
}
