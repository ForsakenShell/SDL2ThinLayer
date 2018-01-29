/*
 * Platform.cs
 *
 * Some simple system non-specific platform enumerations.
 *
 * User: 1000101
 * Date: 27/01/2018
 * Time: 4:21 AM
 * 
 */

using System;

/// <summary>
/// Description of Platform.
/// </summary>
public static class Platform
{
    
    public static bool Is64Bit { get { return IntPtr.Size == 8; } }
    
}
