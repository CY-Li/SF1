# ROSCA 平安商會系統 - .NET 8 API Gateway Dockerfile (Zeabur 優化版)
# 多階段建置：建置階段 + 運行階段

# ===== 建置階段 =====
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# 複製解決方案文件
COPY *.sln .

# 複製所有專案文件以進行依賴項還原 (優化順序以利用 Docker 快取)
COPY Application/AppAbstraction/*.csproj ./Application/AppAbstraction/
COPY Application/AppService/*.csproj ./Application/AppService/
COPY Domain/DomainAbstraction/*.csproj ./Domain/DomainAbstraction/
COPY Domain/DomainEntity/*.csproj ./Domain/DomainEntity/
COPY Domain/DomainEntityDTO/*.csproj ./Domain/DomainEntityDTO/
COPY Infrastructure/Persistence/*.csproj ./Infrastructure/Persistence/
COPY Infrastructure/InfraCommon/*.csproj ./Infrastructure/InfraCommon/
COPY Presentation/DotNetBackEndService/*.csproj ./Presentation/DotNetBackEndService/
COPY DotNetBackEndApi/*.csproj ./DotNetBackEndApi/

# 還原 NuGet 套件 (使用快取優化)
RUN dotnet restore DotNetBackEndApi/DotNetBackEndApi.csproj --runtime linux-musl-x64

# 複製所有原始碼 (排除不必要的檔案)
COPY Application/ ./Application/
COPY Domain/ ./Domain/
COPY Infrastructure/ ./Infrastructure/
COPY Presentation/ ./Presentation/
COPY DotNetBackEndApi/ ./DotNetBackEndApi/

# 建置和發布應用程式 (優化發布設定)
WORKDIR /src/DotNetBackEndApi
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false \
    --no-dependencies \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# ===== 運行階段 =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# 安裝必要的系統套件 (僅保留必要套件以減少映像檔案大小)
RUN apk add --no-cache icu-libs curl tzdata && \
    rm -rf /var/cache/apk/*

# 建立非 root 使用者
RUN addgroup -g 1001 -S appuser && \
    adduser -S appuser -G appuser -u 1001

# 建立應用程式目錄和檔案存儲目錄 (Zeabur 將使用持久化存儲)
RUN mkdir -p /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss /app/logs && \
    chown -R appuser:appuser /app && \
    chmod -R 755 /app

# 複製發布的應用程式
COPY --from=build --chown=appuser:appuser /app/publish .

# 切換到非 root 使用者
USER appuser

# 暴露端口
EXPOSE 5001

# 設定環境變數 (針對 Zeabur 優化)
ENV ASPNETCORE_URLS=http://+:5001 \
    ASPNETCORE_ENVIRONMENT=Production \
    TZ=Asia/Taipei \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 健康檢查 (使用更輕量的檢查方式)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# 啟動應用程式
ENTRYPOINT ["dotnet", "DotNetBackEndApi.dll"]