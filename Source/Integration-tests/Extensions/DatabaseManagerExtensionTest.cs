using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Data.Extensions;
using RegionOrebroLan.Data.IntegrationTests.SqlClient;

namespace RegionOrebroLan.Data.IntegrationTests.Extensions
{
	[TestClass]
	public class DatabaseManagerExtensionTest : BasicDatabaseManagerTest
	{
		#region Methods

		[TestMethod]
		public void CreateDatabaseIfItDoesNotExist_IfTheDatabaseDoesNotExist_ShouldRecreateTheDatabase_1()
		{
			var connectionSetting = Global.ConnectionSettings["Create-Database-Extension-Test-1"];
			var databaseManager = this.GetDatabaseManager(connectionSetting);

			databaseManager.CreateDatabase(connectionSetting.ConnectionString, false);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));

			databaseManager.DropDatabase(connectionSetting.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(connectionSetting.ConnectionString));

			databaseManager.CreateDatabaseIfItDoesNotExist(connectionSetting.ConnectionString);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
		}

		[TestMethod]
		public void CreateDatabaseIfItDoesNotExist_IfTheDatabaseDoesNotExist_ShouldRecreateTheDatabase_2()
		{
			var connectionSetting = Global.ConnectionSettings["Create-Database-Extension-Test-2"];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(connectionSetting.ConnectionString, connectionSetting.ProviderName);
			var databaseManager = this.GetDatabaseManager(connectionSetting);

			databaseManager.CreateDatabase(connectionSetting.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DetachDatabase(connectionSetting.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabaseIfItDoesNotExist(connectionSetting.ConnectionString);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[TestMethod]
		public void CreateDatabaseIfItDoesNotExist_IfTheDatabaseExistButNotTheDatabaseFile_ShouldNotRecreateTheDatabase()
		{
			var connectionSetting = Global.ConnectionSettings["Create-Database-Extension-Test-3"];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(connectionSetting.ConnectionString, connectionSetting.ProviderName);
			var databaseManager = this.GetDatabaseManager(connectionSetting);

			databaseManager.CreateDatabase(connectionSetting.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			Thread.Sleep(TimeSpan.FromSeconds(1));

			databaseManager.FileSystem.File.Delete(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabaseIfItDoesNotExist(connectionSetting.ConnectionString);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[TestMethod]
		public void CreateDatabaseIfItDoesNotExistOrIfTheDatabaseFileDoesNotExist_IfTheDatabaseDoesNotExist_ShouldRecreateTheDatabase()
		{
			var connectionSetting = Global.ConnectionSettings["Create-Database-Extension-Test-4"];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(connectionSetting.ConnectionString, connectionSetting.ProviderName);
			var databaseManager = this.GetDatabaseManager(connectionSetting);

			databaseManager.CreateDatabase(connectionSetting.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DropDatabase(connectionSetting.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabaseIfItDoesNotExistOrIfTheDatabaseFileDoesNotExist(connectionSetting.ConnectionString);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[TestMethod]
		public void CreateDatabaseIfItDoesNotExistOrIfTheDatabaseFileDoesNotExist_IfTheDatabaseExistButNotTheDatabaseFile_ShouldRecreateTheDatabase()
		{
			var connectionSetting = Global.ConnectionSettings["Create-Database-Extension-Test-5"];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(connectionSetting.ConnectionString, connectionSetting.ProviderName);
			var databaseManager = this.GetDatabaseManager(connectionSetting);

			databaseManager.CreateDatabase(connectionSetting.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			Thread.Sleep(TimeSpan.FromSeconds(1));

			databaseManager.FileSystem.File.Delete(databaseLogFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabaseIfItDoesNotExistOrIfTheDatabaseFileDoesNotExist(connectionSetting.ConnectionString);

			Assert.IsTrue(databaseManager.DatabaseExists(connectionSetting.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		#endregion
	}
}