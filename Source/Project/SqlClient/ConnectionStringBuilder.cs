using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace RegionOrebroLan.Data.SqlClient
{
	public class ConnectionStringBuilder : IConnectionStringBuilder
	{
		#region Fields

		private const string _databaseFilePathKey = "AttachDbFilename";
		private const string _databaseKey = "Initial Catalog";
		private const string _serverKey = "Data Source";
		private SqlConnectionStringBuilder _sqlConnectionStringBuilder;

		#endregion

		#region Constructors

		public ConnectionStringBuilder() : this(null) { }

		public ConnectionStringBuilder(string connectionString)
		{
			this.OriginalConnectionString = connectionString;
		}

		#endregion

		#region Properties

		public virtual string ConnectionString
		{
			get => this.SqlConnectionStringBuilder.ConnectionString;
			set => this.SqlConnectionStringBuilder.ConnectionString = value;
		}

		public virtual string Database
		{
			get => !this.IsNull(() => this.SqlConnectionStringBuilder.InitialCatalog, this.DatabaseKey) ? this.SqlConnectionStringBuilder.InitialCatalog : null;
			set
			{
				if(value == null)
					this.SqlConnectionStringBuilder.Remove(this.DatabaseKey);
				else
					this.SqlConnectionStringBuilder.InitialCatalog = value;
			}
		}

		public virtual string DatabaseFilePath
		{
			get => !this.IsNull(() => this.SqlConnectionStringBuilder.AttachDBFilename, this.DatabaseFilePathKey) ? this.SqlConnectionStringBuilder.AttachDBFilename : null;
			set
			{
				if(value == null)
					this.SqlConnectionStringBuilder.Remove(this.DatabaseFilePathKey);
				else
					this.SqlConnectionStringBuilder.AttachDBFilename = value;
			}
		}

		protected internal virtual string DatabaseFilePathKey => _databaseFilePathKey;
		protected internal virtual string DatabaseKey => _databaseKey;
		protected internal virtual string OriginalConnectionString { get; }

		public virtual string Server
		{
			get => !this.IsNull(() => this.SqlConnectionStringBuilder.DataSource, this.ServerKey) ? this.SqlConnectionStringBuilder.DataSource : null;
			set
			{
				if(value == null)
					this.SqlConnectionStringBuilder.Remove(this.ServerKey);
				else
					this.SqlConnectionStringBuilder.DataSource = value;
			}
		}

		protected internal virtual string ServerKey => _serverKey;
		protected internal virtual SqlConnectionStringBuilder SqlConnectionStringBuilder => this._sqlConnectionStringBuilder ??= new SqlConnectionStringBuilder(this.OriginalConnectionString);

		#endregion

		#region Methods

		public virtual void Clear()
		{
			this.SqlConnectionStringBuilder.Clear();
		}

		public virtual IConnectionStringBuilder Copy()
		{
			return new ConnectionStringBuilder(this.ConnectionString);
		}

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
		[SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
		protected internal virtual bool IsNull(Func<object> property, string keyword)
		{
			if(property == null)
				throw new ArgumentNullException(nameof(property));

			var value = property.Invoke()?.ToString();

			if(!string.IsNullOrEmpty(value))
				return false;

			return !this.ConnectionString.Contains(keyword + "=");
		}

		#endregion
	}
}