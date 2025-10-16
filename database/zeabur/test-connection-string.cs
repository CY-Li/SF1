using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace ZeaburDatabaseTest
{
    /// <summary>
    /// Zeabur 資料庫連接字串測試程式
    /// 用於驗證不同環境下的資料庫連接
    /// </summary>
    public class ConnectionStringTester
    {
        /// <summary>
        /// 測試連接字串是否有效
        /// </summary>
        /// <param name="connectionString">要測試的連接字串</param>
        /// <returns>測試結果</returns>
        public static async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                // 執行簡單查詢測試
                using var command = new MySqlCommand("SELECT 1", connection);
                var result = await command.ExecuteScalarAsync();
                
                Console.WriteLine($"✅ 連接成功！查詢結果: {result}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 連接失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 測試資料庫基本資訊
        /// </summary>
        /// <param name="connectionString">連接字串</param>
        /// <returns>資料庫資訊</returns>
        public static async Task<DatabaseInfo> GetDatabaseInfoAsync(string connectionString)
        {
            var info = new DatabaseInfo();
            
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                // 取得版本資訊
                using var versionCommand = new MySqlCommand("SELECT VERSION()", connection);
                info.Version = await versionCommand.ExecuteScalarAsync() as string;
                
                // 取得字符集資訊
                using var charsetCommand = new MySqlCommand("SELECT @@character_set_database, @@collation_database", connection);
                using var reader = await charsetCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    info.CharacterSet = reader.GetString(0);
                    info.Collation = reader.GetString(1);
                }
                reader.Close();
                
                // 取得時區資訊
                using var timezoneCommand = new MySqlCommand("SELECT @@time_zone", connection);
                info.TimeZone = await timezoneCommand.ExecuteScalarAsync() as string;
                
                // 取得連接資訊
                info.ServerInfo = connection.ServerVersion;
                info.Database = connection.Database;
                info.DataSource = connection.DataSource;
                
                Console.WriteLine("📊 資料庫資訊:");
                Console.WriteLine($"   版本: {info.Version}");
                Console.WriteLine($"   字符集: {info.CharacterSet}");
                Console.WriteLine($"   排序規則: {info.Collation}");
                Console.WriteLine($"   時區: {info.TimeZone}");
                Console.WriteLine($"   資料庫: {info.Database}");
                Console.WriteLine($"   主機: {info.DataSource}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 取得資料庫資訊失敗: {ex.Message}");
            }
            
            return info;
        }

        /// <summary>
        /// 測試所有預定義的連接字串範本
        /// </summary>
        public static async Task TestAllConnectionTemplatesAsync()
        {
            Console.WriteLine("🧪 開始測試所有連接字串範本...\n");
            
            var templates = new Dictionary<string, string>
            {
                ["Zeabur Production"] = BuildZeaburConnectionString(),
                ["Local Development"] = BuildLocalConnectionString(),
                ["Docker Compose"] = BuildDockerConnectionString()
            };
            
            foreach (var template in templates)
            {
                Console.WriteLine($"🔍 測試 {template.Key}:");
                Console.WriteLine($"   連接字串: {MaskPassword(template.Value)}");
                
                var success = await TestConnectionAsync(template.Value);
                if (success)
                {
                    await GetDatabaseInfoAsync(template.Value);
                }
                
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 建立 Zeabur 環境連接字串
        /// </summary>
        private static string BuildZeaburConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "rosca_db";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "rosca_user";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
            
            return $"Server={host};Port={port};User Id={user};Password={password};Database={database};" +
                   $"CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;" +
                   $"ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;";
        }

        /// <summary>
        /// 建立本地開發環境連接字串
        /// </summary>
        private static string BuildLocalConnectionString()
        {
            return "Server=localhost;Port=3306;User Id=rosca_user;Password=development_password;" +
                   "Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;";
        }

        /// <summary>
        /// 建立 Docker Compose 環境連接字串
        /// </summary>
        private static string BuildDockerConnectionString()
        {
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "rosca_db";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "rosca_user";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
            
            return $"Server=mariadb;Port=3306;User Id={user};Password={password};" +
                   $"Database={database};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;";
        }

        /// <summary>
        /// 隱藏連接字串中的密碼
        /// </summary>
        private static string MaskPassword(string connectionString)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"Password=([^;]+)", 
                "Password=***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        /// <summary>
        /// 主程式入口
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Zeabur 資料庫連接字串測試工具");
            Console.WriteLine("=====================================\n");
            
            // 顯示環境變數
            Console.WriteLine("🔧 環境變數:");
            Console.WriteLine($"   ZEABUR_MARIADB_CONNECTION_HOST: {Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_HOST") ?? "未設定"}");
            Console.WriteLine($"   ZEABUR_MARIADB_CONNECTION_PORT: {Environment.GetEnvironmentVariable("ZEABUR_MARIADB_CONNECTION_PORT") ?? "未設定"}");
            Console.WriteLine($"   DB_NAME: {Environment.GetEnvironmentVariable("DB_NAME") ?? "未設定"}");
            Console.WriteLine($"   DB_USER: {Environment.GetEnvironmentVariable("DB_USER") ?? "未設定"}");
            Console.WriteLine($"   DB_PASSWORD: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PASSWORD")) ? "未設定" : "***")}\n");
            
            // 如果提供了特定的連接字串參數
            if (args.Length > 0)
            {
                Console.WriteLine($"🔍 測試自定義連接字串:");
                Console.WriteLine($"   連接字串: {MaskPassword(args[0])}");
                
                var success = await TestConnectionAsync(args[0]);
                if (success)
                {
                    await GetDatabaseInfoAsync(args[0]);
                }
            }
            else
            {
                // 測試所有範本
                await TestAllConnectionTemplatesAsync();
            }
            
            Console.WriteLine("✅ 測試完成！");
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
    }
}