using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.IntegrationTests.Configuration;
using RegionOrebroLan.Data.SqlClient;

namespace RegionOrebroLan.Data.IntegrationTests.SqlClient
{
	public abstract class BasicDatabaseManagerTest
	{
		#region Fields

		private static IConnectionStringBuilderFactory _connectionStringBuilderFactory;
		private static IDatabaseManagerFactory _databaseManagerFactory;

		#endregion

		#region Properties

		protected internal virtual IConnectionStringBuilderFactory ConnectionStringBuilderFactory => _connectionStringBuilderFactory ??= new ConnectionStringBuilderFactory(new DbProviderFactoriesWrapper());

		protected internal virtual IDatabaseManagerFactory DatabaseManagerFactory
		{
			get
			{
				// ReSharper disable InvertIf
				if(_databaseManagerFactory == null)
				{
					var applicationDomain = new AppDomainWrapper(AppDomain.CurrentDomain);
					var connectionStringBuilderFactory = new ConnectionStringBuilderFactory(new DbProviderFactoriesWrapper());
					var fileSystem = new FileSystem();

					_databaseManagerFactory = new DatabaseManagerFactory(applicationDomain, connectionStringBuilderFactory, fileSystem, connectionStringBuilderFactory.ProviderFactories);
				}
				// ReSharper restore InvertIf

				return _databaseManagerFactory;
			}
		}

		#endregion

		#region Methods

		[CLSCompliant(false)]
		[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
		protected internal virtual DatabaseManager GetDatabaseManager(IConnectionSetting connectionSetting)
		{
			return (DatabaseManager) this.DatabaseManagerFactory.Create(connectionSetting.ProviderName);
		}

		[CLSCompliant(false)]
		protected internal virtual DatabaseManager GetDatabaseManager(string connectionName)
		{
			return this.GetDatabaseManager(Global.ConnectionSettings[connectionName]);
		}

		#endregion
	}
}