using System;
using System.Data.SqlClient;
using System.Globalization;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.SqlClient;
using RegionOrebroLan.ServiceLocation;

namespace RegionOrebroLan.Data
{
	[ServiceConfiguration(InstanceMode = InstanceMode.Singleton, ServiceType = typeof(IConnectionStringBuilderFactory))]
	public class ConnectionStringBuilderFactory : IConnectionStringBuilderFactory
	{
		#region Constructors

		public ConnectionStringBuilderFactory(IProviderFactories providerFactories)
		{
			this.ProviderFactories = providerFactories ?? throw new ArgumentNullException(nameof(providerFactories));
		}

		#endregion

		#region Properties

		protected internal virtual IProviderFactories ProviderFactories { get; }

		#endregion

		#region Methods

		public virtual IConnectionStringBuilder Create(string providerName)
		{
			return this.Create(null, providerName);
		}

		public virtual IConnectionStringBuilder Create(string connectionString, string providerName)
		{
			try
			{
				var providerFactory = this.ProviderFactories.Get(providerName);

				if(providerFactory is SqlClientFactory)
					return new ConnectionStringBuilder(connectionString);

				throw new InvalidOperationException("For the moment this factory can only handle SQL Server as provider.");
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not create a connection-string-builder from provider-name \"{0}\".", providerName), exception);
			}
		}

		#endregion
	}
}