namespace OneScript.Contexts.Enums
{
    public interface IEnumMetadataProvider : INameAndAliasProvider
    {
        public string TypeUUID { get; set; }
		
        public string ValueTypeUUID { get; set; }
    }
}