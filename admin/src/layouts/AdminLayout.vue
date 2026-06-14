<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { runtimeConfig } from '@/utils/runtimeConfig'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

/** 侧边栏是否折叠 */
const isCollapsed = ref(false)

/** 站点名称（运行时从环境变量读取，部署时由 .env 注入） */
const siteName = runtimeConfig.SITE_NAME
const user = computed(() => authStore.user)

/** 菜单分组定义 */
interface MenuGroup {
  title: string
  icon: string
  children: { path: string; title: string; icon: string }[]
}

const menuGroups = computed<MenuGroup[]>(() => [
  {
    title: '数据统计',
    icon: 'DataLine',
    children: [
      { path: '/dashboard', title: '仪表盘', icon: 'Odometer' },
      { path: '/visits', title: '访问记录', icon: 'DataLine' },
      { path: '/statistics', title: '数据统计', icon: 'TrendCharts' },
    ]
  },
  {
    title: '内容管理',
    icon: 'Document',
    children: [
      { path: '/articles', title: '文章管理', icon: 'Document' },
      { path: '/categories', title: '分类管理', icon: 'Collection' },
      { path: '/tags', title: '标签管理', icon: 'PriceTag' },
      { path: '/comments', title: '评论管理', icon: 'ChatDotSquare' },
      { path: '/messages', title: '留言管理', icon: 'ChatLineSquare' },
      { path: '/media', title: '媒体库', icon: 'Picture' },
    ]
  },
  {
    title: '页面管理',
    icon: 'Notebook',
    children: [
      { path: '/pages', title: '自定义页面', icon: 'Notebook' },
      { path: '/profile', title: '个人页面', icon: 'User' },
      { path: '/projects', title: '项目展示', icon: 'Monitor' },
      { path: '/friends', title: '友情链接', icon: 'Link' },
      { path: '/navigation', title: '导航管理', icon: 'Menu' },
      { path: '/footer', title: '页脚管理', icon: 'Bottom' },
    ]
  },
  {
    title: '系统设置',
    icon: 'Setting',
    children: [
      { path: '/seo', title: 'SEO 配置', icon: 'Search' },
      { path: '/theme', title: '主题配置', icon: 'Brush' },
      { path: '/layout', title: '布局管理', icon: 'Grid' },
      { path: '/settings', title: '系统设置', icon: 'Setting' },
      { path: '/email-settings', title: '邮件设置', icon: 'Message' },
      { path: '/email-templates', title: '邮件模板', icon: 'DocumentCopy' },
      { path: '/subscribers', title: '订阅管理', icon: 'Message' },
      { path: '/email-logs', title: '发送记录', icon: 'List' },
      { path: '/llm-settings', title: '大模型设置', icon: 'Cpu' },
      { path: '/image-gen-settings', title: '文生图设置', icon: 'PictureFilled' },
      { path: '/llm-chat', title: 'AI 对话', icon: 'ChatDotRound' },
      { path: '/image-generate', title: '文生图', icon: 'Picture' },
      { path: '/change-password', title: '修改密码', icon: 'Lock' },
    ]
  },
  {
    title: '数据管理',
    icon: 'FolderOpened',
    children: [
      { path: '/backup', title: '数据备份', icon: 'Download' },
    ]
  }
])

/** 当前激活的菜单 — 匹配父级路径（如 /articles/edit/xxx 高亮 /articles） */
const activeMenu = computed(() => {
  const path = route.path
  // 匹配二级路径（如 /articles, /categories, /pages 等）
  const match = path.match(/^\/([^/]+)/)
  if (match) {
    const base = '/' + match[1]
    // 检查是否在菜单中存在
    for (const group of menuGroups.value) {
      for (const item of group.children) {
        if (item.path === base || item.path.startsWith(base + '/')) {
          return item.path
        }
      }
    }
  }
  return path
})

/** 当前展开的分组 — 根据 activeMenu 自动展开对应分组 */
const openedGroups = ref<string[]>([])

// 监听路由变化，自动展开对应分组
 watch(() => route.path, () => {
  const path = route.path
  for (const group of menuGroups.value) {
    for (const item of group.children) {
      if (path.startsWith(item.path)) {
        if (!openedGroups.value.includes(group.title)) {
          openedGroups.value.push(group.title)
        }
        return
      }
    }
  }
}, { immediate: true })

/** 切换菜单 */
function handleMenuSelect(index: string) {
  router.push(index)
}

/** 下拉菜单命令处理 */
function handleDropdownCommand(command: string) {
  if (command === 'logout') {
    handleLogout()
  } else if (command === 'change-password') {
    router.push('/change-password')
  }
}

/** 退出登录 */
async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}
</script>

<template>
  <el-container class="admin-layout">
    <!-- 侧边栏 -->
    <el-aside :width="isCollapsed ? '64px' : '220px'" class="admin-aside">
      <!-- 站点名称 -->
      <div class="aside-header">
        <h2 v-show="!isCollapsed" class="aside-title">{{ siteName }}</h2>
        <span v-show="isCollapsed" class="aside-title-mini">B</span>
      </div>

      <!-- 二级菜单 -->
      <el-menu
        :default-active="activeMenu"
        :collapse="isCollapsed"
        :collapse-transition="false"
        :default-openeds="openedGroups"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#409eff"
        @select="handleMenuSelect"
      >
        <template v-for="group in menuGroups" :key="group.title">
          <el-sub-menu :index="group.title">
            <template #title>
              <el-icon><component :is="group.icon" /></el-icon>
              <span>{{ group.title }}</span>
            </template>
            <el-menu-item v-for="item in group.children" :key="item.path" :index="item.path">
              <el-icon><component :is="item.icon" /></el-icon>
              <template #title>{{ item.title }}</template>
            </el-menu-item>
          </el-sub-menu>
        </template>
      </el-menu>
    </el-aside>

    <!-- 右侧区域 -->
    <el-container>
      <!-- 顶部栏 -->
      <el-header class="admin-header">
        <div class="header-left">
          <el-icon class="collapse-btn" @click="isCollapsed = !isCollapsed">
            <Fold v-if="!isCollapsed" />
            <Expand v-else />
          </el-icon>
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/dashboard' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-if="route.meta.title">{{ route.meta.title as string }}</el-breadcrumb-item>
          </el-breadcrumb>
        </div>
        <div class="header-right">
          <el-dropdown @command="handleDropdownCommand">
            <span class="user-info">
              <el-avatar :size="32" :src="user?.avatarUrl || undefined">
                {{ user?.username?.charAt(0)?.toUpperCase() || 'U' }}
              </el-avatar>
              <span class="username">{{ user?.username || '未登录' }}</span>
              <el-icon><ArrowDown /></el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="change-password">
                  <el-icon style="margin-right:6px"><Lock /></el-icon>修改密码
                </el-dropdown-item>
                <el-dropdown-item command="logout">
                  <el-icon style="margin-right:6px"><SwitchButton /></el-icon>退出登录
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 内容区 -->
      <el-main class="admin-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.admin-layout {
  height: 100vh;
}
.admin-aside {
  background-color: #304156;
  overflow-y: auto;
  overflow-x: hidden;
  transition: width 0.3s;
}
.aside-header {
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}
.aside-title {
  font-size: 16px;
  white-space: nowrap;
  overflow: hidden;
}
.aside-title-mini {
  font-size: 20px;
  font-weight: bold;
}
.admin-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  background: #fff;
  border-bottom: 1px solid #e6e6e6;
  height: 56px;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.collapse-btn {
  font-size: 20px;
  cursor: pointer;
}
.header-right {
  display: flex;
  align-items: center;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}
.username {
  font-size: 14px;
}
.admin-main {
  background-color: #f0f2f5;
  padding: 16px;
  overflow-y: auto;
}
</style>