using System;
using System.Collections.Generic;

namespace DotNetBackEndApi.Configuration
{
    /// <summary>
    /// 資料庫連接配置類 (Zeabur 版本)
    /// 用於管理不同環境下的資料庫連接字串
    /// </summary>
    public class DatabaseConnectionConfig
    {
        /// <summary>
        /// 建立 Zeabur 環境的連接字串
        /// </summary>
        /// <param name="host">資料庫主機 (通常是 Zeabur 服務發現變數)</param>
        /// <param name="port">資料庫端口 (預設 3306)</param>
        /// <param name="database">資料庫名稱</param>
        /// <param name="user">資料庫用戶</param>
        /// <param name="password">資料庫密碼</param>
        /// <returns>完整的連接字串</returns>
        public static string BuildZeaburConnectionString(
            string host, 
            string port = "3306", 
            string database = "rosca_db", 
            string user = "rosca_user", 
            string password = "")
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("Database host cannot be null or empty", nameof(host));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Database password cannot be null or empty", nameof(password));

            return $"Server={host};Port={port};User Id={user};Password={password};Database={database};" +
                   $"CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;" +
                   $"ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;";
        }

        /// <summary>
        /// 從環境變數建立連接字串
        /// </summary>
        /// <returns>連接字串</returns>
        public static string BuildFromEnvironmentVariables()
        {
            var host = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") 
                      ?? Environment.GetEnvironmentVariable("DB_HOST") 
                      ?? "localhost";
            
            var port = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") 
                      ?? Environment.GetEnvironmentVariable("DB_PORT") 
                      ?? "3306";
            
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "rosca_db";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "rosca_user";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            return BuildZeaburConnectionString(host, port, database, user, password);
        }

        /// <summary>
        /// 驗證連接字串格式
        /// </summary>
        /// <param name="connectionString">要驗證的連接字串</param>
        /// <returns>是否有效</returns>
        public static bool ValidateConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return false;

            var requiredKeys = new[] { "Server", "Database", "User Id", "Password" };
            
            foreach (var key in requiredKeys)
            {
                if (!connectionString.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 從連接字串提取資料庫資訊
        /// </summary>
        /// <param name="connectionString">連接字串</param>
        /// <returns>資料庫資訊字典</returns>
        public static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (string.IsNullOrEmpty(connectionString))
                return result;

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var keyValue = part.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    result[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            return result;
        }

        /// <summary>
        /// 取得不同環境的連接字串範本
        /// </summary>
        public static class Templates
        {
            /// <summary>
            /// Zeabur 生產環境連接字串範本
            /// </summary>
            public const string ZeaburProduction = 
                "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};" +
                "User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};" +
                "CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;" +
                "ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;";

            /// <summary>
            /// 本地開發環境連接字串範本
            /// </summary>
            public const string LocalDevelopment = 
                "Server=localhost;Port=3306;User Id=rosca_user;Password=development_password;" +
                "Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;";

            /// <summary>
            /// Docker Compose 環境連接字串範本
            /// </summary>
            public const string DockerCompose = 
                "Server=mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};" +
                "Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;";

            /// <summary>
            /// 啟用 SSL 的連接字串範本
            /// </summary>
            public const string WithSSL = 
                "Server=${DB_HOST};Port=${DB_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};" +
                "Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;" +
                "SslMode=Required;SslCert=${SSL_CERT_PATH};SslKey=${SSL_KEY_PATH};SslCa=${SSL_CA_PATH};";
        }

        /// <summary>
        /// 連接字串建構器
        /// </summary>
        public class Builder
        {
            private string _server = "localhost";
            private string _port = "3306";
            private string _database = "rosca_db";
            private string _userId = "rosca_user";
            private string _password = "";
            private string _charset = "utf8mb4";
            private bool _allowUserVariables = true;
            private bool _useAffectedRows = false;
            private int _connectionTimeout = 30;
            private int _commandTimeout = 60;
            private bool _pooling = true;
            private int _minPoolSize = 5;
            private int _maxPoolSize = 100;
            private int _connectionLifetime = 300;

            public Builder Server(string server) { _server = server; return this; }
            public Builder Port(string port) { _port = port; return this; }
            public Builder Database(string database) { _database = database; return this; }
            public Builder UserId(string userId) { _userId = userId; return this; }
            public Builder Password(string password) { _password = password; return this; }
            public Builder CharSet(string charset) { _charset = charset; return this; }
            public Builder AllowUserVariables(bool allow) { _allowUserVariables = allow; return this; }
            public Builder UseAffectedRows(bool use) { _useAffectedRows = use; return this; }
            public Builder ConnectionTimeout(int timeout) { _connectionTimeout = timeout; return this; }
            public Builder CommandTimeout(int timeout) { _commandTimeout = timeout; return this; }
            public Builder Pooling(bool pooling) { _pooling = pooling; return this; }
            public Builder MinPoolSize(int size) { _minPoolSize = size; return this; }
            public Builder MaxPoolSize(int size) { _maxPoolSize = size; return this; }
            public Builder ConnectionLifetime(int lifetime) { _connectionLifetime = lifetime; return this; }

            public string Build()
            {
                return $"Server={_server};Port={_port};User Id={_userId};Password={_password};" +
                       $"Database={_database};CharSet={_charset};" +
                       $"AllowUserVariables={_allowUserVariables};UseAffectedRows={_useAffectedRows};" +
                       $"ConnectionTimeout={_connectionTimeout};CommandTimeout={_commandTimeout};" +
                       $"Pooling={_pooling};MinimumPoolSize={_minPoolSize};MaximumPoolSize={_maxPoolSize};" +
                       $"ConnectionLifeTime={_connectionLifetime};";
            }
        }
    }

    /// <summary>
    /// Zeabur 服務發現輔助類
    /// </summary>
    public static class ZeaburServiceDiscovery
    {
        /// <summary>
        /// 取得 MariaDB 服務主機
        /// </summary>
        public static string GetMariaDBHost()
        {
            return Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") 
                   ?? Environment.GetEnvironmentVariable("DB_HOST") 
                   ?? "mariadb";
        }

        /// <summary>
        /// 取得 MariaDB 服務端口
        /// </summary>
        public static string GetMariaDBPort()
        {
            return Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") 
                   ?? Environment.GetEnvironmentVariable("DB_PORT") 
                   ?? "3306";
        }

        /// <summary>
        /// 取得後端服務主機
        /// </summary>
        public static string GetBackendServiceHost()
        {
            return Environment.GetEnvironmentVariable("ZEABUR_BACKEND_SERVICE_DOMAIN") 
                   ?? "backend-service";
        }

        /// <summary>
        /// 取得 API Gateway 主機
        /// </summary>
        public static string GetAPIGatewayHost()
        {
            return Environment.GetEnvironmentVariable("ZEABUR_BACKEND_DOMAIN") 
                   ?? "api-gateway";
        }

        /// <summary>
        /// 檢查是否在 Zeabur 環境中運行
        /// </summary>
        public static bool IsRunningOnZeabur()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST"));
        }
    }
}