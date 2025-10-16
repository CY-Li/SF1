# ROSCA 平安商會系統 - .NET Core 7 Backend Service Dockerfile
# 多階段建置：建置階段 + 運行階段

# ===== 建置階段 =====
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

# 複製解決方案文件
COPY *.sln .

# 複製所有專案文件以進行依賴項還原
COPY Application/AppAbstraction/*.csproj ./Application/AppAbstraction/
COPY Application/AppService/*.csproj ./Application/AppService/
COPY Domain/DomainAbstraction/*.csproj ./Domain/DomainAbstraction/
COPY Domain/DomainEntity/*.csproj ./Domain/DomainEntity/
COPY Domain/DomainEntityDTO/*.csproj ./Domain/DomainEntityDTO/
COPY Infrastructure/Persistence/*.csproj ./Infrastructure/Persistence/
COPY Infrastructure/InfraCommon/*.csproj ./Infrastructure/InfraCommon/
COPY Presentation/DotNetBackEndService/*.csproj ./Presentation/DotNetBackEndService/
COPY DotNetBackEndApi/*.csproj ./DotNetBackEndApi/

# 還原 NuGet 套件
RUN dotnet restore Presentation/DotNetBackEndService/DotNetBackEndService.csproj

# 複製所有原始碼
COPY . .

# 建置和發布應用程式
WORKDIR /src/Presentation/DotNetBackEndService
RUN dotnet publish -c Release -o /app/publish --no-restore --self-contained false

# ===== 運行階段 =====
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
WORKDIR /app

# 安裝必要的系統套件 (Alpine 版本)
RUN apk add --no-cache curl tzdata

# 建立非 root 使用者
RUN addgroup -g 1001 -S appuser && \
    adduser -S appuser -G appuser -u 1001

# 建立應用程式目錄和檔案存儲目錄
RUN mkdir -p /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss /app/logs

# 複製發布的應用程式
COPY --from=build /app/publish .

# 設定目錄權限
RUN chown -R appuser:appuser /app && \
    chmod -R 755 /app

# 切換到非 root 使用者
USER appuser

# 暴露端口
EXPOSE 5001

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:5001 \
    ASPNETCORE_ENVIRONMENT=Production \
    TZ=Asia/Taipei \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# 啟動應用程式
ENTRYPOINT ["dotnet", "DotNetBackEndService.dll"]