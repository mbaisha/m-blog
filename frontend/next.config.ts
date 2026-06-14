import type { NextConfig } from "next";

/**
 * next.config.ts 在构建时执行，rewrites 需要确定的目标地址。
 * 优先级：
 *   1. NEXT_PUBLIC_API_BASE_URL  （构建时显式传入，CI/特殊部署场景）
 *   2. API_BASE_URL              （构建时显式传入）
 *   3. "http://api:5000/api"     （Docker 网络默认值：前端直连后端容器）
 *   4. "http://localhost:5092/api" （本地开发默认值）
 */
const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE_URL ||
  process.env.API_BASE_URL ||
  (process.env.INTERNAL_API_BASE_URL) ||
  "http://localhost:5092/api";

const nextConfig: NextConfig = {
  images: {
    formats: ["image/webp", "image/avif"],
    remotePatterns: [
      {
        protocol: "http",
        hostname: "localhost",
      },
      {
        protocol: "https",
        hostname: "**",
      },
    ],
    minimumCacheTTL: 86400, // 图片缓存 24 小时
    deviceSizes: [640, 750, 828, 1080, 1200, 1920],
    imageSizes: [16, 32, 48, 64, 96, 128, 256, 384],
  },
  async redirects() {
    return [
      {
        source: '/index.php/archives/:id',
        destination: '/articles/show-:id',
        permanent: true,
      },
      {
        source: '/index.php/archives/:id/',
        destination: '/articles/show-:id',
        permanent: true,
      },
    ];
  },
  async rewrites() {
    const backendUrl = API_BASE.replace(/\/api$/, "");
    return [
      {
        source: "/api/:path*",
        destination: `${backendUrl}/api/:path*`,
      },
      {
        source: "/uploads/:path*",
        destination: `${backendUrl}/uploads/:path*`,
      },
    ];
  },
};

export default nextConfig;
