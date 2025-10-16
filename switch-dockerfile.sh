#!/bin/bash

# ROSCA 系統 Dockerfile 切換腳本
# 用於在有 Angular 建置和無 Angular 建置版本之間切換

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== ROSCA Dockerfile 切換工具 ===${NC}"
echo ""

# 檢查當前狀態
if [ -f "Dockerfile.with-angular" ]; then
    CURRENT="no-angular"
    AVAILABLE="with-angular"
elif [ -f "Dockerfile.no-angular" ]; then
    CURRENT="with-angular"
    AVAILABLE="no-angular"
else
    echo -e "${RED}✗ 找不到備用 Dockerfile${NC}"
    echo "請確保 Dockerfile.with-angular 或 Dockerfile.no-angular 存在"
    exit 1
fi

echo -e "${YELLOW}當前使用版本: ${CURRENT}${NC}"
echo -e "${YELLOW}可切換版本: ${AVAILABLE}${NC}"
echo ""

# 詢問是否切換
read -p "是否要切換到 ${AVAILABLE} 版本? (y/N): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}正在切換 Dockerfile...${NC}"
    
    # 備份當前版本
    if [ "$CURRENT" = "with-angular" ]; then
        mv Dockerfile Dockerfile.with-angular
        mv Dockerfile.no-angular Dockerfile
        echo -e "${GREEN}✓ 已切換到無 Angular 建置版本${NC}"
        echo "  - 跳過 Angular 建置，直接使用原始檔案"
        echo "  - 建置速度更快，但後台功能可能受限"
    else
        mv Dockerfile Dockerfile.no-angular
        mv Dockerfile.with-angular Dockerfile
        echo -e "${GREEN}✓ 已切換到 Angular 建置版本${NC}"
        echo "  - 完整建置 Angular 專案"
        echo "  - 功能完整，但建置時間較長"
    fi
    
    echo ""
    echo -e "${BLUE}下一步:${NC}"
    echo "1. 提交變更: git add . && git commit -m 'Switch Dockerfile'"
    echo "2. 推送到 GitHub: git push"
    echo "3. Zeabur 會自動重新部署"
    
else
    echo -e "${YELLOW}取消切換${NC}"
fi

echo ""
echo -e "${BLUE}完成！${NC}"