using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.IntegrationTests.Configuration;
using RegionOrebroLan.Data.IntegrationTests.Configuration.Extensions;

namespace RegionOrebroLan.Data.IntegrationTests
{
	[TestClass]
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Fields

		private static IDictionary<string, IConnectionSetting> _connectionSettings;
		private static string _testRootDirectoryPath;

		// ReSharper disable PossibleNullReferenceException
		public static readonly string ProjectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
		// ReSharper restore PossibleNullReferenceException

		#endregion

		#region Properties

		public static IDictionary<string, IConnectionSetting> ConnectionSettings
		{
			get
			{
				// ReSharper disable InvertIf
				if(_connectionSettings == null)
				{
					var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(ProjectDirectoryPath, "Connections.json")).Build();
					_connectionSettings = configuration.ConnectionSettings();
				}
				// ReSharper restore InvertIf

				return _connectionSettings;
			}
		}

		#endregion

		#region Methods

		[AssemblyCleanup]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static void Cleanup()
		{
			var applicationDomain = new AppDomainWrapper(AppDomain.CurrentDomain);
			var connectionSetting = ConnectionSettings["Delete-Database"];
			var databaseNames = new List<string>();
			var fileSystem = new FileSystem();
			var providerFactories = new DbProviderFactoriesWrapper();

			var connectionStringBuilderFactory = new ConnectionStringBuilderFactory(providerFactories);
			var connectionStringBuilder = connectionStringBuilderFactory.Create(connectionSetting.ConnectionString, connectionSetting.ProviderName);
			var databaseManagerFactory = new DatabaseManagerFactory(applicationDomain, connectionStringBuilderFactory, fileSystem, providerFactories);
			var databaseManager = databaseManagerFactory.Create(connectionSetting.ProviderName);
			var dbProviderFactory = providerFactories.Get(connectionSetting.ProviderName);

			using(var connection = dbProviderFactory.CreateConnection())
			{
				// ReSharper disable PossibleNullReferenceException
				connection.ConnectionString = connectionStringBuilder.ConnectionString;
				// ReSharper restore PossibleNullReferenceException
				connection.Open();

				using(var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT name FROM master.sys.databases;";
					command.CommandType = CommandType.Text;

					using(var reader = command.ExecuteReader())
					{
						while(reader.Read())
						{
							databaseNames.Add(reader.GetString(0));
						}
					}
				}
			}

			foreach(var databaseName in databaseNames.Where(name => name.StartsWith(_testRootDirectoryPath, StringComparison.OrdinalIgnoreCase)))
			{
				connectionStringBuilder.DatabaseFilePath = databaseName;

				// ReSharper disable EmptyGeneralCatchClause
				try
				{
					databaseManager.DropDatabase(connectionStringBuilder.ConnectionString);
				}
				catch { }
				// ReSharper restore EmptyGeneralCatchClause
			}

			Thread.Sleep(200);

			if(Directory.Exists(_testRootDirectoryPath))
				Directory.Delete(_testRootDirectoryPath, true);
		}

		[AssemblyInitialize]
		[CLSCompliant(false)]
		[SuppressMessage("Usage", "CA1801:Review unused parameters")]
		public static void Initialize(TestContext testContext)
		{
			const string dataDirectoryName = "App_Data";
			_testRootDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var dataDirectoryPath = Path.Combine(_testRootDirectoryPath, dataDirectoryName);
			var sourceDataDirectoryPath = Path.Combine(ProjectDirectoryPath, dataDirectoryName);

			Directory.CreateDirectory(dataDirectoryPath);

			foreach(var filePath in Directory.GetFiles(sourceDataDirectoryPath, "*", SearchOption.AllDirectories))
			{
				File.Copy(filePath, filePath.Replace(sourceDataDirectoryPath, dataDirectoryPath, StringComparison.OrdinalIgnoreCase));
			}

			AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectoryPath);
		}

		#endregion
	}
}