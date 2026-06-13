import type { NextConfig } from "next";

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5092/api";

const nextConfig: NextConfig = {
  images: {
    formats: ["image/webp", "image/avif"],
    // 本地 /uploads 路径使用自定义 loader，直接用原路径，不经过 /_next/image 代理
    // 因为图片在后端服务器上（通过 rewrites 代理），不是 public/ 目录下的静态文件
    loader: "custom",
    loaderFile: "./src/lib/imageLoader.ts",
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
