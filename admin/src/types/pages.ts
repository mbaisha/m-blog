/** 自定义页面 */
export interface PageListItem {
  id: string
  title: string
  slug: string
  summary: string | null
  status: string
  isVisible: boolean
  enableComments: boolean
  sortOrder: number
  createdAt: string
  updatedAt: string
}

export interface PageDetail extends PageListItem {
  content: string | null
  seoTitle: string | null
  seoDescription: string | null
  seoKeywords: string | null
  publishedAt: string | null
}

export interface CreatePageRequest {
  title: string
  slug: string
  content?: string | null
  summary?: string | null
  status?: string
  isVisible?: boolean
  enableComments?: boolean
  sortOrder?: number
  seoTitle?: string | null
  seoDescription?: string | null
  seoKeywords?: string | null
}

export interface UpdatePageRequest extends CreatePageRequest {}

/** 导航菜单 */
export interface NavigationItemNode {
  id: string
  title: string
  url: string
  parentId: string | null
  icon: string | null
  openInNewTab: boolean
  sortOrder: number
  isVisible: boolean
  location: string
  children: NavigationItemNode[]
}

export interface CreateNavigationItemRequest {
  title: string
  url: string
  parentId?: string | null
  icon?: string | null
  openInNewTab?: boolean
  sortOrder?: number
  isVisible?: boolean
  location?: string
}

export interface UpdateNavigationItemRequest extends CreateNavigationItemRequest {}

/** 页脚配置 */
export interface FooterConfig {
  id: string
  copyright: string | null
  icpNumber: string | null
  icpUrl: string | null
  policeNumber: string | null
  policeUrl: string | null
  socialLinks: string
  blocks: string
  style: string
  isVisible: boolean
  contactEmail: string | null
  contactWeChat: string | null
  contactPhone: string | null
}

export interface SaveFooterConfigRequest {
  copyright?: string | null
  icpNumber?: string | null
  icpUrl?: string | null
  policeNumber?: string | null
  policeUrl?: string | null
  socialLinks?: string
  blocks?: string
  style?: string
  isVisible?: boolean
  contactEmail?: string | null
  contactWeChat?: string | null
  contactPhone?: string | null
}

/** 主题配置 */
export interface ThemeSetting {
  id: string
  themeName: string
  primaryColor: string
  accentColor: string
  backgroundColor: string
  textColor: string
  linkColor: string
  navbarBackground: string | null
  navbarTextColor: string | null
  surfaceColor: string | null
  textSecondaryColor: string | null
  borderColor: string | null
  fontFamily: string | null
  successColor: string | null
  dangerColor: string | null
  warningColor: string | null
  footerBackground: string | null
  footerTextColor: string | null
  heroBackground: string | null
  codeBackground: string | null
  borderRadius: string
  customCss: string
  isActive: boolean
}

export interface SaveThemeSettingRequest {
  themeName?: string
  primaryColor?: string
  accentColor?: string
  backgroundColor?: string
  textColor?: string
  linkColor?: string
  navbarBackground?: string | null
  navbarTextColor?: string | null
  surfaceColor?: string | null
  textSecondaryColor?: string | null
  borderColor?: string | null
  fontFamily?: string | null
  successColor?: string | null
  dangerColor?: string | null
  warningColor?: string | null
  footerBackground?: string | null
  footerTextColor?: string | null
  heroBackground?: string | null
  codeBackground?: string | null
  borderRadius?: string
  customCss?: string
}

/** 模块布局 */
export interface ModuleLayout {
  id: string
  pageKey: string
  moduleKey: string
  title: string | null
  sortOrder: number
  isEnabled: boolean
  config: string
}

export interface SaveModuleLayoutRequest {
  moduleKey: string
  title?: string | null
  sortOrder: number
  isEnabled: boolean
  config?: string
}

export interface BatchSaveLayoutRequest {
  pageKey: string
  modules: SaveModuleLayoutRequest[]
}

/** 站点设置 */
export interface SiteSetting {
  id: string
  siteName: string
  siteDescription: string | null
  logoImageId: string | null
  logoImageUrl: string | null
  faviconImageId: string | null
  faviconImageUrl: string | null
  commentModerationEnabled: boolean
  replyNotificationEnabled: boolean
  loginCaptchaEnabled: boolean
  ipBanEnabled: boolean
  ipBanThreshold: number
  ipBanWindowSeconds: number
  ipBanDurationMinutes: number
  visitRetentionDays: number
}

export interface SaveSiteSettingRequest {
  siteName?: string
  siteDescription?: string | null
  logoImageId?: string | null
  faviconImageId?: string | null
  commentModerationEnabled?: boolean
  replyNotificationEnabled?: boolean
  loginCaptchaEnabled?: boolean
  ipBanEnabled?: boolean
  ipBanThreshold?: number
  ipBanWindowSeconds?: number
  ipBanDurationMinutes?: number
  visitRetentionDays?: number
}

/** 订阅者 */
export interface SubscriberItem {
  id: string
  email: string
  subscribedAt: string
  isActive: boolean
  unsubscribedAt: string | null
  confirmedAt: string | null
}

/** 邮件设置 */
export interface EmailSetting {
  id: string
  smtpServer: string
  smtpPort: number
  smtpUsername: string
  smtpPasswordMasked: string
  senderEmail: string
  senderName: string
  useSsl: boolean
}

/** 邮件发送日志 */
export interface EmailLogItem {
  id: string
  email: string
  subject: string
  templateKey: string | null
  sentAt: string
  isSuccess: boolean
  errorMessage: string | null
}

/** 邮件模板 */
export interface EmailTemplate {
  id: string
  templateKey: string
  subject: string
  htmlContent: string
  description: string | null
  createdAt: string
  updatedAt: string
}