using System;
using System.Text.RegularExpressions;
using RegionOrebroLan.Extensions;

namespace RegionOrebroLan.Data.Extensions
{
	public static class ConnectionStringBuilderExtension
	{
		#region Methods

		public static string GetActualDatabase(this IConnectionStringBuilder connectionStringBuilder, IApplicationDomain applicationDomain)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			return connectionStringBuilder.Database ?? connectionStringBuilder.GetActualDatabaseFilePath(applicationDomain);
		}

		public static string GetActualDatabaseFilePath(this IConnectionStringBuilder connectionStringBuilder, IApplicationDomain applicationDomain)
		{
			if(connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(connectionStringBuilder));

			if(applicationDomain == null)
				throw new ArgumentNullException(nameof(applicationDomain));

			var databaseFilePath = connectionStringBuilder.DatabaseFilePath;

			// ReSharper disable InvertIf
			if(!string.IsNullOrWhiteSpace(databaseFilePath))
			{
				const string dataDirectory = "DataDirectory";
				var pattern = Regex.Escape("|") + dataDirectory + Regex.Escape("|");

				if(Regex.IsMatch(databaseFilePath, pattern, RegexOptions.IgnoreCase))
				{
					var dataDirectoryPath = applicationDomain.GetDataDirectoryPath();

					const string backslash = @"\";

					if(!dataDirectoryPath.Trim().EndsWith(backslash, StringComparison.OrdinalIgnoreCase))
						dataDirectoryPath += backslash;

					databaseFilePath = Regex.Replace(databaseFilePath, pattern, dataDirectoryPath, RegexOptions.IgnoreCase);
				}
			}
			// ReSharper restore InvertIf

			return databaseFilePath;
		}

		#endregion
	}
}