# 🚨 Critical Database Fix

## 問題分析
應用仍然使用 `appuser` 連接，儘管 zeabur.json 已改為 `root`。

## 根本問題
zeabur.json 中同時定義了：
1. 外部資料庫連接 (43.167.174.222:31500)
2. 內建 MariaDB 服務定義

這造成配置衝突！

## 🎯 解決方案：移除內建 MariaDB 服務

### 步驟 1: 創建純外部資料庫配置