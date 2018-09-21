namespace RegionOrebroLan.Data
{
	public interface IConnectionStringBuilderFactory
	{
		#region Methods

		IConnectionStringBuilder Create(string providerName);
		IConnectionStringBuilder Create(string connectionString, string providerName);

		#endregion
	}
}