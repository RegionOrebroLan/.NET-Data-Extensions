using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.SqlClient;
using RegionOrebroLan.ServiceLocation;

namespace RegionOrebroLan.Data
{
	[CLSCompliant(false)]
	[ServiceConfiguration(InstanceMode = InstanceMode.Singleton, ServiceType = typeof(IDatabaseManagerFactory))]
	public class DatabaseManagerFactory : IDatabaseManagerFactory
	{
		#region Constructors

		public DatabaseManagerFactory(IApplicationDomain applicationDomain, IConnectionStringBuilderFactory connectionStringBuilderFactory, IFileSystem fileSystem, IProviderFactories providerFactories)
		{
			this.ApplicationDomain = applicationDomain ?? throw new ArgumentNullException(nameof(applicationDomain));
			this.ConnectionStringBuilderFactory = connectionStringBuilderFactory ?? throw new ArgumentNullException(nameof(connectionStringBuilderFactory));
			this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			this.ProviderFactories = providerFactories ?? throw new ArgumentNullException(nameof(providerFactories));
		}

		#endregion

		#region Properties

		protected internal virtual IApplicationDomain ApplicationDomain { get; }
		protected internal virtual IConnectionStringBuilderFactory ConnectionStringBuilderFactory { get; }
		protected internal virtual IFileSystem FileSystem { get; }
		protected internal virtual IProviderFactories ProviderFactories { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
		public virtual IDatabaseManager Create(string providerName)
		{
			try
			{
				var databaseProviderFactory = this.ProviderFactories.Get(providerName);

				if(databaseProviderFactory is SqlClientFactory)
					return new DatabaseManager(this.ApplicationDomain, this.ConnectionStringBuilderFactory.Create(providerName), databaseProviderFactory, this.FileSystem);

				throw new InvalidOperationException("For the moment this factory can only handle SQL Server as provider.");
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not create a database-manager from provider-name \"{0}\".", providerName), exception);
			}
		}

		#endregion
	}
}