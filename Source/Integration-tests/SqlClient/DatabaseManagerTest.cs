using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Data.Common;
using RegionOrebroLan.Data.Extensions;
using RegionOrebroLan.Data.IntegrationTests.Configuration;
using RegionOrebroLan.Data.SqlClient;

namespace RegionOrebroLan.Data.IntegrationTests.SqlClient
{
	[TestClass]
	public class DatabaseManagerTest
	{
		#region Fields

		private static IConnectionStringBuilderFactory _connectionStringBuilderFactory;
		private static IDatabaseManagerFactory _databaseManagerFactory;
		private const string _dropDatabaseFailTestConnectionKey = "Drop-Database-Fail-Test";
		private const string _existingDatabaseConnectionKey = "Existing-Database";
		private const string _existingDatabaseFileConnectionKey = "Existing-Database-File";
		private const string _existingDatabaseFileWithoutLogFileConnectionKey = "Existing-Database-File-Without-Log-File";
		private const string _nonexistingDatabaseConnectionKey = "Nonexisting-Database";
		private const string _onlyProviderConnectionKey = "Only-Provider";

		#endregion

		#region Properties

		protected internal virtual IConnectionStringBuilderFactory ConnectionStringBuilderFactory => _connectionStringBuilderFactory ?? (_connectionStringBuilderFactory = new ConnectionStringBuilderFactory(new DbProviderFactoriesWrapper()));

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

		protected internal virtual string DropDatabaseFailTestConnectionKey => _dropDatabaseFailTestConnectionKey;
		protected internal virtual string ExistingDatabaseConnectionKey => _existingDatabaseConnectionKey;
		protected internal virtual string ExistingDatabaseFileConnectionKey => _existingDatabaseFileConnectionKey;
		protected internal virtual string ExistingDatabaseFileWithoutLogFileConnectionKey => _existingDatabaseFileWithoutLogFileConnectionKey;
		protected internal virtual string NonexistingDatabaseConnectionKey => _nonexistingDatabaseConnectionKey;
		protected internal virtual string OnlyProviderConnectionKey => _onlyProviderConnectionKey;

		#endregion

		#region Methods

		[TestMethod]
		[ExpectedException(typeof(SqlException))]
		public void CreateDatabase_IfTheDatabaseAlreadyExistsAndTheForceParameterIsFalse_ShouldThrowASqlException()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

			try
			{
				databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);
			}
			catch(Exception exception)
			{
				if(exception is SqlException)
					throw;
			}
			finally
			{
				databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);
			}
		}

		[TestMethod]
		public void CreateDatabase_IfTheDatabaseAlreadyExistsAndTheForceParameterIsTrue_ShouldRecreateTheDatabase()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(nonexistingDatabaseConnection.ConnectionString, nonexistingDatabaseConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var numberOfFilesCreated = 0;

			using(var fileSystemWatcher = new FileSystemWatcher())
			{
				fileSystemWatcher.Created += (sender, e) => { numberOfFilesCreated++; };
				fileSystemWatcher.Filter = "Nonexisting-Database*.*";
				fileSystemWatcher.Path = Path.GetDirectoryName(databaseFilePath);

				fileSystemWatcher.EnableRaisingEvents = true;

				databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

				databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, true);

				databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);

				Assert.AreEqual(4, numberOfFilesCreated);
			}
		}

		[TestMethod]
		public void CreateDatabase_IfTheDatabaseAndLogFileDoesNotExistButTheDataFileDoes_ShouldDoAnAttach()
		{
			var existingDatabaseFileWithoutLogFile = Global.ConnectionSettings[this.ExistingDatabaseFileWithoutLogFileConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(existingDatabaseFileWithoutLogFile.ConnectionString, existingDatabaseFileWithoutLogFile.ProviderName);
			var databaseManager = this.GetDatabaseManager(existingDatabaseFileWithoutLogFile);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsFalse(databaseManager.DatabaseExists(existingDatabaseFileWithoutLogFile.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabase(existingDatabaseFileWithoutLogFile.ConnectionString, false);

			Assert.IsTrue(databaseManager.DatabaseExists(existingDatabaseFileWithoutLogFile.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DetachDatabase(existingDatabaseFileWithoutLogFile.ConnectionString);
			databaseManager.FileSystem.File.Delete(databaseLogFilePath);
		}

		[TestMethod]
		public void CreateDatabase_IfTheDatabaseDoesNotExistAndDatabaseFilesDoesNotExist_ShouldCreateTheDatabaseAndFiles()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(nonexistingDatabaseConnection.ConnectionString, nonexistingDatabaseConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			Assert.IsFalse(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));

			databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

			Assert.IsTrue(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);
		}

		[TestMethod]
		public void CreateDatabase_IfTheDatabaseDoesNotExistButTheDataFileAndLogFileDoes_ShouldDoAnAttach()
		{
			var existingDatabaseFileConnection = Global.ConnectionSettings[this.ExistingDatabaseFileConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(existingDatabaseFileConnection.ConnectionString, existingDatabaseFileConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(existingDatabaseFileConnection);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsFalse(databaseManager.DatabaseExists(existingDatabaseFileConnection.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.CreateDatabase(existingDatabaseFileConnection.ConnectionString, false);

			Assert.IsTrue(databaseManager.DatabaseExists(existingDatabaseFileConnection.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DetachDatabase(existingDatabaseFileConnection.ConnectionString);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DatabaseExists_IfTheConnectionStringParameterIsNull_ShouldThrowAnInvalidOperationException()
		{
			this.GetDatabaseManager(this.OnlyProviderConnectionKey).DatabaseExists((string) null);
		}

		[TestMethod]
		public void DatabaseExists_IfTheDatabaseDoesNotExistInTheDatabaseEngine_ShouldReturnFalse()
		{
			this.DatabaseExistsTest(Global.ConnectionSettings[this.ExistingDatabaseFileConnectionKey], false);
			this.DatabaseExistsTest(Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey], false);
		}

		protected internal virtual void DatabaseExistsTest(IConnectionSetting connectionSetting, bool expectedValue)
		{
			Assert.AreEqual(expectedValue, this.GetDatabaseManager(connectionSetting).DatabaseExists(connectionSetting.ConnectionString));
		}

		[TestMethod]
		[ExpectedException(typeof(SqlException))]
		public void DropDatabase_IfTheDatabaseDoesNotExist_ShouldThrowASqlException()
		{
			var dropDatabaseFailTestConnectionKey = Global.ConnectionSettings[this.DropDatabaseFailTestConnectionKey];
			var databaseManager = this.GetDatabaseManager(dropDatabaseFailTestConnectionKey);

			databaseManager.DropDatabase(dropDatabaseFailTestConnectionKey.ConnectionString);
		}

		[TestMethod]
		public void DropDatabase_IfTheDatabaseFileDoesNotExistAndTheDatabaseLogFileDoesNotExist_ShouldDropTheDatabase()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(nonexistingDatabaseConnection.ConnectionString, nonexistingDatabaseConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			Thread.Sleep(TimeSpan.FromSeconds(1));
			databaseManager.FileSystem.File.Delete(databaseFilePath);
			databaseManager.FileSystem.File.Delete(databaseLogFilePath);

			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[TestMethod]
		public void DropDatabase_IfTheDatabaseFileDoesNotExistButTheDatabaseLogFileExists_ShouldDropTheDatabaseAndDeleteTheDatabaseLogFile()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(nonexistingDatabaseConnection.ConnectionString, nonexistingDatabaseConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			Thread.Sleep(TimeSpan.FromSeconds(1));
			databaseManager.FileSystem.File.Delete(databaseFilePath);

			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));

			databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[TestMethod]
		public void DropDatabase_IfTheDatabaseLogFileDoesNotExistButTheDatabaseFileExists_ShouldDropTheDatabaseAndDeleteTheDatabaseFile()
		{
			var nonexistingDatabaseConnection = Global.ConnectionSettings[this.NonexistingDatabaseConnectionKey];
			var connectionStringBuilder = this.ConnectionStringBuilderFactory.Create(nonexistingDatabaseConnection.ConnectionString, nonexistingDatabaseConnection.ProviderName);
			var databaseManager = this.GetDatabaseManager(nonexistingDatabaseConnection);

			databaseManager.CreateDatabase(nonexistingDatabaseConnection.ConnectionString, false);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(databaseManager.ApplicationDomain);
			var databaseLogFilePath = databaseManager.GetDatabaseLogFilePath(databaseFilePath);

			Assert.IsTrue(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsTrue(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			Thread.Sleep(TimeSpan.FromSeconds(1));
			databaseManager.FileSystem.File.Delete(databaseLogFilePath);

			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));

			databaseManager.DropDatabase(nonexistingDatabaseConnection.ConnectionString);

			Assert.IsFalse(databaseManager.DatabaseExists(nonexistingDatabaseConnection.ConnectionString));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseFilePath));
			Assert.IsFalse(databaseManager.FileSystem.File.Exists(databaseLogFilePath));
		}

		[CLSCompliant(false)]
		protected internal virtual DatabaseManager GetDatabaseManager(string connectionName)
		{
			return this.GetDatabaseManager(Global.ConnectionSettings[connectionName]);
		}

		[CLSCompliant(false)]
		protected internal virtual DatabaseManager GetDatabaseManager(IConnectionSetting connectionSetting)
		{
			return (DatabaseManager) this.DatabaseManagerFactory.Create(connectionSetting.ProviderName);
		}

		#endregion
	}
}