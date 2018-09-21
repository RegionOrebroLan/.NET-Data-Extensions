namespace RegionOrebroLan.Data.IntegrationTests.Configuration
{
	public class ConnectionSetting : IConnectionSetting
	{
		#region Properties

		public virtual string ConnectionString { get; set; }
		public virtual string ProviderName { get; set; }

		#endregion
	}
}