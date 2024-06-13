using OneScript.Contexts.Enums;

namespace OneScript.Web.Server
{
    [EnumerationType("РежимSameSite", "SameSiteMode",
        TypeUUID = "C3D11188-2520-46CE-99A1-58CEFAE4CFE9",
        ValueTypeUUID = "84C26CD9-C55C-4688-89F4-74BC97FC6F8E")]
    public enum SameSiteModeEnum
    {
        [EnumValue("Unspecified")]
        Unspecified = -1,

        [EnumValue("None")]
        None = 0,

        [EnumValue("Lax")]
        Lax = 1,

        [EnumValue("Strict")]
        Strict = 2
    }
}
