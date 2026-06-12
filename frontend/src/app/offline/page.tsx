import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "离线模式 - 个人博客",
};

export default function OfflinePage() {
  return (
    <main className="min-h-[70vh] flex items-center justify-center px-4">
      <div className="text-center">
        <div className="text-6xl mb-6">📡</div>
        <h1 className="text-2xl font-bold mb-3" style={{ color: "var(--color-text-primary)" }}>
          当前处于离线状态
        </h1>
        <p className="mb-8" style={{ color: "var(--color-text-secondary)" }}>
          请检查您的网络连接，稍后重试
        </p>
        <div className="space-y-3">
          <a
            href="/"
            className="inline-block px-8 py-3 rounded-lg font-medium text-white transition-all hover:opacity-90"
            style={{ backgroundColor: "var(--color-primary)" }}
          >
            返回首页
          </a>
          <p className="text-sm" style={{ color: "var(--color-text-tertiary)" }}>
            或尝试刷新页面重新连接
          </p>
        </div>
      </div>
    </main>
  );
}