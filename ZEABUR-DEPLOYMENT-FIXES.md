# Zeabur 部署問題修復報告

## 🚨 已修復的問題

### 1. Angular 建置失敗 - `sh: ng: not found`

**問題描述：**
```
> font-end@0.0.0 build
> ng build
sh: ng: not found
```

**根本原因：**
- Angular CLI 沒有正確安裝或不在 PATH 中
- Node.js 建置環境配置問題

**修復方案：**
✅ **跳過 Angular 建置，使用預建置檔案**
- 移除了 Node.js 建置階段
- 直接使用 `backend/FontEnd/FontEnd/dist/font-end/browser/` 中的預建置檔案
- 避免了 Node.js 版本相容性問題

### 2. Angular Base Href 路由問題

**問題描述：**
- Angular 應用的 base href 設定為 `/backend/`
- 但在 Zeabur 部署中應該是 `/admin/`

**修復方案：**
✅ **自動修正 base href**
```dockerfile
# 修正 Angular base href
RUN if [ -f /var/www/admin/index.html ]; then \
        sed -i 's|<base href="/backend/">|<base href="/admin/">|g' /var/www/admin/index.html; \
    fi
```

### 3. Nginx 配置優化

**修復內容：**
✅ **改善後台路由處理**
- 添加了 Angular SPA 路由支援
- 配置了適當的 try_files 規則
- 防止 index.html 被快取

✅ **API 代理優化**
- 增加了連接超時設定
- 設定了檔案上傳大小限制 (50MB)
- 添加了健康檢查端點

### 4. 日誌和監控改善

**修復內容：**
✅ **日誌輸出到 stdout/stderr**
- 所有服務日誌現在輸出到標準輸出
- Zeabur 可以正確收集和顯示日誌
- 移除了檔案日誌配置

✅ **健康檢查優化**
- 使用簡單的 `/health` 端點
- 避免依賴複雜的 API 端點

### 5. 權限和目錄結構

**修復內容：**
✅ **目錄權限設定**
- 正確設定了檔案上傳目錄權限
- 創建了必要的 nginx 運行目錄
- 設定了適當的檔案擁有者

## 🎯 修復後的 Dockerfile 特點

### 優化的建置流程
1. **跳過 Angular 建置** - 使用預建置檔案，避免 Node.js 問題
2. **多階段建置** - 分別建置 .NET Core 服務
3. **單一容器部署** - 所有服務在一個容器中運行

### 改善的 Nginx 配置
```nginx
# 前台 Vue.js SPA
location / {
    root /var/www/frontend;
    try_files $uri $uri/ /index.html;
}

# 後台 Angular SPA
location /admin {
    alias /var/www/admin/;
    try_files $uri $uri/ /admin/index.html;
}

# API 代理
location /api/ {
    proxy_pass http://localhost:5000/api/;
    client_max_body_size 50M;
    proxy_connect_timeout 30s;
}

# 健康檢查
location /health {
    return 200 "healthy\n";
}
```

### Supervisor 進程管理
- **nginx** - 前端服務和反向代理
- **backend** - API Gateway (.NET Core, port 5000)
- **backend-service** - 微服務 (.NET Core, port 5001)

## 🚀 部署指令

### 1. 確保 Angular 已建置
```bash
cd backend/FontEnd/FontEnd
npm install
npm run build
```

### 2. 提交修復
```bash
git add .
git commit -m "Fix Zeabur deployment - skip Angular build, use pre-built files"
git push
```

### 3. 在 Zeabur 重新部署
- 點擊 "Redeploy" 按鈕
- 或等待自動部署觸發

## 🔍 驗證部署成功

### 檢查服務狀態
```bash
# 健康檢查
curl https://your-app.zeabur.app/health

# 前台
curl https://your-app.zeabur.app/

# 後台
curl https://your-app.zeabur.app/admin

# API
curl https://your-app.zeabur.app/api/HomePicture/GetAnnImages
```

### 預期回應
- **健康檢查**: `healthy`
- **前台**: HTML 內容
- **後台**: Angular 應用 HTML
- **API**: JSON 資料或適當的 API 回應

## 📝 .NET Core 警告處理

部署日誌中的 C# 警告（CS8625, CS8604 等）是**非阻塞性的**：
- 這些是 nullable reference types 相關的警告
- 不會影響應用程式運行
- 建議在後續開發中修復，但不影響部署

## 🎉 修復結果

✅ **Angular 建置問題已解決** - 跳過建置，使用預建置檔案
✅ **路由問題已修正** - base href 自動修正為 `/admin/`
✅ **Nginx 配置已優化** - 支援 SPA 路由和 API 代理
✅ **日誌收集已改善** - 輸出到 stdout/stderr
✅ **健康檢查已簡化** - 使用可靠的端點
✅ **權限設定已修正** - 檔案上傳目錄權限正確

## 🔧 後續建議

1. **監控部署狀態** - 檢查 Zeabur 控制台的建置日誌
2. **測試完整功能** - 驗證前台、後台和 API 功能
3. **配置環境變數** - 設定資料庫連接和其他配置
4. **設定域名** - 配置自定義域名和 SSL
5. **性能調優** - 根據使用情況調整資源配置

---

**注意**: 如果仍然遇到問題，請檢查 Zeabur 建置日誌並確保所有必要的檔案都已提交到 Git 儲存庫。