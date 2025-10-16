# ROSCA 檔案上傳系統

本文檔說明 ROSCA 平安商會系統的檔案上傳功能配置和使用方法。

## 📁 檔案結構

```
Infrastructure/
├── Common/
│   └── FileUploadConfiguration.cs     # 檔案上傳配置類
├── Services/
│   └── FileUploadService.cs          # 檔案上傳服務
├── Extensions/
│   └── FileUploadServiceExtensions.cs # 服務註冊擴展
└── HealthChecks/
    └── FileUploadHealthCheck.cs       # 健康檢查

Presentation/
└── Controllers/
    └── FileUploadController.cs        # 檔案上傳控制器
```

## 🚀 功能特性

### 核心功能
- ✅ 多類型檔案上傳支援
- ✅ 檔案大小和類型驗證
- ✅ 安全檔案名稱處理
- ✅ 檔案完整性驗證 (SHA256)
- ✅ 自動目錄管理
- ✅ 過期檔案自動清理
- ✅ 檔案存取權限控制

### 安全特性
- 🔒 檔案類型白名單驗證
- 🔒 檔案名稱清理和驗證
- 🔒 路徑遍歷攻擊防護
- 🔒 檔案大小限制
- 🔒 病毒掃描支援 (可選)
- 🔒 存取日誌記錄

### 管理功能
- 📊 存儲使用統計
- 🧹 自動檔案清理
- 💾 檔案備份支援
- 📈 健康狀態監控
- 🔄 背景任務處理

## 📋 支援的檔案類型

### 通用上傳 (uploads)
- **路徑**: `/app/uploads`
- **大小限制**: 10MB
- **允許類型**: `.jpg`, `.jpeg`, `.png`, `.gif`, `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`
- **保留期限**: 365天

### KYC 身份認證圖片 (kyc-images)
- **路徑**: `/app/KycImages`
- **大小限制**: 5MB
- **允許類型**: `.jpg`, `.jpeg`, `.png`
- **保留期限**: 永久保留
- **安全等級**: 高 (加密存儲)

### 存款憑證圖片 (deposit-images)
- **路徑**: `/app/DepositImages`
- **大小限制**: 5MB
- **允許類型**: `.jpg`, `.jpeg`, `.png`, `.pdf`
- **保留期限**: 3年 (1095天)
- **安全等級**: 高 (稽核日誌)

### 提款憑證圖片 (withdraw-images)
- **路徑**: `/app/WithdrawImages`
- **大小限制**: 5MB
- **允許類型**: `.jpg`, `.jpeg`, `.png`, `.pdf`
- **保留期限**: 3年 (1095天)
- **安全等級**: 高 (稽核日誌)

### 公告圖片 (announcement-images)
- **路徑**: `/app/AnnImagessss`
- **大小限制**: 3MB
- **允許類型**: `.jpg`, `.jpeg`, `.png`, `.gif`
- **保留期限**: 2年 (730天)
- **安全等級**: 中 (公開存取)

### 應用程式日誌 (logs)
- **路徑**: `/app/logs`
- **大小限制**: 100MB
- **允許類型**: `.log`, `.txt`, `.json`
- **保留期限**: 30天
- **壓縮**: 7天後自動壓縮

## ⚙️ 配置設定

### appsettings.zeabur.json

```json
{
  "FileUpload": {
    "MaxFileSize": "${FILE_UPLOAD_MAX_SIZE}",
    "AllowedExtensions": "${FILE_UPLOAD_EXTENSIONS}",
    "UploadPath": "/app/uploads",
    "KycImagesPath": "/app/KycImages",
    "DepositImagesPath": "/app/DepositImages",
    "WithdrawImagesPath": "/app/WithdrawImages",
    "AnnouncementImagesPath": "/app/AnnImagessss",
    "LogsPath": "/app/logs",
    "EnableCompression": true,
    "EnableEncryption": false,
    "EnableVirusScanning": true,
    "RetentionSettings": {
      "UploadRetentionDays": 365,
      "KycImageRetentionDays": -1,
      "DepositImageRetentionDays": 1095,
      "WithdrawImageRetentionDays": 1095,
      "AnnouncementImageRetentionDays": 730,
      "LogRetentionDays": 30
    },
    "SecuritySettings": {
      "EnableContentValidation": true,
      "EnableFileNameSanitization": true,
      "EnableAccessLogging": true,
      "MaxFileNameLength": 255,
      "AllowOverwrite": false
    }
  }
}
```

### 環境變數

```bash
# 基本設定
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 路徑配置
UPLOADS_PATH=/app/uploads
KYC_IMAGES_PATH=/app/KycImages
DEPOSIT_IMAGES_PATH=/app/DepositImages
WITHDRAW_IMAGES_PATH=/app/WithdrawImages
ANN_IMAGES_PATH=/app/AnnImagessss
LOGS_PATH=/app/logs

# 安全設定
FILE_UPLOAD_ENABLE_COMPRESSION=true
FILE_UPLOAD_ENABLE_ENCRYPTION=false
FILE_UPLOAD_ENABLE_VIRUS_SCANNING=true
```

## 🔧 服務註冊

### Program.cs

```csharp
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 註冊檔案上傳服務
builder.Services.AddFileUploadServices(builder.Configuration);
builder.Services.ConfigureFileUploadMiddleware(builder.Configuration);
builder.Services.AddFileUploadHealthCheck();

var app = builder.Build();

// 確保上傳目錄存在
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<FileUploadConfiguration>();
    config.EnsureUploadDirectoriesExist();
}

app.Run();
```

## 📡 API 端點

### 上傳檔案
```http
POST /api/fileupload/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

{
  "file": [檔案],
  "fileType": "Upload|KycImage|DepositImage|WithdrawImage|AnnouncementImage"
}
```

### 下載檔案
```http
GET /api/fileupload/download?filePath={檔案路徑}
Authorization: Bearer {token}
```

### 刪除檔案
```http
DELETE /api/fileupload/delete?filePath={檔案路徑}
Authorization: Bearer {token}
```

### 取得檔案資訊
```http
GET /api/fileupload/info?filePath={檔案路徑}
Authorization: Bearer {token}
```

### 取得存儲統計 (管理員)
```http
GET /api/fileupload/storage-stats
Authorization: Bearer {admin_token}
```

### 清理過期檔案 (管理員)
```http
POST /api/fileupload/cleanup
Authorization: Bearer {admin_token}
```

### 取得上傳配置
```http
GET /api/fileupload/config
Authorization: Bearer {token}
```

## 🏥 健康檢查

檔案上傳系統包含完整的健康檢查功能：

```http
GET /health
```

檢查項目：
- ✅ 配置有效性驗證
- ✅ 目錄存在性和權限
- ✅ 磁碟空間使用情況
- ✅ 檔案系統可用性

## 🧹 自動清理

系統會自動執行以下清理任務：

### 背景服務
- **執行頻率**: 每24小時
- **清理範圍**: 所有存儲目錄
- **清理規則**: 根據保留政策刪除過期檔案

### 手動清理
```bash
# 透過 API 手動觸發清理
curl -X POST /api/fileupload/cleanup \
  -H "Authorization: Bearer {admin_token}"
```

## 📊 監控和日誌

### 日誌記錄
- 檔案上傳/下載/刪除操作
- 檔案驗證失敗
- 清理任務執行結果
- 錯誤和異常情況

### 監控指標
- 存儲使用量
- 檔案數量統計
- 操作成功率
- 系統健康狀態

## 🔒 安全考量

### 檔案驗證
1. **檔案類型驗證**: 基於副檔名和 MIME 類型
2. **檔案大小限制**: 防止大檔案攻擊
3. **檔案名稱清理**: 移除危險字符
4. **內容驗證**: 檢查檔案實際內容

### 存取控制
1. **路徑驗證**: 防止目錄遍歷攻擊
2. **權限檢查**: 基於角色的存取控制
3. **存取日誌**: 記錄所有檔案操作
4. **檔案隔離**: 不同類型檔案分別存儲

### 資料保護
1. **檔案加密**: 敏感檔案加密存儲 (可選)
2. **完整性驗證**: SHA256 雜湊值驗證
3. **備份策略**: 定期備份重要檔案
4. **稽核追蹤**: 完整的操作記錄

## 🚨 故障排除

### 常見問題

#### 1. 檔案上傳失敗
```bash
# 檢查目錄權限
ls -la /app/uploads

# 檢查磁碟空間
df -h /app

# 檢查健康狀態
curl /health
```

#### 2. 檔案無法存取
```bash
# 檢查檔案是否存在
ls -la /app/uploads/filename

# 檢查檔案權限
stat /app/uploads/filename

# 檢查服務日誌
tail -f /app/logs/application.log
```

#### 3. 自動清理不工作
```bash
# 檢查背景服務狀態
curl /health

# 手動觸發清理
curl -X POST /api/fileupload/cleanup

# 檢查清理日誌
grep "cleanup" /app/logs/application.log
```

### 效能調優

#### 1. 大檔案上傳優化
```json
{
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 52428800
    }
  }
}
```

#### 2. 並發上傳限制
```csharp
services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024;
});
```

#### 3. 檔案壓縮設定
```json
{
  "FileUpload": {
    "EnableCompression": true,
    "CompressionLevel": 6
  }
}
```

## 📞 支援

如遇到檔案上傳相關問題，請提供：

1. 錯誤訊息和堆疊追蹤
2. 檔案類型和大小資訊
3. 健康檢查結果
4. 相關日誌檔案

---

**注意**: 在生產環境中，請確保所有敏感檔案都有適當的存取控制和備份策略。