namespace RegionOrebroLan.Data.IntegrationTests.Configuration
{
	public interface IConnectionSetting
	{
		#region Properties

		string ConnectionString { get; }
		string ProviderName { get; }

		#endregion
	}
}