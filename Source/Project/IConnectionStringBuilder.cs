namespace RegionOrebroLan.Data
{
	public interface IConnectionStringBuilder
	{
		#region Properties

		string ConnectionString { get; set; }
		string Database { get; set; }
		string DatabaseFilePath { get; set; }
		string Server { get; set; }

		#endregion

		#region Methods

		void Clear();
		IConnectionStringBuilder Copy();

		#endregion
	}
}