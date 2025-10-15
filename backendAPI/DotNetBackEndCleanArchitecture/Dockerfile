# ROSCA 平安商會系統 - .NET Core 7 API Dockerfile
# 多階段建置：建置階段 + 運行階段

# ===== 建置階段 =====
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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
RUN dotnet restore

# 複製所有原始碼
COPY . .

# 建置應用程式
WORKDIR /src/DotNetBackEndApi
RUN dotnet build -c Release -o /app/build

# 發布應用程式
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===== 運行階段 =====
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# 安裝必要的系統套件
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 建立非 root 使用者
RUN groupadd -r appuser && useradd -r -g appuser appuser

# 建立應用程式目錄
RUN mkdir -p /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss

# 複製發布的應用程式
COPY --from=build /app/publish .

# 設定目錄權限
RUN chown -R appuser:appuser /app
RUN chmod -R 755 /app

# 切換到非 root 使用者
USER appuser

# 暴露端口
EXPOSE 5000

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=Asia/Taipei

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# 啟動應用程式
ENTRYPOINT ["dotnet", "DotNetBackEndApi.dll"]