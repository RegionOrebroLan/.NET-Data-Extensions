using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using RegionOrebroLan.Data.Extensions;

namespace RegionOrebroLan.Data.SqlClient
{
	[CLSCompliant(false)]
	public class DatabaseManager : IDatabaseManager
	{
		#region Fields

		private const string _databaseLogFilePathSuffix = "_log.ldf";

		#endregion

		#region Constructors

		public DatabaseManager(IApplicationDomain applicationDomain, IConnectionStringBuilder connectionStringBuilder, DbProviderFactory databaseProviderFactory, IFileSystem fileSystem)
		{
			this.ApplicationDomain = applicationDomain ?? throw new ArgumentNullException(nameof(applicationDomain));
			this.ConnectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
			this.DatabaseProviderFactory = databaseProviderFactory ?? throw new ArgumentNullException(nameof(databaseProviderFactory));
			this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		}

		#endregion

		#region Properties

		protected internal virtual IApplicationDomain ApplicationDomain { get; }
		protected internal virtual IConnectionStringBuilder ConnectionStringBuilder { get; }
		protected internal virtual string DatabaseLogFilePathSuffix => _databaseLogFilePathSuffix;
		protected internal virtual DbProviderFactory DatabaseProviderFactory { get; }
		protected internal virtual IFileSystem FileSystem { get; }

		#endregion

		#region Methods

		protected internal virtual IConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
		{
			var connectionStringBuilder = this.ConnectionStringBuilder.Copy();

			connectionStringBuilder.ConnectionString = connectionString;

			return connectionStringBuilder;
		}

		protected internal virtual IConnectionStringBuilder CreateConnectionStringBuilderWithoutDatabaseAndDatabaseFilePath(string connectionString)
		{
			var connectionStringBuilder = this.CreateConnectionStringBuilder(connectionString);

			connectionStringBuilder.Database = null;
			connectionStringBuilder.DatabaseFilePath = null;

			return connectionStringBuilder;
		}

		public virtual void CreateDatabase(string connectionString, bool force)
		{
			this.CreateDatabase(this.CreateConnectionStringBuilder(connectionString), force);
		}

		protected internal virtual void CreateDatabase(IConnectionStringBuilder connectionStringBuilder, bool force)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			if(force)
				this.DropDatabase(connectionStringBuilder);

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(this.ApplicationDomain);
			var databaseName = connectionStringBuilder.GetActualDatabase(this.ApplicationDomain);
			var serverConnectionStringBuilder = this.CreateConnectionStringBuilderWithoutDatabaseAndDatabaseFilePath(connectionStringBuilder.ConnectionString);

			var commandText = "CREATE DATABASE [" + databaseName + "]";

			if(string.IsNullOrEmpty(databaseFilePath))
			{
				commandText += ";";
			}
			else
			{
				if(this.FileSystem.File.Exists(databaseFilePath))
				{
					commandText += " ON(FILENAME=[" + databaseFilePath + "])";
					commandText += " FOR ATTACH_REBUILD_LOG;";
				}
				else
				{
					var databaseLogFilePath = this.GetDatabaseLogFilePath(databaseFilePath);
					databaseName = this.FileSystem.Path.GetFileNameWithoutExtension(databaseFilePath);

					commandText += " ON PRIMARY(NAME=[" + databaseName + "-Data], FILENAME=[" + databaseFilePath + "])";
					commandText += " LOG ON(NAME=[" + databaseName + "-Log], FILENAME=[" + databaseLogFilePath + "]);";
				}
			}

			this.ExecuteCommand(commandText, serverConnectionStringBuilder);
		}

		public virtual bool DatabaseExists(string connectionString)
		{
			return this.DatabaseExists(this.CreateConnectionStringBuilder(connectionString));
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		protected internal virtual bool DatabaseExists(IConnectionStringBuilder connectionStringBuilder)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			var databaseName = connectionStringBuilder.GetActualDatabase(this.ApplicationDomain);

			var serverConnectionStringBuilder = this.CreateConnectionStringBuilderWithoutDatabaseAndDatabaseFilePath(connectionStringBuilder.ConnectionString);

			using(var connection = this.DatabaseProviderFactory.CreateConnection())
			{
				// ReSharper disable PossibleNullReferenceException
				connection.ConnectionString = serverConnectionStringBuilder.ConnectionString;
				// ReSharper restore PossibleNullReferenceException
				connection.Open();

				using(var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT DB_ID('" + databaseName + "');";
					command.CommandType = CommandType.Text;

					var reader = command.ExecuteReader();

					while(reader.Read())
					{
						var value = reader[0];

						return !(value is DBNull);
					}

					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not select the database \"{0}\" to check if it exist.", databaseName));
				}
			}
		}

		protected internal virtual void DetachDatabase(string connectionString)
		{
			this.DetachDatabase(this.CreateConnectionStringBuilder(connectionString));
		}

		protected internal virtual void DetachDatabase(IConnectionStringBuilder connectionStringBuilder)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			var databaseName = connectionStringBuilder.GetActualDatabase(this.ApplicationDomain);

			var commandText = "EXEC sp_detach_db '" + databaseName + " ', 'true';";

			var serverConnectionStringBuilder = this.CreateConnectionStringBuilderWithoutDatabaseAndDatabaseFilePath(connectionStringBuilder.ConnectionString);

			this.ExecuteCommand(commandText, serverConnectionStringBuilder);
		}

		public virtual void DropDatabase(string connectionString)
		{
			this.DropDatabase(this.CreateConnectionStringBuilder(connectionString));
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		protected internal virtual void DropDatabase(IConnectionStringBuilder connectionStringBuilder)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			var databaseFilePath = connectionStringBuilder.GetActualDatabaseFilePath(this.ApplicationDomain);
			var databaseName = connectionStringBuilder.GetActualDatabase(this.ApplicationDomain);

			var serverConnectionStringBuilder = this.CreateConnectionStringBuilderWithoutDatabaseAndDatabaseFilePath(connectionStringBuilder.ConnectionString);

			if(string.IsNullOrEmpty(databaseFilePath) || this.FileSystem.File.Exists(databaseFilePath))
			{
				var alterDatabaseCommandText = "ALTER DATABASE [" + databaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"; // To close existing connections.

				this.ExecuteCommand(alterDatabaseCommandText, serverConnectionStringBuilder);
			}
			else if(this.DatabaseExists(connectionStringBuilder))
			{
				var databaseLogFilePath = this.GetDatabaseLogFilePath(databaseFilePath);

				if(this.FileSystem.File.Exists(databaseLogFilePath))
					this.FileSystem.File.Delete(databaseLogFilePath);
			}

			var dropDatabaseCommandText = "DROP DATABASE [" + databaseName + "];";

			try
			{
				this.ExecuteCommand(dropDatabaseCommandText, serverConnectionStringBuilder);
			}
			catch(SqlException sqlException)
			{
				// If the mdf-file does not exist we get this exception. The database is dropped anyhow.
				var isFileDoesNotExistException = sqlException.Class == 16 && sqlException.ErrorCode == -2146232060 && sqlException.Number == 5120;

				if(!isFileDoesNotExistException)
					throw;
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		protected internal virtual void ExecuteCommand(string commandText, IConnectionStringBuilder connectionStringBuilder)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			using(var connection = this.DatabaseProviderFactory.CreateConnection())
			{
				// ReSharper disable PossibleNullReferenceException
				connection.ConnectionString = connectionStringBuilder.ConnectionString;
				// ReSharper restore PossibleNullReferenceException
				connection.Open();

				using(var command = connection.CreateCommand())
				{
					command.CommandText = commandText;
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery();
				}
			}
		}

		protected internal virtual string GetDatabaseLogFilePath(string databaseFilePath)
		{
			var databaseDirectoryPath = this.FileSystem.Path.GetDirectoryName(databaseFilePath);
			var databaseName = this.FileSystem.Path.GetFileNameWithoutExtension(databaseFilePath);

			return this.FileSystem.Path.Combine(databaseDirectoryPath, databaseName + this.DatabaseLogFilePathSuffix);
		}

		#endregion
	}
}