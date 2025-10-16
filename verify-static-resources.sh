#!/bin/bash

# 驗證前台靜態資源修復腳本

echo "🔍 驗證前台靜態資源載入..."

DOMAIN="https://sf-test.zeabur.app"

# 定義需要測試的靜態資源
declare -a STATIC_RESOURCES=(
    "/scss/home.css"
    "/css/library/bootstrap.min.css"
    "/js/library/jquery.min.js"
    "/js/library/popper.min.js"
    "/js/library/bootstrap.bundle.min.js"
    "/js/library/axios.min.js"
    "/js/library/vue.min.js"
    "/js/header.js"
    "/js/home.js"
    "/images/home/member1.png"
    "/images/home/member2.png"
)

echo "📋 測試 ${#STATIC_RESOURCES[@]} 個靜態資源..."
echo ""

SUCCESS_COUNT=0
FAIL_COUNT=0

# 測試每個靜態資源
for resource in "${STATIC_RESOURCES[@]}"; do
    echo "🔍 測試: $resource"
    
    # 獲取 HTTP 狀態碼
    STATUS_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN$resource")
    
    if [ "$STATUS_CODE" = "200" ]; then
        echo "✅ 成功 (200) - $resource"
        ((SUCCESS_COUNT++))
    else
        echo "❌ 失敗 ($STATUS_CODE) - $resource"
        ((FAIL_COUNT++))
    fi
    echo ""
done

# 測試前台頁面
echo "🏠 測試前台頁面載入..."
PAGE_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/")
if [ "$PAGE_STATUS" = "200" ]; then
    echo "✅ 前台頁面載入成功 (200)"
else
    echo "❌ 前台頁面載入失敗 ($PAGE_STATUS)"
fi
echo ""

# 測試後台頁面
echo "🏢 測試後台頁面載入..."
ADMIN_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/admin")
if [ "$ADMIN_STATUS" = "200" ]; then
    echo "✅ 後台頁面載入成功 (200)"
else
    echo "❌ 後台頁面載入失敗 ($ADMIN_STATUS)"
fi
echo ""

# 總結報告
echo "📊 測試總結"
echo "===================="
echo "✅ 成功載入: $SUCCESS_COUNT 個資源"
echo "❌ 載入失敗: $FAIL_COUNT 個資源"
echo "📄 前台頁面: $([ "$PAGE_STATUS" = "200" ] && echo "正常" || echo "異常")"
echo "📄 後台頁面: $([ "$ADMIN_STATUS" = "200" ] && echo "正常" || echo "異常")"
echo ""

# 建議
if [ "$FAIL_COUNT" -gt 0 ]; then
    echo "🔧 修復建議："
    echo "1. 檢查 Zeabur 部署是否完成"
    echo "2. 確認容器已重新啟動"
    echo "3. 檢查 nginx 配置是否正確"
    echo "4. 驗證前台檔案是否正確複製到容器"
    echo ""
    echo "🔍 除錯命令："
    echo "curl -I $DOMAIN/scss/home.css"
    echo "curl -I $DOMAIN/js/library/jquery.min.js"
else
    echo "🎉 所有靜態資源載入正常！"
    echo "前台系統應該能正常運作了。"
fi

echo ""
echo "🌐 測試 URL："
echo "前台: $DOMAIN/"
echo "後台: $DOMAIN/admin"