# 🔧 Backend Service URL 修復

## 問題分析
錯誤 `"Name or service not known (backend-service:5001)"` 的原因：

### 🔍 **根本原因**
- `appsettings.json` 中的 `APIUrl` 設定為 `"http://backend-service:5001/"`
- `backend-service` 是 Docker Compose 環境中的服務名稱
- 在 Zeabur 單一容器部署中，所有服務都在同一個容器內
- 應該使用 `localhost` 而不是服務名稱

### 📋 **專案架構**
這個專案有兩個後端服務：
1. **backend-service** - .NET Core 微服務（端口 5001）
2. **backend** - .NET Core API Gateway（端口 5000）

## ✅ **已修復**

### 1. 修改 appsettings.json
```json
// 修改前
"APIUrl": "http://backend-service:5001/"

// 修改後
"APIUrl": "http://localhost:5001/"
```

### 2. Dockerfile 配置確認
Supervisord 配置正確：
```ini
[program:backend-service]
command=dotnet DotNetBackEndService.dll
directory=/app/backend-service
environment=ASPNETCORE_URLS="http://+:5001"
```

## 🚀 **測試步驟**

### 1. 重新部署應用
在 Zeabur 控制台重新部署 `rosca-app` 服務

### 2. 測試 API
```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"admin","mm_pwd":"Admin123456"}'
```

### 3. 檢查服務健康狀態
```bash
curl https://sf-test.zeabur.app/health
```

## 🎯 **預期結果**
- ✅ 不再出現 "Name or service not known" 錯誤
- ✅ backend-service 可以正常通過 localhost:5001 訪問
- ✅ API 請求正常處理
- ✅ 登入功能正常運作

## 📝 **技術說明**

### Docker Compose vs Zeabur 部署差異
- **Docker Compose**: 多容器，服務間通過服務名稱通信
- **Zeabur**: 單容器，服務間通過 localhost 通信

### 服務端口配置
- **backend-service**: localhost:5001
- **backend (API Gateway)**: localhost:5000
- **nginx**: 對外端口 80/443

這個修復確保了在 Zeabur 單一容器環境中，服務間的內部通信正常運作。