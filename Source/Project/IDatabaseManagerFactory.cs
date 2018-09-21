namespace RegionOrebroLan.Data
{
	public interface IDatabaseManagerFactory
	{
		#region Methods

		IDatabaseManager Create(string providerName);

		#endregion
	}
}