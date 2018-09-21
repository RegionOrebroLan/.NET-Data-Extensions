using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Data.IntegrationTests.Configuration;
using RegionOrebroLan.Data.IntegrationTests.Configuration.Extensions;

namespace RegionOrebroLan.Data.IntegrationTests
{
	[TestClass]
	[SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable")]
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public class Global
	{
		#region Fields

		private static IDictionary<string, IConnectionSetting> _connectionSettings;

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

		[AssemblyInitialize]
		[CLSCompliant(false)]
		[SuppressMessage("Usage", "CA1801:Review unused parameters")]
		public static void Initialize(TestContext testContext)
		{
			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(ProjectDirectoryPath, "App_Data"));
		}

		#endregion
	}
}