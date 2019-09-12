using System;
using System.IO;

namespace RegionOrebroLan.Data.Extensions
{
	public static class DatabaseManagerExtension
	{
		#region Methods

		public static void CreateDatabase(this IDatabaseManager databaseManager, string connectionString)
		{
			if(databaseManager == null)
				throw new ArgumentNullException(nameof(databaseManager));

			databaseManager.CreateDatabase(connectionString, false);
		}

		public static void CreateDatabaseIfItDoesNotExist(this IDatabaseManager databaseManager, string connectionString)
		{
			if(databaseManager == null)
				throw new ArgumentNullException(nameof(databaseManager));

			if(databaseManager.DatabaseExists(connectionString))
				return;

			databaseManager.CreateDatabase(connectionString, false);
		}

		public static void CreateDatabaseIfItDoesNotExistOrIfTheDatabaseFileDoesNotExist(this IDatabaseManager databaseManager, string connectionString)
		{
			if(databaseManager == null)
				throw new ArgumentNullException(nameof(databaseManager));

			var force = false;

			if(databaseManager.DatabaseExists(connectionString))
			{
				if(databaseManager.TryConnection(connectionString, out var exception))
					return;

				if(exception is FileNotFoundException)
					force = true;
				else
					throw exception;
			}

			databaseManager.CreateDatabase(connectionString, force);
		}

		public static void DropDatabaseIfItExists(this IDatabaseManager databaseManager, string connectionString)
		{
			if(databaseManager == null)
				throw new ArgumentNullException(nameof(databaseManager));

			if(!databaseManager.DatabaseExists(connectionString))
				return;

			databaseManager.DropDatabase(connectionString);
		}

		#endregion
	}
}