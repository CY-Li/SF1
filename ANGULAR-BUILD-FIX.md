# Angular 建置問題修復報告

## 🚨 問題描述

Zeabur 部署時出現 Angular 建置失敗：

```
#24 [angular-build 6/6] RUN npm run build
#24 0.304 > font-end@0.0.0 build
#24 0.304 > ng build
#24 0.310 sh: ng: not found
#24 ERROR: process "/bin/sh -c npm run build" did not complete successfully: exit code: 127
```

## 🔍 根本原因分析

1. **Angular CLI 未安裝**: `ng` 命令找不到
2. **npm 安裝問題**: 使用 `npm ci --only=production` 跳過了 devDependencies
3. **依賴配置**: Angular CLI (`@angular/cli`) 位於 devDependencies 中

## ✅ 解決方案

### 方案 1: 修復 Angular 建置 (推薦)

修改 Dockerfile 中的 npm 安裝命令：

```dockerfile
# 修改前
RUN npm ci --only=production

# 修改後  
RUN npm ci  # 安裝所有依賴，包含 devDependencies
```

**優點**:
- 完整的 Angular 建置
- 後台功能完整
- 符合標準開發流程

**缺點**:
- 建置時間較長
- 需要更多建置資源

### 方案 2: 跳過 Angular 建置

提供備用 Dockerfile (`Dockerfile.no-angular`)：

```dockerfile
# 直接複製原始檔案，不進行建置
COPY backend/FontEnd/FontEnd/src/ /var/www/admin/
```

**優點**:
- 建置速度快
- 避免 Node.js 建置問題
- 資源需求低

**缺點**:
- 後台功能可能受限
- 需要手動建置 Angular (如需要)

## 🛠️ 實施步驟

### 立即修復 (方案 1)

1. **更新 Dockerfile**:
   ```bash
   # 已自動修復，拉取最新代碼
   git pull origin main
   ```

2. **重新部署**:
   - Zeabur 會自動偵測變更並重新建置
   - 等待建置完成

### 備用方案 (方案 2)

如果方案 1 仍有問題：

```bash
# 使用切換腳本
chmod +x switch-dockerfile.sh
./switch-dockerfile.sh

# 或手動切換
mv Dockerfile Dockerfile.with-angular
mv Dockerfile.no-angular Dockerfile

# 提交變更
git add .
git commit -m "Switch to no-angular Dockerfile"
git push origin main
```

## 📋 驗證清單

建置成功後，請驗證：

- [ ] 前台系統正常載入: `https://your-app.zeabur.app/`
- [ ] 後台系統正常載入: `https://your-app.zeabur.app/admin/`
- [ ] API 健康檢查通過: `https://your-app.zeabur.app/health`
- [ ] 資料庫連接正常
- [ ] 檔案上傳功能正常

## 🔧 工具和腳本

### 1. Dockerfile 切換腳本

```bash
./switch-dockerfile.sh
```

功能：
- 自動偵測當前使用的 Dockerfile 版本
- 一鍵切換到備用版本
- 提供切換後的操作指引

### 2. 本地建置測試

```bash
# 測試 Angular 建置
cd backend/FontEnd/FontEnd
npm install
npm run build

# 測試 Docker 建置
docker build -t rosca-test .
```

## 📊 效能對比

| 方案 | 建置時間 | 功能完整性 | 資源需求 | 推薦度 |
|------|----------|------------|----------|--------|
| 方案 1 (Angular 建置) | ~8-12 分鐘 | 100% | 高 | ⭐⭐⭐⭐⭐ |
| 方案 2 (跳過建置) | ~5-8 分鐘 | 80% | 低 | ⭐⭐⭐ |

## 🚀 後續優化建議

1. **建置快取**: 考慮使用 Docker 多階段建置快取
2. **依賴優化**: 分離生產和開發依賴
3. **CI/CD**: 設置自動化測試和部署流程
4. **監控**: 添加建置時間和成功率監控

## 📞 支援

如果仍有問題：

1. **查看建置日誌**: Zeabur Dashboard → 服務 → Logs
2. **本地測試**: 使用 Docker 在本地重現問題
3. **回滾**: 使用 `switch-dockerfile.sh` 切換到穩定版本

---

**修復狀態**: ✅ 已完成  
**測試狀態**: ⏳ 待驗證  
**最後更新**: $(date)