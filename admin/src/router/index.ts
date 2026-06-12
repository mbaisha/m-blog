import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AdminLayout from '@/layouts/AdminLayout.vue'

/** 路由配置 */
const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { title: '登录', noAuth: true }
  },
  {
    path: '/',
    component: AdminLayout,
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '仪表盘', icon: 'Odometer' }
      },
      {
        path: 'articles',
        name: 'Articles',
        component: () => import('@/views/Articles.vue'),
        meta: { title: '文章管理', icon: 'Document', group: 'content' }
      },
      {
        path: 'articles/create',
        name: 'ArticleCreate',
        component: () => import('@/views/ArticleEditor.vue'),
        meta: { title: '新建文章', hidden: true }
      },
      {
        path: 'articles/edit/:id',
        name: 'ArticleEdit',
        component: () => import('@/views/ArticleEditor.vue'),
        meta: { title: '编辑文章', hidden: true }
      },
      {
        path: 'categories',
        name: 'Categories',
        component: () => import('@/views/Categories.vue'),
        meta: { title: '分类管理', icon: 'Collection' }
      },
      {
        path: 'tags',
        name: 'Tags',
        component: () => import('@/views/Tags.vue'),
        meta: { title: '标签管理', icon: 'PriceTag' }
      },
      {
        path: 'comments',
        name: 'Comments',
        component: () => import('@/views/Comments.vue'),
        meta: { title: '评论管理', icon: 'ChatDotSquare', group: 'content' }
      },
      {
        path: 'messages',
        name: 'Messages',
        component: () => import('@/views/Messages.vue'),
        meta: { title: '留言管理', icon: 'ChatLineSquare', group: 'content' }
      },
      {
        path: 'media',
        name: 'Media',
        component: () => import('@/views/Media.vue'),
        meta: { title: '媒体库', icon: 'Picture', group: 'content' }
      },
      {
        path: 'profile',
        name: 'Profile',
        component: () => import('@/views/Profile.vue'),
        meta: { title: '个人页面', icon: 'User' }
      },
      {
        path: 'change-password',
        name: 'ChangePassword',
        component: () => import('@/views/ChangePassword.vue'),
        meta: { title: '修改密码', icon: 'Lock' }
      },
      {
        path: 'projects',
        name: 'Projects',
        component: () => import('@/views/Projects.vue'),
        meta: { title: '项目展示', icon: 'Monitor', group: 'pages' }
      },
      {
        path: 'projects/create',
        name: 'ProjectCreate',
        component: () => import('@/views/ProjectEditor.vue'),
        meta: { title: '新建项目', hidden: true }
      },
      {
        path: 'projects/edit/:id',
        name: 'ProjectEdit',
        component: () => import('@/views/ProjectEditor.vue'),
        meta: { title: '编辑项目', hidden: true }
      },
      {
        path: 'friends',
        name: 'Friends',
        component: () => import('@/views/Friends.vue'),
        meta: { title: '友情链接', icon: 'Link' }
      },
      {
        path: 'seo',
        name: 'Seo',
        component: () => import('@/views/Seo.vue'),
        meta: { title: 'SEO 配置', icon: 'Search' }
      },
      {
        path: 'pages',
        name: 'Pages',
        component: () => import('@/views/Pages.vue'),
        meta: { title: '自定义页面', icon: 'Notebook' }
      },
      {
        path: 'pages/create',
        name: 'PageCreate',
        component: () => import('@/views/PageEditor.vue'),
        meta: { title: '新建页面', hidden: true }
      },
      {
        path: 'pages/edit/:id',
        name: 'PageEdit',
        component: () => import('@/views/PageEditor.vue'),
        meta: { title: '编辑页面', hidden: true }
      },
      {
        path: 'navigation',
        name: 'Navigation',
        component: () => import('@/views/Navigation.vue'),
        meta: { title: '导航管理', icon: 'Menu', group: 'pages' }
      },
      {
        path: 'footer',
        name: 'Footer',
        component: () => import('@/views/Footer.vue'),
        meta: { title: '页脚管理', icon: 'Bottom', group: 'pages' }
      },
      {
        path: 'layout',
        name: 'LayoutManage',
        component: () => import('@/views/LayoutManage.vue'),
        meta: { title: '布局管理', icon: 'Grid' }
      },
      {
        path: 'theme',
        name: 'Theme',
        component: () => import('@/views/Theme.vue'),
        meta: { title: '主题配置', icon: 'Brush', group: 'config' }
      },
      {
        path: 'visits',
        name: 'Visits',
        component: () => import('@/views/Visits.vue'),
        meta: { title: '访问记录', icon: 'DataLine', group: 'stats' }
      },
      {
        path: 'statistics',
        name: 'Statistics',
        component: () => import('@/views/Statistics.vue'),
        meta: { title: '数据统计', icon: 'TrendCharts', group: 'stats' }
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('@/views/Settings.vue'),
        meta: { title: '系统设置', icon: 'Setting' }
      },
      {
        path: 'email-settings',
        name: 'EmailSettings',
        component: () => import('@/views/EmailSettings.vue'),
        meta: { title: '邮件设置', icon: 'Message', group: 'config' }
      },
      {
        path: 'email-templates',
        name: 'EmailTemplates',
        component: () => import('@/views/EmailTemplates.vue'),
        meta: { title: '邮件模板', icon: 'DocumentCopy', group: 'config' }
      },
      {
        path: 'subscribers',
        name: 'Subscribers',
        component: () => import('@/views/Subscribers.vue'),
        meta: { title: '订阅管理', icon: 'Message', group: 'config' }
      },
      {
        path: 'email-logs',
        name: 'EmailLogs',
        component: () => import('@/views/EmailLogs.vue'),
        meta: { title: '发送记录', icon: 'List', group: 'config' }
      },
      {
        path: 'llm-settings',
        name: 'LLMSettings',
        component: () => import('@/views/LLMSettings.vue'),
        meta: { title: '大模型设置', icon: 'Cpu', group: 'config' }
      },
      {
        path: 'image-gen-settings',
        name: 'ImageGenSettings',
        component: () => import('@/views/ImageGenSettings.vue'),
        meta: { title: '文生图设置', icon: 'PictureFilled', group: 'config' }
      },
      {
        path: 'llm-chat',
        name: 'LLMChat',
        component: () => import('@/views/LLMChat.vue'),
        meta: { title: 'AI 对话', icon: 'ChatDotRound', group: 'config' }
      },
      {
        path: 'image-generate',
        name: 'ImageGenerate',
        component: () => import('@/views/ImageGenerate.vue'),
        meta: { title: '文生图', icon: 'Picture', group: 'config' }
      },
      {
        path: 'backup',
        name: 'Backup',
        component: () => import('@/views/Backup.vue'),
        meta: { title: '数据备份', icon: 'FolderOpened' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

/** 路由守卫：未登录重定向到登录页 */
router.beforeEach((to, _from, next) => {
  // 更新页面标题
  document.title = `${to.meta.title || '后台管理'} - ${import.meta.env.VITE_SITE_NAME || '个人博客'}`

  // 登录页和无需认证页面直接放行
  if (to.meta.noAuth) {
    next()
    return
  }

  // 检查登录态
  const authStore = useAuthStore()
  if (authStore.isLoggedIn) {
    next()
  } else {
    // 尝试从 localStorage 恢复登录态
    const token = localStorage.getItem('accessToken')
    if (token) {
      // 有 token 但 store 中无用户信息，需要重新获取（暂不处理，跳过到登录页）
      // 简化处理：跳转到登录页
      next('/login')
    } else {
      next('/login')
    }
  }
})

export default router