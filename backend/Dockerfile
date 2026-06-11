FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件并还原
COPY *.csproj .
RUN dotnet restore

# 复制所有源码并构建
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# 创建上传目录和日志目录
RUN mkdir -p /app/uploads /app/logs

# 从构建阶段复制输出
COPY --from=build /app/publish .

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

ENTRYPOINT ["dotnet", "Mblog.API.dll"]