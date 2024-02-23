namespace gView.Framework.Blazor.Models;

public enum VirtualDriveType
{
    Drive,
    EnvironmentVariable
}
public record VirtualDrive(
        string Name, 
        string PhysicalPath, 
        VirtualDriveType DriveType,
        System.IO.DriveType SystemDriveType = System.IO.DriveType.Unknown);
