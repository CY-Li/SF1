using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DotNetBackEndCleanArchitecture.Infrastructure.Common
{
    /// <summary>
    /// Zeabur 資料庫配置類別
    /// 用於管理 Zeabur 環境下的資料庫連接配置
    /// </summary>
    public static class ZeaburDatabaseConfiguration
    {
        /// <summary>
        /// 配置 Zeabur 資料庫連接
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddZeaburDatabase<TContext>(
            this IServiceCollection services, 
            IConfiguration configuration) 
            where TContext : DbContext
        {
            var connectionString = GetZeaburConnectionString(configuration);
            
            services.AddDbContext<TContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.CharSet(CharSet.Utf8Mb4);
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: configuration.GetValue<int>("DatabaseConfiguration:MaxRetryCount", 3),
                        maxRetryDelay: TimeSpan.Parse(configuration.GetValue<string>("DatabaseConfiguration:MaxRetryDelay", "00:00:30")),
                        errorNumbersToAdd: null
                    );
                    mysqlOptions.CommandTimeout(configuration.GetValue<int>("DatabaseConfiguration:CommandTimeout", 60));
                });
                
                // 根據環境設定日誌等級
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (environment == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
                else
                {
                    options.EnableSensitiveDataLogging(configuration.GetValue<bool>("DatabaseConfiguration:EnableSensitiveDataLogging", false));
                    options.EnableDetailedErrors(configuration.GetValue<bool>("DatabaseConfiguration:EnableDetailedErrors", false));
                }
            });
            
            return services;
        }

        /// <summary>
        /// 取得 Zeabur 環境的連接字串
        /// </summary>
        /// <param name="configuration">配置</param>
        /// <returns>連接字串</returns>
        public static string GetZeaburConnectionString(IConfiguration configuration)
        {
            // 優先使用 Zeabur 服務發現變數
            var host = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") 
                      ?? Environment.GetEnvironmentVariable("DB_HOST") 
                      ?? "localhost";
            
            var port = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") 
                      ?? Environment.GetEnvironmentVariable("DB_PORT") 
                      ?? "3306";
            
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "rosca_db";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "rosca_user";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("資料庫密碼未設定。請設定 DB_PASSWORD 環境變數。");
            }

            // 從配置取得連接參數
            var connectionTimeout = configuration.GetValue<int>("DatabaseConfiguration:ConnectionTimeout", 30);
            var commandTimeout = configuration.GetValue<int>("DatabaseConfiguration:CommandTimeout", 60);
            var minPoolSize = configuration.GetValue<int>("DatabaseConfiguration:MinPoolSize", 5);
            var maxPoolSize = configuration.GetValue<int>("DatabaseConfiguration:MaxPoolSize", 100);
            var connectionLifetime = configuration.GetValue<int>("DatabaseConfiguration:ConnectionLifetime", 300);

            return $"Server={host};Port={port};User Id={user};Password={password};Database={database};" +
                   $"CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;" +
                   $"ConnectionTimeout={connectionTimeout};CommandTimeout={commandTimeout};" +
                   $"Pooling=true;MinimumPoolSize={minPoolSize};MaximumPoolSize={maxPoolSize};" +
                   $"ConnectionLifeTime={connectionLifetime};";
        }

        /// <summary>
        /// 驗證資料庫連接
        /// </summary>
        /// <param name="connectionString">連接字串</param>
        /// <returns>是否連接成功</returns>
        public static async Task<bool> ValidateConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySql.Data.MySqlClient.MySqlCommand("SELECT 1", connection);
                await command.ExecuteScalarAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得資料庫資訊
        /// </summary>
        /// <param name="connectionString">連接字串</param>
        /// <returns>資料庫資訊</returns>
        public static async Task<DatabaseInfo> GetDatabaseInfoAsync(string connectionString)
        {
            var info = new DatabaseInfo();
            
            try
            {
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                // 取得版本資訊
                using var versionCommand = new MySql.Data.MySqlClient.MySqlCommand("SELECT VERSION()", connection);
                info.Version = await versionCommand.ExecuteScalarAsync() as string ?? "";
                
                // 取得字符集資訊
                using var charsetCommand = new MySql.Data.MySqlClient.MySqlCommand(
                    "SELECT @@character_set_database, @@collation_database", connection);
                using var reader = await charsetCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    info.CharacterSet = reader.GetString(0);
                    info.Collation = reader.GetString(1);
                }
                reader.Close();
                
                // 取得時區資訊
                using var timezoneCommand = new MySql.Data.MySqlClient.MySqlCommand("SELECT @@time_zone", connection);
                info.TimeZone = await timezoneCommand.ExecuteScalarAsync() as string ?? "";
                
                // 取得連接資訊
                info.ServerInfo = connection.ServerVersion;
                info.Database = connection.Database;
                info.DataSource = connection.DataSource;
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
            }
            
            return info;
        }

        /// <summary>
        /// 檢查是否在 Zeabur 環境中運行
        /// </summary>
        /// <returns>是否在 Zeabur 環境</returns>
        public static bool IsRunningOnZeabur()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST"));
        }

        /// <summary>
        /// 取得環境資訊
        /// </summary>
        /// <returns>環境資訊字典</returns>
        public static Dictionary<string, string> GetEnvironmentInfo()
        {
            return new Dictionary<string, string>
            {
                ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["IsZeabur"] = IsRunningOnZeabur().ToString(),
                ["MariaDBHost"] = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") ?? "Not Set",
                ["MariaDBPort"] = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") ?? "Not Set",
                ["DatabaseName"] = Environment.GetEnvironmentVariable("DB_NAME") ?? "Not Set",
                ["DatabaseUser"] = Environment.GetEnvironmentVariable("DB_USER") ?? "Not Set",
                ["HasPassword"] = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PASSWORD")) ? "Yes" : "No"
            };
        }
    }

    /// <summary>
    /// 資料庫資訊類別
    /// </summary>
    public class DatabaseInfo
    {
        public string Version { get; set; } = "";
        public string CharacterSet { get; set; } = "";
        public string Collation { get; set; } = "";
        public string TimeZone { get; set; } = "";
        public string ServerInfo { get; set; } = "";
        public string Database { get; set; } = "";
        public string DataSource { get; set; } = "";
        public string Error { get; set; } = "";
        
        public bool IsValid => string.IsNullOrEmpty(Error);
    }

    /// <summary>
    /// Zeabur 健康檢查擴展
    /// </summary>
    public static class ZeaburHealthCheckExtensions
    {
        /// <summary>
        /// 添加 Zeabur 資料庫健康檢查
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddZeaburDatabaseHealthCheck(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var connectionString = ZeaburDatabaseConfiguration.GetZeaburConnectionString(configuration);
            
            services.AddHealthChecks()
                .AddMySql(connectionString, 
                    name: "mariadb",
                    timeout: TimeSpan.FromSeconds(30),
                    tags: new[] { "database", "mariadb", "zeabur" });
            
            return services;
        }
    }
}