<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { ThemeSetting } from '@/types/pages'
import { getThemeListApi, saveThemeApi, resetThemeApi, activateThemeApi, createThemeApi, deleteThemeApi, duplicateThemeApi } from '@/api/theme'

const loading = ref(false)
const saving = ref(false)
const themeList = ref<ThemeSetting[]>([])
const selectedId = ref<string | null>(null)

const selectedTheme = computed(() =>
  themeList.value.find(t => t.id === selectedId.value)
)

// 编辑表单
const form = ref({
  themeName: '',
  primaryColor: '#6366f1',
  accentColor: '#a855f7',
  backgroundColor: '#f8fafc',
  surfaceColor: null as string | null,
  textColor: '#111827',
  textSecondaryColor: null as string | null,
  linkColor: '#6366f1',
  borderColor: null as string | null,
  navbarBackground: null as string | null,
  navbarTextColor: null as string | null,
  fontFamily: null as string | null,
  successColor: null as string | null,
  dangerColor: null as string | null,
  warningColor: null as string | null,
  footerBackground: null as string | null,
  footerTextColor: null as string | null,
  heroBackground: null as string | null,
  codeBackground: null as string | null,
  borderRadius: 'medium',
  customCss: '{}'
})

async function load() {
  loading.value = true
  try {
    const res = await getThemeListApi()
    themeList.value = (res.data.data || []) as unknown as ThemeSetting[]
    if (themeList.value.length > 0 && !selectedId.value) {
      selectTheme(themeList.value[0].id)
    }
  } finally {
    loading.value = false
  }
}

function selectTheme(id: string) {
  selectedId.value = id
  const t = themeList.value.find(x => x.id === id)
  if (t) {
    form.value = {
      themeName: t.themeName,
      primaryColor: t.primaryColor,
      accentColor: t.accentColor,
      backgroundColor: t.backgroundColor,
      surfaceColor: t.surfaceColor,
      textColor: t.textColor,
      textSecondaryColor: t.textSecondaryColor,
      linkColor: t.linkColor,
      borderColor: t.borderColor,
      navbarBackground: t.navbarBackground,
      navbarTextColor: t.navbarTextColor,
      fontFamily: t.fontFamily,
      successColor: t.successColor,
      dangerColor: t.dangerColor,
      warningColor: t.warningColor,
      footerBackground: t.footerBackground,
      footerTextColor: t.footerTextColor,
      heroBackground: t.heroBackground,
      codeBackground: t.codeBackground,
      borderRadius: t.borderRadius,
      customCss: t.customCss
    }
  }
}

async function handleSave() {
  if (!selectedId.value) return
  saving.value = true
  try {
    const res = await saveThemeApi(selectedId.value, form.value)
    // 更新列表中的值
    const idx = themeList.value.findIndex(t => t.id === selectedId.value)
    if (idx >= 0) {
      themeList.value[idx] = res.data.data as unknown as ThemeSetting
    }
    ElMessage.success('主题已保存')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function handleActivate(id: string) {
  try {
    await activateThemeApi(id)
    themeList.value.forEach(t => t.isActive = t.id === id)
    ElMessage.success('主题已激活')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '激活失败')
  }
}

async function handleCreate() {
  try {
    const { value } = await ElMessageBox.prompt('请输入主题名称', '新建主题', {
      inputValue: '新主题',
      confirmButtonText: '创建',
      cancelButtonText: '取消'
    })
    if (!value) return
    const res = await createThemeApi(value)
    const created = res.data.data!
    themeList.value.push(created as unknown as ThemeSetting)
    selectTheme(created.id)
    ElMessage.success('主题已创建')
  } catch { /* cancelled */ }
}

async function handleDuplicate() {
  if (!selectedId.value) return
  try {
    const { value } = await ElMessageBox.prompt('请输入副本名称', '复制主题', {
      inputValue: `${selectedTheme.value?.themeName || '主题'} (副本)`,
      confirmButtonText: '复制',
      cancelButtonText: '取消'
    })
    if (!value) return
    const res = await duplicateThemeApi(selectedId.value, value)
    const dup = res.data.data!
    themeList.value.push(dup as unknown as ThemeSetting)
    selectTheme(dup.id)
    ElMessage.success('主题已复制')
  } catch { /* cancelled */ }
}

async function handleDelete() {
  if (!selectedId.value) return
  const t = selectedTheme.value
  if (!t) return
  if (t.isActive) {
    ElMessage.warning('请先激活其他主题再删除当前主题')
    return
  }
  try {
    await ElMessageBox.confirm(`确定删除主题「${t.themeName}」？`, '删除主题', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消'
    })
  } catch { return }

  try {
    await deleteThemeApi(selectedId.value)
    themeList.value = themeList.value.filter(x => x.id !== selectedId.value)
    if (themeList.value.length > 0) {
      selectTheme(themeList.value[0].id)
    } else {
      selectedId.value = null
    }
    ElMessage.success('主题已删除')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

async function handleReset() {
  if (!selectedId.value) return
  try {
    await ElMessageBox.confirm('确定将当前主题恢复默认颜色吗？', '恢复默认', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
  } catch { return }

  saving.value = true
  try {
    // 先激活，再重置
    await activateThemeApi(selectedId.value)
    const res = await resetThemeApi()
    const d = res.data.data
    if (!d) return
    form.value = {
      themeName: d.themeName,
      primaryColor: d.primaryColor,
      accentColor: d.accentColor,
      backgroundColor: d.backgroundColor,
      surfaceColor: d.surfaceColor,
      textColor: d.textColor,
      textSecondaryColor: d.textSecondaryColor,
      linkColor: d.linkColor,
      borderColor: d.borderColor,
      navbarBackground: d.navbarBackground,
      navbarTextColor: d.navbarTextColor,
      fontFamily: d.fontFamily,
      successColor: d.successColor,
      dangerColor: d.dangerColor,
      warningColor: d.warningColor,
      footerBackground: d.footerBackground,
      footerTextColor: d.footerTextColor,
      heroBackground: d.heroBackground,
      codeBackground: d.codeBackground,
      borderRadius: d.borderRadius,
      customCss: d.customCss
    }
    const idx = themeList.value.findIndex(t => t.id === selectedId.value)
    if (idx >= 0) {
      themeList.value[idx] = d as unknown as ThemeSetting
      themeList.value.forEach(t => t.isActive = t.id === selectedId.value)
    }
    ElMessage.success('主题已恢复默认')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '重置失败')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="theme-page" v-loading="loading">
    <!-- 工具条 -->
    <div class="page-header">
      <h3>主题配置</h3>
      <div class="page-actions">
        <el-button size="small" @click="handleCreate">+ 新建主题</el-button>
        <el-button size="small" :disabled="!selectedId" @click="handleDuplicate">复制</el-button>
        <el-button size="small" :disabled="!selectedId" @click="handleReset">恢复默认</el-button>
        <el-button size="small" :disabled="!selectedId || selectedTheme?.isActive" @click="selectedId && handleActivate(selectedId)">启用主题</el-button>
        <el-button size="small" :disabled="!selectedId || selectedTheme?.isActive" @click="handleDelete" type="danger">删除</el-button>
        <el-button type="primary" size="small" :loading="saving" :disabled="!selectedId" @click="handleSave">保存</el-button>
      </div>
    </div>

    <div class="theme-layout" v-if="themeList.length > 0">
      <!-- 左侧：主题列表 -->
      <div class="theme-sidebar">
        <div
          v-for="t in themeList"
          :key="t.id"
          class="theme-card"
          :class="{ active: t.id === selectedId }"
          @click="selectTheme(t.id)"
        >
          <div class="theme-card-header">
            <span class="theme-card-name">{{ t.themeName }}</span>
            <el-tag v-if="t.isActive" size="small" type="success" effect="dark">启用</el-tag>
          </div>
          <div class="theme-colors-row">
            <span class="color-dot" :style="{ backgroundColor: t.primaryColor }" :title="t.primaryColor" />
            <span class="color-dot" :style="{ backgroundColor: t.accentColor }" :title="t.accentColor" />
            <span class="color-dot" :style="{ backgroundColor: t.backgroundColor, border: '0.5px solid #e5e7eb' }" :title="t.backgroundColor" />
            <span class="color-dot" :style="{ backgroundColor: t.textColor }" :title="t.textColor" />
          </div>
        </div>
      </div>

      <!-- 右侧：编辑表单 -->
      <div class="theme-editor">
        <el-card shadow="never">
          <el-form label-width="140px" size="small">
            <el-form-item label="主题名称">
              <el-input v-model="form.themeName" maxlength="50" style="width:280px" />
            </el-form-item>

            <el-divider content-position="left">基础颜色</el-divider>

            <div class="color-grid">
              <el-form-item label="主色">
                <div class="cp-row"><el-color-picker v-model="form.primaryColor" /><el-input v-model="form.primaryColor" style="width:110px;margin-left:6px" /></div>
              </el-form-item>
              <el-form-item label="强调色">
                <div class="cp-row"><el-color-picker v-model="form.accentColor" /><el-input v-model="form.accentColor" style="width:110px;margin-left:6px" /></div>
              </el-form-item>
              <el-form-item label="背景色">
                <div class="cp-row"><el-color-picker v-model="form.backgroundColor" /><el-input v-model="form.backgroundColor" style="width:110px;margin-left:6px" /></div>
              </el-form-item>
              <el-form-item label="卡片底色">
                <div class="cp-row"><el-color-picker v-model="form.surfaceColor" /><el-input v-model="form.surfaceColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="主文字色">
                <div class="cp-row"><el-color-picker v-model="form.textColor" /><el-input v-model="form.textColor" style="width:110px;margin-left:6px" /></div>
              </el-form-item>
              <el-form-item label="次文字色">
                <div class="cp-row"><el-color-picker v-model="form.textSecondaryColor" /><el-input v-model="form.textSecondaryColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="链接色">
                <div class="cp-row"><el-color-picker v-model="form.linkColor" /><el-input v-model="form.linkColor" style="width:110px;margin-left:6px" /></div>
              </el-form-item>
              <el-form-item label="边框色">
                <div class="cp-row"><el-color-picker v-model="form.borderColor" /><el-input v-model="form.borderColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
            </div>

            <el-divider content-position="left">导航栏</el-divider>
            <div class="color-grid">
              <el-form-item label="导航栏背景">
                <div class="cp-row"><el-color-picker v-model="form.navbarBackground" /><el-input v-model="form.navbarBackground" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="导航栏文字色">
                <div class="cp-row"><el-color-picker v-model="form.navbarTextColor" /><el-input v-model="form.navbarTextColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
            </div>

            <el-divider content-position="left">扩展颜色</el-divider>
            <div class="color-grid">
              <el-form-item label="成功色">
                <div class="cp-row"><el-color-picker v-model="form.successColor" /><el-input v-model="form.successColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="危险色">
                <div class="cp-row"><el-color-picker v-model="form.dangerColor" /><el-input v-model="form.dangerColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="警告色">
                <div class="cp-row"><el-color-picker v-model="form.warningColor" /><el-input v-model="form.warningColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="代码块背景">
                <div class="cp-row"><el-color-picker v-model="form.codeBackground" /><el-input v-model="form.codeBackground" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
            </div>

            <el-divider content-position="left">页脚 & Hero</el-divider>
            <div class="color-grid">
              <el-form-item label="页脚背景">
                <div class="cp-row"><el-color-picker v-model="form.footerBackground" /><el-input v-model="form.footerBackground" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="页脚文字色">
                <div class="cp-row"><el-color-picker v-model="form.footerTextColor" /><el-input v-model="form.footerTextColor" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
              <el-form-item label="Hero 背景色">
                <div class="cp-row"><el-color-picker v-model="form.heroBackground" /><el-input v-model="form.heroBackground" style="width:110px;margin-left:6px" placeholder="自动" /></div>
              </el-form-item>
            </div>

            <el-divider content-position="left">样式</el-divider>
            <el-form-item label="字体族">
              <el-radio-group v-model="form.fontFamily">
                <el-radio value="">系统默认</el-radio>
                <el-radio value="sans-serif">无衬线</el-radio>
                <el-radio value="serif">衬线</el-radio>
                <el-radio value="monospace">等宽</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="圆角大小">
              <el-radio-group v-model="form.borderRadius">
                <el-radio value="none">无</el-radio>
                <el-radio value="small">小</el-radio>
                <el-radio value="medium">中</el-radio>
                <el-radio value="large">大</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-divider content-position="left">自定义 CSS 变量 JSON</el-divider>
            <el-form-item label="自定义变量">
              <el-input v-model="form.customCss" type="textarea" :rows="3" />
            </el-form-item>
          </el-form>
        </el-card>
      </div>
    </div>

    <!-- 空状态 -->
    <el-empty v-else-if="!loading" description="暂无主题" />

    <!-- 没有主题详情时的提示 -->
    <el-empty v-if="!selectedId && themeList.length > 0 && !loading" description="请选择一个主题" />
  </div>
</template>

<style scoped>
.theme-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; flex-wrap: wrap; gap: 8px; }
.page-header h3 { margin: 0; font-size: 20px; }
.page-actions { display: flex; gap: 6px; flex-wrap: wrap; }

.theme-layout { display: flex; gap: 20px; min-height: 500px; }
.theme-sidebar { width: 220px; flex-shrink: 0; overflow-y: auto; max-height: calc(100vh - 220px); }
.theme-editor { flex: 1; overflow-y: auto; max-height: calc(100vh - 220px); }

.theme-card {
  padding: 12px;
  margin-bottom: 8px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  background: #fff;
}
.theme-card:hover { border-color: #6366f1; }
.theme-card.active { border-color: #6366f1; box-shadow: 0 0 0 2px rgba(99,102,241,0.15); }
.theme-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
.theme-card-name { font-size: 14px; font-weight: 500; }
.theme-colors-row { display: flex; gap: 6px; }
.color-dot { display: inline-block; width: 18px; height: 18px; border-radius: 50%; }

.color-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0; }
.cp-row { display: flex; align-items: center; }
</style>