namespace sys_oraclefusionhcm_mgmt.ConfigModels;

/// <summary>
/// Application configuration model for Oracle Fusion HCM System Layer
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// Oracle Fusion HCM connection configuration
    /// </summary>
    public OracleFusionHCMConfig OracleFusionHCM { get; set; } = new();
}

/// <summary>
/// Oracle Fusion HCM connection configuration
/// </summary>
public class OracleFusionHCMConfig
{
    /// <summary>
    /// Base URL for Oracle Fusion HCM API (e.g., https://iaaxey-dev3.fa.ocs.oraclecloud.com:443)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Resource path for absences API (e.g., hcmRestApi/resources/11.13.18.05/absences)
    /// </summary>
    public string ResourcePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Username for Basic Authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for Basic Authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection timeout in milliseconds (default: 30000)
    /// </summary>
    public int ConnectTimeout { get; set; } = 30000;
    
    /// <summary>
    /// Read timeout in milliseconds (default: 60000)
    /// </summary>
    public int ReadTimeout { get; set; } = 60000;
}
