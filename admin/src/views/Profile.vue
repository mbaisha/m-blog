<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import type { ProfileSection, SaveProfileSectionRequest } from '@/types/profile'
import { SECTION_TYPE_OPTIONS } from '@/types/profile'
import { getProfileSectionsApi, saveProfileSectionsApi } from '@/api/profile'
import { MdEditor } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'

const loading = ref(false)
const saving = ref(false)
const sections = ref<ProfileSection[]>([])
const activeTab = ref('edit')

/** 加载配置 */
async function loadSections() {
  loading.value = true
  try {
    const res = await getProfileSectionsApi()
    sections.value = res.data.data || []
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

/** 添加空白模块 */
function addSection() {
  sections.value.push({
    id: '',
    sectionType: 'custom',
    title: '',
    content: '',
    sortOrder: sections.value.length,
    metadata: '{}',
    isEnabled: true,
    createdAt: '',
    updatedAt: ''
  })
}

/** 删除模块 */
function removeSection(index: number) {
  sections.value.splice(index, 1)
}

/** 上移 */
function moveUp(index: number) {
  if (index <= 0) return
  const temp = sections.value[index]
  sections.value[index] = sections.value[index - 1]
  sections.value[index - 1] = temp
}

/** 下移 */
function moveDown(index: number) {
  if (index >= sections.value.length - 1) return
  const temp = sections.value[index]
  sections.value[index] = sections.value[index + 1]
  sections.value[index + 1] = temp
}

/** 保存 */
async function handleSave() {
  saving.value = true
  try {
    const requests: SaveProfileSectionRequest[] = sections.value.map((s, i) => ({
      sectionType: s.sectionType,
      title: s.title || null,
      content: s.content || null,
      sortOrder: i,
      metadata: s.metadata || '{}',
      isEnabled: s.isEnabled
    }))
    await saveProfileSectionsApi(requests)
    ElMessage.success('个人页面配置已保存')
    await loadSections()
  } catch {
    ElMessage.error('保存失败')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadSections()
})
</script>

<template>
  <div class="profile-page">
    <div class="page-header">
      <h3>个人页面配置</h3>
      <div class="header-actions">
        <el-button @click="addSection" :disabled="sections.length >= 12">添加模块</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
      </div>
    </div>

    <div v-loading="loading" class="profile-body">
      <el-alert
        title="个人页面由多个模块组成，按顺序从上到下渲染。支持 Hero、简介、技能、经历、教育、联系、自定义等模块类型。"
        type="info"
        show-icon
        closable
        class="mb-4"
      />

      <template v-if="sections.length === 0">
        <el-empty description="暂无模块，点击「添加模块」开始配置" />
      </template>

      <template v-for="(section, index) in sections" :key="index">
        <el-card shadow="hover" class="section-card">
          <template #header>
            <div class="section-header">
              <span class="section-index">#{{ index + 1 }}</span>
              <el-tag size="small">{{ SECTION_TYPE_OPTIONS.find(o => o.value === section.sectionType)?.label || section.sectionType }}</el-tag>
              <div class="section-actions">
                <el-button link type="primary" :disabled="index === 0" @click="moveUp(index)">上移</el-button>
                <el-button link type="primary" :disabled="index === sections.length - 1" @click="moveDown(index)">下移</el-button>
                <el-button link type="danger" @click="removeSection(index)">删除</el-button>
              </div>
            </div>
          </template>

          <el-form label-width="80px" size="small">
            <el-form-item label="模块类型">
              <el-select v-model="section.sectionType" style="width: 200px">
                <el-option v-for="opt in SECTION_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="模块标题">
              <el-input v-model="section.title" placeholder="模块标题，可选" maxlength="255" />
            </el-form-item>
            <el-form-item label="模块内容">
              <MdEditor
                :model-value="section.content ?? ''"
                @update:model-value="section.content = $event ?? null"
                :toolbars="[
                  'bold', 'italic', 'underline', 'strikeThrough',
                  '-',
                  'title', 'quote', 'unorderedList', 'orderedList',
                  '-',
                  'code', 'codeRow', 'link', 'image', 'table',
                  '-',
                  'preview', 'fullscreen'
                ]"
                placeholder="Markdown 格式内容..."
                style="min-height: 300px; width: 100%"
              />
            </el-form-item>
            <el-form-item label="是否启用">
              <el-switch v-model="section.isEnabled" />
            </el-form-item>
          </el-form>
        </el-card>
      </template>

      <div v-if="sections.length > 0" class="save-bottom">
        <el-button type="primary" :loading="saving" size="large" @click="handleSave">保存配置</el-button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-page {
  padding: 0;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-title {
  margin: 0;
  font-size: 20px;
}
.header-actions {
  display: flex;
  gap: 8px;
}
.profile-body {
}
.mb-4 {
  margin-bottom: 16px;
}
.section-card {
  margin-bottom: 12px;
}
.section-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
.section-index {
  font-weight: bold;
  color: #909399;
  font-size: 14px;
}
.section-actions {
  margin-left: auto;
  display: flex;
  gap: 4px;
}
.save-bottom {
  text-align: center;
  padding: 24px 0;
}
</style>