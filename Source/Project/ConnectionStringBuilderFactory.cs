using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Data.SqlClient;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.SqlClient;
using RegionOrebroLan.DependencyInjection;

namespace RegionOrebroLan.Data
{
	[ServiceConfiguration(ServiceType = typeof(IConnectionStringBuilderFactory))]
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

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
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