# ROSCA Zeabur 存儲管理系統

本目錄包含 ROSCA 平安商會系統在 Zeabur 平台上的存儲管理配置和腳本。

## 📁 檔案結構

```
storage/zeabur/
├── README.md                    # 本說明文件
├── zeabur-volumes.json         # Zeabur 存儲卷配置
├── persistent-volumes.yml      # Kubernetes 持久化卷配置
├── storage-config.json         # 存儲配置設定
├── init-storage.sh            # 存儲初始化腳本
├── manage-storage.sh          # 存儲管理腳本
├── monitor-storage.sh         # 存儲監控腳本
└── backup-storage.sh          # 存儲備份腳本
```

## 🗂️ 存儲卷配置

### 應用程式存儲卷 (總計 15GB)

| 存儲卷 | 大小 | 掛載路徑 | 用途 | 保留期限 |
|--------|------|----------|------|----------|
| uploads | 5GB | /app/uploads | 通用檔案上傳 | 365天 |
| kyc-images | 2GB | /app/KycImages | KYC身份認證圖片 | 永久 |
| deposit-images | 2GB | /app/DepositImages | 存款憑證圖片 | 3年 |
| withdraw-images | 2GB | /app/WithdrawImages | 提款憑證圖片 | 3年 |
| ann-images | 2GB | /app/AnnImagessss | 公告圖片 | 2年 |
| logs | 1GB | /app/logs | 應用程式日誌 | 30天 |

### 資料庫存儲卷 (總計 11GB)

| 存儲卷 | 大小 | 掛載路徑 | 用途 |
|--------|------|----------|------|
| db-data | 10GB | /var/lib/mysql | MariaDB 資料庫資料 |
| db-logs | 1GB | /var/log/mysql | MariaDB 日誌 |

## 🚀 部署配置

### 1. Zeabur 服務配置

在 `zeabur.json` 中，每個需要存儲的服務都配置了相應的存儲卷：

```json
{
  "volumes": [
    {
      "name": "uploads",
      "dir": "/app/uploads",
      "size": "5GB"
    },
    {
      "name": "kyc-images",
      "dir": "/app/KycImages",
      "size": "2GB"
    }
    // ... 其他存儲卷
  ]
}
```

### 2. 環境變數配置

在 `.env.zeabur.example` 中設定存儲相關的環境變數：

```bash
# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 存儲路徑配置
UPLOADS_PATH=/app/uploads
KYC_IMAGES_PATH=/app/KycImages
DEPOSIT_IMAGES_PATH=/app/DepositImages
WITHDRAW_IMAGES_PATH=/app/WithdrawImages
ANN_IMAGES_PATH=/app/AnnImagessss
LOGS_PATH=/app/logs
```

## 🛠️ 管理腳本

### 初始化腳本 (init-storage.sh)

用於在 Zeabur 部署時初始化存儲目錄：

```bash
# 在容器啟動時執行
./init-storage.sh
```

功能：
- 建立所有必要的存儲目錄
- 設定適當的權限
- 建立配置檔案
- 驗證存儲設定

### 管理腳本 (manage-storage.sh)

日常存儲管理工具：

```bash
# 顯示存儲狀態
./manage-storage.sh status

# 初始化存儲
./manage-storage.sh init

# 清理過期檔案
./manage-storage.sh cleanup

# 檢查健康狀態
./manage-storage.sh check

# 優化存儲空間
./manage-storage.sh optimize

# 持續監控
./manage-storage.sh monitor
```

### 監控腳本 (monitor-storage.sh)

存儲使用情況監控：

```bash
# 顯示存儲狀態
./monitor-storage.sh status

# 顯示詳細資訊
./monitor-storage.sh detailed

# 檢查健康狀態
./monitor-storage.sh health

# 顯示系統資訊
./monitor-storage.sh system

# 生成監控報告
./monitor-storage.sh report

# 持續監控模式
./monitor-storage.sh watch
```

### 備份腳本 (backup-storage.sh)

存儲資料備份管理：

```bash
# 執行完整備份
./backup-storage.sh backup

# 列出所有備份
./backup-storage.sh list

# 驗證備份完整性
./backup-storage.sh verify

# 清理舊備份
./backup-storage.sh cleanup

# 恢復備份
./backup-storage.sh restore --backup-id 20250115-120000
```

## 📊 監控和告警

### 監控指標

- **磁碟使用率**: 監控各存儲卷的使用情況
- **檔案數量**: 追蹤檔案數量變化
- **存取頻率**: 分析檔案存取模式
- **錯誤率**: 監控存儲操作錯誤

### 告警閾值

- **警告**: 使用率 > 70%
- **告警**: 使用率 > 85%
- **危險**: 使用率 > 95%

### 自動清理

系統會根據設定的保留政策自動清理過期檔案：

- **通用上傳**: 365天後清理
- **KYC圖片**: 永久保留
- **存款/提款憑證**: 3年後清理
- **公告圖片**: 2年後清理
- **應用日誌**: 30天後清理

## 🔒 安全配置

### 檔案類型限制

每個存儲卷都有嚴格的檔案類型限制：

- **圖片存儲**: 僅允許 .jpg, .jpeg, .png, .gif
- **文件存儲**: 允許 .pdf, .doc, .docx, .xls, .xlsx
- **日誌存儲**: 僅允許 .log, .txt, .json

### 檔案大小限制

- **一般檔案**: 最大 10MB
- **圖片檔案**: 最大 5MB
- **公告圖片**: 最大 3MB
- **日誌檔案**: 最大 100MB

### 存取控制

- **KYC圖片**: 限制存取，需要加密
- **存款/提款憑證**: 限制存取，需要稽核日誌
- **公告圖片**: 公開存取，啟用快取
- **應用日誌**: 限制存取

## 🔄 備份策略

### 自動備份

- **排程**: 每日凌晨 2:00 執行
- **保留**: 30天
- **壓縮**: 啟用 gzip 壓縮
- **驗證**: 自動驗證備份完整性

### 備份類型

1. **完整備份**: 備份所有存儲卷
2. **增量備份**: 僅備份變更的檔案
3. **差異備份**: 備份自上次完整備份後的變更

### 恢復程序

1. 停止相關服務
2. 選擇要恢復的備份
3. 執行恢復腳本
4. 驗證資料完整性
5. 重新啟動服務

## 🚨 故障排除

### 常見問題

#### 1. 存儲空間不足

```bash
# 檢查使用情況
./monitor-storage.sh status

# 清理過期檔案
./manage-storage.sh cleanup

# 優化存儲空間
./manage-storage.sh optimize
```

#### 2. 檔案上傳失敗

```bash
# 檢查目錄權限
./manage-storage.sh check

# 檢查磁碟空間
df -h /app

# 檢查檔案類型和大小限制
```

#### 3. 備份失敗

```bash
# 檢查備份目錄權限
ls -la /app/backups

# 檢查磁碟空間
df -h /app/backups

# 手動執行備份
./backup-storage.sh backup
```

### 日誌檢查

```bash
# 檢查應用程式日誌
tail -f /app/logs/application.log

# 檢查存儲操作日誌
tail -f /app/logs/storage.log

# 檢查系統日誌
journalctl -u zeabur-storage
```

## 📞 支援聯絡

如遇到存儲相關問題，請聯絡系統管理員並提供：

1. 錯誤訊息和日誌
2. 存儲使用情況報告
3. 系統環境資訊
4. 操作步驟重現

---

**注意**: 在生產環境中修改存儲配置前，請務必先備份重要資料。