/** SEO 配置 */
export interface SeoSetting {
  id: string
  pageKey: string
  title: string | null
  description: string | null
  keywords: string | null
  ogImageUrl: string | null
  canonicalUrl: string | null
}

/** 更新 SEO 配置请求 */
export interface UpdateSeoSettingRequest {
  pageKey: string
  title: string | null
  description: string | null
  keywords: string | null
  ogImageId: string | null
  canonicalUrl: string | null
}

/** 页面标识选项 */
export const PAGE_KEY_OPTIONS = [
  { value: 'home', label: '首页' },
  { value: 'articles', label: '文章列表' },
  { value: 'article', label: '文章详情' },
  { value: 'category', label: '分类页' },
  { value: 'tag', label: '标签页' },
  { value: 'archive', label: '归档页' },
  { value: 'search', label: '搜索页' },
  { value: 'about', label: '关于页' },
  { value: 'project', label: '项目展示' },
  { value: 'friends', label: '友情链接' },
  { value: 'message', label: '留言板' },
  { value: 'global', label: '全局默认' },
]