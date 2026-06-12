// Service Worker for M-Blog PWA
const CACHE_NAME = 'mblog-cache-v1';
const OFFLINE_PAGE = '/offline';

// 需要预缓存的静态资源
const PRECACHE_ASSETS = [
  '/',
  '/offline',
  '/articles',
  '/projects',
];

// Install - 预缓存核心页面
self.addEventListener('install', (event) => {
  event.waitUntil(
    (async () => {
      const cache = await caches.open(CACHE_NAME);
      await cache.addAll(PRECACHE_ASSETS);
    })()
  );
  self.skipWaiting();
});

// Activate - 清理旧缓存
self.addEventListener('activate', (event) => {
  event.waitUntil(
    (async () => {
      const keys = await caches.keys();
      await Promise.all(
        keys.filter(key => key !== CACHE_NAME).map(key => caches.delete(key))
      );
    })()
  );
  self.clients.claim();
});

// Fetch - 网络优先，失败时回退到缓存
self.addEventListener('fetch', (event) => {
  // 只处理 GET 请求
  if (event.request.method !== 'GET') return;

  // 跳过 API 请求
  if (event.request.url.includes('/api/')) return;

  event.respondWith(
    (async () => {
      try {
        // 网络优先
        const response = await fetch(event.request);
        // 缓存成功的响应
        if (response.ok) {
          const cache = await caches.open(CACHE_NAME);
          cache.put(event.request, response.clone());
        }
        return response;
      } catch (error) {
        // 网络失败，尝试从缓存读取
        const cached = await caches.match(event.request);
        if (cached) return cached;

        // 对于页面导航，返回离线页面
        if (event.request.mode === 'navigate') {
          const offlineCache = await caches.match(OFFLINE_PAGE);
          if (offlineCache) return offlineCache;
        }

        throw error;
      }
    })()
  );
});