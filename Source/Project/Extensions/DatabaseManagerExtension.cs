using System;

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