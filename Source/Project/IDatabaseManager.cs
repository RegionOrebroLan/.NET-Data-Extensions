﻿using System;

namespace RegionOrebroLan.Data
{
	public interface IDatabaseManager
	{
		#region Methods

		void CreateDatabase(string connectionString, bool force);
		bool DatabaseExists(string connectionString);
		void DropDatabase(string connectionString);
		bool TryConnection(string connectionString, out Exception exception);

		#endregion
	}
}